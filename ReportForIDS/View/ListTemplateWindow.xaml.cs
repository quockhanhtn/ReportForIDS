using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for ListTemplateWindow.xaml
   /// </summary>
   public partial class ListTemplateWindow : Window
   {
      public ListTemplateWindow()
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