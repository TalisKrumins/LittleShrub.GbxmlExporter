using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using LittleShrub.GbxmlExporter.Core;
using LittleShrub.GbxmlExporter.Model;

namespace LittleShrub.GbxmlExporter.Emit
{
  public static class GbWriter
  {
    public static Dictionary<ElementId, Space> AddSpaces(
      Campus campus,
      IEnumerable<Autodesk.Revit.DB.Mechanical.Space> spaces,
      Document doc)
    {
      var map = new Dictionary<ElementId, Space>();
      foreach (var s in spaces)
      {
        var sp = new Space
        {
          Id = $"S-{s.Id.IntegerValue}",
          Name = s.Name ?? $"Space {s.Number}",
          Area = UnitUtils.ConvertFromInternalUnits(s.Area, DisplayUnitType.DUT_SQUARE_METERS),
          Volume = UnitUtils.ConvertFromInternalUnits(s.Volume, DisplayUnitType.DUT_CUBIC_METERS),
          CADObjectId = s.Id.IntegerValue.ToString()
        };
        campus.Building.Spaces.Add(sp);
        map[s.Id] = sp;
      }
      return map;
    }

    public static void AddSurfacesForSpace(
      Autodesk.Revit.DB.Mechanical.Space hostSpace,
      IList<BoundaryUtils.Shell> shells,
      Campus campus,
      Dictionary<ElementId, Space> spaceMap,
      Document doc)
    {
      foreach (var sh in shells)
      {
        foreach (var bf in sh.Faces)
        {
          var face = bf.Face!;
          GeomMath.GetTiltAzimuth(face, out var tilt, out var azimuth);
          var surfType = Classification.SurfaceTypeFor(face, bf.AdjacentSpaceId, doc);

          var surf = new Surface
          {
            Id = $"F-{hostSpace.Id.IntegerValue}-{face.ComputeHashCode()}",
            Name = surfType,
            SurfaceType = surfType,
            Tilt = Math.Round(tilt, 3),
            Azimuth = Math.Round(azimuth, 3),
            CADObjectId = bf.HostElemId?.IntegerValue.ToString() ?? "",
            ConstructionIdRef = "Generic",
            PlanarGeometry = new PlanarGeometry
            {
              PolyLoop = ToPolyLoop(face)
            }
          };

          // Interior adjacency
          if (bf.AdjacentSpaceId is ElementId adjId && spaceMap.TryGetValue(adjId, out var adjSpace))
          {
            surf.AdjacentSpaceId = new AdjacentSpaceId { SpaceIdRef = adjSpace.Id };
          }

          // Openings (holes in the face)
          foreach (var hole in bf.OpeningLoops)
          {
            var opening = new Opening
            {
              Id = $"{surf.Id}-O-{hole.GetHashCode()}",
              OpeningType = "FixedWindow",
              CADObjectId = bf.HostElemId?.IntegerValue.ToString() ?? "",
              PlanarGeometry = new PlanarGeometry { PolyLoop = ToPolyLoop(hole) }
            };
            surf.Openings.Add(opening);
          }

          campus.Building.Surfaces.Add(surf);
        }
      }
    }

    private static PolyLoop ToPolyLoop(PlanarFace face)
    {
      var outer = face.GetEdgesAsCurveLoops()[0];
      return ToPolyLoop(outer);
    }

    private static PolyLoop ToPolyLoop(CurveLoop loop)
    {
      var pl = new PolyLoop();
      foreach (var c in loop)
      {
        var p = c.GetEndPoint(0);
        pl.CartesianPoints.Add(new CartesianPoint { Coordinate = new() { p.X, p.Y, p.Z } });
      }
      // close the loop
      var first = loop.First().GetEndPoint(0);
      pl.CartesianPoints.Add(new CartesianPoint { Coordinate = new() { first.X, first.Y, first.Z } });
      return pl;
    }

    public static void AddConstructions(Campus campus, Document doc)
    {
      // Minimal: one Generic; expand to map Revit assemblies → gbXML Constructions
      if (!campus.Constructions.Any(c => c.Id == "Generic"))
        campus.Constructions.Add(new Construction { Id = "Generic", Name = "Generic" });
    }
  }
}
