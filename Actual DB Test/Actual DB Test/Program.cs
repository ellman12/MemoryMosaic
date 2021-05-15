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
            // connection.MediaAndAlbumInsert("item1", id, DateTime.Now);
            // connection.MediaAndAlbumInsert("item2", id, DateTime.Now);
            // connection.MediaAndAlbumInsert("item3", id, DateTime.Now);
            connection.MediaAndAlbumInsert("item4", id, DateTime.Now);
            connection.MediaAndAlbumInsert("item4", 52, DateTime.Now);
            connection.MediaAndAlbumInsert("item4", 44, DateTime.Now);
            connection.DeleteItem("item4");
            // connection.DeleteAlbum(name);
            // connection.ExecuteQuery("SELECT * FROM media WHERE path=\"item3\"");
        }
    }
}
