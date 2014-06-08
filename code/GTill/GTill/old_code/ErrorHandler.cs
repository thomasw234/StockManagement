using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GTill
{
    class ErrorHandler
    {
        public static void LogError(string sErrorDesc)
        {
            if (!File.Exists("log.txt"))
            {
                File.Create("log.txt");
            }
            TextWriter writeLog = new StreamWriter("log.txt", true);
            writeLog.WriteLine("Error " + DateTime.Now.ToString() + " : " + sErrorDesc);
            writeLog.WriteLine("");
            writeLog.Close();
        }
    }
}
