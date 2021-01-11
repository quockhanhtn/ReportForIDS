using Microsoft.Win32;

namespace ReportForIDS.Utils
{
   public class DialogUtils
   {
      public static string ShowOpenFileDialog(string title, string filter)
      {
         string filePath = "";
         OpenFileDialog dialog = new OpenFileDialog();

         if (!string.IsNullOrEmpty(title))
         {
            dialog.Title = title;
         }
         if (!string.IsNullOrEmpty(filter))
         {
            dialog.Filter = filter;
         }

         if (dialog.ShowDialog() == true)
         {
            filePath = dialog.FileName;
         }

         return filePath;
      }

      public static string ShowSaveFileDialog(string title, string filter, string initialDirectory = null)
      {
         SaveFileDialog dialog = new SaveFileDialog();

         if (!string.IsNullOrEmpty(title)) { dialog.Title = title; }

         if (!string.IsNullOrEmpty(filter)) { dialog.Filter = filter; }

         if (!string.IsNullOrEmpty(initialDirectory)) { dialog.InitialDirectory = initialDirectory; }

         if (dialog.ShowDialog() == true)
         {
            return dialog.FileName;
         }
         else { return ""; }
      }

      public static string ShowFolderBrowserDialog(string currentFolder)
      {
         string folderPath = "";

         using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
         {
            dialog.SelectedPath = currentFolder;

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
               folderPath = dialog.SelectedPath;
            }
         }

         return folderPath;
      }
   }
}