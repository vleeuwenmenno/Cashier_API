using System.Collections.Generic;
using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Item")]
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int id {get;set;}

        public string barcode {get;set;}
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
        public decimal priceExVat 
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
    }
}