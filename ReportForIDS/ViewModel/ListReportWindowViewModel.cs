using ReportForIDS.Model;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class ListReportWindowViewModel : BaseViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand ApplyFilterCommand { get; set; }
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
            if (value == true)
            {
               GridFilterVisibility = Visibility.Visible;
               ApplyFilterCommand.Execute(null);
            }
            else
            {
               GridFilterVisibility = Visibility.Collapsed;
               //remove filter
               ListReport = MyReport.XMLData.Deserialize().ToObservableCollection();
            }
            OnPropertyChanged(nameof(IsEnableFilter));
         }
      }

      public Visibility GridFilterVisibility
      {
         get => gridFilterVisibility; set
         {
            gridFilterVisibility = value;
            OnPropertyChanged(nameof(GridFilterVisibility));
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

      public MyReport SelectedReport { get => selectedReport; set { selectedReport = value; OnPropertyChanged(nameof(SelectedReport)); } }
      public ObservableCollection<MyReport> ListReport { get => listReport; set { listReport = value; OnPropertyChanged(nameof(ListReport)); } }

      public ListReportWindowViewModel()
      {
         LoadedCommand = new RelayCommand<Window>((p) => { return p != null; }, (p) =>
         {
            ListReport = MyReport.XMLData.Deserialize().ToObservableCollection();
         });

         ApplyFilterCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            ListReport = MyReport.XMLData.Deserialize().ToObservableCollection();
            ListReport = ListReport.Where(r => r.CreateTime >= FromDate && r.CreateTime <= ToDate).ToObservableCollection();
         });

         OpenCommand = new RelayCommand<object>((p) => { return SelectedReport != null; }, (p) =>
         {
            if (File.Exists(SelectedReport.FilePath))
            {
               ExcelUtils.OpenFile(SelectedReport.FilePath);
            }
            else
            {
               string mess = $"Can't not find file \"{SelectedReport.FilePath}\"\n\r\n\rDo you want remove this report from list ?";
               var messageBoxResult = MessageBox.Show(mess, Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);
               if (messageBoxResult == MessageBoxResult.Yes)
               {
                  RemoveCommand.Execute(SelectedReport);
               }
            }
         });

         OpenDirectoryCommand = new RelayCommand<object>((p) => { return SelectedReport != null; }, (p) =>
         {
            string dirPath = Path.GetDirectoryName(SelectedReport.FilePath);

            if (Directory.Exists(dirPath))
            {
               System.Diagnostics.Process.Start(dirPath);
            }
            else
            {
               string mess = $"Can't find directory \"{dirPath}\"\n\r\n\rDo you want remove this report from list ?";
               var messageBoxResult = MessageBox.Show(mess, Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);
               if (messageBoxResult == MessageBoxResult.Yes)
               {
                  RemoveCommand.Execute(SelectedReport);
               }
            }
         });

         EditCommand = new RelayCommand<object>((p) => { return SelectedReport != null; }, (p) =>
         {
            SelectedReport = EditReportInfoWindow.Show(SelectedReport);
            MyReport.Update.Edit(SelectedReport);
         });

         RemoveCommand = new RelayCommand<object>((p) => { return SelectedReport != null; }, (p) =>
         {
            ListReport.Remove(SelectedReport);
            MyReport.XMLData.Serialize(ListReport.ToList());
         });

         RemoveAllCommand = new RelayCommand<object>((p) => { return SelectedReport != null; }, (p) =>
         {
            var messageBoxResult = MessageBox.Show("", Cons.ToolName, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
               ListReport.Clear();
               MyReport.XMLData.Serialize(ListReport.ToList());
            }
         });

         CloseWindowCommand = new RelayCommand<Window>((p) => { return p != null; }, (p) =>
         {
            p.Close();
         });
      }

      private DateTime GetTime(DateTime sour, DateTime time)
      {
         return new DateTime(sour.Year, sour.Month, sour.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
      }

      Visibility gridFilterVisibility = Visibility.Collapsed;
      bool isEnableFilter = false;
      DateTime fromDate = DateTime.Now.AddDays(-10);
      DateTime toDate = DateTime.Now;

      private MyReport selectedReport;
      private ObservableCollection<MyReport> listReport = new ObservableCollection<MyReport>();
   }
}
