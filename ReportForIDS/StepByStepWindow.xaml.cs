using ReportForIDS.UC;
using System.Windows;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for StepByStepWindow.xaml
   /// </summary>
   public partial class StepByStepWindow : Window
   {
      public StepByStepWindow()
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
