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
    public class ItemsController : ControllerBase 
    {
        [HttpPost("item")]
        public IActionResult NewItem([FromBody] Item item, [FromHeader] string token)
        {
            if (Logins.Verify(token,  true) != null)
            {
                if (!string.IsNullOrEmpty(item.description) && item.margin != 0 && item.price != 0)
                {
                    Program.db.Insert(item);
                    Program.db.Insert(new ItemStock() { id = item.id, stock = 0 });
                    return Ok(item);
                }
                else
                    return BadRequest("Item description, margin and price are required fields!");
            }
            else
                return Unauthorized();
        }

        [HttpGet("item/search")]
        public ActionResult<IEnumerable<Item>> SearchItem([FromBody] string query, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                // Prevent searching everything, else this would causes a major performance hit.
                if (string.IsNullOrEmpty(query))
                    return NotFound();

                List<Item> items = Program.db.Query<Item>($"SELECT * FROM Item WHERE barcode LIKE '%{query}%' OR description LIKE '%{query}%' OR supplier LIKE '%{query}%' OR category LIKE '%{query}%';");

                if (items.Count > 0)
                    return items;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpGet("item/{id}")]
        public ActionResult<IEnumerable<Item>> GetItem(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Item> items = Items.getItem(id);

                if (items.Count > 0)
                    return items;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpDelete("item/{id}")]
        public ActionResult DeleteItem(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token, true) != null)
            {
                List<Item> items = Program.db.Query<Item>($"SELECT * FROM Item WHERE id = '{id}';");

                if (items.Count > 0)
                {
                    List<ItemStock> itemsStocks = Program.db.Query<ItemStock>($"SELECT * FROM ItemStock WHERE id = '{id}';");

                    if (itemsStocks.Count > 0)
                    {
                        Program.db.Delete(itemsStocks.First());
                        Program.db.Delete(items.First());
                        return Ok();
                    }
                    else
                        return NotFound();
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPut("item/{id}")]
        public ActionResult UpdateItem(string id, [FromBody] Item update, [FromHeader] string token)
        {
            if (Logins.Verify(token, true) != null)
            {
                List<Item> items = Program.db.Query<Item>($"SELECT * FROM Item WHERE id = '{id}';");

                if (items.Count > 0)
                {
                    Item i = items.First();

                    if (update.price != 0)
                        i.price = update.price;

                    if (update.margin != 0)
                        i.margin = update.margin;

                    if (!string.IsNullOrEmpty(update.barcode))
                        i.barcode = update.barcode;

                    if (!string.IsNullOrEmpty(update.description))
                        i.description = update.description;

                    if (!string.IsNullOrEmpty(update.category))
                        i.category = update.category;

                    if (!string.IsNullOrEmpty(update.supplier))
                        i.supplier = update.supplier;

                    Program.db.Update(i);
                    return Ok(i);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
        
        [HttpPut("item/{id}/stock/{stock}")]
        public ActionResult UpdateItemStock(string id, int stock, [FromHeader] string token)
        {
            if (Logins.Verify(token, true) != null)
            {
                List<ItemStock> items = Program.db.Query<ItemStock>($"SELECT * FROM ItemStock WHERE id = '{id}';");

                if (items.Count > 0)
                {
                    ItemStock s = items.First();

                    s.stock = stock;

                    Program.db.Update(s);
                    return Ok(s);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
        
        [HttpGet("item/{count}/{offset}")]
        public ActionResult<IEnumerable<Item>> GetItemRange(int count, int offset, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Item> items = Program.db.Query<Item>($"SELECT * FROM Item LIMIT {count} OFFSET {offset};");

                if (items.Count > 0)
                    return items;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
    }

    public class Items 
    {
        public static List<Item> getItem(string barCodeOrId)
        {
            return Program.db.Query<Item>($"SELECT * FROM Item WHERE id = '{barCodeOrId}' OR barcode = '{barCodeOrId}';");
        }
    }
}