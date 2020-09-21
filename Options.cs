using System;
using System.IO;
using System.Text.Json;
using SQLite;

namespace Cashier_API
{
    public struct Options
    {
        #region SMTP
        public string smtpHost {get;set;}
        public string smtpUser {get;set;}
        public string smtpPassword {get;set;}
        public string smtpSecurity {get;set;}
        public Int16 smtpPort {get;set;}
        public string[] bccEmails {get;set;}
        #endregion

        #region Financial
        public decimal tax {get;set;}
        public string taxString {get;set;}
        public char currencySymbol {get;set;}
        public string currency {get;set;}
        #endregion

        #region Company
        public string companyName {get;set;}
        public string companyLogo {get;set;}
        public string companyLogoSquare {get;set;}
        #endregion

        internal static string confLocation = Environment.CurrentDirectory +  "/config";

        public void LoadOptions()
        {
            if (!File.Exists($"{confLocation}/config.json"))
            {
                Directory.CreateDirectory($"{confLocation}");
                File.WriteAllText($"{confLocation}/config.json", JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true }));
            }
            else
            {
                this = JsonSerializer.Deserialize<Options>(File.ReadAllText($"{confLocation}/config.json"));
            }
        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
        }

        public void SaveOptions()
        {
            File.WriteAllText($"{confLocation}/config.json", JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}