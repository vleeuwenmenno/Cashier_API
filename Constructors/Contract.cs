using System;
using System.Collections.Generic;
using System.Text.Json;
using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Contract")]
    public class Contract
    {
        [PrimaryKey, AutoIncrement]
        public int id {get;set;}
        public int runDay {get;set;}
        public int customerId {get;set;}

        public bool? enabled {get;set;}
        public bool? archived {get;set;}

        public Period period {get;set;}

        public DateTime start {get;set;}

        public string itemsBlob {get;set;}

        [Ignore]
        public List<ItemProperty> items 
        {
            get
            {
                return !string.IsNullOrEmpty(itemsBlob) ? JsonSerializer.Deserialize<List<ItemProperty>>(itemsBlob) : new List<ItemProperty>();
            }
            set
            {
                itemsBlob = JsonSerializer.Serialize(value);
            }
        }

        public PaymentMethod paymentMethod {get;set;}
    }

    public enum Period
    {
        NotSet = 0,
        Monthly = 1,
        Quarterly = 2,
        Yearly = 3
    }

    public enum PaymentMethod
    {
        NotSet = 0,
        Cash = 1,
        Card = 2,
        BankTransfer = 3
    }
}