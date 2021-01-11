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
   }
}