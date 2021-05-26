using System;
using System.Collections.Generic;

namespace Actual_DB_Test
{
    class Program
    {
        static void Main()
        {
            Connection c = new();
            List<Connection.Media> yes = new();
            c.InsertMedia("item1", DateTime.Now);
            c.InsertMedia("item2", DateTime.Now);
            c.InsertMedia("item3", DateTime.Now);
            c.InsertMedia("item4", DateTime.Now);
            c.InsertMedia("item5", DateTime.Now);
            yes = c.LoadMediaTable();

            foreach (var y in yes)
            {
                Console.WriteLine(y.path + '\t' + y.dateAdded + '\t' + y.dateTaken);
            }
        }
    }
}