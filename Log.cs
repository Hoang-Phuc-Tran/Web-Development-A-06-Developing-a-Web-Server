/*  
*  FILE          : Logging.cs 
*  PROJECT       : PROG2001 - Assignment #6
*  PROGRAMMER    : BUMSU YI, HOANG PHUC TRAN
*  FIRST VERSION : 2022-11-24 
*  DESCRIPTION   : this is to include the logging logic for myOwnWebServer 
*/

using System;
using System.IO;

namespace LoggingLibrary
{
    /*
    * NAME          : Log
    * DESCRIPTION   : Log method writes messages to a file in the current directory where the executable is.
    */
    public static class Log
    {
        /*  FUNCTION        : MakeLogFile
         *  DESCRIPTION     : This function checks whether the log filepath exists or not and creats it or not according to that
         *  PARAMETERS      : VOID
         *  RETURNS         : string currentLogFilePath
         */
        private static string MakeLogFile()
        {
            string logPath = "./myOwnWebServer.log";
            if (File.Exists(logPath)) 
            {
            }
            else
            {
                File.CreateText(logPath).Close();
            }
            return logPath;
        }

        /*  FUNCTION        : LogToFile
         *  DESCRIPTION     : This function uses the current time as a prefix to log the specified message to the current log file.
         *  PARAMETERS      : string logMessage : The message to be saved to the log file.
         *  RETURNS         : VOID
         */
        public static void LogToFile(string logMessage)
        {
            StreamWriter logStream = File.AppendText(MakeLogFile());
            logStream.WriteLine(string.Format("{0} {1} ", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), logMessage));
            logStream.Close();
        }
    }
}