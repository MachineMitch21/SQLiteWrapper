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


        public bool WriteLogsToFile
        {
            get
            {
                return logger.LogToFile;
            }
            set
            {
                logger.LogToFile = value;
            }
        }

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
            string sql = String.Format("CREATE TABLE IF NOT EXISTS {0} (", name);
            
            //We need to retrieve the amount of elements withint the Dictionary so we can
            //know when the last element has been reached in our foreach loop
            int columnCount = columns.Count();
            int i = 0;

            foreach(KeyValuePair<string, string> pair in columns)
            {
                bool lastElement = false;
                if (i == columnCount - 1)
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
                logger.writeErrReport(String.Format("ERROR WHILE CREATING TABLE *({0})*  ::{1}", name, sql_e.Message));
                stat = SQL_STATUS.SQL_ERR;
            }

            CloseConnection();

            return stat;
        }


        public SQL_STATUS DeleteTable(string name)
        {
            SQL_STATUS stat = SQL_STATUS.SQL_OKAY;
            string sql = String.Format("DROP TABLE IF EXISTS {0};", name);
            
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
            CloseConnection();

            return stat;
        }


        public SQL_STATUS DeleteRecords(string table_name, string where_condition)
        {
            SQL_STATUS stat = SQL_STATUS.SQL_OKAY;
            string sql = String.Format("DELETE FROM {0} WHERE {1};", table_name, where_condition);

            SQLiteCommand cmd = new SQLiteCommand(sql, db_connection);

            OpenConnection();

            try
            {
                cmd.ExecuteNonQuery();
            }catch(SQLiteException sql_e)
            {
                stat = SQL_STATUS.SQL_ERR;
                logger.writeErrReport(sql_e.Message);
            }

            CloseConnection();
            return stat;
        }

        //You will need to specify an alter type when calling this method since there are several different possibilities
        public SQL_STATUS Alter(string table_name, string alter_type, string column)
        {
            SQL_STATUS stat = SQL_STATUS.SQL_OKAY;
            string sql = String.Format("ALTER TABLE {0} {1} {2}", table_name, alter_type, column);

            SQLiteCommand cmd = new SQLiteCommand(sql, db_connection);

            OpenConnection();

            try
            {
                cmd.ExecuteNonQuery();
            }catch(SQLiteException sql_e)
            {
                stat = SQL_STATUS.SQL_ERR;
                logger.writeErrReport(sql_e.Message);
            }

            CloseConnection();

            return stat;
        }


        public SQL_STATUS Update(string table_name, Dictionary<string, string> columns_values, string conditions)
        {
            SQL_STATUS stat = SQL_STATUS.SQL_OKAY;
            string sql = String.Format("UPDATE {0} ", table_name);

            int columnCount = columns_values.Count;

            int i = 0;
            foreach (KeyValuePair<string, string> pair in columns_values)
            {
                bool lastElement = false;
                if (i == columnCount - 1)
                {
                    lastElement = true;
                }
                
                sql += pair.Key.ToString() + " = " + pair.Value.ToString() + (lastElement ? String.Format(" {0};", conditions) : ", ");
                i++;
            }

            SQLiteCommand cmd = new SQLiteCommand(sql, db_connection);

            OpenConnection();

            try
            {
                cmd.ExecuteNonQuery();
            }catch(SQLiteException sql_e)
            {
                stat = SQL_STATUS.SQL_ERR;
                logger.writeErrReport(sql_e.Message);
            }

            CloseConnection();
            return stat;
        }


        public SQLiteDataReader Select(string table_name, string[] columns, string where)
        {
            SQLiteDataReader sql_data_reader = null;
            string sql = "SELECT ";

            for (int i = 0; i < columns.Length; i++)
            {
                bool lastElement = false;
                if(i == columns.Length - 1)
                {
                    lastElement = true;
                }

                sql += String.Format("{0}{1}", columns[i], (lastElement ? String.Format(" FROM {0} WHERE {1}", table_name, where) : ", "));
            }

            SQLiteCommand cmd = new SQLiteCommand(sql, db_connection);

            OpenConnection();

            try
            {
                sql_data_reader = cmd.ExecuteReader();
            }catch(SQLiteException sql_e)
            {
                sql_data_reader = null;
                logger.writeErrReport(sql_e.Message);
            }

            CloseConnection();

            return sql_data_reader;
        }


        public SQLiteDataReader SelectJoin(string table_name, string[] columns, string join_table, string join_type, string join_condition, string where)
        {
            //We have already written a select method so we don't need another one.
            //We know that SQL joins follow the FROM {TABLE} statement so we just concatenate the table_name with our join data
            //Then pass give that information to the select statement to retrieve the desired data
            return Select(String.Format("{0} {1} {2} ON {3}", table_name, join_type, join_table, join_condition), columns, where);
        }

        public DB_STATUS setWorkingDB(string db_name)
        {
            DB_STATUS stat;

            if (!dbExists(ref db_name))
            {
                CreateDatabase(db_name);   
            }

            working_db = db_name;
            stat = DB_STATUS.WORKING_DB_SET;

            return stat;
        }

        public void setErrLogFileName(string file)
        {
            logger.setLogPath(file);
        }

        public string getErrLogFileName()
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

            if(db_name.Substring(db_name.Length - 3) == ".db")
            {
                hasExt = true;
            }
            
            return hasExt;
        }

        private void OpenConnection()
        {
            db_connection = new SQLiteConnection(String.Format("Data Source={0};Version=3", working_db));
            db_connection.Open();
        }
        
        private void CloseConnection()
        {
            if(db_connection != null)
                db_connection.Close();
        }
    }
}
