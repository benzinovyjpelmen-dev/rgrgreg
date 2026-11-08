using System;

namespace CultureDocs.Models
{
    public class Version
    {
        public int VersionID { get; set; }
        public int DocumentID { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public int AuthorID { get; set; }
        public string Comment { get; set; }
        public string FilePath { get; set; }

        public string AuthorName { get; set; }
    }
}