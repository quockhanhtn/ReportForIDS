using OfficeOpenXml;
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
   public class UCSaveReportViewModel : UCViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand SelectDirectoryCommand { get; set; }
      public ICommand PrevCommand { get; set; }
      public ICommand SaveReportCommand { get; set; }

      public string FilePath
      {
         get
         {
            var fullFileName = ReportFileName;
            switch (ReportFileType)
            {
               case 0:
                  fullFileName += ".csv";
                  break;

               case 1:
                  fullFileName += ".xlsx";
                  break;

               case 2:
                  fullFileName += ".xls";
                  break;

               default:
                  break;
            }

            return Path.Combine(ReportDirectory, fullFileName);
         }
      }

      public int NumFieldToGroup { get; set; }
      public DataTable ResultDatatable { get; set; }
      public ProgressBar PbStatus { get; set; }

      public Thread DataThread { get; set; }

      public string ReportName { get => reportName; set { reportName = value; OnPropertyChanged(nameof(ReportName)); } }
      public string ReportDirectory { get => reportDirectory; set { reportDirectory = value; OnPropertyChanged(nameof(ReportDirectory)); } }
      public string ReportFileName { get => reportFileName; set { reportFileName = value; OnPropertyChanged(nameof(ReportFileName)); } }
      public int ReportFileType { get => reportFileType; set { reportFileType = value; OnPropertyChanged(nameof(ReportFileType)); } }
      public string ReportDesc { get => reportDesc; set { reportDesc = value; OnPropertyChanged(nameof(ReportDesc)); } }

      public UCSaveReportViewModel(Action prevAction)
      {
         LoadedCommand = new RelayCommand<UserControl>((p) => { return p != null; }, (p) =>
         {
            PbStatus = p.FindName("pbStatus") as ProgressBar;

            if (DataThread is null || !DataThread.IsAlive)
            {
               PbStatus.Maximum = ResultDatatable.Rows.Count;
               PbStatus.Maximum *= 1.1;
               PbStatus.Value = 0;
            }
            //PbStatus.Maximum = ResultDatatable.Rows.Count;
            //PbStatus.Maximum *= 1.1;
            //PbStatus.Value = 0;
         });

         SelectDirectoryCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            string path = DialogUtils.ShowFolderBrowserDialog(ReportDirectory);
            if (!string.IsNullOrEmpty(path))
            {
               ReportDirectory = path;
            }
         });

         PrevCommand = new RelayCommand<object>((p) => { return true; }, (p) => prevAction());

         SaveReportCommand = new RelayCommand<Grid>((p) => { return p != null; }, (p) =>
         {
            if (string.IsNullOrEmpty(ReportFileName))
            {
               MessageBox.Show("Report file name can't empty !", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
               return;
            }

            if (ReportFileName.IndexOfAny(new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }) > 0)
            {
               MessageBox.Show("Report file can't contain any of fpllowing characters { < > : \" / \\ | ? * }", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
               return;
            }

            if (string.IsNullOrEmpty(ReportDirectory))
            {
               MessageBox.Show("Please select Directory to save report", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
               SelectDirectoryCommand.Execute(null);
               return;
            }

            if (File.Exists(FilePath))
            {
               var messageBoxResult = MessageBox.Show($"\"{FilePath}\" already exixts.\r\nDo you want to replace it ?", Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
               if (messageBoxResult == MessageBoxResult.No) { return; }
               MyReport.Update.Delete(FilePath);
            }

            try
            {
               File.Create(FilePath);
            }
            catch (Exception e)
            {
               MessageBox.Show("Error\r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
               return;
            }

            WriteSaveOptionConfig();

            p.IsEnabled = false;

            PbStatus.Value = 0;
            Thread thread = new Thread(() =>
            {
               SaveReport();

               Application.Current.Dispatcher.Invoke(new Action(delegate ()
               {
                  PbStatus.Value = PbStatus.Maximum;
                  p.IsEnabled = true;
               }));
            });
            thread.IsBackground = true;
            thread.Start();
         });
      }

      protected void AssignCellValue(ExcelWorksheet worksheet, int row, int col, object value, string valueIfNull = "")
      {
         worksheet.Cells[row, col].Value = value ?? valueIfNull;
      }

      protected bool IsDifferrentGroup(List<string> lastGroup, DataRow dataRow)
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

      protected bool IsDifferrentGroup(List<string> lastGroup, DataRowView dataRow)
      {
         //ResultDatatable.Rows.Cast<DataRow>().GroupBy(n => new { id = n["kkkk"], k = n["k"]}).To

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

      protected void WriteQueryToLog(string str)
      {
         string dirPath = Cons.GetDataDirectory + "Log\\";
         if (!Directory.Exists(dirPath))
         {
            Directory.CreateDirectory(dirPath);
         }

         string path = dirPath + "query_" + DateTime.Now.ToString("yyyy.MM.dd@HH-mm-ss") + ".sql";
         File.WriteAllText(path, str);
      }

      protected void ReadSaveOptionConfig()
      {
         IniFile iniFile = new IniFile(Cons.GetDataDirectory + "Config.ini", "SaveOptionConfig");

         ReportDirectory = iniFile.Read(nameof(ReportDirectory));

         #region Set default value if null or empty

         ReportDirectory = string.IsNullOrEmpty(ReportDirectory) ? "" : ReportDirectory;

         #endregion Set default value if null or empty
      }

      protected void WriteSaveOptionConfig()
      {
         IniFile iniFile = new IniFile(Cons.GetDataDirectory + "Config.ini", "SaveOptionConfig");
         iniFile.Write(nameof(ReportDirectory), ReportDirectory);
      }

      public override void ReLoad()
      {
         ReadSaveOptionConfig();
         ExecuteQuery();

         //if (ResultDatatable == null || ResultDatatable.Rows.Count == 0)
         //{
         //   MessageBox.Show("Execute query error: DataTable null, please check condition !", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
         //   PrevCommand.Execute(null);
         //}
      }

      public virtual void SaveReport()
      {
      }

      public virtual void ExecuteQuery()
      {
      }

      public virtual void GetExcelPackageFromDataTable()
      {
      }

      private string reportName;
      private string reportDirectory;
      private string reportFileName;
      private int reportFileType = 0;
      private string reportDesc;
   }

   public class UCSaveReportStepByStepVM : UCSaveReportViewModel
   {
      public UCSaveReportStepByStepVM(Action prevAction) : base(prevAction)
      {
      }

      public override void SaveReport()
      {
         try
         {
            using (var excelPackage = ExcelUtils.CreateExcelPackage(new List<string>() { "Sheet1" }))
            {
               ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

               var lastGroup = new List<string>();
               int maxRecordOnRow = 0, recordsOnRow = 0;
               int colIndex = 1, rowIndex = 1;

               // wait thread execute done
               while (DataThread.IsAlive)
               {
               }

               foreach (DataRow item in ResultDatatable.Rows)
               {
                  Application.Current.Dispatcher.Invoke(new Action(delegate ()
                  {
                     PbStatus.Value++;
                  }));

                  if (IsDifferrentGroup(lastGroup, item))
                  {
                     if (recordsOnRow > maxRecordOnRow) { maxRecordOnRow = recordsOnRow; }
                     recordsOnRow = 1;

                     rowIndex++;
                     colIndex = 1;

                     lastGroup.Clear();
                     for (int i = 0; i < NumFieldToGroup; i++)
                     {
                        AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
                        lastGroup.Add(item[i].ToString());
                     }
                     for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count; i++)
                     {
                        AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
                     }
                  }
                  else
                  {
                     recordsOnRow += 1;
                     for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count; i++)
                     {
                        AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
                     }
                  }
               }

               if (recordsOnRow > maxRecordOnRow) { maxRecordOnRow = recordsOnRow; }

               #region Create header

               colIndex = 1;
               rowIndex = 1;

               if (NumFieldToGroup > 0)
               {
                  for (int i = 0; i < NumFieldToGroup; i++)
                  {
                     AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName);
                  }
                  for (int k = 1; k <= maxRecordOnRow; k++)
                  {
                     for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count; i++)
                     {
                        AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName + "_" + k.ToString());
                     }
                  }
               }
               else
               {
                  for (int i = 0; i < ResultDatatable.Columns.Count; i++)
                  {
                     AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName);
                  }
               }

               #endregion Create header

               ExcelUtils.SaveExcelPackage(excelPackage, FilePath);
            }
            MyReport.Update.Add(ReportName, FilePath, ReportDesc);

            var messageBoxResult = MessageBox.Show("Do you want to open report file ?", Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
               ExcelUtils.OpenFile(FilePath);
            }
         }
         catch (Exception e)
         {
            MessageBox.Show("Error :\r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

      public override void ExecuteQuery()
      {
         string query = GenerateQuery();
         try
         {
            WriteQueryToLog(query);
            DataThread = new Thread(() =>
            {
               ResultDatatable = DatabaseUtils.ExecuteQuery(query);
               Application.Current.Dispatcher.Invoke(new Action(delegate ()
               {
                  if (PbStatus != null)
                  {
                     PbStatus.Maximum = ResultDatatable.Rows.Count;
                     PbStatus.Maximum *= 1.1;
                     PbStatus.Value = 0;
                  }
               }));
            });
            DataThread.IsBackground = true;
            DataThread.Start();
         }
         catch (Exception e)
         {
            MessageBox.Show("Error \r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK);
         }
      }

      private void ExecuteQuery(int limit)
      {
         string query = GenerateQuery() + $" limit {limit};";

         try
         {
            ResultDatatable = DatabaseUtils.ExecuteQuery(query);
         }
         catch (Exception) { }
      }

      private string GenerateQuery()
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
   }

   public class UCSaveReportFromQueryVM : UCSaveReportViewModel
   {
      public int NumFieldToHide { get; set; }

      public UCSaveReportFromQueryVM(Action prevAction) : base(prevAction)
      {
      }

      public override void SaveReport()
      {
         try
         {
            using (var excelPackage = ExcelUtils.CreateExcelPackage(new List<string>() { "Sheet1" }))
            {
               ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

               var lastGroup = new List<string>();
               int maxRecordOnRow = 0, recordsOnRow = 0;
               int colIndex = 1, rowIndex = 1;

               foreach (DataRowView item in ResultDatatable.DefaultView)
               {
                  Application.Current.Dispatcher.Invoke(new Action(delegate ()
                  {
                     PbStatus.Value++;
                  }));

                  if (IsDifferrentGroup(lastGroup, item))
                  {
                     if (recordsOnRow > maxRecordOnRow) { maxRecordOnRow = recordsOnRow; }
                     recordsOnRow = 1;

                     rowIndex++;
                     colIndex = 1;

                     lastGroup.Clear();
                     for (int i = 0; i < NumFieldToGroup; i++)
                     {
                        AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
                        lastGroup.Add(item[i].ToString());
                     }
                     for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
                     {
                        AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
                     }
                  }
                  else
                  {
                     recordsOnRow += 1;
                     for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
                     {
                        AssignCellValue(worksheet, rowIndex, colIndex++, item[i]);
                     }
                  }
               }
               if (recordsOnRow > maxRecordOnRow) { maxRecordOnRow = recordsOnRow; }

               #region Create header

               colIndex = rowIndex = 1;
               if (NumFieldToGroup > 0)
               {
                  for (int i = 0; i < NumFieldToGroup; i++)
                  {
                     AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName);
                  }
                  for (int k = 1; k <= maxRecordOnRow; k++)
                  {
                     for (int i = NumFieldToGroup; i < ResultDatatable.Columns.Count - NumFieldToHide; i++)
                     {
                        AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName + "_" + k.ToString());
                     }
                  }
               }
               else
               {
                  for (int i = 0; i < ResultDatatable.Columns.Count; i++)
                  {
                     AssignCellValue(worksheet, rowIndex, colIndex++, ResultDatatable.Columns[i].ColumnName);
                  }
               }

               #endregion Create header

               ExcelUtils.SaveExcelPackage(excelPackage, FilePath);
            }
            MyReport.Update.Add(ReportName, FilePath, ReportDesc);

            var messageBoxResult = MessageBox.Show("Do you want to open report file ?", Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
            if (messageBoxResult == MessageBoxResult.Yes) { ExcelUtils.OpenFile(FilePath); }
         }
         catch (Exception e)
         {
            MessageBox.Show("Error :\r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

      public override void ExecuteQuery()
      {
         while (ReportFromQueryData.ThreadExcecuteData.IsAlive)
         {
         }

         ResultDatatable = ReportFromQueryData.ExecuteDataTable;

         // order column by group -> not group
         var listColumnOrder = ReportFromQueryData.ListFieldToGroup.ToList();
         listColumnOrder.AddUniqueRange(ReportFromQueryData.ListAllFields);

         ResultDatatable.SetColumnsOrder(listColumnOrder.Select(f => f.FieldName).ToArray());
         ResultDatatable.SortByColumns(ReportFromQueryData.ListFieldToGroup.Select(f => f.FieldName + " ASC").ToArray());

         NumFieldToGroup = ReportFromQueryData.ListFieldToGroup.Count;
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

      //      var messageBoxResult = MessageBox.Show("Do you want to open report file ?", Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
      //      if (messageBoxResult == MessageBoxResult.Yes)
      //      {
      //         ExcelUtils.OpenFile(FilePath);
      //      }
      //   }

      //   catch (Exception e)
      //   {
      //      MessageBox.Show("Error :\r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
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
      //      MessageBox.Show("Error \r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK);
      //   }
      //}

      #endregion Old code
   }
}