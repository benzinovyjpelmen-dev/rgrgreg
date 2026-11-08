namespace CultureDocs.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public int RoleID { get; set; }
        public int? AuthorID { get; set; }
        public string RoleName { get; set; }
        public string AuthorName { get; set; }
    }
}