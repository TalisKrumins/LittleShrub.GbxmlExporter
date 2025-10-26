using System.Linq;
using Autodesk.Revit.DB;
using LittleShrub.GbxmlExporter.Model;

namespace LittleShrub.GbxmlExporter.Emit
{
  public static class GbFactory
  {
    public static GbXML CreateDocument703(string lengthUnit = "Meters", string tempUnit = "C")
      => new GbXML { LengthUnit = lengthUnit, TemperatureUnit = tempUnit, Version = "7.03" };

    public static Campus AddCampusFrom(Document doc, GbXML gb)
    {
      var siteLoc = doc.ActiveProjectLocation.GetSiteLocation();
      gb.Campus = new Campus
      {
        Id = "Campus-1",
        Location = new Location
        {
          Latitude = siteLoc?.Latitude * 180.0 / System.Math.PI ?? 0,
          Longitude = siteLoc?.Longitude * 180.0 / System.Math.PI ?? 0,
          Elevation = siteLoc?.Elevation ?? 0
        },
        Building = new Building { Id = "Bldg-1" }
      };
      return gb.Campus;
    }
  }
}
