using System;

namespace Actual_DB_Test
{
    class Program
    {
        static void Main()
        {
            PSSDBConnection connection = new();
            //connection.InsertMedia("h8887", DateTime.Now);
            //connection.CreateAlbum("vacay");
            connection.AddToAlbum("path1", 3);
        }
    }
}
