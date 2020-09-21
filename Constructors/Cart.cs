using System;
using System.Collections.Generic;
using System.Text.Json;
using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Cart")]
    public class Cart : InvoiceCart
    {
        [PrimaryKey]
        public string id {get;set;}

        /// <summary>
        /// If this is true, the cart will not be destroyed upon executing the processing into an invoice. This can be useful if the same type of items are often used on an invoice.
        /// </summary>
        /// <value></value>
        public bool? isTemplate {get;set;}
    }
}