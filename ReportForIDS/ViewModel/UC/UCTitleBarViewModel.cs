using MaterialDesignThemes.Wpf;
using ReportForIDS.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class UCTitleBarViewModel : BaseViewModel
   {
      public ICommand MouseMoveWindowCommand { get; set; }
      public ICommand WindowMinimizeCommand { get; set; }
      public ICommand WindowMaximizeCommand { get; set; }
      public ICommand WindowCloseCommand { get; set; }
      public UCTitleBarViewModel()
      {
         #region Define Command
         MouseMoveWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
         {
            var window = FrameworkElementExtend.GetRootParent(p) as Window;
            if (window != null)
            {
               try { window.DragMove(); }
               catch (System.Exception) { }
            }
         });

         WindowMinimizeCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
         {
            var window = FrameworkElementExtend.GetRootParent(p) as Window;
            if (window != null)
            {
               if (window.WindowState != WindowState.Minimized) { window.WindowState = WindowState.Minimized; }
            }
         });

         WindowMaximizeCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
         {
            var iconWindowMaximize = p.FindName("iconWindowMaximize") as PackIcon;
            var btnWindowMaximize = p.FindName("btnWindowMaximize") as Button;

            Window window = FrameworkElementExtend.GetRootParent(p) as Window;

            if (window != null)
            {
               if (window.WindowState != WindowState.Maximized)
               {
                  window.WindowState = WindowState.Maximized;
                  iconWindowMaximize.Kind = PackIconKind.WindowRestore;
                  btnWindowMaximize.ToolTip = "Restore";
               }
               else
               {
                  window.WindowState = WindowState.Normal;
                  iconWindowMaximize.Kind = PackIconKind.WindowMaximize;
                  btnWindowMaximize.ToolTip = "Maximize";
               }
            }
         });

         WindowCloseCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
         {
            Window window = FrameworkElementExtend.GetRootParent(p) as Window;
            if (window != null) { window.Close(); }
         });
         #endregion
      }
   }
}
