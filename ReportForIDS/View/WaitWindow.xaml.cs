using ReportForIDS.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for WaitWindow.xaml
   /// </summary>
   public partial class WaitWindow : Window
   {
      public Task DoingTask { get; set; }
      public CancellationTokenSource TokenSource { get; set; }
      public Action DoingAction { get; set; }
      public Action CancelAction { get; set; }

      public WaitWindow(Action doingAcction, Action cancelAction = null)
      {
         DoingAction = doingAcction;
         CancelAction = cancelAction;

         InitializeComponent();

         if (cancelAction == null)
         {
            btnCancel.IsEnabled = false;
         }
      }

      public override void EndInit()
      {
         base.EndInit();
         TokenSource = new CancellationTokenSource();

         DoingTask = Task.Factory.StartNew(DoingAction, TokenSource.Token).ContinueWith(
            (t) => { this.Close(); },
            TaskScheduler.FromCurrentSynchronizationContext()
            );
      }

      private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         if (this.GetRootParent() is Window window)
         {
            try { window.DragMove(); }
            catch (Exception) { }
         }
      }

      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         TokenSource.Cancel();
         CancelAction?.Invoke();
         this.Close();
      }

      public static void Show(Action doingAcction, Action cancelAction = null)
      {
         WaitWindow waitWindow = new WaitWindow(doingAcction, cancelAction);
         waitWindow.ShowDialog();
      }
   }
}