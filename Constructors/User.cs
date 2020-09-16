using SQLite;

namespace Cashier_API.Constructors
{
    [Table("User")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int id {get;set;}

        [MaxLength(255)]
        public string displayName {get;set;}
        [MaxLength(255), Unique]
        public string username {get;set;}
        public string Hash { get; set; }
        public string Salt { get; set; }
        
        public bool isAdmin {get;set;}
    }
}