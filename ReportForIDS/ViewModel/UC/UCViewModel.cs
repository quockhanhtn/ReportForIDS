using MaterialDesignThemes.Wpf;
using System;

namespace ReportForIDS.ViewModel
{
   public abstract class UCViewModel : BaseViewModel
   {
      public SnackbarMessageQueue SnackbarMessageQueue { get; set; }
      public bool Done { get; set; } = false;

      public virtual void ReLoad()
      {
      }

      public virtual bool LoadPreviosData()
      {
         return false;
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