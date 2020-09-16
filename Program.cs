using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

            // Check if there is atleast 1 user
            if (db.Query<User>("SELECT * FROM User WHERE 1;").Count == 0)
            {
                // Seems like there are no users, let's make a default admin/admin
                const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string pass = new string(Enumerable.Repeat(chars, 8).Select(s => s[new Random().Next(s.Length)]).ToArray());
                User u = new User();
                HashSalt hs = LoginCrypto.GenerateSaltedHash(64, pass);

                u.displayName = "Admin";
                u.username = "admin";
                u.Hash = hs.Hash;
                u.Salt = hs.Salt;
                u.isAdmin = true;

                db.Insert(u);

                Console.Write("\nWARNING! No user was found! Initial admin user created, you can now login with the following credentials: \n\n\tUsername: ");
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("admin");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\tPassword: ");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(pass);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n\t!!! It's recommended to change this ASAP !!!\n\n");
                Console.ForegroundColor = ConsoleColor.White;

                Thread.Sleep(500);
            }

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
