using System;
using SQLite;

namespace Cashier_API.Constructors
{
    [Table("LoginSession")]
    public class LoginSession
    {
        [PrimaryKey]
        public string id {get;set;}
        public int userId {get;set;}
        public DateTime expiry {get;set;}
    }
}