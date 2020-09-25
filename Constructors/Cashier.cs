using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Cashier")]
    public class Cashier
    {
        [PrimaryKey, AutoIncrement]
        public int id {get;set;}
        public int session {get;set;}

        public int[] allowedUsers {get;set;}
        public string name {get;set;}
    }
}