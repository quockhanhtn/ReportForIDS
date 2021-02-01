using ReportForIDS.SessionData;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace ReportForIDS.Model
{
   public class MyQuery
   {
      public static int LastOrder = 0;
      public string DisplayOrder { get { return Order.ToString().PadLeft(2, '0'); } }
      public int Order
      {
         get => order; 
         set
         {
            order = value;
            if (CompareField != null)
            {
               CompareField.TableName = "DT" + DisplayOrder;
            }
            for (int i = 0; i < ListFeilds.Count; i++)
            {
               ListFeilds[i].TableName = "DT" + DisplayOrder;
            }
         }
      }
      public bool IsPrimary { get; set; }
      public string SQLQuery { get; set; }
      public string ExecResult { get; set; }
      public MyField CompareField { get; set; }
      public ObservableCollection<MyField> ListFeilds { get; set; } = new ObservableCollection<MyField>();
      public MyQuery()
      {
         Order = ++LastOrder;
      }

      public DataTable GetDataTableNotGroup()
      {
         string query = "";
         var listF = new List<MyField>();
         listF.Add(CompareField);
         listF.AddUniqueRange(ListFeilds);

         query = "select " + string.Join(", ", listF.Select(x => x.GetFullName()).ToArray());
         query += $" from {SQLQuery.AliasSQL("DT" + DisplayOrder)}";
         query += " order by " + CompareField.GetFullName() + " asc";

         DataTable dataTable = DatabaseUtils.ExecuteQuery(query.ToUpper());
         return dataTable;
      }

      //void passvalue(List<string>, DataTable)
      //{

      //}

      public DataTable GetDatatable()
      {
         string query = "";
         var listF = new List<MyField>();
         listF.Add(CompareField);
         listF.AddUniqueRange(ListFeilds);

         query = "select " + string.Join(", ", listF.Select(x => x.GetFullName()).ToArray());
         query += $" from {SQLQuery.AliasSQL("DT" + DisplayOrder)}";
         query += " order by " + CompareField.GetFullName() + " asc";

         DataTable dataTable = DatabaseUtils.ExecuteQuery(query.ToUpper());

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
         string query = "";

         ListFeilds.Move(ListFeilds.IndexOf(CompareField), 0);
         CompareField = ListFeilds[0];

         query = "select " + string.Join(", ", ListFeilds.Select(x => x.GetFullName()).ToArray());
         query += $" from {SQLQuery.AliasSQL("DT" + DisplayOrder)}";
         query += " order by " + CompareField.GetFullName() + " asc limit 0";

         return DatabaseUtils.ExecuteQuery(query.ToUpper());
      }

      private int order;
   }
}
