using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportForIDS
{
   /// <summary>
   /// Interaction logic for ViewDatatableWindow.xaml
   /// </summary>
   public partial class ViewDatatableWindow : Window
   {
      public DataTable DTSource { get; set; }
      public int CurrentPage
      {
         get => currentPage;
         set
         {
            currentPage = value;

            //this.MainDataGrid.DataContext = DTSource.Select()
            //   .Skip((currentPage - 1) * RowsPerPage).Take(RowsPerPage)
            //   .Select(x => x.ItemArray).ToList();

            //this.MainDataGrid.ItemsSource = DTSource.Select()
            //   .Skip((currentPage - 1) * RowsPerPage).Take(RowsPerPage)
            //   .Select(x => string.Join(",", x.ItemArray)).ToList();

            UpdateView();

            this.txtCurrentPage.Text = currentPage.ToString();
         }
      }
      public int NoOfPage { get; set; }
      public int RowsPerPage { get; set; }

      public ViewDatatableWindow()
      {
         InitializeComponent();
      }

      void UpdateView()
      {
         stackPnlPagination.IsEnabled = false;
         progressBar.Visibility = Visibility.Visible;

         DataTable currentPageDt = DTSource.Clone();
         int firstRow = (currentPage - 1) * RowsPerPage;
         for (int i = firstRow; i < firstRow + RowsPerPage && i < DTSource.Rows.Count; i++)
         {
            currentPageDt.ImportRow(DTSource.Rows[i]);
         }

         foreach (DataColumn column in currentPageDt.Columns)
         {
            column.ColumnName = column.ColumnName.Replace("_", " ");
         }

         MainDataGrid.DataContext = currentPageDt.DefaultView;
         MainDataGrid.ColumnWidth = 150;

         progressBar.Visibility = Visibility.Collapsed;
         stackPnlPagination.IsEnabled = true;
      }

      public ViewDatatableWindow(DataTable source)
      {
         InitializeComponent();
         DTSource = source ?? new DataTable();
         txtTotalRow.Text = DTSource.Rows.Count.ToString();
         CmbRowPerPage_SelectionChanged(this.cmbRowPerPage, null);
      }

      private int currentPage = 0;

      private void CmbRowPerPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         RowsPerPage = Convert.ToInt32((sender as ComboBox).SelectedValue);
         if (DTSource != null)
         {
            NoOfPage = (int)Math.Ceiling((double)DTSource.Rows.Count / RowsPerPage);
            txtNoOfPage.Text = NoOfPage.ToString();
            CurrentPage = 1;
         }
      }

      private void BtnNextPage_Click(object sender, RoutedEventArgs e)
         => CurrentPage += CurrentPage < NoOfPage ? 1 : 0;

      private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
         => CurrentPage -= CurrentPage > 1 ? 1 : 0;

      private void TxtCurrentPage_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.Enter)
         {
            var textbox = sender as TextBox;
            int.TryParse(textbox.Text, out int displayPage);
            if (displayPage <= 0 || displayPage > NoOfPage)
            {
               textbox.Text = CurrentPage.ToString();
            }
            else
            {
               CurrentPage = displayPage;
            }
         }
      }

      private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();

      public static void Show(DataTable dataTable)
      {
         ViewDatatableWindow viewWindow = new ViewDatatableWindow(dataTable);
         try
         {
            viewWindow.Show();
         }
         catch (Exception e)
         {
            CustomMessageBox.Show(Cons.TOOL_NAME,
                                  "Error \r\n" + e.Message,
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);

            if (viewWindow != null)
            {
               viewWindow.Close();
            }
         }
      }
   }
}