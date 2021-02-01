using ReportForIDS.Model;
using ReportForIDS.UC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class UCQueryItemViewModel : BaseViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand MoveTopCommand { get; set; }
      public ICommand MoveUpCommand { get; set; }
      public ICommand MoveDownCommand { get; set; }
      public ICommand MoveBotCommand { get; set; }
      public ICommand EditQueryCommand { get; set; }
      public ICommand RemoveQueryCommand { get; set; }
      public ICommand SetPrimaryQueryCommand { get; set; }

      public UserControl ucQueryItem { get; set; }
      public Panel ParentPanel { get; set; }

      public MyQuery QueryItem
      {
         get => queryItem; 
         set
         {
            queryItem = value;
            OnPropertyChanged(nameof(QueryItem));
         }
      }

      public bool IsPrimaryQuery
      {
         get => QueryItem.IsPrimary; 
         set
         {
            if (value)
            {
               for (int i = 0; i < ParentPanel.Children.Count; i++)
               {
                  ((ParentPanel.Children[i] as UCQueryItem).DataContext as UCQueryItemViewModel).IsPrimaryQuery = false;
               }
               MoveTopCommand.Execute(null);
            }
            QueryItem.IsPrimary = value;
            OnPropertyChanged();
         }
      }

      public int CurrentIndex { get => ParentPanel.Children.IndexOf(ucQueryItem); }

      public UCQueryItemViewModel(Panel parrent, MyQuery myQuery)
      {
         ParentPanel = parrent;
         QueryItem = myQuery;

         LoadedCommand = new RelayCommand<UserControl>((p) => { return p != null; }, (p) =>
         {
            ucQueryItem = p;
            IsPrimaryQuery = QueryItem.IsPrimary;
         });

         MoveTopCommand = new RelayCommand<object>((p) => { return CurrentIndex > 0; }, (p) =>
         {
            for (int i = CurrentIndex - 1; i >= 0; i--)
            {
               (((ParentPanel.Children[i] as UCQueryItem).DataContext) as UCQueryItemViewModel).MoveDownCommand.Execute(null);
            }
         });

         MoveUpCommand = new RelayCommand<object>((p) => { return CurrentIndex > 0; }, (p) =>
         {
            SwapOrder(CurrentIndex - 1);
         });

         MoveDownCommand = new RelayCommand<object>((p) => { return CurrentIndex < ParentPanel.Children.Count - 1 && QueryItem.IsPrimary != true; }, (p) =>
         {
            SwapOrder(CurrentIndex + 1);
         });

         MoveBotCommand = new RelayCommand<object>((p) => { return CurrentIndex < ParentPanel.Children.Count - 1 && QueryItem.IsPrimary != true; }, (p) =>
         {
            for (int i = CurrentIndex + 1; i < ParentPanel.Children.Count; i++)
            {
               (((ParentPanel.Children[i] as UCQueryItem).DataContext) as UCQueryItemViewModel).MoveUpCommand.Execute(null);
            }
         });

         EditQueryCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            var enterQueryWindow = new EnterQueryWindow()
            {
               DataContext = new EnterQueryWindowViewModel(QueryItem),
            };
            var result = enterQueryWindow.ShowDialog();

            if (result == true)
            {
               QueryItem = (enterQueryWindow.DataContext as EnterQueryWindowViewModel).QueryItem;
               IsPrimaryQuery = QueryItem.IsPrimary;
            }
            else { /*nothing */ }
         });

         RemoveQueryCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            MoveBotCommand.Execute(null);
            ParentPanel.Children.Remove(ucQueryItem);
            MyQuery.LastOrder--;
         });

         SetPrimaryQueryCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
         {
            
         });
      }

      void SwapOrder(int newIndex)
      {
         var curIndex = CurrentIndex;

         var swapControl = ParentPanel.Children[newIndex] as UCQueryItem;
         var swapControlVM = swapControl.DataContext as UCQueryItemViewModel;

         if (newIndex > curIndex)
         {
            ParentPanel.Children.RemoveAt(newIndex);
            ParentPanel.Children.RemoveAt(curIndex);

            ParentPanel.Children.Insert(curIndex, swapControl);
            ParentPanel.Children.Insert(newIndex, ucQueryItem);
         }
         else if (newIndex < curIndex)
         {
            ParentPanel.Children.RemoveAt(curIndex);
            ParentPanel.Children.RemoveAt(newIndex);

            ParentPanel.Children.Insert(newIndex, ucQueryItem);
            ParentPanel.Children.Insert(curIndex, swapControl);
         }
         else { return; }

         int temp = QueryItem.Order;
         QueryItem.Order = swapControlVM.QueryItem.Order;
         swapControlVM.QueryItem.Order = temp;

         OnPropertyChanged(nameof(QueryItem));
         swapControlVM.OnPropertyChanged(nameof(QueryItem));
      }
      private MyQuery queryItem;
   }
}
