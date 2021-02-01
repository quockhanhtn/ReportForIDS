using ReportForIDS.Utils;
using System.Collections.Generic;

namespace ReportForIDS.Model
{
   public class MyTable
   {
      public string TableName { get; set; }
      public List<MyField> Fields { get; set; }

      public MyTable(string tableName)
      {
         this.TableName = tableName;
         this.Fields = DatabaseUtils.GetListField(tableName);
      }

      public MyTable()
      {
      }

      public static string InitJoinQuery(List<MyTable> tables)
      {
         string query = "";

         var listConditionCompare = new List<string>();

         for (int i = 0; i < tables.Count; i++)
         {
            for (int j = i - 1; j >= 0; j--)
            {
               var findResult = Cons.ListRelatedTables.Find(x => x.Table1.Equals(tables[i].TableName.ToLower()) && x.Table2.Equals(tables[j].TableName.ToLower()));
               if (findResult != null)
               {
                  listConditionCompare.Add(findResult.Query);
               }
            }
            for (int j = i + 1; j < tables.Count; j++)
            {
               var findResult = Cons.ListRelatedTables.Find(x => x.Table1.Equals(tables[i].TableName.ToLower()) && x.Table2.Equals(tables[j].TableName.ToLower()));
               if (findResult != null)
               {
                  listConditionCompare.Add(findResult.Query);
               }
            }
         }

         query += string.Join(" and ", listConditionCompare);

         return query;
      }

      //public static string InitInnerJoinQuery(MyTable table1, MyTable table2)
      //{
      //   string query = $"{table1.TableName} inner join {table2.TableName} on ";

      //   var commonField = table1.Fields.Intersect(table2.Fields).ToList();
      //   commonField.ForEach(f => query += $" {table1.TableName}.{f} = {table2.TableName}.{f} and");

      //   return query.Substring(0, query.LastIndexOf("and"));
      //}

      //public static string InitInnerJoinQuery(List<MyTable> tables)
      //{
      //   string query = string.Join(", ", tables.Select(tb => tb.TableName).ToArray()) + " where ";

      //   for (int i = 0; i < tables.Count; i++)
      //   {
      //      for (int j = i - 1; j >= 0; j--)
      //      {
      //         var commonField = tables[i].Fields.Intersect(tables[j].Fields).ToList();
      //         commonField.ForEach(f => query += $" {tables[i].TableName}.{f} = {tables[j].TableName}.{f} and ");
      //      }
      //      for (int j = i + 1; j < tables.Count; j++)
      //      {
      //         var commonField = tables[i].Fields.Intersect(tables[j].Fields).ToList();
      //         commonField.ForEach(f => query += $" {tables[i].TableName}.{f} = {tables[j].TableName}.{f} and ");
      //      }
      //   }

      //   return query.Substring(0, query.LastIndexOf("and"));
      //}
   }
}