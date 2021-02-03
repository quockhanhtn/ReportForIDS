using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace ReportForIDS.Model
{
   public class MyQuery
   {
      public static int LastOrder = 0;
      public string DisplayOrder { get => Order.ToString().PadLeft(2, '0'); }

      public string AliasTableName { get => "Query_" + DisplayOrder; }

      public string SQLQuery { get; set; }

      public MyField CompareField { get; set; }

      [XmlIgnore]
      public ObservableCollection<MyField> ListFeilds { get; set; } = new ObservableCollection<MyField>();

      [XmlIgnore]
      public int Order
      {
         get => order;
         set
         {
            order = value;
            if (CompareField != null)
            {
               CompareField.TableName = AliasTableName;
            }
            for (int i = 0; i < ListFeilds.Count; i++)
            {
               ListFeilds[i].TableName = AliasTableName;
            }
         }
      }

      [XmlIgnore]
      public bool IsPrimary { get; set; }

      [XmlIgnore]
      public string ExecResult { get; set; }

      [XmlIgnore]
      public bool ExecDone { get; set; }

      [XmlIgnore]
      public int MaxRecordSameId { get; set; }

      [XmlIgnore]
      public DataTable RawDataTable { get; set; }

      [XmlIgnore]
      public Thread ExecThread { get; set; }

      public MyQuery()
      {
         Order = ++LastOrder;
      }

      public void ExecuteQuery()
      {
         ExecDone = false;
         ExecThread = new Thread(() =>
         {
            if (ListFeilds.IndexOf(CompareField) != 0)
            {
               ListFeilds.Move(ListFeilds.IndexOf(CompareField), 0);
            }

            string query = "select " + string.Join(", ", ListFeilds.Select(x => x.GetFullName()).ToArray());
            query += " from " + SQLQuery.AliasSQL(AliasTableName);
            query += " order by " + CompareField.GetFullName() + " asc";

            RawDataTable = DatabaseUtils.ExecuteQuery(query.ToUpper());

            if (RawDataTable != null)
            {
               MaxRecordSameId = (
                  from row in RawDataTable.AsEnumerable()
                  group row by row.Field<Object>(0) into compareField
                  orderby compareField.Count() descending
                  select compareField.Count()
               ).Take(1).FirstOrDefault();
            }

            ExecDone = true;
         })
         { IsBackground = true };
         ExecThread.Start();
      }

      public DataTable GetGroupDataTable()
      {
         DataTable dt = RawDataTable.CopyAndConvertToString();

         string primaryFeild = this.CompareField.FieldName;
         for (int i = dt.Rows.Count - 1; i > 0; i--)
         {
            if (dt.EqualWithBeforeRow(i, new string[] { primaryFeild }))
            {
               for (int j = 1; j < dt.Columns.Count; j++)
               {
                  DataColumn column = dt.Columns[j];
                  dt.Rows[i - 1][column] += ", " + dt.Rows[i][column];
               }
               dt.Rows.RemoveAt(i);
            }
         }

         return dt;
      }

      public DataTable GetDataTableNotGroup()
      {
         if (ListFeilds.IndexOf(CompareField) != 0)
         {
            ListFeilds.Move(ListFeilds.IndexOf(CompareField), 0);
         }

         string query = "select " + string.Join(", ", ListFeilds.Select(x => x.GetFullName()).ToArray());
         query += " from " + SQLQuery.AliasSQL(AliasTableName);
         query += " order by " + CompareField.GetFullName() + " asc";

         DataTable dataTable = DatabaseUtils.ExecuteQuery(query.ToUpper());
         return dataTable;
      }

      public DataTable GetDatatable()
      {
         if (ListFeilds.IndexOf(CompareField) != 0)
         {
            ListFeilds.Move(ListFeilds.IndexOf(CompareField), 0);
         }

         string query = "select " + string.Join(", ", ListFeilds.Select(x => x.GetFullName()).ToArray());
         query += " from " + SQLQuery.AliasSQL(AliasTableName);
         query += " order by " + CompareField.GetFullName() + " asc";

         DataTable dataTable = DatabaseUtils.ExecuteQuery(query.ToUpper()).CopyAndConvertToString();

         //foreach(DataRow dataRow in dataTable.Rows)
         //{
         //   var listValue = new List<dynamic>();
         //   foreach (var field in ListFeilds)
         //   {
         //      dynamic fieldValue = new ExpandoObject();
         //      fieldValue[field.FieldName] = dataRow[field.FieldName];
         //      listValue.Add(fieldValue);
         //   }
         //}

         //var test = [0].
         //dataTable.Rows.Cast<DataRow>().GroupBy(n => new { n["kkkk"] }).Into(dataTable).Sele
         //var a = dataTable.Rows.Cast<DataRow>().GroupBy(n => new { n[0], n[1] }).

         string primaryFeild = this.CompareField.FieldName;
         for (int i = dataTable.Rows.Count - 1; i > 0; i--)
         {
            if (dataTable.EqualWithBeforeRow(i, new string[] { primaryFeild }))
            {
               for (int j = 1; j < dataTable.Columns.Count; j++)
               {
                  DataColumn column = dataTable.Columns[j];
                  dataTable.Rows[i - 1][column] += ", " + dataTable.Rows[i][column];
               }
               dataTable.Rows.RemoveAt(i);
            }
         }

         return dataTable;
      }

      public DataTable GetHeader()
      {
         if (ListFeilds.IndexOf(CompareField) != 0)
         {
            ListFeilds.Move(ListFeilds.IndexOf(CompareField), 0);
         }

         string query = "select " + string.Join(", ", ListFeilds.Select(x => x.GetFullName()).ToArray());
         query += " from " + SQLQuery.AliasSQL(AliasTableName);
         query += " order by " + CompareField.GetFullName() + " asc limit 0";

         return DatabaseUtils.ExecuteQuery(query.ToUpper());
      }

      public void Reload()
      {
         ListFeilds = DatabaseUtils.GetListField(this.SQLQuery, AliasTableName);
         if (ListFeilds.Count == 0)
         {
            CompareField = null;
         }
         else
         {
            CompareField = ListFeilds.First(f => f.FieldName.Equals(CompareField.FieldName));
         }
         ExecuteQuery();
      }

      private int order;

      #region XML DATA

      public class XMLData
      {
         [XmlElement(typeof(MyQuery))]
         public List<MyQuery> ListQueryData { get; set; }

         public static bool Serialize(List<MyQuery> list, string filePath, out string errorMessage)
         {
            errorMessage = "";
            //delete file if exits
            if (File.Exists(filePath)) { File.Delete(filePath); }

            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
               try
               {
                  var data = new XMLData() { ListQueryData = list };
                  XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLData));
                  xmlSerializer.Serialize(fileStream, data);
               }
               catch (Exception e)
               {
                  errorMessage = e.Message;
               }
               finally
               {
                  fileStream.Close();
               }
            }
            return string.IsNullOrEmpty(errorMessage);
         }

         public static List<MyQuery> Deserialize(string filePath, out string errorMessage)
         {
            var list = new List<MyQuery>();
            errorMessage = "";

            // if file not Exists, return empty list
            if (!File.Exists(filePath))
            {
               errorMessage = "File not found";
               return list;
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
               try
               {
                  XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLData));
                  list = (xmlSerializer.Deserialize(fileStream) as XMLData).ListQueryData;
               }
               catch (Exception e)
               {
                  errorMessage = e.Message;
               }
               finally
               {
                  fileStream.Close();
               }
            }
            return list;
         }
      }

      #endregion XML DATA
   }
}