﻿using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ReportForIDS.Utils
{
   public class IniFile
   {
      private readonly string Section;
      private readonly string Path;
      private readonly string EXE = Assembly.GetExecutingAssembly().GetName().Name;

      [DllImport("kernel32", CharSet = CharSet.Unicode)]
      private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

      [DllImport("kernel32", CharSet = CharSet.Unicode)]
      private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

      public IniFile(string IniPath = null, string IniSection = null)
      {
         Path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
         Section = IniSection ?? EXE;
      }

      public string Read(string Key)
      {
         var RetVal = new StringBuilder(255);
         GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
         return RetVal.ToString();
      }

      public void Write(string key, string value)
      {
         WritePrivateProfileString(Section ?? EXE, key, value, Path);
      }

      public void DeleteKey(string key)
      {
         Write(key, null);
      }

      public bool KeyExists(string Key)
      {
         return Read(Key).Length > 0;
      }
   }
}