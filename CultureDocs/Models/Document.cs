using System;

namespace CultureDocs.Models
{
    public class Document
    {
        public int DocumentID { get; set; }
        public string Title { get; set; }
        public string DocType { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public int AuthorID { get; set; }
        public int? EventID { get; set; }
        public int? CategoryID { get; set; }

        // Для отображения в таблицах
        public string AuthorName { get; set; }
        public string EventName { get; set; }
        public string CategoryName { get; set; }
    }
}