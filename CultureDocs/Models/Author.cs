namespace CultureDocs.Models
{
    public class Author
    {
        public int AuthorID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Position { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string FullName
        {
            get { return (LastName + " " + FirstName + " " + MiddleName).Trim(); }
        }
    }
}