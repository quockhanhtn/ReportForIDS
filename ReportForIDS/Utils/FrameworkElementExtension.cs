using System.Windows;

namespace ReportForIDS.Utils
{
   public static class FrameworkElementExtension
   {
      public static FrameworkElement GetRootParent(this FrameworkElement f)
      {
         FrameworkElement parent = f;

         while (parent.Parent != null)
         {
            parent = parent.Parent as FrameworkElement;
         }

         return parent;
      }
   }
}