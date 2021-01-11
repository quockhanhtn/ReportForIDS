using System;
using System.IO;
using System.Windows;

namespace ReportForIDS.Utils
{
   public static class FileUtils
   {
      public static bool Move(string sourcePath, string destPath, out string error)
      {
         error = "";
         try
         {
            if (Path.Equals(sourcePath, destPath)) { return true; }

            if (!File.Exists(sourcePath))
            {
               error = $"\"{sourcePath}\" not found";
               return false;
            }

            if (File.Exists(destPath)) { File.Delete(destPath); }

            File.Move(sourcePath, destPath);
         }
         catch (Exception e)
         {
            error = e.Message;
            return false;
         }
         return true;
      }
   }
}