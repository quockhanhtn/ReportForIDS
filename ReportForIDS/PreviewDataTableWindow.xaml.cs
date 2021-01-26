using System;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for PreviewDataTableWindow.xaml
   /// </summary>
   public partial class PreviewDataTableWindow : Window
   {
      public PreviewDataTableWindow()
      {
         InitializeComponent();
      }

      private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
      {
         ScrollViewer scv = (ScrollViewer)sender;
         scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
         e.Handled = true;
      }

      private void Button_Click(object sender, RoutedEventArgs e) => this.Close();

      public static void Show(DataTable dataTable, int maxRowDisplay = 500)
      {
         PreviewDataTableWindow previewWindow = new PreviewDataTableWindow();
         DataTable displayDataTable = new DataTable();

         Thread threadShow = new Thread(() =>
         {
            if (dataTable.Rows.Count < maxRowDisplay)
            {
               displayDataTable = dataTable.Copy();
            }
            else
            {
               displayDataTable = dataTable.Clone();
               for (int i = 0; i < maxRowDisplay; i++)
               {
                  displayDataTable.ImportRow(dataTable.Rows[i]);
               }
            }
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
               previewWindow.MainDataGrid.DataContext = displayDataTable.DefaultView;
               previewWindow.MainDataGrid.ColumnWidth = 150;
            }));
         })
         {
            IsBackground = true
         };
         threadShow.Start();

         WaitWindow.Show(() =>
         {
            while (threadShow.IsAlive) { }
         }, () =>
         {
            displayDataTable.Dispose();
            return;
         });

         try
         {
            previewWindow.ShowDialog();
         }
         catch (Exception e)
         {
            CustomMessageBox.Show("Errorr\r\n\r\n" + e.Message,
                                  Cons.TOOL_NAME,
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
         }
         finally
         {
            if (previewWindow != null) { previewWindow.Close(); }
            displayDataTable.Dispose();
         }
      }
   }
}