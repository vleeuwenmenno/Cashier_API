using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashier_API.Constructors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cashier_API.Controllers
{
    [ApiController]
    public class CustomerController : ControllerBase 
    {
        [HttpPost("customer")]
        public IActionResult NewCustomer([FromBody] Customer customer, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                if (!string.IsNullOrEmpty(customer.initials) && !string.IsNullOrEmpty(customer.familyName) && !string.IsNullOrEmpty(customer.email))
                {
                    if (Utilities.IsValidEmail(customer.email))
                    {
                        Program.db.Insert(customer);
                        return Ok(customer);
                    }
                    else
                        return BadRequest($"Malformed email address. ({customer.email})");
                }
                else
                    return BadRequest("Customer initials, family name and email are required fields!");
            }
            else
                return Unauthorized();
        }

        [HttpGet("customer/search")]
        public ActionResult<IEnumerable<Customer>> SearchCustomer([FromBody] string query, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                // Prevent searching everything, else this would causes a major performance hit.
                if (string.IsNullOrEmpty(query))
                    return NotFound();

                List<Customer> customers = Program.db.Query<Customer>($"SELECT * FROM Customer WHERE initials LIKE '%{query}%' OR familyName LIKE '%{query}%' OR email LIKE '%{query}%' OR phone LIKE '%{query}%' OR address LIKE '%{query}%' OR postalCode LIKE '%{query}%';");

                if (customers.Count > 0)
                    return customers;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpGet("customer/{id}")]
        public ActionResult<IEnumerable<Customer>> GetItem(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                List<Customer> customers = Program.db.Query<Customer>($"SELECT * FROM Customer WHERE id='{id}';");

                if (customers.Count > 0)
                    return customers;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpGet("customer/{id}/contracts")]
        public ActionResult<IEnumerable<Contract>> GetAllCustomerContracts(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                List<Contract> contracts = Program.db.Query<Contract>($"SELECT * FROM Contract WHERE customerId='{id}';");

                if (contracts.Count > 0)
                    return contracts;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpDelete("customer/{id}")]
        public ActionResult DeleteCustomer(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                List<Customer> customers = Program.db.Query<Customer>($"SELECT * FROM Customer WHERE id='{id}';");

                if (customers.Count > 0)
                {
                    List<Contract> contracts = Program.db.Query<Contract>($"SELECT id FROM Contract WHERE customerId='{customers.First().id}';");

                    if (contracts.Count > 0)
                        return BadRequest("Cannot remove customer, customer still has contracts assigned to it!");

                    Program.db.Delete(customers.First());
                    return Ok();
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPut("customer/{id}")]
        public ActionResult<IEnumerable<Customer>> UpdateCustomer(string id, [FromBody] Customer update, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                List<Customer> customer = Program.db.Query<Customer>($"SELECT * FROM Customer WHERE id = '{id}';");

                if (customer.Count > 0)
                {
                    Customer i = customer.First();

                    if (!string.IsNullOrEmpty(update.initials))
                        i.initials = update.initials;

                    if (!string.IsNullOrEmpty(update.familyName))
                        i.familyName = update.familyName;

                    if (!string.IsNullOrEmpty(update.email))
                        i.email = update.email;

                    if (!string.IsNullOrEmpty(update.phone))
                        i.phone = update.phone;

                    if (!string.IsNullOrEmpty(update.address))
                        i.address = update.address;

                    if (!string.IsNullOrEmpty(update.postalCode))
                        i.postalCode = update.postalCode;

                    if (!string.IsNullOrEmpty(update.country))
                        i.country = update.country;

                    if (!string.IsNullOrEmpty(update.company))
                        i.company = update.company;

                    Program.db.Update(i);
                    return Ok(i);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
        
        
        [HttpGet("customer/{offset}/{count}")]
        public ActionResult<IEnumerable<Customer>> GetCustomerRange(int offset, int count, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                List<Customer> customers = Program.db.Query<Customer>($"SELECT * FROM Customer LIMIT {count} OFFSET {offset};");

                if (customers.Count > 0)
                    return customers;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
    }
}