using ReportForIDS.Model;
using System.Collections.Generic;

namespace ReportForIDS.SessionData
{
   public class StepByStepData
   {
      public static int Step;

      public static List<MyTable> ListTable = new List<MyTable>();
      public static List<MyField> ListFieldDisplay = new List<MyField>();
      public static List<MyCondition> ListCondition = new List<MyCondition>();
      public static List<MyField> ListFieldGroup = new List<MyField>();
   }
}