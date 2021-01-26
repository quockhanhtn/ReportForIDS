using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace ReportForIDS.Model
{
   public class ReportTemplate
   {
      public List<MyQuery> ListQuery { get => listQuery; set => listQuery = value; }
      public string ShortFilePath { get => (FilePath.Length > 50) ? "..." + FilePath.Substring(FilePath.Length - 50) : FilePath; }

      public string ShortDescription
      {
         get
         {
            if (string.IsNullOrEmpty(Description)) { return "No description"; }
            return (Description.Length > 40) ? Description.Substring(0, 50) + "..." : Description;
         }
      }

      public string Name { get; set; }
      public string FilePath { get; set; }
      public string Description { get; set; }
      public DateTime CreateTime { get; set; }

      public ReportTemplate()
      {
      }

      public ReportTemplate(string name, string filePath, string desc, List<MyQuery> listQuery)
      {
         this.Name = name;
         this.FilePath = filePath;
         this.Description = desc;
         this.CreateTime = DateTime.Now;
         this.ListQuery.Clear();
         this.ListQuery.AddRange(listQuery);
         MyQuery.XMLData.Serialize(listQuery, filePath, out _);
      }

      public class XMLData
      {
         [XmlElement(typeof(ReportTemplate))]
         public List<ReportTemplate> ListReportTemplate;

         private static readonly string filePath = Cons.GetDataDirectory + "ReportTemplateData.xml";

         public static void Serialize(List<ReportTemplate> list)
         {
            //delete file if exits
            if (File.Exists(filePath)) { File.Delete(filePath); }

            using FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
               var data = new XMLData() { ListReportTemplate = list };
               XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLData));
               xmlSerializer.Serialize(fileStream, data);
            }
            catch (Exception e)
            {
               CustomMessageBox.Show("Error\r\n\r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
               fileStream.Close();
            }
         }

         public static List<ReportTemplate> Deserialize()
         {
            var list = new List<ReportTemplate>();

            // if file not Exists, return empty list
            if (!File.Exists(filePath)) { return list; }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
               try
               {
                  XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLData));
                  list = (xmlSerializer.Deserialize(fileStream) as XMLData).ListReportTemplate;
               }
               catch (Exception e)
               {
                  CustomMessageBox.Show("Error\r\n\r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
               }
               finally
               {
                  fileStream.Close();
               }
            }

            // sort descending by CreateTime
            list.Sort((rp1, rp2) => DateTime.Compare(rp2.CreateTime, rp1.CreateTime));
            return list;
         }
      }

      public class Update
      {
         public static bool Add(string name, string filePath, string desc, List<MyQuery> listQuery)
         {
            try
            {
               ReportTemplate newReport = new ReportTemplate(name, filePath, desc, listQuery);
               var reportTemplates = XMLData.Deserialize();
               reportTemplates.Add(newReport);
               XMLData.Serialize(reportTemplates);

               return true;
            }
            catch (Exception) { return false; }
         }

         public static bool Edit(ReportTemplate ReportTemplate)
         {
            var listReport = XMLData.Deserialize();
            var index = listReport.FindIndex(x => x.CreateTime == ReportTemplate.CreateTime);
            if (index != -1)
            {
               listReport[index] = ReportTemplate;
               XMLData.Serialize(listReport);
               return true;
            }
            return false;
         }

         public static bool Delete(string filePath)
         {
            var listReport = XMLData.Deserialize();

            var deleteItem = listReport.FirstOrDefault(rp => rp.FilePath.Equals(filePath));
            if (deleteItem != null)
            {
               listReport.Remove(deleteItem);
               XMLData.Serialize(listReport);
               return true;
            }
            return false;
         }
      }

      private List<MyQuery> listQuery = new List<MyQuery>();
   }
}