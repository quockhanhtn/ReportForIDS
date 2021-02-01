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
   public class UCInsertSqlQueryViewModel : UCBaseViewModel
   {
      public ICommand LoadedCommand { get; set; }
      public ICommand AddQueryCommand { get; set; }
      public ICommand LoadQueryCommand { get; set; }
      public ICommand SaveQueryCommand { get; set; }

      public StackPanel QueriesStackPnl { get; set; }

      public List<MyQuery> ListInsertedQuery
      {
         get
         {
            var insertedQueries = new List<MyQuery>();
            if (QueriesStackPnl != null)
            {
               foreach (var item in QueriesStackPnl.Children)
               {
                  insertedQueries.Add(((item as UCQueryItem).DataContext as UCQueryItemViewModel).QueryItem);
               }
            }
            return insertedQueries;
         }
         set
         {
            foreach (var newQuery in value)
            {
               newQuery.Reload();
               var ucQueryItem = new UCQueryItem()
               {
                  DataContext = new UCQueryItemViewModel(QueriesStackPnl, newQuery),
               };
               QueriesStackPnl.Children.Add(ucQueryItem);
            }
         }
      }

      public UCInsertSqlQueryViewModel(Action prevAction, Action nextAction)
      {
         LoadedCommand = new RelayCommand<UserControl>((p) => { return p != null; }, (p) =>
         {
            try
            {
               QueriesStackPnl = p.FindName("queriesStackPanel") as StackPanel;
            }
            catch (Exception e)
            {
               CustomMessageBox.Show("Error while find StackPanel in class UCInsertSqlQueryViewModel\r\n" + e.Message, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // check null
            if (QueriesStackPnl == null)
            {
               CustomMessageBox.Show("Error while find StackPanel in class UCInsertSqlQueryViewModel\r\n", Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
         });

         AddQueryCommand = new RelayCommand<object>((p) => true, (p) =>
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
                  DataContext = new UCQueryItemViewModel(QueriesStackPnl, queryItem),
               };
               QueriesStackPnl.Children.Add(ucQueryItem);
            }
         });

         LoadQueryCommand = new RelayCommand<object>((p) => true, (p) => LoadQueryFromFile(p));

         SaveQueryCommand = new RelayCommand<object>((p) => { return ListInsertedQuery.Count > 0; }, (p) =>
         {
            string filter = $"Custom file (*{Cons.REPORT_TEMPLATE_EXTENSION})|*{Cons.REPORT_TEMPLATE_EXTENSION}|All file |*.*";
            string filePath = DialogUtils.ShowSaveFileDialog("Save report template to file", filter);
            if (string.IsNullOrEmpty(filePath)) { return; }

            var addDataContext = new EditReportTemplateWindowViewModel(ListInsertedQuery)
            {
               InputFilePath = filePath
            };
            var editReportTemplateWindow = new EditReportTemplateWindow()
            {
               DataContext = addDataContext,
            };
            editReportTemplateWindow.ShowDialog();

            if (addDataContext.Result)
            {
               ShowSnackbarMessage("Queries file saved successfully");
            }
            else
            {
               ShowSnackbarMessage("No such file to save");
            }
         });

         PrevCommand = new RelayCommand<object>((p) => true, (p) => prevAction());

         NextCommand = new RelayCommand<object>((p) => true, (p) =>
         {
            if (ListInsertedQuery.Count == 0)
            {
               CustomMessageBox.Show("Missing input\r\n\r\nPlease insert at least one query",
                                     Cons.TOOL_NAME,
                                     MessageBoxButton.OK,
                                     MessageBoxImage.Error);
            }
            else
            {
               ReportFromQueryData.SetListQueries(ListInsertedQuery.OrderBy(x => x.Order).ToList());
               nextAction();
            }
         });
      }

      private void LoadQueryFromFile(object p)
      {
         string filePath = p?.ToString() ?? "";
         if (ListInsertedQuery.Count > 0)
         {
            MyQuery.LastOrder = ListInsertedQuery.Select(q => q.Order).Max();
         }
         else
         {
            MyQuery.LastOrder = 0;
         }

         if (string.IsNullOrEmpty(filePath))
         {
            string filter = $"Custom file (*{Cons.REPORT_TEMPLATE_EXTENSION})|*{Cons.REPORT_TEMPLATE_EXTENSION}|All file |*.*";
            filePath = DialogUtils.ShowOpenFileDialog("Open file to load list query", filter);
         }

         if (!string.IsNullOrEmpty(filePath))
         {
            var loadQueries = MyQuery.XMLData.Deserialize(filePath, out var error);
            if (loadQueries.Count == 0 || !string.IsNullOrEmpty(error))
            {
               error = "Error while loading list query from file\r\n" + error;
               CustomMessageBox.Show(error, Cons.TOOL_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
               ListInsertedQuery = loadQueries;
               ShowSnackbarMessage("Queries file loaded successfully");
            }
         }
         else
         {
            ShowSnackbarMessage("No file selected");
         }
      }
   }
}