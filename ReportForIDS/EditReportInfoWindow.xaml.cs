using ReportForIDS.Model;
using ReportForIDS.ViewModel;
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
   /// Interaction logic for EditReportInfoWindow.xaml
   /// </summary>
   public partial class EditReportInfoWindow : Window
   {
      public EditReportInfoWindow()
      {
         InitializeComponent();
      }

      public static MyReport Show(MyReport myReport)
      {
         var windowDataContext = new EditReportInfoWindowViewModel(myReport);
         EditReportInfoWindow editReportInfoWindow = new EditReportInfoWindow() { DataContext = windowDataContext };
         var result = editReportInfoWindow.ShowDialog();
         if (result == true)
         {
            myReport = windowDataContext.EditReport;
         }
         return myReport;
      }
   }
}
