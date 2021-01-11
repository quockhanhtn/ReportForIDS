using System.Windows;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for ReportFromQueryWindow.xaml
   /// </summary>
   public partial class ReportFromQueryWindow : Window
   {
      public ReportFromQueryWindow()
      {
         InitializeComponent();
      }

      private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
      {
         btnCloseMenu.Visibility = Visibility.Visible;
         btnOpenMenu.Visibility = Visibility.Collapsed;
      }

      private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
      {
         btnCloseMenu.Visibility = Visibility.Collapsed;
         btnOpenMenu.Visibility = Visibility.Visible;
      }
   }
}