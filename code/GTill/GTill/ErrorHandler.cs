using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GTill
{
    class ErrorHandler
    {
        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="sErrorDesc">A description of the error</param>
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

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="sErrorDesc">The message to log</param>
        public static void LogMessage(string sErrorDesc)
        {
            if (!File.Exists("log.txt"))
            {
                File.Create("log.txt");
            }
            TextWriter writeLog = new StreamWriter("log.txt", true);
            writeLog.WriteLine("Message " + DateTime.Now.ToString() + " : " + sErrorDesc);
            writeLog.WriteLine("");
            writeLog.Close();
        }
    }
}
