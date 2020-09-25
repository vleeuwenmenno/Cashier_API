using SQLite;

namespace Cashier_API.Constructors
{
    [Table("CashierSession")]
    public class CashierSession
    {
        [AutoIncrement, PrimaryKey]
        public int id {get;set;}
        public int userId {get;set;}
        public int cashierId {get;set;}

        public decimal cashIn {get;set;}
        public decimal cashOut {get;set;}
    }
}