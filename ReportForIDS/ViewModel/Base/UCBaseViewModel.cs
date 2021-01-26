using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public abstract class UCBaseViewModel : BaseViewModel
   {
      public ICommand ShowMessageCommand { get => new RelayCommand<object>((p) => p != null, (p) => { ShowSnackbarMessage(p, 5); }); }
      public ICommand PrevCommand { get; set; } = new RelayCommand<object>((p) => false, (p) => { });
      public ICommand NextCommand { get; set; } = new RelayCommand<object>((p) => false, (p) => { });

      public SnackbarMessageQueue SnackbarMessageQueue { get; set; }
      public bool Done { get; set; } = false;

      public virtual void ReLoad()
      {
      }

      protected void ShowSnackbarMessage(object message, double durationSecond = 5)
      {
         this.SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(durationSecond));
         this.SnackbarMessageQueue.Clear();
         this.SnackbarMessageQueue.Enqueue(message);
         OnPropertyChanged(nameof(this.SnackbarMessageQueue));
      }

      protected void ShowSnackbarMessage(object message, string actionContent, Action actionHandler, double durationSecond = 5)
      {
         this.SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(durationSecond));
         this.SnackbarMessageQueue.Clear();
         this.SnackbarMessageQueue.Enqueue(message, actionContent, actionHandler);
         OnPropertyChanged(nameof(this.SnackbarMessageQueue));
      }
   }
}