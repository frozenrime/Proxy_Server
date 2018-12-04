using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary
{
    public class DBController
    {
        private static DBController controller = new DBController();
        private static MongoClient client;
        private DBController()
        {
            client = new MongoDB.Driver.MongoClient(
                new MongoDB.Driver.MongoClientSettings
                {
                    //Credentials = new[] { MongoCredential.CreateCredential("BookStore", "BookStoreAdmin", "3881818986")},
                    Credential = MongoCredential.CreateCredential("BookStore", "BookStoreAdmin", "3881818986"),
                    Server = new MongoServerAddress("89.41.100.129", 55560),
                    UseSsl = false
                });
        }

        public static DBController getDbController()
        {
            lock (controller)
            {
                if (controller != null)
                    controller = new DBController();
            }
            return controller;
        }

        public static MongoClient getDBClient()
        {
            if (client == null)
                getDbController();
            return client;
        }

        public bool connectToDB()
        {

            var ss = client.Settings.Server.Host;

            var db = client.GetDatabase("BookStore");
            var collection = db.GetCollection<Book>("BestBook");

            var aaa = db.ListCollectionNames();
            Console.WriteLine();

            var publisherId = new ObjectId("5bf023c8d83dab3c056f1eeb");

            var filter = new BsonDocument("pages", "216");
            var book2 = collection.Find(b => b.pages == 216).ToListAsync().Result;



            Console.WriteLine(book2);
            var books = collection
                .Find(book => book.language == "English")
                .Limit(2);



            var first = books.First();
            var titl = first.title;

            Task<List<Book>> books1 = collection
                .Find(b => b.language == "English")
                .Limit(2)
                .ToListAsync();

            return true;
        }
    }
}
