using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace LittleShrub.GbxmlExporter.Core
{
  public static class BoundaryUtils
  {
    // Allows treating a Room as a pseudo-Space when MEP Spaces don't exist
    public static Autodesk.Revit.DB.Mechanical.Space ToPseudoSpace(Autodesk.Revit.DB.Architecture.Room room)
      => (Autodesk.Revit.DB.Mechanical.Space) room; // safe cast trick for collector → we only use Geometry/Area/Id

    public class Shell
    {
      public ElementId HostSpaceId { get; init; } = ElementId.InvalidElementId;
      public List<BoundaryFace> Faces { get; } = new();
    }

    public class BoundaryFace
    {
      public PlanarFace? Face { get; init; }
      public ElementId? HostElemId { get; init; }
      public ElementId? AdjacentSpaceId { get; init; } // null for exterior
      public IList<CurveLoop> OpeningLoops { get; init; } = new List<CurveLoop>();
    }

    public static IList<Shell> GetSecondLevelBoundaries(Autodesk.Revit.DB.Mechanical.Space sp, Document doc)
    {
      var shells = new List<Shell>();
      var calc = new SpatialElementGeometryCalculator(doc, new SpatialElementBoundaryOptions
      {
        SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish
      });

      var result = calc.CalculateSpatialElementGeometry(sp);
      var solid = result.GetGeometry();
      var shell = new Shell { HostSpaceId = sp.Id };

      foreach (var face in solid.Faces.OfType<PlanarFace>())
      {
        var bf = new BoundaryFace { Face = face, HostElemId = FindHost(face, doc) };
        // Adjacent space detection
        var adj = result.GetBoundaryFaceInfo(face);
        if (adj != null && adj.SpatialElement != null && adj.SpatialElement.Id != sp.Id)
          bf.AdjacentSpaceId = adj.SpatialElement.Id;

        // Openings (windows/doors) via face’s BoundaryLoops (holes)
        // CurveLoop[0] is the outer; subsequent loops are holes
        var loops = face.GetEdgesAsCurveLoops();
        for (int i = 1; i < loops.Count; i++) bf.OpeningLoops.Add(loops[i]);

        shell.Faces.Add(bf);
      }

      shells.Add(shell);
      return shells;
    }

    private static ElementId? FindHost(PlanarFace face, Document doc)
    {
      var refIntersector = face.Reference;
      if (refIntersector == null) return null;
      try { return refIntersector.ElementId; } catch { return null; }
    }
  }
}
