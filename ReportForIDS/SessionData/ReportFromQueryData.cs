using ReportForIDS.Model;
using ReportForIDS.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;

namespace ReportForIDS.SessionData
{
   public static class ReportFromQueryData
   {
      public static List<MyQuery> ListQueries { get => listQueries; set => listQueries = value; }
      public static DataTable ExecuteDataTable { get => executeDataTable; set => executeDataTable = value; }
      public static ObservableCollection<MyField> ListAllFields { get => listAllFields; set => listAllFields = value; }
      public static ObservableCollection<MyField> ListFieldToGroup { get => listFieldToGroup; set => listFieldToGroup = value; }
      public static List<string> ListFieldToHide { get => listFieldToHide; set => listFieldToHide = value; }

      /// <summary>
      /// True -> create multicolumn <br/>
      /// False -> group into one column, seperate by comma
      /// </summary>
      public static bool GroupToMultiColumn { get; set; }

      public static Thread ThreadExcecuteData { get; set; }

      public static void SetListQueries(List<MyQuery> queries)
      {
         ListQueries.Clear();
         ListQueries.AddRange(queries);
         //ListQueries.ForEach(x => x.ExecuteQuery());
      }

      public static void SetListFieldToGroup(IEnumerable<MyField> fields)
      {
         ListFieldToGroup.Clear();
         ListFieldToGroup.AddRange(fields);
      }

      public static void SetListFieldToHide(IEnumerable<MyField> fields)
      {
         ListFieldToHide.Clear();

         foreach (MyField f in fields)
         {
            string fieldName = f.FieldName.ToUpper();
            while (ListFieldToHide.Contains(fieldName))
            {
               fieldName += "_";
            }

            ListFieldToHide.Add(fieldName);
         }
      }

      public static void JoinDatatableInQuery()
      {
         ThreadExcecuteData = new Thread(() =>
         {
            #region Update Primary Query
            ListQueries.ForEach(x => { while (!x.ExecDone) { } });

            ListQueries.ForEach(q => q.IsPrimary = false);
            int primaryIndex = 0;

            var listQueryHasSelectedField = ListFieldToGroup.GroupBy(f => f.TableName).Select(x => x.Key).ToList();
            if (listQueryHasSelectedField.Count > 1)
            {
               for (int i = 1; i < ListQueries.Count; i++)
               {
                  if (listQueryHasSelectedField.Contains(listQueries[i].AliasTableName))
                  {
                     if (ListQueries[i].MaxRecordSameId > ListQueries[primaryIndex].MaxRecordSameId)
                     {
                        primaryIndex = i;
                     }
                  }
               }
            }
            else
            {
               for (int i = 1; i < ListQueries.Count; i++)
               {
                  if (listQueryHasSelectedField.Contains(listQueries[i].AliasTableName))
                  {
                     primaryIndex = i;
                     break;
                  }
               }
            }

            ListQueries[primaryIndex].IsPrimary = true;
            #endregion

            if (ListFieldToGroup.IndexOf(ListQueries[0].CompareField) >= 0)
            {
               ListFieldToGroup[ListFieldToGroup.IndexOf(ListQueries[0].CompareField)] = ListQueries[primaryIndex].CompareField;
            }

            ExecuteDataTable = ListQueries[primaryIndex].RawDataTable.Copy();
            ListQueries.FindAll(x => x.IsPrimary != true).ForEach(x => ExecuteDataTable.AddDatatable(x.GetGroupDataTable()));
         })
         { IsBackground = true };
         ThreadExcecuteData.Start();
      }

      private static List<MyQuery> listQueries = new List<MyQuery>();
      private static DataTable executeDataTable = new DataTable();

      private static ObservableCollection<MyField> listAllFields = new ObservableCollection<MyField>();
      private static ObservableCollection<MyField> listFieldToGroup = new ObservableCollection<MyField>();
      private static List<string> listFieldToHide = new List<string>();
   }
}