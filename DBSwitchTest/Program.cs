using System;
using Npgsql;

namespace DBSwitchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime now = DateTime.Now;

            //Connection.AddToAlbum("hsdfjdhsfjdhjsfghjdsgfjhdsgfgdhjsfghdjsfdghjf", 7000);
            //Connection.MediaAndAlbumInsert("delete this item", 12345, now);
            //Connection.RestoreItem("delete this item");
            //Connection.CreateAlbum("");
            //Connection.AddToAlbum("new lbu dhfjkhjk 1", Connection.GetAlbumID("new album 843387"));
            //Connection.AddToAlbum("new lbu dhfjkhjk 2", Connection.GetAlbumID("new album 843387"));
            //Connection.AddToAlbum("new lbu dhfjkhjk 3", Connection.GetAlbumID("new album 843387"));
            //Connection.AddToAlbum("new lbu dhfjkhjk 4", Connection.GetAlbumID("new album 843387"));
            //var yes = Connection.LoadAlbum("new album 843387");
            //Connection.CreateAlbum("test album");
            //Connection.MediaAndAlbumInsert("item 1", Connection.GetAlbumID("test album"), now);
            //Connection.MediaAndAlbumInsert("item 2", Connection.GetAlbumID("test album"), now);
            //Connection.MediaAndAlbumInsert("item 3", Connection.GetAlbumID("test album"), now);
            //Connection.MediaAndAlbumInsert("item 4", Connection.GetAlbumID("test album"), now);
            //Connection.MediaAndAlbumInsert("item 5", Connection.GetAlbumID("test album"), now);
            //Connection.MediaAndAlbumInsert("item 6", Connection.GetAlbumID("test album"), now);
            //Connection.MediaAndAlbumInsert("item 7", Connection.GetAlbumID("test album"), now);

            //Guid yes = Guid.Parse("db4e022a-28c0-11ec-813f-1c1b0d0d5ac3");
            string yes = "db4e7764-28c0-11ec-8143-1c1b0d0d5ac3";
            Connection.InsertMedia("new PM item", now);
            var yes2 = Connection.GetPeriod("new PM item");
            ;
        }
    }
}
