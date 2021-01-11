//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Xml.Serialization;

//namespace ReportForIDS.Model
//{
//   public class MyReport
//   {
//      public string ShortFilePath { get { return (FilePath.Length > 50) ? "..." + FilePath.Substring(FilePath.Length - 50) : FilePath; } }

//      public string ShortDescription
//      {
//         get
//         {
//            if (string.IsNullOrEmpty(Description)) { return "No description"; }
//            return (Description.Length > 40) ? Description.Substring(0, 50) + "..." : Description;
//         }
//      }

//      public string Name { get; set; }
//      public string FilePath { get; set; }
//      public string Description { get; set; }
//      public DateTime CreateTime { get; set; }

//      public MyReport()
//      {
//      }

//      public MyReport(string name, string filePath, string desc)
//      {
//         this.Name = name;
//         this.FilePath = filePath;
//         this.Description = desc;
//         this.CreateTime = DateTime.Now;
//      }

//      public class XMLData
//      {
//         [XmlElement(typeof(MyReport))]
//         public List<MyReport> ReportData;

//         public static void Serialize(List<MyReport> list)
//         {
//            string filePath = Cons.GetReportFilePath;

//            //delete file if exits
//            if (File.Exists(filePath)) { File.Delete(filePath); }

//            using FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
//            var data = new XMLData() { ReportData = list };
//            XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLData));
//            xmlSerializer.Serialize(fileStream, data);
//            fileStream.Close();
//         }

//         public static List<MyReport> Deserialize()
//         {
//            string filePath = Cons.GetReportFilePath;
//            var list = new List<MyReport>();

//            // if file not Exists, return empty list
//            if (!File.Exists(filePath)) { return list; }

//            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
//            {
//               try
//               {
//                  XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLData));
//                  list = (xmlSerializer.Deserialize(fileStream) as XMLData).ReportData;
//                  fileStream.Close();
//               }
//               catch (Exception) { fileStream.Close(); }
//            }

//            // sort descending by CreateTime
//            list.Sort((rp1, rp2) => DateTime.Compare(rp2.CreateTime, rp1.CreateTime));
//            return list;
//         }
//      }

//      public class Update
//      {
//         public static void Add(string name, string filePath, string desc)
//         {
//            MyReport newReport = new MyReport(name, filePath, desc);
//            var listReport = XMLData.Deserialize();
//            listReport.Add(newReport);
//            XMLData.Serialize(listReport);
//         }

//         public static void Edit(MyReport myReport)
//         {
//            var listReport = XMLData.Deserialize();
//            var index = listReport.FindIndex(x => x.CreateTime == myReport.CreateTime);
//            if (index != -1)
//            {
//               listReport[index] = myReport;
//               XMLData.Serialize(listReport);
//            }
//         }

//         public static void Delete(string filePath)
//         {
//            var listReport = XMLData.Deserialize();

//            var deleteItem = listReport.FirstOrDefault(rp => rp.FilePath.Equals(filePath));
//            if (deleteItem != null)
//            {
//               listReport.Remove(deleteItem);
//               XMLData.Serialize(listReport);
//            }
//         }
//      }
//   }
//}