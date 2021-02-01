using ReportForIDS.Utils;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public abstract class ReportWindowBaseViewModel : BaseViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand MenuLoadConfigCommand { get; set; }
      public ICommand MenuSaveCommand { get; set; }
      public ICommand ViewRecentReportCommand { get; set; }
      public ICommand NavigationSelectionChangedCommand { get; set; }

      public Grid GridCursor { get; set; }
      public Grid GridMain { get; set; }
      public List<UserControl> ListUC { get; set; }

      protected ReportWindowBaseViewModel()
      {
         LoadedCommand = new RelayCommand<Window>((p) => p != null, (p) =>
         {
            step = 0;
            ListUC = new List<UserControl>();

            InitUC();

            GridCursor = p.FindName("gridCursor") as Grid;
            GridMain = p.FindName("gridMain") as Grid;

            (ListUC[step].DataContext as UCBaseViewModel).ReLoad();
            GridMain.Children.Add(ListUC[step]);
         });

         MenuLoadConfigCommand = new RelayCommand<object>((p) => true, (p) => { });

         MenuSaveCommand = new RelayCommand<object>((p) => true, (p) => { });

         NavigationSelectionChangedCommand = new RelayCommand<Window>((p) => p != null, (p) =>
         {
            var listViewMenu = p.FindName("ListViewMenu") as ListView;

            if (listViewMenu.SelectedIndex <= step)
            {
               GridCursor.Margin = new Thickness(0, 60 * listViewMenu.SelectedIndex, 0, 0);

               GridMain.Children.Clear();
               step = listViewMenu.SelectedIndex;
               GridMain.Children.Add(ListUC[step]);
            }
         });

         ViewRecentReportCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            ListTemplateWindow templateWindow = new ListTemplateWindow();
            templateWindow.ShowDialog();
         });
      }

      protected int step = 0;

      protected virtual void InitUC()
      {
      }

      protected void NextStep()
      {
         if (step < ListUC.Count - 1)
         {
            step += 1;
            (ListUC[step].DataContext as UCBaseViewModel).ReLoad();

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
            var window = GridMain.GetRootParent() as Window;
            window.Close();
         }
      }
   }
}