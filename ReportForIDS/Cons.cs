using ReportForIDS.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReportForIDS
{
   public static class Cons
   {
      /// <summary>
      /// Danh sách database không hiển thị khi chọn
      /// </summary>
      public static List<string> ListHiddenSchemas
      {
         get
         {
            string filePath = GetDataDirectory + "\\HiddenSchemas.inf";
            if (listHiddenSchemas.Count == 0 && File.Exists(filePath))
            {
               listHiddenSchemas = File.ReadAllLines(filePath).ToList();
            }
            return listHiddenSchemas;
         }
         set
         {
            listHiddenSchemas = value;
            string filePath = GetDataDirectory + "\\HiddenSchemas.inf";
            if (File.Exists(filePath)) { File.Delete(filePath); }
            File.WriteAllLines(filePath, listHiddenSchemas.ToArray());
         }
      }

      /// <summary>
      /// Danh sách table không hiển thị khi chọn
      /// </summary>
      public static List<string> ListHiddenTables
      {
         get
         {
            string filePath = GetDataDirectory + "\\HiddenTables.inf";
            if (listHiddenTables.Count == 0 && File.Exists(filePath))
            {
               listHiddenTables = File.ReadAllLines(filePath).ToList();
            }
            return listHiddenTables;
         }
         set
         {
            listHiddenTables = value;
            string filePath = GetDataDirectory + "\\HiddenTables.inf";
            if (File.Exists(filePath)) { File.Delete(filePath); }
            File.WriteAllLines(filePath, listHiddenTables.ToArray());
         }
      }

      /// <summary>
      /// Danh sách các trường không hiển thị
      /// </summary>
      public static List<string> ListHiddenFields
      {
         get
         {
            string filePath = GetDataDirectory + "\\HiddenFields.inf";
            if (listHiddenFields.Count == 0 && File.Exists(filePath))
            {
               listHiddenFields = File.ReadAllLines(filePath).ToList();
            }
            return listHiddenFields;
         }
         set
         {
            listHiddenFields = value;
            string filePath = GetDataDirectory + "\\HiddenFields.inf";
            if (File.Exists(filePath)) { File.Delete(filePath); }
            File.WriteAllLines(filePath, listHiddenFields.ToArray());
         }
      }

      public static List<RelatedTable> ListRelatedTables = new List<RelatedTable>();

      public static List<string> ListConditionType = new List<string>()
      {
         "Greater than",
         "Greater than or equal",
         "Less than",
         "Less than or equal",
         "Equal",
         "Not equal",
         "Contain",
         "Not contain",
      };

      static Cons()
      {
         //Load list table not report

         //Load file RelatedTable.xml
         ListRelatedTables = RelatedTable.XMLData.Deserialize(GetDataDirectory + "RelatedTable.xml");

         ToolName = "Report for iDS";
         NullValue = "none";
         ReportTemplateExtension = ".rptemp";
      }

      public static string ToolName { get; set; }
      public static string NullValue { get; set; }
      public static string ReportTemplateExtension { get; set; }

      public static string GetDataDirectory
      {
         get
         {
            string dataDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            dataDir = dataDir.Substring(0, dataDir.LastIndexOf("\\")) + "\\Data\\";
            return dataDir;
         }
      }

      public static string GetReportFilePath { get => GetDataDirectory + "RencentReport.xml"; }
      public static string GetReportTemplateFilePath { get => GetDataDirectory + "ReportTemplateData.xml"; }

      private static List<string> listHiddenSchemas = new List<string>();
      private static List<string> listHiddenTables = new List<string>();
      private static List<string> listHiddenFields = new List<string>();
   }
}