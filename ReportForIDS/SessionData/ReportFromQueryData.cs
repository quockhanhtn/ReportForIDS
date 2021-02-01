using ReportForIDS.Model;
using ReportForIDS.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;

namespace ReportForIDS.SessionData
{
   public static class ReportFromQueryData
   {
      public static List<MyQuery> ListQueries { get => listQueries; set => listQueries = value; }
      public static DataTable ExecuteDataTable { get => executeDataTable; set => executeDataTable = value; }
      public static ObservableCollection<MyField> ListAllFields { get => listAllFields; set => listAllFields = value; }
      public static ObservableCollection<MyField> ListFieldToGroup { get => listFieldToGroup; set => listFieldToGroup = value; }
      public static ObservableCollection<MyField> ListFieldToHide { get => listFieldToHide; set => listFieldToHide = value; }

      public static Thread ThreadExcecuteData { get; set; }

      /// <summary>
      /// DataTable execute lưu header, datarow = 0
      /// </summary>
      public static DataTable ExecuteDataTableOnlyHeader { get => headerExecuteDataTable; set => headerExecuteDataTable = value; }

      public static void SetListQueries(List<MyQuery> queries)
      {
         ListQueries.Clear();
         ListQueries.AddRange(queries);

         var primaryQuery = ListQueries[0];
         primaryQuery.IsPrimary = true;

         var secondaryQueries = new List<MyQuery>();
         for (int i = 1; i < ListQueries.Count; i++)
         {
            ListQueries[i].IsPrimary = false;
            secondaryQueries.Add(ListQueries[i]);
         }

         //Assign value for ListAllFields
         ListAllFields.Clear();
         ExecuteDataTableOnlyHeader = primaryQuery.GetHeader();
         foreach (var item in secondaryQueries)
         {
            ExecuteDataTableOnlyHeader.AddDatatable(item.GetHeader());
         }
         ListAllFields = ExecuteDataTableOnlyHeader.GetListColumnName();

         ThreadExcecuteData = new Thread(() =>
         {
            #region Assign value for ExecuteDataTable

            ExecuteDataTable = primaryQuery.GetDataTableNotGroup();
            foreach (var item in secondaryQueries)
            {
               ExecuteDataTable.AddDatatable(item.GetDatatable());
            }

            #endregion Assign value for ExecuteDataTable
         });
         ThreadExcecuteData.IsBackground = true;
         ThreadExcecuteData.Start();
      }

      private static List<MyQuery> listQueries = new List<MyQuery>();
      private static DataTable executeDataTable = new DataTable();
      private static DataTable headerExecuteDataTable = new DataTable();

      private static ObservableCollection<MyField> listAllFields = new ObservableCollection<MyField>();
      private static ObservableCollection<MyField> listFieldToGroup = new ObservableCollection<MyField>();
      private static ObservableCollection<MyField> listFieldToHide = new ObservableCollection<MyField>();
   }
}