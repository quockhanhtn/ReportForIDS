using OfficeOpenXml;
using ReportForIDS.Model;
using ReportForIDS.SessionData;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class UCSaveReportViewModel : UCBaseViewModel
   {
      #region Command

      public ICommand LoadedCommand { get; set; }
      public ICommand SelectDirectoryCommand { get; set; }
      public ICommand SelectTemplateFilePathCommand { get; set; }
      public ICommand PreviewReportCommand { get; set; }
      public ICommand SaveReportCommand { get; set; }

      #endregion Command

      #region Property

      public int NumFieldToGroup { get; set; }
      public DataTable ResultDatatable { get; set; }
      public DataTable GroupResultDatatable { get; set; }
      public ProgressBar PbStatus { get; set; }
      public Thread DataThread { get; set; }

      public string ReportDirectory { get => reportDirectory; set { reportDirectory = value; OnPropertyChanged(nameof(ReportDirectory)); } }

      public string ReportFileName
      {
         get => reportFileName;
         set
         {
            reportFileName = value.RemoveChar(new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' });
            OnPropertyChanged(nameof(ReportFileName));
         }
      }

      public int ReportFileType { get => reportFileType; set { reportFileType = value; OnPropertyChanged(nameof(ReportFileType)); } }

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

      public bool IsOpenFileAfterSave { get => isOpenFileAfterSave; set { isOpenFileAfterSave = value; OnPropertyChanged(nameof(IsOpenFileAfterSave)); } }
      public bool IsSaveReportTemplate { get => isSaveReportTemplate; set { isSaveReportTemplate = value; OnPropertyChanged(nameof(IsSaveReportTemplate)); } }
      public string ReportTemplateDesc { get => reportTemplateDesc; set { reportTemplateDesc = value; OnPropertyChanged(nameof(ReportTemplateDesc)); } }
      public string ReportTemplateName { get => reportTemplateName; set { reportTemplateName = value; OnPropertyChanged(nameof(ReportTemplateName)); } }
      public string TemplateFilePath { get => templateFilePath; set { templateFilePath = value; OnPropertyChanged(nameof(TemplateFilePath)); } }

      #endregion Property

      protected UCSaveReportViewModel(Action prevAction)
      {
         LoadedCommand = new RelayCommand<UserControl>((p) => p != null, (p) =>
         {
            PbStatus = p.FindName("pbStatus") as ProgressBar;
            PbStatus.Visibility = Visibility.Collapsed;
         });

         SelectDirectoryCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            string path = DialogUtils.ShowFolderBrowserDialog(ReportDirectory);
            if (!string.IsNullOrEmpty(path)) { ReportDirectory = path; }
         });

         SelectTemplateFilePathCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            string filter = $"Custom file (*{Cons.REPORT_TEMPLATE_EXTENSION})|*{Cons.REPORT_TEMPLATE_EXTENSION}|All file |*.*";
            string path = DialogUtils.ShowSaveFileDialog("Save report template file", filter);

            if (!string.IsNullOrEmpty(path)) { TemplateFilePath = path; }
         });

         PrevCommand = new RelayCommand<object>((p) => true, (p) => prevAction());

         PreviewReportCommand = new RelayCommand<object>((p) => true, (p) => PreviewReport());

         SaveReportCommand = new RelayCommand<Grid>((p) => p != null, (p) =>
         {
            if (!CheckSaveReportInput()) { return; }
            if (IsSaveReportTemplate && !CheckSaveTemplateInput()) { return; }

            WriteSaveOptionConfig();
            // save template
            ReportTemplate.Update.Add(ReportTemplateName, TemplateFilePath, ReportTemplateDesc, ReportFromQueryData.ListQueries);

            p.IsEnabled = false;
            Thread saveThread = new Thread(() =>
            {
               SaveReport();

               Application.Current.Dispatcher.Invoke(new Action(delegate ()
               {
                  PbStatus.Visibility = Visibility.Collapsed;
                  p.IsEnabled = true;
               }));
            })
            {
               IsBackground = true
            };

            WaitWindow.Show(() =>
            {
               while (DataThread.IsAlive) { }
               Application.Current.Dispatcher.Invoke(new Action(delegate ()
               {
                  PbStatus.Visibility = Visibility.Visible;
                  PbStatus.Value = 0;
                  PbStatus.Maximum = GroupResultDatatable.Rows.Count;
               }));
               saveThread.Start();
               //while (thread.IsAlive) { }
            }, () =>
            {
               p.IsEnabled = true;
               saveThread.Abort();
            });
         });
      }

      protected bool CheckSaveTemplateInput()
      {
         if (string.IsNullOrEmpty(TemplateFilePath))
         {
            CustomMessageBox.Show("Incorrect input\r\n\r\nPlease select path to save report template file!",
                                  Cons.TOOL_NAME,
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
            return false;
         }

         try
         {
            using FileStream stream = File.Open(TemplateFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            stream.Close();
         }
         catch (Exception e)
         {
            CustomMessageBox.Show("Error\r\n\r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
         }

         return true;
      }

      protected bool CheckSaveReportInput()
      {
         if (string.IsNullOrEmpty(ReportDirectory))
         {
            CustomMessageBox.Show("Please select Directory to save report", Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            SelectDirectoryCommand.Execute(null);
            return false;
         }

         if (string.IsNullOrEmpty(ReportFileName))
         {
            CustomMessageBox.Show("Incorrect input\r\n\r\nReport file name can't empty !",
                                  Cons.TOOL_NAME,
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
            return false;
         }

         //if (ReportFileName.IndexOfAny(new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }) > 0)
         //{
         //   CustomMessageBox.Show("Incorrect input\r\n\r\nReport file name can't contain any of following characters { < > : \" / \\ | ? * }",
         //                         Cons.ToolName,
         //                         MessageBoxButton.OK,
         //                         MessageBoxImage.Error);
         //   return false;
         //}

         if (File.Exists(FilePath))
         {
            var messageBoxResult = CustomMessageBox.Show($"File \"{FilePath}\" already exixts.\r\n\r\nDo you want to replace it ?",
                                                         Cons.TOOL_NAME,
                                                         MessageBoxButton.YesNo,
                                                         MessageBoxImage.Warning,
                                                         MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.No) { return false; }
         }

         try
         {
            using FileStream stream = File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            stream.Close();
         }
         catch (Exception e)
         {
            CustomMessageBox.Show("Error\r\n\r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
         }

         return true;
      }

      protected void SaveReport()
      {
         try
         {
            using var excelPackage = ExcelUtils.CreateExcelPackage(new List<string>() { "Sheet1" });
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];
            int colIndex = 1, rowIndex = 1;

            foreach (DataColumn dataColumn in GroupResultDatatable.Columns)
            {
               worksheet.Cells[rowIndex, colIndex++].Value = dataColumn.ColumnName;
            }

            foreach (DataRow item in GroupResultDatatable.Rows)
            {
               Application.Current.Dispatcher.Invoke(new Action(delegate ()
               {
                  PbStatus.Value++;
               }));

               rowIndex++;
               colIndex = 1;

               for (int i = 0; i < GroupResultDatatable.Columns.Count; i++)
               {
                  worksheet.Cells[rowIndex, colIndex++].Value = item[i] ?? "";
               }
            }

            // save file
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
               WaitWindow.Show(() =>
               {
                  ExcelUtils.SaveExcelPackage(excelPackage, FilePath);
                  if (IsOpenFileAfterSave) { ExcelUtils.OpenFile(FilePath); }
               });

               ShowSnackbarMessage("Report file saved successfully", "OPEN", () =>
               {
                  ExcelUtils.OpenFile(FilePath);
               }, 10);
            }));
            //MyReport.Update.Add(ReportName, FilePath, ReportDesc);
         }
         catch (Exception e)
         {
            CustomMessageBox.Show("Error :\r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

      protected void PreviewReport()
      {
         WaitWindow.Show(() =>
         {
            while (DataThread.IsAlive) { }
         }, () => { return; });

         ViewDatatableWindow.Show(GroupResultDatatable);
      }

      protected void ReadSaveOptionConfig()
      {
         IniFile iniFile = new IniFile(Cons.GetDataDirectory + "Config.ini", "SaveOptionConfig");

         ReportDirectory = iniFile.Read(nameof(ReportDirectory)) ?? "";
         IsOpenFileAfterSave = iniFile.Read(nameof(IsOpenFileAfterSave)).ToBool(true);
         IsSaveReportTemplate = iniFile.Read(nameof(IsSaveReportTemplate)).ToBool(false);
      }

      protected void WriteSaveOptionConfig()
      {
         IniFile iniFile = new IniFile(Cons.GetDataDirectory + "Config.ini", "SaveOptionConfig");

         iniFile.Write(nameof(ReportDirectory), ReportDirectory);
         iniFile.Write(nameof(IsOpenFileAfterSave), IsOpenFileAfterSave.ToString());
         iniFile.Write(nameof(IsSaveReportTemplate), IsSaveReportTemplate.ToString());
      }

      public override void ReLoad()
      {
         ReadSaveOptionConfig();

         WaitWindow.Show(() =>
         {
            DataThread = new Thread(() =>
            {
               ExecuteQuery();
               GroupResultDataTable();
            })
            { IsBackground = true };
            DataThread.Start();
         }, () => PrevCommand.Execute(null));
      }

      public virtual void GroupResultDataTable()
      {
      }

      public virtual void ExecuteQuery()
      {
      }

      #region Class field

      private string reportDirectory, reportFileName;
      private int reportFileType = 0;
      private bool isOpenFileAfterSave = false;

      private bool isSaveReportTemplate = false;
      private string reportTemplateDesc, reportTemplateName, templateFilePath;

      #endregion Class field
   }
}