using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SQLiteWrapper;

namespace WrapperTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DB_Manager man = new DB_Manager("test2");

            DB_STATUS db_stat;
            SQL_STATUS sql_stat;
            
            sql_stat = man.CreateTable("yetanother", new Dictionary<string, string> {
                                                        { "ID", "INTEGER PRIMARY KEY AUTOINCREMENT" },
                                                        { "testcol", "varchar(255)" },
                                                        { "anothertestcol", "varchar(255)" }
                                                    });

            Thread.Sleep(10000);
        }
    }
}
