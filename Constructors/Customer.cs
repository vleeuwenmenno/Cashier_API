using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Customer")]
    public class Customer
    {
        [PrimaryKey, AutoIncrement]
        public int id {get;set;}

        public string initials {get;set;}
        public string familyName {get;set;}
        public string email {get;set;}
        public string phone {get;set;}
        public string country {get;set;}
        public string address {get;set;}
        public string postalCode {get;set;}
        public string company {get;set;}
    }
}