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
        private string seperator = "-------------------------------------------------------";

        public ErrLogger(string logPath)
        {
            setLogPath(logPath);
        }

        public void setLogPath(string path)
        {
            log_path = path;
        }

        public string getLogPath()
        {
            return log_path;
        }

        public void writeErrReport(string err_report)
        {
            try
            {
                StreamWriter writer = File.AppendText(log_path);
                writer.WriteLine("Date of LOG: " + DateTime.Now.ToString() + Environment.NewLine 
                                    + seperator + Environment.NewLine 
                                    + err_report + Environment.NewLine + seperator + Environment.NewLine);
                writer.Close();
            }catch(IOException ioe)
            {
                Console.WriteLine(ioe.Message);
            }
   
        }
    }
}
