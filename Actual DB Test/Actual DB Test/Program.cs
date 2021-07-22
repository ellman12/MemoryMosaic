using System;
using System.Collections.Generic;
using PSS.Client.Backend;

namespace Actual_DB_Test
{
    class Program
    {
        static void Main()
        {
            Connection c = new();
            //c.OpenConnection();
            //c.ClearTables();
            //c.MediaAndAlbumInsert("new item1", 5, DateTime.Now);
            //c.MediaAndAlbumInsert("new item2", 5, DateTime.Now);
            //c.MediaAndAlbumInsert("new item3", 5, DateTime.Now);
            //c.MediaAndAlbumInsert("new item4", 5, DateTime.Now);
            //c.DeleteItem("new item3");
            //c.RestoreItem("new item3");
            c.InsertMedia("path1", DateTime.Now);
            c.InsertMedia("path2", DateTime.Now);
            List<Connection.Media> media = c.LoadMediaTable();
            foreach (var med in media)
            {
                Console.WriteLine(med.path);
            }
            c.CloseConnection();
        }
    }
}