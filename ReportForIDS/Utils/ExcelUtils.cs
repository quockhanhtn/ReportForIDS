using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ReportForIDS.Utils
{
   public class ExcelUtils
   {
      public static ExcelPackage CreateExcelPackage(List<string> wookSheetName, OfficeProperties officeProperties = null)
      {
         ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
         var excelPackage = new ExcelPackage();

         foreach (var sheet in wookSheetName)
         {
            excelPackage.Workbook.Worksheets.Add(sheet);
         }

         if (officeProperties != null)
         {
            excelPackage.Workbook.Properties.Title = officeProperties.Title;
            excelPackage.Workbook.Properties.Subject = officeProperties.Subject;
            excelPackage.Workbook.Properties.Category = officeProperties.Category;
            excelPackage.Workbook.Properties.Comments = officeProperties.Comments;

            excelPackage.Workbook.Properties.Author = officeProperties.Author;
            excelPackage.Workbook.Properties.Company = officeProperties.Company;
            excelPackage.Workbook.Properties.Manager = officeProperties.Manager;
         }

         return excelPackage;
      }

      public static void OpenFile(string path)
      {
         Thread thread = new Thread(() =>
         {
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = true;
            Microsoft.Office.Interop.Excel.Workbooks workbooks = excelApp.Workbooks;
            Microsoft.Office.Interop.Excel.Workbook sheet = workbooks.Open(path);
         });
         thread.IsBackground = true;
         thread.Start();
      }

      public static void SaveExcelPackage(ExcelPackage excelPackage, string filePath)
      {
         Byte[] bin = excelPackage.GetAsByteArray();
         File.WriteAllBytes(filePath, bin);
      }
   }
}
