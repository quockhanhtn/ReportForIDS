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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReportForIDS.UC
{
   /// <summary>
   /// Interaction logic for UCQueryItem.xaml
   /// </summary>
   public partial class UCQueryItem : UserControl
   {
      public UCQueryItem()
      {
         InitializeComponent();
      }
      private void ComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
      {
         e.Handled = !((System.Windows.Controls.ComboBox)sender).IsDropDownOpen;
      }
   }
}
