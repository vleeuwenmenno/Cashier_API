using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cashier_API.Constructors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLite;

namespace Cashier_API
{
    public class Program
    {
        public static SQLiteConnection db;
        
        public static void Main(string[] args)
        {
            // Check if we have a database
            db = new SQLiteConnection(Environment.CurrentDirectory + "/database.sqlite");
            
            db.CreateTable<User>();
            db.CreateTable<LoginSession>();
            db.CreateTable<Item>();
            db.CreateTable<ItemStock>();
            db.CreateTable<Customer>();
            db.CreateTable<Contract>();
            db.CreateTable<Invoice>();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
