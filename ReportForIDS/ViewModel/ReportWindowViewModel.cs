using ReportForIDS.Model;
using ReportForIDS.UC;
using ReportForIDS.Utils;
using System.Collections.Generic;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class ReportWindowViewModel : BaseViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand MenuLoadConfigCommand { get; set; }
      public ICommand MenuSaveCommand { get; set; }
      public ICommand ViewRecentReportCommand { get; set; }
      public ICommand NavigationSelectionChangedCommand { get; set; }
      public Grid GridCursor { get; set; }
      public Grid GridMain { get; set; }

      public List<UserControl> ListUC { get; set; } = new List<UserControl>();

      public ReportWindowViewModel()
      {
         LoadedCommand = new RelayCommand<Window>((p) => { return (p != null); }, (p) =>
         {
            InitUC();

            GridCursor = p.FindName("gridCursor") as Grid;
            GridMain = p.FindName("gridMain") as Grid;
            (ListUC[step].DataContext as UCViewModel).ReLoad();
            GridMain.Children.Add(ListUC[step]);
         });

         MenuLoadConfigCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            string filePath = DialogUtils.ShowOpenFileDialog("Load config - " + Cons.ToolName, "All file | *.*");
            if (string.IsNullOrEmpty(filePath)) { return; }

            try
            {
               var loadedConfig = PrevSetting.Deserialize(filePath) as PrevSetting;
               loadedConfig.UpdateDataPrevSetting();

               foreach (var item in ListUC)
               {
                  (item.DataContext as UCViewModel).LoadPreviosData();
               }
            }
            catch (System.Exception)
            {
            }
         });

         MenuSaveCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            string filePath = DialogUtils.ShowSaveFileDialog("Save config - " + Cons.ToolName, "All file | *.*");
            if (string.IsNullOrEmpty(filePath)) { return; }

            try
            {
               PrevSetting settingData = new PrevSetting(step);
               PrevSetting.Serialize(settingData, filePath);

               MessageBox.Show("Save config succeeded", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception e)
            {
               MessageBox.Show("Error when save config.\n\r" + e.Message, Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
         });

         NavigationSelectionChangedCommand = new RelayCommand<Window>((p) => { return (p != null); }, (p) =>
         {
            var listViewMenu = p.FindName("ListViewMenu") as ListView;

            if (listViewMenu.SelectedIndex <= step)
            {
               GridCursor.Margin = new Thickness(0, 60 * listViewMenu.SelectedIndex, 0, 0);

               GridMain.Children.Clear();
               step = listViewMenu.SelectedIndex;
               GridMain.Children.Add(ListUC[step]);
            }
            //else if (listViewMenu.SelectedIndex > step)
            //{
            //   for (int i = 0; i < listViewMenu.SelectedIndex; i++)
            //   {
            //      if ((ListUC[i].DataContext as UCViewModel).HasChange)
            //      {
            //         return;
            //      }
            //   }

            //   GridCursor.Margin = new Thickness(0, 60 * listViewMenu.SelectedIndex, 0, 0);

            //   GridMain.Children.Clear();
            //   step = listViewMenu.SelectedIndex;
            //   GridMain.Children.Add(ListUC[step]);
            //}
         });

         ViewRecentReportCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            ListReportWindow listReportWindow = new ListReportWindow() { DataContext = new ListReportWindowViewModel() };
            listReportWindow.Show();
         });
      }

      protected int step = 0;

      protected virtual void InitUC() { }

      protected void NextStep()
      {
         if (step < ListUC.Count - 1)
         {
            step += 1;
            (ListUC[step].DataContext as UCViewModel).ReLoad();

            GridMain.Children.Clear();
            GridMain.Children.Add(ListUC[step]);
            GridCursor.Margin = new Thickness(0, 60 * step, 0, 0);
         }
      }

      protected void PrevStep()
      {
         if (step > 0)
         {
            // back to previous step
            step -= 1;
            GridMain.Children.Clear();
            GridMain.Children.Add(ListUC[step]);
            GridCursor.Margin = new Thickness(0, 60 * step, 0, 0);
         }
         else
         {
            // close this window (will return MainWindow)
            var window = FrameworkElementExtend.GetRootParent(GridMain) as Window;
            window.Close();
         }
      }
   }

   public class StepByStepWindowViewModel : ReportWindowViewModel
   {
      public StepByStepWindowViewModel() : base() { }

      protected override void InitUC()
      {
         ListUC.Add(new UCSelectTable() { DataContext = new UCSelectTableViewModel(PrevStep, NextStep) });
         ListUC.Add(new UCSelectField() { DataContext = new UCSelectFieldViewModel(SelectFieldType.TO_DISPLAY, PrevStep, NextStep) });
         ListUC.Add(new UCSetCondition() { DataContext = new UCSetConditionViewModel(PrevStep, NextStep) });
         ListUC.Add(new UCSelectField() { DataContext = new UCSelectFieldViewModel(SelectFieldType.TO_GROUP, PrevStep, NextStep) });
         ListUC.Add(new UCSaveReport() { DataContext = new UCSaveReportStepByStepVM(PrevStep) });
      }
   }

   public class ReportFromQueryWindowViewModel : ReportWindowViewModel
   {
      protected override void InitUC()
      {
         ListUC.Add(new UCInsertSqlQuery() { DataContext = new UCInsertSqlQueryViewModel(PrevStep, NextStep) });
         ListUC.Add(new UCSelectField() { DataContext = new UCSelectFieldViewModel(SelectFieldType.TO_GROUP_FROM_QUERY, PrevStep, NextStep) });
         //ListUC.Add(new UCSelectField() { DataContext = new UCSelectFieldViewModel(SelectFieldType.TO_HIDE, PrevStep, NextStep) });
         ListUC.Add(new UCSaveReport() { DataContext = new UCSaveReportFromQueryVM(PrevStep) });
      }
   }
}
