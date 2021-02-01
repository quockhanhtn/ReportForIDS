using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace ReportForIDS.Utils
{
   public static class ExcelUtils
   {
      public static void AssiginValue(this ExcelWorksheet worksheet, int row, int col, object value, string valueIfNull = "")
      {
         worksheet.Cells[row, col].Value = value ?? valueIfNull;
      }

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
         var excelApp = new Microsoft.Office.Interop.Excel.Application
         {
            Visible = true
         };
         Microsoft.Office.Interop.Excel.Workbooks workbooks = excelApp.Workbooks;
         Microsoft.Office.Interop.Excel.Workbook sheet = workbooks.Open(path);
      }

      public static void SaveExcelPackage(ExcelPackage excelPackage, string filePath)
      {
         Byte[] bin = excelPackage.GetAsByteArray();
         File.WriteAllBytes(filePath, bin);
      }
   }
}