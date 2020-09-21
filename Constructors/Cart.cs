using System;
using System.Collections.Generic;
using System.Text.Json;
using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Cart")]
    public class Cart
    {
        [AutoIncrement, PrimaryKey]
        public int id {get;set;}
        public int customerId {get;set;}
        public int userId {get;set;}

        public string itemsBlob {get;set;}
        public string notice {get;set;}

        public PaymentMethod paymentMethod {get;set;}

        /// <summary>
        /// These fields will be displayed evenly over the invoice PDF, they are added to accomodate space for specialized uses.
        /// 
        /// For example:
        ///     A hotel might want to add roomNumber, arrival and departure fields to the invoice.
        ///     A garage might want to add the car registration or number plate to the invoice.
        /// </summary>
        /// <value></value>
        public string fieldsBolb {get;set;}
        [Ignore]
        public Dictionary<string, string> fields 
        {
            get
            {
                return !string.IsNullOrEmpty(fieldsBolb) ? JsonSerializer.Deserialize<Dictionary<string, string>>(fieldsBolb) : new Dictionary<string, string>();
            }
            set
            {
                fieldsBolb = JsonSerializer.Serialize(value);
            }
        }

        /// <summary>
        /// If this is true, the cart will not be destroyed upon executing the processing into an invoice. This can be useful if the same type of items are often used on an invoice.
        /// </summary>
        /// <value></value>
        public bool? isTemplate {get;set;}

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