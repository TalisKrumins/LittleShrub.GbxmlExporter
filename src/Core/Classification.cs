using Autodesk.Revit.DB;

namespace LittleShrub.GbxmlExporter.Core
{
  public static class Classification
  {
    public static string SurfaceTypeFor(PlanarFace face, ElementId? adjacentSpaceId, Document doc)
    {
      // Exterior if no adjacency detected
      if (adjacentSpaceId is null) {
        // Horizontal up or down?
        var n = face.FaceNormal;
        if (n.Z > 0.707) return "Roof";
        if (n.Z < -0.707) return "SlabOnGrade"; // heuristic; could refine using Level elevation & Site
        return "ExteriorWall";
      }
      // Interior partition if adjacent to another space
      return "InteriorWall";
    }
  }
}
