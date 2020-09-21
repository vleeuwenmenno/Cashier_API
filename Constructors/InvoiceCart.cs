using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using SQLite;

namespace Cashier_API.Constructors
{
    public class InvoiceCart
    {
        public int customerId {get;set;}
        public int userId {get;set;}
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

        /// <summary>
        /// These are the items for the invoice, this string gets parsed into a proper item list.
        /// </summary>
        /// <value></value>
        public string itemsBlob {get;set;}
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
        
        public decimal TotalPrice()
        {
            decimal d = 0m;
            foreach (ItemProperty i in items)
                d+=i.TotalPrice();
            return d;
        }

        public decimal TotalPriceExTax()
        {
            decimal d = 0m;
            foreach (ItemProperty i in items)
                d+=i.TotalPriceExTax();
            return d;
        }

        public decimal TotalTax()
        {
            return TotalPrice() - TotalPriceExTax();
        }

        public string PrettyTotalPrice()
        {
            CultureInfo l = new CultureInfo(Program.options.locale);
            return TotalPrice().ToString("c", l);
        }

        public string PrettyTotalPriceExTax()
        {
            CultureInfo l = new CultureInfo(Program.options.locale);
            return TotalPriceExTax().ToString("c", l);
        }

        public string PrettyTotalTax()
        {
            CultureInfo l = new CultureInfo(Program.options.locale);
            return TotalTax().ToString("c", l);
        }
    }
}