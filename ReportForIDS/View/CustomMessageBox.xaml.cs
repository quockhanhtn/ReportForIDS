using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for CustomMessageBox.xaml
   /// </summary>
   public partial class CustomMessageBox : Window
   {
      private MessageBoxResult ShowResult;

      public CustomMessageBox()
      {
         InitializeComponent();
      }

      private void SetIcon(MessageBoxImage icon)
      {
         switch (icon)
         {
            case MessageBoxImage.None:
               icoBox.Visibility = Visibility.Hidden;
               break;

            case MessageBoxImage.Question:
               icoBox.Kind = PackIconKind.QuestionMarkRhombus;
               icoBox.Foreground = (Brush)(new BrushConverter()).ConvertFrom("#007acc");
               break;

            case MessageBoxImage.Error:
               icoBox.Kind = PackIconKind.MultiplyBox;
               icoBox.Foreground = (Brush)(new BrushConverter()).ConvertFrom("#f38b76");
               break;

            case MessageBoxImage.Warning:
               icoBox.Kind = PackIconKind.WarningBox;
               icoBox.Foreground = (Brush)(new BrushConverter()).ConvertFrom("#d9b172");
               break;

            case MessageBoxImage.Information:
               icoBox.Kind = PackIconKind.InformationOutline;
               icoBox.Foreground = (Brush)(new BrushConverter()).ConvertFrom("#66b158");
               break;

            default:
               break;
         }
      }

      private void SetButtons(MessageBoxButton buttons)
      {
         switch (buttons)
         {
            case MessageBoxButton.OK:
               btnleft.Visibility = Visibility.Visible;
               txtLeft.Text = "OK";
               icoLeft.Kind = PackIconKind.HandOkay;
               btnleft.Tag = "1";   // OK

               btnMid.Visibility = Visibility.Collapsed;
               btnRight.Visibility = Visibility.Collapsed;
               break;

            case MessageBoxButton.OKCancel:
               btnleft.Visibility = Visibility.Visible;
               txtLeft.Text = "OK";
               icoLeft.Kind = PackIconKind.HandOkay;
               btnleft.Tag = "1";   // OK

               btnMid.Visibility = Visibility.Visible;
               txtMid.Text = "CANCEL";
               icoMid.Kind = PackIconKind.Cancel;
               btnMid.Tag = "2";    // Cancel

               btnRight.Visibility = Visibility.Collapsed;
               break;

            case MessageBoxButton.YesNoCancel:
               btnleft.Visibility = Visibility.Visible;
               txtLeft.Text = "YES";
               icoLeft.Kind = PackIconKind.Check;
               btnleft.Tag = "6";   // Yes

               btnMid.Visibility = Visibility.Visible;
               txtMid.Text = "NO";
               icoMid.Kind = PackIconKind.Close;
               btnMid.Tag = "7";    // No

               btnRight.Visibility = Visibility.Visible;
               txtRight.Text = "CANCEL";
               icoRight.Kind = PackIconKind.Cancel;
               btnRight.Tag = "2";    // Cancel
               break;

            case MessageBoxButton.YesNo:
               btnleft.Visibility = Visibility.Visible;
               txtLeft.Text = "YES";
               icoLeft.Kind = PackIconKind.Check;
               btnleft.Tag = "6";   // Yes

               btnMid.Visibility = Visibility.Visible;
               txtMid.Text = "NO";
               icoMid.Kind = PackIconKind.Close;
               btnMid.Tag = "7";    // No

               btnRight.Visibility = Visibility.Collapsed;
               break;
         }
      }

      public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultResult = MessageBoxResult.Cancel)
      {
         CustomMessageBox customMessageBox = new CustomMessageBox();
         customMessageBox.titleBar.Tag = caption;
         customMessageBox.tblContent.Text = messageText;
         customMessageBox.SetButtons(buttons);
         customMessageBox.SetIcon(icon);
         customMessageBox.ShowResult = defaultResult;

         customMessageBox.ShowDialog();

         return customMessageBox.ShowResult;
      }

      private void BtnClick(object sender, RoutedEventArgs e)
      {
         ShowResult = (sender as Button).Tag switch
         {
            "0" => MessageBoxResult.None,
            "1" => MessageBoxResult.OK,
            "2" => MessageBoxResult.Cancel,
            "6" => MessageBoxResult.Yes,
            "7" => MessageBoxResult.No,
            _ => MessageBoxResult.None,
         };
         this.Close();
      }
   }
}