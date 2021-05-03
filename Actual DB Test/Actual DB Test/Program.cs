using System;

namespace Actual_DB_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var connection = new PSSDBConnection();
            connection.InsertPhoto("test6", DateTime.Now, "0");
        }
    }
}
