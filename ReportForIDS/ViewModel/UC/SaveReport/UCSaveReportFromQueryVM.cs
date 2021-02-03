using ReportForIDS.SessionData;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ReportForIDS.ViewModel
{
   public class UCSaveReportFromQueryVM : UCSaveReportViewModel
   {
      public int NumFieldToHide { get; set; }

      public UCSaveReportFromQueryVM(Action prevAction) : base(prevAction)
      {
      }

      public override void ExecuteQuery()
      {
         ReportFromQueryData.JoinDatatableInQuery();
         while (ReportFromQueryData.ThreadExcecuteData.IsAlive) { }

         ResultDatatable = ReportFromQueryData.ExecuteDataTable;

         // order column by group -> not group
         var listColumnOrder = ReportFromQueryData.ListFieldToGroup.ToList();
         listColumnOrder.AddUniqueRange(ReportFromQueryData.ListAllFields);

         ResultDatatable.SetColumnsOrder(listColumnOrder.Select(f => f.FieldName).ToArray());
         ResultDatatable.SortByColumns(ReportFromQueryData.ListFieldToGroup.Select(f => f.FieldName + " ASC").ToArray());

         NumFieldToGroup = ReportFromQueryData.ListFieldToGroup.Count;
      }

      public override void GroupResultDataTable()
      {
         static bool IsDifferrentGroup(List<string> lastGroup, DataRowView dataRow)
         {
            if (lastGroup.Count == 0) { return true; }

            for (int i = 0; i < lastGroup.Count; i++)
            {
               if (dataRow[i].ToString() != lastGroup[i])
               {
                  return true;
               }
            }
            return false;
         }

         GroupResultDatatable = ResultDatatable.CloneAndConvertToString();

         var lastGroup = new List<string>();
         int recordsOnRow = 0, colIndex = 0;
         DataRow newDataRow = GroupResultDatatable.NewRow();

         if (ReportFromQueryData.GroupToMultiColumn)
         {
            foreach (DataRowView item in ResultDatatable.DefaultView)
            {
               if (IsDifferrentGroup(lastGroup, item))
               {
                  recordsOnRow = 1;
                  colIndex = 0;

                  GroupResultDatatable.Rows.Add(newDataRow);
                  newDataRow = GroupResultDatatable.NewRow();

                  lastGroup.Clear();
                  for (int i = 0; i < NumFieldToGroup; i++)
                  {
                     newDataRow[colIndex++] = item[i];
                     lastGroup.Add(item[i].ToString());
                  }
                  for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
                  {
                     newDataRow[colIndex++] = item[i];
                  }
               }
               else
               {
                  recordsOnRow += 1;
                  for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
                  {
                     if (colIndex >= GroupResultDatatable.Columns.Count)
                     {
                        string newColumnName = ResultDatatable.Columns[i].ColumnName + "_" + recordsOnRow.ToString();
                        GroupResultDatatable.Columns.Add(newColumnName);
                     }
                     newDataRow[colIndex++] = item[i];
                  }
               }
            }
         }
         else
         {
            foreach (DataRowView item in ResultDatatable.DefaultView)
            {
               if (IsDifferrentGroup(lastGroup, item))
               {
                  colIndex = 0;

                  GroupResultDatatable.Rows.Add(newDataRow);
                  newDataRow = GroupResultDatatable.NewRow();

                  lastGroup.Clear();
                  for (int i = 0; i < NumFieldToGroup; i++)
                  {
                     newDataRow[colIndex++] = item[i];
                     lastGroup.Add(item[i].ToString());
                  }
                  for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
                  {
                     newDataRow[colIndex++] = item[i];
                  }
               }
               else
               {
                  for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
                  {
                     if (!string.IsNullOrEmpty(newDataRow[ResultDatatable.Columns[i].ColumnName].ToString().Trim()))
                     {
                        if (!string.IsNullOrEmpty(item[i].ToString().Trim()))
                        {
                           newDataRow[ResultDatatable.Columns[i].ColumnName] += ", " + item[i];
                        }
                     }
                     else
                     {
                        newDataRow[ResultDatatable.Columns[i].ColumnName] = item[i];
                     }
                  }
               }
            }
         }

         GroupResultDatatable.Rows.Add(newDataRow);      // add last row
         GroupResultDatatable.Rows.RemoveAt(0);          // remove first empty row

         GroupResultDatatable.RemoveColumns(ReportFromQueryData.ListFieldToHide);
      }
   }
}