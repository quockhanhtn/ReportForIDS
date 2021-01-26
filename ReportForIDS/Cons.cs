using ReportForIDS.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReportForIDS
{
   public static class Cons
   {
      public static readonly string TOOL_NAME = "Tool custom report";
      public static readonly string REPORT_TEMPLATE_EXTENSION = ".rptemp";
      public static readonly string NULL_VALUE = "none";

      public static readonly List<string> LIST_CONDITION_TYPE = new List<string>()
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

      /// <summary>
      /// List Schema (Database) will not display in select
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
      /// List table will not display in select
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
      /// List Field will not display in select
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

      /// <summary>
      /// List object describe table relationship
      /// </summary>
      public static List<RelatedTable> ListRelatedTables
      {
         get
         {
            string filePath = GetDataDirectory + "RelatedTable.xml";
            if (listRelatedTables.Count == 0 && File.Exists(filePath))
            {
               listRelatedTables = RelatedTable.XMLData.Deserialize(filePath);
            }
            return listRelatedTables;
         }
      }

      public static string GetDataDirectory
      {
         get
         {
            string dataDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            dataDir = dataDir.Substring(0, dataDir.LastIndexOf("\\")) + "\\Data\\";
            return dataDir;
         }
      }

      private static List<string> listHiddenSchemas = new List<string>();
      private static List<string> listHiddenTables = new List<string>();
      private static List<string> listHiddenFields = new List<string>();
      private static List<RelatedTable> listRelatedTables = new List<RelatedTable>();
   }
}