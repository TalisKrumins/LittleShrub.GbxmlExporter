using System;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using LittleShrub.GbxmlExporter.Emit;
using LittleShrub.GbxmlExporter.Model;
using LittleShrub.GbxmlExporter.Util;
using LittleShrub.GbxmlExporter.Core;

namespace LittleShrub.GbxmlExporter.Command
{
  [Transaction(TransactionMode.ReadOnly)]
  public class ExportGbxml703 : IExternalCommand
  {
    public Result Execute(ExternalCommandData c, ref string msg, ElementSet elements)
    {
      UIDocument uidoc = c.Application.ActiveUIDocument;
      if (uidoc == null) { msg = "No active document."; return Result.Failed; }
      Document doc = uidoc.Document;

      try
      {
        // 1) Collect Spaces (or fallback to Rooms)
        var spaces = new FilteredElementCollector(doc)
          .OfClass(typeof(Autodesk.Revit.DB.Mechanical.Space))
          .Cast<Autodesk.Revit.DB.Mechanical.Space>()
          .Where(s => s?.Area > 0)
          .ToList();

        bool usingRooms = false;
        if (spaces.Count == 0)
        {
          usingRooms = true;
          spaces = new FilteredElementCollector(doc)
            .OfClass(typeof(Autodesk.Revit.DB.Architecture.Room))
            .Cast<Autodesk.Revit.DB.Architecture.Room>()
            .Where(r => r?.Area > 0)
            .Select(r => BoundaryUtils.ToPseudoSpace(r))
            .ToList();
        }

        // 2) Create base gbXML document
        var gb = GbFactory.CreateDocument703(lengthUnit: "Meters", tempUnit: "C");
        var campus = GbFactory.AddCampusFrom(doc, gb);    // Includes Location, Building

        // 3) Write Spaces
        var spaceMap = GbWriter.AddSpaces(campus, spaces, doc);

        // 4) Boundaries → Surfaces + Openings (2nd-level)
        foreach (var sp in spaces)
        {
          var shells = BoundaryUtils.GetSecondLevelBoundaries(sp, doc);
          GbWriter.AddSurfacesForSpace(sp, shells, campus, spaceMap, doc);
        }

        // 5) Minimal constructions (single generic per type; feel free to map real assemblies)
        GbWriter.AddConstructions(campus, doc);

        // 6) Serialize
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var outPath = Path.Combine(desktop, $"{Path.GetFileNameWithoutExtension(doc.PathName)}-gbxml703.xml");
        GbSerializer.Save(gb, outPath);

        // 7) Validate against XSD shipped beside addin (gbXML-7.03.xsd)
        var addinFolder = Path.GetDirectoryName(typeof(ExportGbxml703).Assembly.Location) ?? Environment.CurrentDirectory;
        var xsdPath = Path.Combine(addinFolder, "gbXML-7.03.xsd");
        var validation = GbValidator.Validate703(outPath, xsdPath);

        TaskDialog.Show("gbXML 7.03 Export",
          $"Exported:\n{outPath}\n\nSpaces: {spaces.Count} (source: {(usingRooms ? "Rooms" : "Spaces")})\n" +
          $"Validation: {(validation.IsValid ? "PASS" : "FAIL")}\n{validation.Message}");

        return validation.IsValid ? Result.Succeeded : Result.Succeeded; // still produce file for inspection
      }
      catch (Exception ex)
      {
        msg = ex.Message;
        Log.Error(ex);
        return Result.Failed;
      }
    }
  }
}
