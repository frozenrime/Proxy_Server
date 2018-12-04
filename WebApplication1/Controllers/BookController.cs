using MongoDB.Bson;
using MongoDB.Driver;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace WebApplication1.Controllers
{
    public class BookController : ApiController
    {
        private MongoClient client;
        private IMongoDatabase db;
        private IMongoCollection<Book> collection;

        private BookController()
        {
            client = DBController.getDBClient();
            //MongoDB.Driver.Core.Operations.PingOperation;
            db = client.GetDatabase("BookStore");
            //bool isMongoLive = db.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
            //if (!isMongoLive)
            //{
            //    throw new Exception("Can not conect to Data Base.");
            //}
            collection = db.GetCollection<Book>("BestBook");

            var server = client.Cluster.Description.Servers.FirstOrDefault();
            var isAlive = (server != null &&
                       server.HeartbeatException == null &&
                       server.State == MongoDB.Driver.Core.Servers.ServerState.Connected);
            if (!isAlive)
            {
                throw new Exception("Can not conect to Data Base.");
            }
        }

        // GET: api/Book
        public List<Book> Get()
        {
            //var books = collection.AsQueryable<Book>().ToList();

            var books = new List<Book>();
            try
            {
                books = collection.Find(b => b._id != null).ToListAsync().Result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} Exception caught.", e);
                HttpResponseMessage response =
                    this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Can not optain Books from Data Base.");
                throw new HttpResponseException(response);
            }

            return books;
        }

        // GET: api/Book/5
        public List<Book> Get(String id)
        {
            ObjectId str_id = ObjectId.Parse(id);

            //var book = collection.AsQueryable<Book>().SingleOrDefault(b => b._id == str_id);
            var books = collection.Find(b => b._id == str_id).ToListAsync().Result;

            return books;
        }

        // POST: api/Book
        public void Post(String title, [FromUri] String[] author, DateTime published_date, int pages, String language)
        {
            //var obj = new Book();
            //obj.title = "sdafdsaf";

            var a = Request.RequestUri.Query;

            Book book = new Book();
            book.title = title;
            book.author = author;
            book.published_date = published_date;
            book.pages = pages;
            book.language = language;

            collection.InsertOne(book);
        }

        // PUT: api/Book/...
        public void Put(String id, String title, [FromUri] String[] author, DateTime published_date, int pages, String language)
        {
            ObjectId str_id = ObjectId.Parse(id);
            var book = collection.AsQueryable<Book>().SingleOrDefault(b => b._id == str_id);
            book.title = title;
            book.author = author;
            book.published_date = published_date;
            book.pages = pages;
            book.language = language;

            //collection.SaveAsync(book).Wait();
            collection.ReplaceOne(b => b._id == str_id, book);
        }

        // DELETE: api/Book/5
        public void Delete(String id)
        {
            ObjectId str_id = ObjectId.Parse(id);
            collection.DeleteOneAsync(book => book._id == str_id);
        }
    }
}
