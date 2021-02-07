using MaterialDesignThemes.Wpf;
using ReportForIDS.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ReportForIDS.UC
{
   /// <summary>
   /// Interaction logic for UCTitleBar.xaml
   /// </summary>
   public partial class UCTitleBar : UserControl
   {
      public Visibility MaximinButtonVisibility
      {
         get => btnMaximizeWindow.Visibility;
         set => btnMaximizeWindow.Visibility = value;
      }

      public Visibility MinimizeButtonVisibility
      {
         get => btnMinimizeWindow.Visibility;
         set => btnMinimizeWindow.Visibility = value;
      }

      public UCTitleBar()
      {
         InitializeComponent();
         //this.DataContext = new ViewModel.UCTitleBarViewModel();
      }

      private void BtnMinimizeWindow_Click(object sender, RoutedEventArgs e)
      {
         if (this.GetRootParent() is Window window)
         {
            if (window.WindowState != WindowState.Minimized)
            {
               window.WindowState = WindowState.Minimized;
            }
         }
      }

      private void BtnMaximizeWindow_Click(object sender, RoutedEventArgs e)
      {
         if (this.GetRootParent() is Window window)
         {
            if (window.WindowState != WindowState.Maximized)
            {
               window.WindowState = WindowState.Maximized;
               iconWindowMaximize.Kind = PackIconKind.WindowRestore;
               btnMaximizeWindow.ToolTip = "Restore";
            }
            else
            {
               window.WindowState = WindowState.Normal;
               iconWindowMaximize.Kind = PackIconKind.WindowMaximize;
               btnMaximizeWindow.ToolTip = "Maximize";
            }
         }
      }

      private void BtnCloseWindow_Click(object sender, RoutedEventArgs e)
      {
         if (this.GetRootParent() is Window window)
         {
            window.Close();
         }
      }

      private void UcTitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         if (this.GetRootParent() is Window window)
         {
            try { window.DragMove(); }
            catch (Exception) { }
         }
      }
   }
}