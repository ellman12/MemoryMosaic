using System;
using System.Collections.Generic;

namespace Actual_DB_Test
{
    class Program
    {
        static void Main()
        {
            PSSDBConnection connection = new();
            connection.MediaAndAlbumInsert("item1", 1, DateTime.Now);
            connection.MediaAndAlbumInsert("item2", 1, DateTime.Now);
            connection.MediaAndAlbumInsert("item3", 1, DateTime.Now);
            connection.MediaAndAlbumInsert("item4", 1, DateTime.Now);
            connection.CreateAlbum("new album");
            connection.MediaAndAlbumInsert("item1_album2", 2, DateTime.Now);
            connection.MediaAndAlbumInsert("item2_album2", 2, DateTime.Now);
            connection.MediaAndAlbumInsert("item3_album2", 2, DateTime.Now);
            connection.MediaAndAlbumInsert("item4_album2", 2, DateTime.Now);
            connection.AssignAlbumCover("new album", "album comver");
            List<PSSDBConnection.Media> list = connection.SelectAlbum("new album");
            foreach(var row in list)
            {
                Console.WriteLine(row.path + '\t' + row.dateAdded + '\t' + row.dateTaken);
            }
        }
    }
}