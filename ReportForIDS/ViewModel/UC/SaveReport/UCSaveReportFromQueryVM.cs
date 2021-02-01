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

         GroupResultDatatable = ResultDatatable.Clone();
         foreach (DataColumn dataColumn in GroupResultDatatable.Columns)
         {
            dataColumn.DataType = typeof(string);
         }

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
      }

      #region Old code

      //public override void SaveReport()
      //{
      //   try
      //   {
      //      using (var excelPackage = GetExcelPackage())
      //      {
      //         ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

      //         var lastGroup = new List<string>();
      //         int maxRecordOnRow = 0, recordsOnRow = 0;
      //         int colIndex = 1, rowIndex = 1;

      //         foreach (DataRow item in ResultDatatable.Rows)
      //         {
      //            if (IsDifferrentGroup(lastGroup, item))
      //            {
      //               if (recordsOnRow > maxRecordOnRow) { maxRecordOnRow = recordsOnRow; }
      //               recordsOnRow = 1;

      //               rowIndex++;
      //               colIndex = 1;

      //               lastGroup.Clear();
      //               for (int i = 0; i < NumFieldToGroup; i++)
      //               {
      //                  AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
      //                  lastGroup.Add(item[i].ToString());
      //               }
      //               for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
      //               {
      //                  AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
      //               }
      //            }
      //            else
      //            {
      //               recordsOnRow += 1;
      //               for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
      //               {
      //                  AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
      //               }
      //            }
      //         }
      //         if (recordsOnRow > maxRecordOnRow) { maxRecordOnRow = recordsOnRow; }

      //         #region Create header
      //         colIndex = 1;
      //         rowIndex = 1;

      //         if (NumFieldToGroup > 0)
      //         {
      //            for (int i = 0; i < NumFieldToGroup; i++)
      //            {
      //               AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName);
      //            }
      //            for (int k = 1; k <= maxRecordOnRow; k++)
      //            {
      //               for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
      //               {
      //                  AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName + "_" + k.ToString());
      //               }
      //            }
      //         }
      //         else
      //         {
      //            for (int i = 0; i < ResultDatatable.Columns.Count; i++)
      //            {
      //               AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName);
      //            }
      //         }
      //         #endregion

      //         ExcelUtils.SaveExcelPackage(excelPackage, FilePath);
      //      }

      //      MyReport.Update.Add(ReportName, FilePath, ReportDesc);

      //      var messageBoxResult = CustomMessageBox.Show("Do you want to open report file ?", Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
      //      if (messageBoxResult == MessageBoxResult.Yes)
      //      {
      //         ExcelUtils.OpenFile(FilePath);
      //      }
      //   }

      //   catch (Exception e)
      //   {
      //      CustomMessageBox.Show("Error :\r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
      //   }
      //}

      //public override void ExecuteQuery()
      //{
      //   string query;
      //   var listFieldGroup = ReportFromQueryData.ListFieldToGroup;
      //   var listFieldSort = new List<MyField>();
      //   listFieldSort.AddRange(listFieldGroup);

      //   var myFieldComparer = new MyFieldEqualityComparer();
      //   foreach (var item in ReportFromQueryData.ListField1)
      //   {
      //      if (!listFieldGroup.Contains(item, myFieldComparer) && !ReportFromQueryData.ListFieldToHide.Contains(item, myFieldComparer))
      //      {
      //         listFieldSort.Add(item);
      //      }
      //   }
      //   foreach (var item in ReportFromQueryData.ListField2)
      //   {
      //      if (!listFieldGroup.Contains(item, myFieldComparer) && !ReportFromQueryData.ListFieldToHide.Contains(item, myFieldComparer))
      //      {
      //         listFieldSort.Add(item);
      //      }
      //   }
      //   listFieldSort.AddRange(ReportFromQueryData.ListFieldToHide);

      //   NumFieldToGroup = listFieldGroup.Count;
      //   NumFieldToHide = ReportFromQueryData.ListFieldToHide.Count;

      //   if (ReportFromQueryData.SelectTwoQuery)
      //   {
      //      query = "select " + string.Join(", ", listFieldSort.Select(x => x.GetFullName()).ToArray());
      //      query += $" from {ReportFromQueryData.SqlQuery1.AliasSQL("DT1")} LEFT JOIN  {ReportFromQueryData.SqlQuery2.AliasSQL("DT2")}";
      //      query += $" ON {ReportFromQueryData.Field1ToCompare.GetFullName()} = {ReportFromQueryData.Field2ToCompare.GetFullName()}";
      //   }
      //   else
      //   {
      //      query = "select " + string.Join(", ", listFieldSort.Select(x => x.GetFullName()).ToArray());
      //      query += $" from {ReportFromQueryData.SqlQuery1.AliasSQL("DT1")}";
      //   }

      //   if (ReportFromQueryData.ListFieldToGroup.Count > 0)
      //   {
      //      query += " order by " + string.Join(", ", StepByStepData.ListFieldGroup.Select(x => x.GetFullName()).ToArray()) + " asc";
      //   }

      //   try
      //   {
      //      WriteQueryToLog(query);
      //      ResultDatatable = DatabaseUtils.ExecuteQuery(query.ToUpper());
      //   }
      //   catch (Exception e)
      //   {
      //      CustomMessageBox.Show("Error \r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK);
      //   }
      //}

      #endregion Old code
   }
}