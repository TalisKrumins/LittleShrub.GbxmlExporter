using System;
using System.IO;

namespace LittleShrub.GbxmlExporter.Util
{
  public static class Log
  {
    public static void Error(Exception ex)
    {
      try
      {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "GbxmlExporter.log.txt");
        File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR {ex}\n");
      }
      catch { /* ignore */ }
    }
  }
}
