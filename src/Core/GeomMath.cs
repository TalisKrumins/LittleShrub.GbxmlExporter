using System;
using Autodesk.Revit.DB;

namespace LittleShrub.GbxmlExporter.Core
{
  public static class GeomMath
  {
    public static void GetTiltAzimuth(PlanarFace face, out double tiltDeg, out double azimuthDeg)
    {
      var n = face.FaceNormal.Normalize(); // Revit: Z up
      // Tilt: 0 = horizontal up (roof), 90 = vertical (wall)
      tiltDeg = Math.Acos(Math.Abs(n.Z)) * 180.0 / Math.PI;
      // Azimuth: 0 = North, clockwise positive
      // Project normal to XY plane:
      var nx = n.X; var ny = n.Y;
      var angle = Math.Atan2(nx, ny); // radians, 0 at +Y
      azimuthDeg = (angle * 180.0 / Math.PI + 360.0) % 360.0;
    }

    public static XYZ LoopCentroid(CurveLoop loop)
    {
      double x = 0, y = 0, z = 0; int count = 0;
      foreach (var c in loop)
      {
        var m = c.Evaluate(0.5, true);
        x += m.X; y += m.Y; z += m.Z; count++;
      }
      return new XYZ(x / count, y / count, z / count);
    }
  }
}
