using ReportForIDS.SessionData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ReportForIDS.Model
{
   public class PrevSetting
   {
      public int Step { get; set; }

      [XmlElement("Tables", typeof(MyTable))]
      public List<MyTable> Tables = new List<MyTable>();

      [XmlElement(typeof(MyField))]
      public List<MyField> FieldsDisplay = new List<MyField>();

      [XmlElement(typeof(MyCondition))]
      public List<MyCondition> Conditions = new List<MyCondition>();

      [XmlElement(typeof(MyField))]
      public List<MyField> FieldsGroup = new List<MyField>();

      public PrevSetting()
      {
      }

      public PrevSetting(int _Step)
      {
         Step = _Step;
         Tables = StepByStepData.ListTable.ToList();
         FieldsDisplay = StepByStepData.ListFieldDisplay.ToList();
         Conditions = StepByStepData.ListCondition.ToList();
         FieldsGroup = StepByStepData.ListFieldGroup.ToList();
      }

      public void UpdateDataPrevSetting()
      {
         StepByStepData.Step = this.Step;
         StepByStepData.ListTable = this.Tables;
         StepByStepData.ListFieldDisplay = this.FieldsDisplay;
         StepByStepData.ListCondition = this.Conditions;
         StepByStepData.ListFieldGroup = this.FieldsGroup;
      }

      public static void Serialize(PrevSetting data, string filePath)
      {
         using FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
         XmlSerializer xmlSerializer = new XmlSerializer(typeof(PrevSetting));
         xmlSerializer.Serialize(fileStream, data);
         fileStream.Close();
      }

      public static object Deserialize(string filePath)
      {
         if (!File.Exists(filePath)) { return null; }

         using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
         {
            try
            {
               XmlSerializer xmlSerializer = new XmlSerializer(typeof(PrevSetting));
               var data = xmlSerializer.Deserialize(fileStream);
               fileStream.Close();
               return data;
            }
            catch (Exception) { fileStream.Close(); }
         }
         return null;
      }
   }
}