using MongoDB.Bson;
using System;


namespace MyLibrary
{
    public interface IIdntified
    {
        ObjectId _id { get; }
    }

    public class Book : IIdntified
    {
        public ObjectId _id { get; set; }
        public string title { get; set; }
        public string[] author { get; set; }
        public DateTime published_date { get; set; }
        public int pages { get; set; }
        public string language { get; set; }
        public Publisher publisher { get; set; }
    }

    public class Publisher
    {
        public string name { get; set; }
        public int founded { get; set; }
        public string location { get; set; }
    }
}
