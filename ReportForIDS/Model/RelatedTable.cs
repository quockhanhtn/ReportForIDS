﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ReportForIDS.Model
{
   /// <summary>
   /// Describe relationship between two table
   /// </summary>
   public class RelatedTable
   {
      /// <summary>
      /// First table name
      /// </summary>
      public string Table1 { get; set; }

      /// <summary>
      /// Second table name
      /// </summary>
      public string Table2 { get; set; }

      /// <summary>
      /// Query for join 2 tables
      /// </summary>
      public string Query { get; set; }

      public bool IsContain(string table) => Table1.Equals(table.ToLower()) || Table2.Equals(table.ToLower());

      public RelatedTable()
      {
      }

      public class XMLData
      {
         [XmlElement(typeof(RelatedTable))]
         public List<RelatedTable> RelatedTable;

         public static void Serialize(List<RelatedTable> list, string filePath)
         {
            using FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            var data = new XMLData() { RelatedTable = list };
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLData));
            xmlSerializer.Serialize(fileStream, data);
            fileStream.Close();
         }

         public static List<RelatedTable> Deserialize(string filePath)
         {
            if (!File.Exists(filePath)) { return new List<RelatedTable>(); }

            List<RelatedTable> list = new List<RelatedTable>();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
               try
               {
                  XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLData));
                  list = (xmlSerializer.Deserialize(fileStream) as XMLData).RelatedTable;
                  fileStream.Close();
                  return list;
               }
               catch (Exception) { fileStream.Close(); }
            }
            return list;
         }
      }
   }
}