using System;
using System.Collections.Generic;
using System.Text.Json;
using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Invoice")]
    public class Invoice
    {
        [AutoIncrement, PrimaryKey]
        public int id {get;set;}
        public int customerId {get;set;}
        public int userId {get;set;}

        public string itemsBlob {get;set;}
        public string notice {get;set;}

        public DateTime processedAt {get;set;}
        public PaymentMethod paymentMethod {get;set;}

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
    }
}