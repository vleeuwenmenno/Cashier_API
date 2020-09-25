using System.Collections.Generic;
using System.Globalization;
using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Item")]
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int id {get;set;}

        public string barcode {get;set;}
        public string image {get;set;}
        public string description {get;set;}
        public string supplier {get;set;}
        public string category {get;set;}

        /// <summary>
        /// Margin indicates how much profit is being made of this item
        /// </summary>
        /// <value></value>
        public decimal margin {get;set;} 
        
        /// <summary>
        /// This is the item price including any VAT applied
        /// </summary>
        /// <value></value>
        public decimal price {get;set;}

        /// <summary>
        /// This is a calculated value based on the price of the item, it indicates the price exluding any VAT.
        /// </summary>
        /// <value></value>
        public decimal priceExTax 
        {
            get
            {
                return price / 1.21m; //TODO Make a JSON or something to store these kind of options!
            }
        }
        
        [Ignore]
        public int stock 
        {
            get
            {
                List<ItemStock> items = Program.db.Query<ItemStock>($"SELECT stock FROM ItemStock WHERE id = '{id}';");

                if (items.Count > 0)
                    return items[0].stock;
                else
                    return 0;
            }
        }
    }

    [Table("ItemStock")]
    public class ItemStock
    {
        [PrimaryKey]
        public int id {get;set;}
        public int stock {get;set;}
    }

    public class ItemProperty : Item
    {
        public int count {get;set;}
        public int multiplier {get;set;}

        public decimal TotalPrice()
        {
            return (this.price * this.count) * this.multiplier;
        }

        public decimal TotalPriceExTax()
        {
            return (this.priceExTax * this.count) * this.multiplier;
        }

        public string PrettyPrice()
        {
            CultureInfo l = new CultureInfo(Program.options.locale);
            return price.ToString("c", l);
        }

        public string PrettyPriceExTax()
        {
            CultureInfo l = new CultureInfo(Program.options.locale);
            return priceExTax.ToString("c", l);
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
    }
}