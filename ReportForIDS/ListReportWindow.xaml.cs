using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for ListReportWindow.xaml
   /// </summary>
   public partial class ListReportWindow : Window
   {
      public ListReportWindow()
      {
         InitializeComponent();
      }

      private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
      {
         ScrollViewer scv = (ScrollViewer)sender;
         scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
         e.Handled = true;
      }
   }
}