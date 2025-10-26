using System.IO;
using System.Xml;
using System.Xml.Serialization;
using LittleShrub.GbxmlExporter.Model;

namespace LittleShrub.GbxmlExporter.Emit
{
  public static class GbSerializer
  {
    public static void Save(GbXML gb, string path)
    {
      var ns = new XmlSerializerNamespaces();
      ns.Add("", ""); // no namespaces

      var ser = new XmlSerializer(typeof(GbXML));
      var settings = new XmlWriterSettings { Indent = true, NewLineOnAttributes = false };
      using var w = XmlWriter.Create(path, settings);
      ser.Serialize(w, gb, ns);
    }
  }
}
