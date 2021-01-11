using ReportForIDS.Model;
using ReportForIDS.Utils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class ListTemplateWindowViewModel : BaseViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand ApplyFilterCommand { get; set; }
      public ICommand CreateFromThisCommand { get; set; }
      public ICommand OpenCommand { get; set; }
      public ICommand OpenDirectoryCommand { get; set; }
      public ICommand EditCommand { get; set; }
      public ICommand RemoveCommand { get; set; }
      public ICommand RemoveAllCommand { get; set; }
      public ICommand CloseWindowCommand { get; set; }

      public bool IsEnableFilter
      {
         get => isEnableFilter;
         set
         {
            isEnableFilter = value;
            if (value == false)
            {
               ListReportTemplate = ReportTemplate.XMLData.Deserialize().ToObservableCollection();
            }
            OnPropertyChanged(nameof(IsEnableFilter));
         }
      }

      public DateTime FromDate
      {
         get => fromDate;
         set
         {
            fromDate = value;
            if (value > ToDate) { ToDate = value.AddDays(5); }
            OnPropertyChanged(nameof(FromDate));
         }
      }

      public DateTime ToDate
      {
         get => toDate;
         set
         {
            toDate = value;
            if (value < FromDate) { FromDate = value.AddDays(-5); }
            OnPropertyChanged(nameof(ToDate));
         }
      }

      public DateTime FromTime
      {
         get => fromDate;
         set
         {
            fromDate = GetTime(fromDate, value);
            OnPropertyChanged(nameof(FromTime));
         }
      }

      public DateTime ToTime
      {
         get => toDate;
         set
         {
            toDate = GetTime(toDate, value);
            OnPropertyChanged(nameof(ToTime));
         }
      }

      public ObservableCollection<ReportTemplate> ListReportTemplate { get => listReportTemplate; set { listReportTemplate = value; OnPropertyChanged(nameof(ListReportTemplate)); } }
      public ReportTemplate SelectedReportTemplate { get => selectedReportTemplate; set { selectedReportTemplate = value; OnPropertyChanged(nameof(SelectedReportTemplate)); } }

      public ListTemplateWindowViewModel()
      {
         LoadedCommand = new RelayCommand<Window>((p) => { return p != null; }, (p) =>
         {
            ListReportTemplate = ReportTemplate.XMLData.Deserialize().ToObservableCollection();
         });

         ApplyFilterCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            ListReportTemplate = ReportTemplate.XMLData.Deserialize().ToObservableCollection();
            ListReportTemplate = ListReportTemplate.Where(r => r.CreateTime >= FromDate && r.CreateTime <= ToDate).ToObservableCollection();
         });

         CreateFromThisCommand = new RelayCommand<object>((p) => { return true; ; }, (p) =>
         {
            if (string.IsNullOrEmpty(DatabaseConnection.DatabaseName))
            {
               CustomMessageBox.Show("Missing input\r\nPlease select 'DatabaseName' first !", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
               foreach (Window w in Application.Current.Windows)
               {
                  if (w != Application.Current.MainWindow) { w.Close(); }
               }
               return;
            }

            string mess = "All window will close and create report in new window!\r\n\r\n";
            mess += "Do you want to continue ?";
            var messBoxResult = CustomMessageBox.Show(mess, Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (messBoxResult == MessageBoxResult.No)
            {
               return;
            }

            try
            {
               foreach (Window w in Application.Current.Windows)
               {
                  if (w != Application.Current.MainWindow) { w.Close(); }
               }

               ReportFromQueryWindow reportFromQueryWindow = new ReportFromQueryWindow()
               {
                  DataContext = new ReportFromQueryWindowViewModel(SelectedReportTemplate.FilePath),
               };
               MyQuery.LastOrder = 0;
               reportFromQueryWindow.ShowDialog();
            }
            catch (Exception e)
            {
               CustomMessageBox.Show("Error\r\n\r\n" + e.Message, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
         });

         OpenCommand = new RelayCommand<object>((p) => { return SelectedReportTemplate != null; }, (p) =>
         {
            if (File.Exists(SelectedReportTemplate.FilePath))
            {
               ExcelUtils.OpenFile(SelectedReportTemplate.FilePath);
            }
            else
            {
               string mess = $"Can't not find file \"{SelectedReportTemplate.FilePath}\"\n\r\n\rDo you want remove this report from list ?";
               var messageBoxResult = CustomMessageBox.Show(mess, Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);
               if (messageBoxResult == MessageBoxResult.Yes)
               {
                  RemoveCommand.Execute(SelectedReportTemplate);
               }
            }
         });

         OpenDirectoryCommand = new RelayCommand<object>((p) => { return SelectedReportTemplate != null; }, (p) =>
         {
            string dirPath = Path.GetDirectoryName(SelectedReportTemplate.FilePath);

            if (Directory.Exists(dirPath))
            {
               System.Diagnostics.Process.Start(dirPath);
            }
            else
            {
               string mess = $"Can't find directory \"{dirPath}\"\n\r\n\rDo you want remove this report from list ?";
               var messageBoxResult = CustomMessageBox.Show(mess, Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);
               if (messageBoxResult == MessageBoxResult.Yes)
               {
                  RemoveCommand.Execute(SelectedReportTemplate);
               }
            }
         });

         EditCommand = new RelayCommand<object>((p) => { return SelectedReportTemplate != null; }, (p) =>
         {
            var updateDataContext = new EditReportTemplateWindowViewModel(SelectedReportTemplate.ListQuery, SelectedReportTemplate);
            var updateReportTemplateWindow = new EditReportTemplateWindow()
            {
               DataContext = updateDataContext,
            };
            updateReportTemplateWindow.ShowDialog();
            if (!updateDataContext.Result)
            {
               CustomMessageBox.Show("Error while edit info", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
               ListReportTemplate = ReportTemplate.XMLData.Deserialize().ToObservableCollection();
            }
         });

         RemoveCommand = new RelayCommand<object>((p) => { return SelectedReportTemplate != null; }, (p) =>
         {
            ListReportTemplate.Remove(SelectedReportTemplate);
            ReportTemplate.XMLData.Serialize(ListReportTemplate.ToList());
         });

         RemoveAllCommand = new RelayCommand<object>((p) => { return SelectedReportTemplate != null; }, (p) =>
         {
            var messageBoxResult = CustomMessageBox.Show("", Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
               ListReportTemplate.Clear();
               ReportTemplate.XMLData.Serialize(ListReportTemplate.ToList());
            }
         });

         CloseWindowCommand = new RelayCommand<Window>((p) => p != null, (p) => p.Close());
      }

      private DateTime GetTime(DateTime sour, DateTime time)
      {
         return new DateTime(sour.Year, sour.Month, sour.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
      }

      private bool isEnableFilter = false;
      private DateTime fromDate = DateTime.Now.AddDays(-10);
      private DateTime toDate = DateTime.Now;

      private ReportTemplate selectedReportTemplate;
      private ObservableCollection<ReportTemplate> listReportTemplate = new ObservableCollection<ReportTemplate>();
   }
}