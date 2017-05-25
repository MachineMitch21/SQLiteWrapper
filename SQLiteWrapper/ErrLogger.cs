using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SQLiteWrapper
{
    class ErrLogger
    {

        private string log_path;
        private bool logToFile = true;

        public bool LogToFile
        {
            get
            {
                return logToFile;
            }
            set
            {
                logToFile = value;
            }
        }

        //This seperates log entries for more readability in the log file
        private string seperator = "-------------------------------------------------------";

        public ErrLogger(string logPath)
        {
            setLogPath(logPath);
        }

        public void setLogPath(string path)
        {
            this.log_path = path;
        }

        public string getLogPath()
        {
            return this.log_path;
        }

        public void setLogSeperator(string seperator)
        {
            this.seperator = seperator;
        }

        public string getLogSeperator()
        {
            return this.seperator;
        }

        public void writeErrReport(string err_report)
        {
            string full_report = "Date of LOG: "    + DateTime.Now.ToString()   + Environment.NewLine
                                                    + seperator                 + Environment.NewLine
                                                    + err_report                + Environment.NewLine
                                                    + seperator                 + Environment.NewLine;

            if (logToFile)
            {

                try
                {
                    StreamWriter writer = File.AppendText(log_path);
                    writer.WriteLine(full_report);
                    writer.Close();
                }catch(IOException ioe)
                {
                    Console.WriteLine(ioe.Message);
                }

            }
            else
            {
                Console.WriteLine(full_report);
            }
   
        }
    }
}
