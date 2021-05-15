using System;

namespace Actual_DB_Test
{
    class Program
    {
        static void Main()
        {
            const int id = 37;
            const string name = "test_album2";
            PSSDBConnection connection = new();
            connection.CreateAlbum(name);
            connection.AddToAlbum("item1", id);
            connection.AddToAlbum("item2", id);
            connection.AddToAlbum("item3", id);
            connection.AddToAlbum("item4", id);
            connection.DeleteAlbum(name);
        }
    }
}
