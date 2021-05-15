using System;

namespace Actual_DB_Test
{
    class Program
    {
        static void Main()
        {
            const int id = 55;
            PSSDBConnection connection = new();
            // connection.CreateAlbum("test_album1");
            // connection.CreateAlbum("test_album2");
            // connection.CreateAlbum("test_album3");
            connection.MediaAndAlbumInsert("item1", 55, DateTime.Now);
            connection.MediaAndAlbumInsert("item2", 55, DateTime.Now);
            connection.MediaAndAlbumInsert("item3", 55, DateTime.Now);
            connection.MediaAndAlbumInsert("item4", 55, DateTime.Now);
            // connection.
            // connection.DeleteAlbum(name);
        }
    }
}
