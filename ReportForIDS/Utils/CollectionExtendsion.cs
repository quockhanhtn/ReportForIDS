using ReportForIDS.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ReportForIDS.Utils
{
   public static class CollectionExtendsion
   {
      public static void AddUnique(this List<MyField> myFields, MyField newField)
      {
         if (myFields.Count(f => f.FieldName.EqualsNotCaseSensitive(newField.FieldName)) == 0)
         {
            myFields.Add(newField);
         }
      }

      public static void AddUniqueRange(this List<MyField> myFields, IEnumerable<MyField> range)
      {
         foreach (var f in range)
         {
            myFields.AddUnique(f);
         }
      }

      public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
      {
         var result = new ObservableCollection<T>();
         foreach (var item in collection)
         {
            result.Add(item);
         }
         return result;
      }

      public static void AddRange<T>(this ObservableCollection<T> thisObCollection, ObservableCollection<T> collection)
      {
         foreach (var item in collection)
         {
            thisObCollection.Add(item);
         }
      }

      public static void AddRange<T>(this ObservableCollection<T> thisObCollection, IEnumerable<T> collection)
      {
         foreach (var item in collection)
         {
            thisObCollection.Add(item);
         }
      }
   }
}