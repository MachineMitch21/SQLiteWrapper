using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace SQLiteWrapper
{

    public enum DB_STATUS
    {
        INVALID_DB,
        UNSPECIFIED_WORKING_DB,
        ERR_CREATING_DB,
        DB_CREATED_OKAY,
        DB_EXISTS,
        DB_NOT_FOUND,
        WORKING_DB_SET
    }

    public enum SQL_STATUS
    {
        SQL_OKAY,
        SQL_ERR
    }

    public class DB_Manager
    {

        public DB_Manager()
        {

        }

        public DB_Manager(string working_db)
        {
            setWorkingDB(working_db);
        }

        //This will represent the working database for this DB_Manager
        //It is being made for convenience to prevent an abundance of 
        //specifications for which database we are interacting with
        private string working_db;

        private ErrLogger logger = new ErrLogger("logs.txt");

        SQLiteConnection db_connection;


        public DB_STATUS CreateDatabase(string db_name)
        {
            DB_STATUS stat = DB_STATUS.ERR_CREATING_DB;

            if (dbExists(ref db_name))
            {
                stat = DB_STATUS.DB_EXISTS;
            }
            else
            {
                SQLiteConnection.CreateFile(db_name);
                stat = DB_STATUS.DB_CREATED_OKAY;
            }
            
            return stat;
        }

        
        public SQL_STATUS CreateTable(string name, Dictionary<string, string> columns)
        {
            SQL_STATUS stat = SQL_STATUS.SQL_OKAY;
            string sql = "CREATE TABLE IF NOT EXISTS " + name + " (";
            
            //We need to retrieve the amount of elements withint the Dictionary so we can
            //know when the last element has been reached in our foreach loop
            int columnsCount = columns.Count();
            int i = 0;

            foreach(KeyValuePair<string, string> pair in columns)
            {
                bool lastElement = false;
                if (i == columnsCount - 1)
                {
                    lastElement = true;
                }

                //We need to add the current element in the pair to the end of the sql statement
                //and verify whether or not we should enclose the statement in its ending paranthesis
                //or just add another comma because the current element is not the last element
                sql += pair.Key.ToString() + " " + pair.Value.ToString() + (lastElement ? ");" : ", ");
                i++;
            }

            OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(sql, db_connection);

            try
            {
                cmd.ExecuteNonQuery();
            }catch(SQLiteException sql_e)
            {
                logger.writeErrReport("ERROR WHILE CREATING TABLE *(" + name + ")*  ::" + sql_e.Message);
                stat = SQL_STATUS.SQL_ERR;
            }

            CloseConnection();

            return stat;
        }


        public SQL_STATUS DeleteTable(string name)
        {
            SQL_STATUS stat = SQL_STATUS.SQL_OKAY;
            string sql = "DROP TABLE IF EXISTS " + name + ";";
            
            OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(sql, db_connection);

            try
            {
                cmd.ExecuteNonQuery();
            }catch(SQLiteException sql_e)
            {
                logger.writeErrReport(sql_e.Message);
                stat = SQL_STATUS.SQL_ERR;
            }

            return stat;
        }

        public DB_STATUS setWorkingDB(string db_name)
        {
            DB_STATUS stat = DB_STATUS.DB_NOT_FOUND;

            if (!dbExists(ref db_name))
            {
                CreateDatabase(db_name);   
            }

            working_db = db_name;
            stat = DB_STATUS.WORKING_DB_SET;

            return stat;
        }

        public void setErrLogFile(string file)
        {
            logger.setLogPath(file);
        }

        public string getErrLogFile()
        {
            return logger.getLogPath();
        }

        //We will need to be checking if a db file exists quite often within this class
        //db_name is passed by reference because we need our changes to reflect on the string passed in
        private bool dbExists(ref string db_name)
        {
            if (!hasExtension(db_name))
            {
                db_name += ".db";
            }
                
            return (File.Exists(db_name));
        }

        //We need to verify that db names always have the proper ".db" extension
        private bool hasExtension(string db_name)
        {
            bool hasExt = false;

            if(db_name.Substring(db_name.Length - 4) == ".db")
            {
                hasExt = true;
            }
            
            return hasExt;
        }

        private void OpenConnection()
        {
            db_connection = new SQLiteConnection("Data Source=" + working_db + ";Version=3");
            db_connection.Open();
        }
        
        private void CloseConnection()
        {
            if(db_connection != null)
                db_connection.Close();
        }
    }
}
