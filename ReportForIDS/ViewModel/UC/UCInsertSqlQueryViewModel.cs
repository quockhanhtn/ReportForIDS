using ReportForIDS.Model;
using ReportForIDS.SessionData;
using ReportForIDS.UC;
using ReportForIDS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS.ViewModel
{
   public class UCInsertSqlQueryViewModel : UCViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand AddQueryCommand { get; set; }
      public ICommand PrevCommand { get; set; }
      public ICommand NextCommand { get; set; }
      public UCInsertSqlQueryViewModel(Action prevAction, Action nextAction)
      {
         AddQueryCommand = new RelayCommand<StackPanel>((p) => { return p != null; }, (p) =>
         {
            var enterQueryWindow = new EnterQueryWindow()
            {
               DataContext = new EnterQueryWindowViewModel(),
            };
            var result = enterQueryWindow.ShowDialog();

            if (result == true)
            {
               MyQuery queryItem = (enterQueryWindow.DataContext as EnterQueryWindowViewModel).QueryItem;
               var ucQueryItem = new UCQueryItem()
               {
                  DataContext = new UCQueryItemViewModel(p, queryItem),
               };
               p.Children.Add(ucQueryItem);
            }
            else { /*nothing */ }
         });

         PrevCommand = new RelayCommand<object>((p) => { return true; }, (p) => prevAction());

         NextCommand = new RelayCommand<StackPanel>((p) => { return p != null; }, (p) =>
         {
            var insertedQueries = new List<MyQuery>();
            foreach (var item in p.Children)
            {
               insertedQueries.Add(((item as UCQueryItem).DataContext as UCQueryItemViewModel).QueryItem);
            }

            if (insertedQueries.Count == 0)
            {
               MessageBox.Show("Please insert SQL query", Cons.ToolName, MessageBoxButton.OK, MessageBoxImage.Error);
               return;
            }

            ReportFromQueryData.SetListQueries(insertedQueries.OrderBy(x => x.Order).ToList());
            nextAction();
         });
      }
   }
}
