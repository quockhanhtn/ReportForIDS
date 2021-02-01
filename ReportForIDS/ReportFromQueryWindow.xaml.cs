using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
