using ReportForIDS.Model;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class EditReportTemplateWindowViewModel : BaseViewModel
   {
      public ICommand MoveFileCommand { get; set; }
      public ICommand SaveChangeCommand { get; set; }
      public ICommand CancelCommand { get; set; }

      public bool Result { get; set; }
      public ReportTemplate EditReportTemplate { get => editReportTemplate; set { editReportTemplate = value; OnPropertyChanged(nameof(EditReportTemplate)); } }
      public DateTime InputCreateTime { get => inputCreateTime; set { inputCreateTime = value; OnPropertyChanged(nameof(InputCreateTime)); } }
      public string InputFilePath { get => inputFilePath; set { inputFilePath = value; OnPropertyChanged(nameof(InputFilePath)); } }
      public string InputName { get => inputName; set { inputName = value; OnPropertyChanged(nameof(InputName)); } }
      public string InputDescription { get => inputDescription; set { inputDescription = value; OnPropertyChanged(nameof(InputDescription)); } }

      public EditReportTemplateWindowViewModel(List<MyQuery> myQueries, ReportTemplate reportTemplate = null)
      {
         InitCommand(myQueries, reportTemplate);

         EditReportTemplate = reportTemplate;

         if (reportTemplate == null)
         {
            Thread threadTimer = new Thread(() =>
            {
               Thread.Sleep(1000);
               Application.Current.Dispatcher.Invoke(new Action(delegate ()
               {
                  InputCreateTime = DateTime.Now;
               }));
            })
            { IsBackground = true };
            threadTimer.Start();

            EditReportTemplate = new ReportTemplate();
         }
         else
         {
            InputCreateTime = EditReportTemplate.CreateTime;
            InputFilePath = EditReportTemplate.FilePath;
            InputName = EditReportTemplate.Name;
            InputDescription = EditReportTemplate.Description;
         }

      }

      private void InitCommand(List<MyQuery> myQueries, ReportTemplate reportTemplate = null)
      {
         MoveFileCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            string filter = $"Custom file (*{Cons.ReportTemplateExtension})|*{Cons.ReportTemplateExtension}|All file |*.*";
            string filePath = DialogUtils.ShowSaveFileDialog("Save report template to file", filter);
            if (string.IsNullOrEmpty(filePath)) { return; }

            if (File.Exists(filePath))
            {
               var messageBoxResult = CustomMessageBox.Show($"\"{filePath}\" already exixts.\r\n\r\nDo you want to replace it ?",
                                                            Cons.ToolName,
                                                            MessageBoxButton.YesNo,
                                                            MessageBoxImage.Warning,
                                                            MessageBoxResult.No);
               if (messageBoxResult == MessageBoxResult.No)
               {
                  return;
               }
               else
               {
                  try
                  {
                     using FileStream stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                     stream.Close();
                  }
                  catch (Exception e)
                  {
                     CustomMessageBox.Show("Error\r\n\r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
                  }
               }
            }

            InputFilePath = filePath;
         });

         SaveChangeCommand = new RelayCommand<Window>((p) => p != null, (p) =>
         {
            EditReportTemplate.Name = InputName;
            EditReportTemplate.Description = InputDescription;

            if (reportTemplate != null)
            {
               if (FileUtils.Move(EditReportTemplate.FilePath, InputFilePath, out string error))
               {
                  EditReportTemplate.FilePath = InputFilePath;
                  Result = ReportTemplate.Update.Edit(EditReportTemplate);
               }
               else
               {
                  CustomMessageBox.Show("Error while moving file\r\n\r\n" + error,
                                        Cons.ToolName,
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                  return;
               }
            }
            else
            {
               EditReportTemplate.FilePath = InputFilePath;
               Result = ReportTemplate.Update.Add(InputName, InputFilePath, InputDescription, myQueries);
            }

            p.Close();
         });

         CancelCommand = new RelayCommand<Window>((p) => p != null, (p) => p.Close());
      }

      private ReportTemplate editReportTemplate;
      private DateTime inputCreateTime;
      private string inputFilePath;
      private string inputName;
      private string inputDescription;
   }
}