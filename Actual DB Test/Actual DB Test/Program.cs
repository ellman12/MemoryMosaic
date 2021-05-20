using System;

namespace Actual_DB_Test
{
    class Program
    {
        static void Main()
        {
            PSSDBConnection connection = new();
            connection.MediaAndAlbumInsert("item1_old", 2, DateTime.Now);
            connection.MediaAndAlbumInsert("item2_old", 2, DateTime.Now);
            connection.MediaAndAlbumInsert("item3_old", 2, DateTime.Now);
            connection.MediaAndAlbumInsert("item4_old", 2, DateTime.Now);
            connection.UpdateDateTaken("item3_old", "item3_new");
        }
    }
}