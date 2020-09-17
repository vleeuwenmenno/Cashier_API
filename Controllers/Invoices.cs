using System;
using System.Collections.Generic;
using System.Linq;
using Cashier_API.Constructors;
using Microsoft.AspNetCore.Mvc;

namespace Cashier_API.Controllers
{
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        [HttpDelete("invoice/{id}")]
        public IActionResult DeleteInvoice(int id,[FromHeader] string token)
        {
            if (Logins.Verify(token, true))
            {
                List<Invoice> invoices = Program.db.Query<Invoice>($"SELECT * FROM Invoice WHERE id='{id}';");

                if (invoices.Count == 0)
                    return NotFound();  

                Invoice invoice = invoices.First();

                // If it is processed we need to check if it's older than 7 years to conform with regulations.
                if (invoice.processedAt.AddYears(7) < DateTime.Now)
                {
                    Program.db.Delete(invoice);
                    return Ok();
                }
                else
                    return BadRequest("Record cannot be deleted as it's younger than 7 years. To conform with regulations you are only allowed to delete an invoice after 7 years of the processing date.");
            
            }
            else
                return Unauthorized();
        }
        
        [HttpGet("invoice/{count}/{offset}")]
        public ActionResult<IEnumerable<Invoice>> GetInvoiceRange(int count, int offset, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                List<Invoice> invoices = Program.db.Query<Invoice>($"SELECT * FROM Invoice LIMIT {count} OFFSET {offset};");

                if (invoices.Count > 0)
                    return invoices;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
        
        
        [HttpGet("invoice/{customerId}")]
        public ActionResult<IEnumerable<Invoice>> GetInvoiceRange(int customerId, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                List<Invoice> invoices = Program.db.Query<Invoice>($"SELECT * FROM Invoice WHERE customerId={customerId};");

                if (invoices.Count > 0)
                    return invoices;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
    }
}