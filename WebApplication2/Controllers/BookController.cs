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
        private MongoClient client = DBController.getDBClient();
        private IMongoDatabase db;
        private IMongoCollection<Book> collection;

        private BookController()
        {
            db = client.GetDatabase("BookStore");
            collection = db.GetCollection<Book>("BestBook");
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
            var obj = new Book();
            obj.title = "sdafdsaf";

            var a = Request.RequestUri.Query;

            Book book = new Book();
            book.title = title;

            collection.InsertOne(book);
        }
        public void Post()
        {
            var a = Request.RequestUri.Query;
        }

        // PUT: api/Book/5
        public void Put(String id, [FromBody]string value)
        {
            ObjectId str_id = ObjectId.Parse(id);
            var book = collection.AsQueryable<Book>().SingleOrDefault(b => b._id == str_id);
            book.title += " New";
            //collection.SaveAsync(book).Wait();
            collection.ReplaceOne(b => b._id == str_id, book);
        }

        // DELETE: api/Book/5
        public void Delete(String id)
        {
            ObjectId str_id = ObjectId.Parse(id);
            collection.DeleteOneAsync(a => a._id == str_id);
        }

        public void Put([FromUri] String value)
        {

        }
    }
}
