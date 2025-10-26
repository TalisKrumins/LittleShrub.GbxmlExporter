using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace LittleShrub.GbxmlExporter.Emit
{
  public static class GbValidator
  {
    public record ValidationResult(bool IsValid, string Message);

    public static ValidationResult Validate703(string xmlPath, string xsdPath)
    {
      if (!File.Exists(xmlPath)) return new(false, "XML not found.");
      if (!File.Exists(xsdPath)) return new(false, "XSD not found (gbXML-7.03.xsd).");

      bool ok = true;
      string msg = "OK";

      var schema = new XmlSchemaSet();
      schema.Add(null, xsdPath);

      var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = schema };
      settings.ValidationEventHandler += (s, e) =>
      {
        ok = false;
        msg = $"{e.Severity}: {e.Message}";
      };

      using var reader = XmlReader.Create(xmlPath, settings);
      try { while (reader.Read()) { } }
      catch (Exception ex) { ok = false; msg = ex.Message; }

      return new(ok, msg);
    }
  }
}
