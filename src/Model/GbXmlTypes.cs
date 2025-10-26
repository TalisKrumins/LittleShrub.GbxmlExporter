using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LittleShrub.GbxmlExporter.Model
{
  [XmlRoot("gbXML")]
  public class GbXML
  {
    [XmlAttribute("lengthUnit")] public string LengthUnit { get; set; } = "Meters";
    [XmlAttribute("temperatureUnit")] public string TemperatureUnit { get; set; } = "C";
    [XmlAttribute("version")] public string Version { get; set; } = "7.03";

    [XmlElement("Campus")] public Campus Campus { get; set; } = new();
  }

  public class Campus
  {
    [XmlAttribute("id")] public string Id { get; set; } = "Campus-1";
    [XmlElement("Location")] public Location Location { get; set; } = new();
    [XmlElement("Building")] public Building Building { get; set; } = new();
    [XmlArray("Constructions"), XmlArrayItem("Construction")]
    public List<Construction> Constructions { get; set; } = new();
  }

  public class Location
  {
    [XmlElement("Latitude")] public double Latitude { get; set; }
    [XmlElement("Longitude")] public double Longitude { get; set; }
    [XmlElement("Elevation")] public double Elevation { get; set; } = 0.0;
  }

  public class Building
  {
    [XmlAttribute("id")] public string Id { get; set; } = "Bldg-1";
    [XmlArray("Spaces"), XmlArrayItem("Space")]
    public List<Space> Spaces { get; set; } = new();
    [XmlArray("Surfaces"), XmlArrayItem("Surface")]
    public List<Surface> Surfaces { get; set; } = new();
  }

  public class Space
  {
    [XmlAttribute("id")] public string Id { get; set; } = "";
    [XmlElement("Name")] public string Name { get; set; } = "";
    [XmlElement("Area")] public double Area { get; set; }
    [XmlElement("Volume")] public double Volume { get; set; }
    [XmlElement("CADObjectId")] public string CADObjectId { get; set; } = "";
  }

  public class Surface
  {
    [XmlAttribute("id")] public string Id { get; set; } = "";
    [XmlAttribute("surfaceType")] public string SurfaceType { get; set; } = "ExteriorWall";
    [XmlAttribute("tilt")] public double Tilt { get; set; }
    [XmlAttribute("azimuth")] public double Azimuth { get; set; }
    [XmlAttribute("constructionIdRef")] public string ConstructionIdRef { get; set; } = "Generic";
    [XmlElement("Name")] public string Name { get; set; } = "";
    [XmlElement("CADObjectId")] public string CADObjectId { get; set; } = "";
    [XmlElement("AdjacentSpaceId")] public AdjacentSpaceId? AdjacentSpaceId { get; set; }  // optional
    [XmlElement("PlanarGeometry")] public PlanarGeometry PlanarGeometry { get; set; } = new();
    [XmlArray("Openings"), XmlArrayItem("Opening")]
    public List<Opening> Openings { get; set; } = new();
  }

  public class AdjacentSpaceId
  {
    [XmlAttribute("spaceIdRef")] public string SpaceIdRef { get; set; } = "";
  }

  public class PlanarGeometry
  {
    [XmlElement("PolyLoop")] public PolyLoop PolyLoop { get; set; } = new();
  }

  public class PolyLoop
  {
    [XmlElement("CartesianPoint")] public List<CartesianPoint> CartesianPoints { get; set; } = new();
  }

  public class CartesianPoint
  {
    [XmlElement("Coordinate")] public List<double> Coordinate { get; set; } = new() { 0,0,0 };
  }

  public class Opening
  {
    [XmlAttribute("id")] public string Id { get; set; } = "";
    [XmlAttribute("openingType")] public string OpeningType { get; set; } = "FixedWindow";
    [XmlElement("PlanarGeometry")] public PlanarGeometry PlanarGeometry { get; set; } = new();
    [XmlElement("CADObjectId")] public string CADObjectId { get; set; } = "";
  }

  public class Construction
  {
    [XmlAttribute("id")] public string Id { get; set; } = "Generic";
    [XmlElement("Name")] public string Name { get; set; } = "Generic";
  }
}
