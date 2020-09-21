using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cashier_API.Constructors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cashier_API.Controllers
{
    [ApiController]
    public class ContractController : ControllerBase 
    {
        [HttpPost("contract")]
        public IActionResult NewContract([FromBody] Contract contract, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                if (contract.runDay >= 1  && contract.runDay <= 27 && contract.customerId > 0)
                {
                    contract.enabled = true;

                    if (Program.db.Query<Customer>($"SELECT * FROM Customer WHERE id='{contract.customerId}';").Count == 0)
                    {
                        return BadRequest($"Customer with id {contract.customerId} does not exist!");
                    }

                    if (contract.start.Year == 1)
                        contract.start = DateTime.Now;

                    Program.db.Insert(contract);
                    return Ok(contract);
                }
                else
                    return BadRequest("Customer initials, family name and email are required fields!");
            }
            else
                return Unauthorized();
        }

        [HttpGet("contract/{id}")]
        public ActionResult<IEnumerable<Contract>> GetContract(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Contract> contracts = Program.db.Query<Contract>($"SELECT * FROM Contract WHERE id='{id}';");

                if (contracts.Count > 0)
                    return contracts;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpGet("contract/search")]
        public ActionResult<IEnumerable<Contract>> SearchContract([FromBody] string query, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                // Prevent searching everything, else this would causes a major performance hit.
                if (string.IsNullOrEmpty(query))
                    return NotFound();

                List<Contract> contracts = Program.db.Query<Contract>($"SELECT con.* FROM Contract as con, Customer as cust WHERE cust.familyName LIKE '%{query}%' OR cust.initials LIKE '%{query}%' OR cust.company LIKE '%{query}%';");

                if (contracts.Count > 0)
                    return contracts;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpDelete("contract/{id}")]
        public ActionResult ArchiveContract(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Contract> contracts = Program.db.Query<Contract>($"SELECT * FROM Contract WHERE id='{id}';");
                
                if (contracts.Count > 0)
                {
                    Contract c = contracts.First();

                    if (c.enabled == false && c.archived == true)
                    {
                        Program.db.Delete(c);
                        return Ok();
                    }
                    else
                    {
                        c.archived = true; 
                        c.enabled = false; 
                        Program.db.Update(c);
                        return Accepted("archived");
                    }
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPut("contract/{id}")]
        public ActionResult<IEnumerable<Contract>> UpdateContract(string id, [FromBody] Contract update, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Contract> contract = Program.db.Query<Contract>($"SELECT * FROM Contract WHERE id = '{id}';");

                if (contract.Count > 0)
                {
                    Contract i = contract.First();

                    if (update.customerId != 0)
                        if (Program.db.Query<Customer>($"SELECT * FROM Customer WHERE id='{update.customerId}';").Count == 0)
                        {
                            return BadRequest($"Customer with id {update.customerId} does not exist!");
                        }

                    if (update.runDay != 0 && update.runDay >= 1  && update.runDay <= 27)
                        i.runDay = update.runDay;

                    if (update.customerId != 0)
                        i.customerId = update.customerId;

                    if (update.period != 0)
                        i.period = update.period;

                    if (update.paymentMethod != 0)
                        i.paymentMethod = update.paymentMethod;

                    if (update.enabled != null)
                        i.enabled = update.enabled;

                    Program.db.Update(i);
                    return Ok(i);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPost("contract/{id}/items")]
        public ActionResult<IEnumerable<Contract>> UpdateContractItems(int id, [FromBody] string[] items, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Contract> contracts = Program.db.Query<Contract>($"SELECT * FROM Contract WHERE id='{id}';");

                if (contracts.Count > 0)
                {
                    Contract c = contracts.First();
                    List<ItemProperty> contractItems = c.items;
                    
                    foreach (string item in items)
                    {
                        // check if the item is already in the contract
                        if (!contractItems.Any(c => c.id == Convert.ToInt32(item)))
                        {
                            //if not so get the item details and add them to the contract while also adding the correct amount
                            ItemProperty rawItem = Program.db.Query<ItemProperty>($"SELECT * FROM Item WHERE id='{item}';").First();

                            //if this is null it means we try adding an item that didn't exist
                            if (rawItem == null)
                                return NotFound($"Item with id {item} couldn't be found.");

                            //Create a new count property and add it to the contract items.
                            ItemProperty itemProp = rawItem;
                            itemProp.count = 1;
                            itemProp.multiplier = 1;

                            contractItems.Add(itemProp);
                        }
                    }

                    c.itemsBlob = JsonSerializer.Serialize(contractItems);

                    Program.db.Update(c);
                    return Ok(c);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpDelete("contract/{id}/items")]
        public ActionResult<IEnumerable<Contract>> DeleteItemsContract(int id, [FromBody] string[] items, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Contract> contracts = Program.db.Query<Contract>($"SELECT * FROM Contract WHERE id='{id}';");

                if (contracts.Count > 0)
                {
                    Contract c = contracts.First();
                    List<ItemProperty> contractItems = c.items;
                    
                    foreach (string item in items)
                    {
                        // check if the item is already in the contract
                        if (contractItems.Any(c => c.id == Convert.ToInt32(item))) 
                        { 
                            contractItems.Remove(contractItems.FirstOrDefault(c => c.id == Convert.ToInt32(item)));
                        }
                    }

                    c.itemsBlob = JsonSerializer.Serialize(contractItems);

                    Program.db.Update(c);
                    return Ok(c);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPut("contract/{id}/items/count")]
        public ActionResult<IEnumerable<Contract>> UpdateContractItemsCount(int id, [FromBody] Dictionary<string, int> items, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Contract> contracts = Program.db.Query<Contract>($"SELECT * FROM Contract WHERE id='{id}';");

                if (contracts.Count > 0)
                {
                    Contract c = contracts.First();
                    List<ItemProperty> contractItems = c.items;
                    
                    foreach (KeyValuePair<string, int> item in items)
                    {
                        // check if the item is already in the contract
                        if (contractItems.Any(c => c.id == Convert.ToInt32(item.Key))) 
                        { 
                            //if so just add/remove the amount
                            contractItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).count += item.Value;

                            //if we have 0 or less added at the moment we will remove it from the contract as we can't sell negatives in contracts
                            if (contractItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).count <= 0)
                                contractItems.Remove(contractItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)));
                        }
                        else
                            return NotFound($"Contract does not contain item with id {item.Key}");
                    }

                    c.itemsBlob = JsonSerializer.Serialize(contractItems);

                    Program.db.Update(c);
                    return Ok(c);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPut("contract/{id}/items/multiplier")]
        public ActionResult<IEnumerable<Contract>> UpdateContractItemsMultipliers(int id, [FromBody] Dictionary<string, int> items, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Contract> contracts = Program.db.Query<Contract>($"SELECT * FROM Contract WHERE id='{id}';");

                if (contracts.Count > 0)
                {
                    Contract c = contracts.First();
                    List<ItemProperty> contractItems = c.items;
                    
                    foreach (KeyValuePair<string, int> item in items)
                    {
                        // check if the item is already in the contract
                        if (contractItems.Any(c => c.id == Convert.ToInt32(item.Key))) 
                        { 
                            //if so just add/remove the amount
                            contractItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).multiplier += item.Value;

                            //If the multiplier is 0 or below we will change it back to 1
                            if (contractItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).multiplier <= 0)
                                contractItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).multiplier = 1;
                        }
                        else
                            return NotFound($"Contract does not contain item with id {item.Key}");
                    }

                    c.itemsBlob = JsonSerializer.Serialize(contractItems);

                    Program.db.Update(c);
                    return Ok(c);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpGet("contract/{offset}/{count}")]
        public ActionResult<IEnumerable<Contract>> GetContract(int offset, int count, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Contract> contracts = Program.db.Query<Contract>($"SELECT * FROM Contract LIMIT {count} OFFSET {offset};");

                if (contracts.Count > 0)
                    return contracts;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
    }
}