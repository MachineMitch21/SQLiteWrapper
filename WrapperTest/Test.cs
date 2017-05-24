using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SQLiteWrapper;

namespace WrapperTest
{
    class Test
    {
        static void Main(string[] args)
        {
            DB_Manager man = new DB_Manager("test.db");
        }
    }
}
