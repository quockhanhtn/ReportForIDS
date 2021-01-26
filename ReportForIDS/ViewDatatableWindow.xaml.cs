using System;
using System.Data;
using System.Linq;
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
         DataTable currentPageDt = DTSource.Clone();
         int firstRow = (currentPage - 1) * RowsPerPage;
         for (int i = firstRow; i < firstRow + RowsPerPage && i < DTSource.Rows.Count; i++)
         {
            currentPageDt.ImportRow(DTSource.Rows[i]);
         }

         MainDataGrid.DataContext = currentPageDt.DefaultView;
         MainDataGrid.ColumnWidth = 150;
      }

      public ViewDatatableWindow(DataTable source)
      {
         InitializeComponent();
         DTSource = source;
         ChangeRowPerPage(20);
      }

      void ChangeRowPerPage(int value)
      {
         if (DTSource != null)
         {
            RowsPerPage = value;
            NoOfPage = (int)Math.Ceiling((double)DTSource.Rows.Count / RowsPerPage);
            txtNoOfPage.Text = NoOfPage.ToString();
            CurrentPage = 1;
         }
      }

      private int currentPage = 0;

      private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
      {
         ScrollViewer scv = (ScrollViewer)sender;
         scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
         e.Handled = true;
      }

      private void CmbRowPerPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
         => ChangeRowPerPage(Convert.ToInt32((sender as ComboBox).SelectedValue));

      private void BtnNextPage_Click(object sender, RoutedEventArgs e)
         => CurrentPage += CurrentPage < NoOfPage ? 1 : 0;

      private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
         => CurrentPage -= CurrentPage > 1 ? 1 : 0;

      private void TxtCurrentPage_TextChanged(object sender, TextChangedEventArgs e)
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
            textbox.CaretIndex = textbox.Text.Length;
            textbox.ScrollToEnd();
            textbox.Focus();
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