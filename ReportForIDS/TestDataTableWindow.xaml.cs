using System;
using System.Collections.Generic;
using System.Data;
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
   /// Interaction logic for TestDataTableWindow.xaml
   /// </summary>
   public partial class TestDataTableWindow : Window
   {
      public TestDataTableWindow()
      {
         InitializeComponent();
      }

      public void SetDataGrid(DataTable dataTable)
      {
         MainDataGrid.DataContext = dataTable.DefaultView;
      }

   }
}
