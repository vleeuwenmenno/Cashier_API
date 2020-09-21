using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using SQLite;

namespace Cashier_API.Constructors
{
    [Table("Invoice")]
    public class Invoice : InvoiceCart
    {
        [AutoIncrement, PrimaryKey]
        public int id {get;set;}
        public int contractId {get;set;}

        public DateTime processedAt {get;set;}
    }
}