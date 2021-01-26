﻿using OfficeOpenXml;
using ReportForIDS.Model;
using ReportForIDS.SessionData;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class UCSaveReportStepByStepVM : UCSaveReportViewModel
   {
      public UCSaveReportStepByStepVM(Action prevAction) : base(prevAction) { }

      public override void ExecuteQuery()
      {
         string query = GenerateQuery();
         try
         {
            WriteQueryToLog(query);

            ResultDatatable = DatabaseUtils.ExecuteQuery(query);
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
               //if (PbStatus != null)
               //{
               //   PbStatus.Maximum = ResultDatatable.Rows.Count;
               //   PbStatus.Maximum *= 1.1;
               //   PbStatus.Value = 0;
               //}
            }));
         }
         catch (Exception e)
         {
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
               CustomMessageBox.Show("Error \r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }));
         }
      }

      public override void GroupResultDataTable()
      {
         static bool IsDifferrentGroup(List<string> lastGroup, DataRow dataRow)
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

         if (StepByStepData.GroupToMultiColumn)
         {
            foreach (DataRow item in ResultDatatable.Rows)
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
                  for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count; i++)
                  {
                     newDataRow[colIndex++] = item[i];
                  }
               }
               else
               {
                  recordsOnRow += 1;
                  for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count; i++)
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
            foreach (DataRow item in ResultDatatable.Rows)
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
                  for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count; i++)
                  {
                     newDataRow[colIndex++] = item[i];
                  }
               }
               else
               {
                  for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count; i++)
                  {
                     if (!string.IsNullOrEmpty(newDataRow[ResultDatatable.Columns[i].ColumnName].ToString().Trim()))
                     {
                        newDataRow[ResultDatatable.Columns[i].ColumnName] += ", " + item[i];
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

      string GenerateQuery()
      {
         NumFieldToGroup = StepByStepData.ListFieldGroup.Count;
         var listFieldSort = new List<MyField>();

         // add item from list field selected to group
         listFieldSort.AddRange(StepByStepData.ListFieldGroup);
         // add item from list field selected display (if not in list)
         listFieldSort.AddUniqueRange(StepByStepData.ListFieldDisplay);

         string query = "select " + string.Join(", ", listFieldSort.Select(x => x.GetFullName()).ToArray());
         query += " from " + string.Join(", ", StepByStepData.ListTable.Select(x => $"`{x.TableName}`").ToArray());

         if (StepByStepData.ListTable.Count > 1)
         {
            query += " where " + MyTable.InitJoinQuery(StepByStepData.ListTable.ToList());
         }

         if (StepByStepData.ListCondition.Count > 0)
         {
            string condtionQuery = string.Join(" and ", StepByStepData.ListCondition.OrderBy(c => c.Order).Select(x => x.ToQuery()).ToArray());
            if (query.IndexOf("where") != -1) { query += " and " + condtionQuery; }
            else { query += " where " + condtionQuery; }
         }

         if (StepByStepData.ListFieldGroup.Count > 0)
         {
            query += " order by " + string.Join(", ", StepByStepData.ListFieldGroup.Select(x => x.GetFullName()).ToArray()) + " asc";
         }

         return query;
      }

      void WriteQueryToLog(string str)
      {
         string dirPath = Cons.GetDataDirectory + "Log\\";
         if (!Directory.Exists(dirPath))
         {
            Directory.CreateDirectory(dirPath);
         }

         string path = dirPath + "query_" + DateTime.Now.ToString("yyyy.MM.dd@HH-mm-ss") + ".sql";
         File.WriteAllText(path, str);
      }
   }

   public class UCSaveReportFromQueryVM : UCSaveReportViewModel
   {
      public int NumFieldToHide { get; set; }
      
      public UCSaveReportFromQueryVM(Action prevAction) : base(prevAction) { }

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