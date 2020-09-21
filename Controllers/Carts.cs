using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cashier_API.Constructors;
using Microsoft.AspNetCore.Mvc;
using SQLite;

namespace Cashier_API.Controllers
{
    [ApiController]
    public class CartController : ControllerBase
    {
        [HttpPost("cart")] 
        public ActionResult NewCart([FromBody] Cart c, [FromHeader] string token)
        {
            LoginSession session = Logins.Verify(token);
            if (session != null)
            {
                c.id = Guid.NewGuid().ToString();
                Program.db.Insert(c);
                session.activeCartId = c.id;
                Program.db.Update(session);
                return Ok(c);
            }
            else
                return Unauthorized();
        }


        [HttpGet("cart/{cartId}/totals")]
        public ActionResult GetCartTotals(string cartId, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id=$1;", new object[] { cartId });
                if (carts.Count > 0)
                {
                    Cart cart = carts.Last();
                    return Ok((new Dictionary<string, object>() 
                    { 
                        { "totalPrice", cart.TotalPrice() }, 
                        { "totalPriceExTax", cart.TotalPriceExTax() },
                        { "totalTax", cart.TotalTax() },
                        { "prettyTotalPrice", cart.PrettyTotalPrice() },
                        { "prettyTotalPriceExVat", cart.PrettyTotalPriceExTax() },
                        { "prettyTotalTax", cart.PrettyTotalTax() },
                    }));
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPost("cart/{id}")]
        public ActionResult ProcessCartToInvoice(string id, [FromHeader] string token)
        {
            LoginSession sess = Logins.Verify(token);
            if (sess != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id=$1;", new object[] { id });
                Cart cart = carts.Count > 0 ? carts.Last() : null;

                // Check if the cart id actually exists
                if (cart == null)
                    return NotFound();

                Invoice invoice = new Invoice();

                invoice.items = cart.items;
                invoice.fields = cart.fields;
                invoice.notice = cart.notice;
                invoice.paymentMethod = cart.paymentMethod;
                invoice.processedAt = DateTime.Now;
                invoice.userId = cart.userId;
                invoice.customerId = cart.customerId;

                // If cart was not a template it means we should delete it once we processed it.
                if (cart.isTemplate == false)
                    Program.db.Delete(cart);

                // Clear the active cart in the session as we processed it.
                sess.activeCartId = "";

                Program.db.Update(sess);
                Program.db.Insert(invoice);
                return Ok(invoice);
            }
            else
                return Unauthorized();
        }

        [HttpPut("cart/{id}")]
        public ActionResult UpdateCart(string id, [FromBody] Cart update, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                Cart cart = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id=$1;", new object[] { id })[0];

                if (update.fields != null)
                    cart.fields = update.fields;

                if (update.isTemplate != null)
                    cart.isTemplate = update.isTemplate;

                if (update.notice != null)
                    cart.notice = update.notice;

                if (update.paymentMethod != 0)
                    cart.paymentMethod = update.paymentMethod;
                    
                cart.customerId = update.customerId;

                Program.db.Update(cart);
                return Ok(cart);
            }
            else
                return Unauthorized();
        }

        [HttpDelete("cart/{id}")]
        public ActionResult DeleteCart(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id=$1;", new object[] { id });
                Cart cart = carts.Count > 0 ? carts.Last() : null;

                // Check if the cart id actually exists
                if (cart == null)
                    return NotFound();

                Program.db.Delete(cart);
                return Ok();
            }
            else
                return Unauthorized();
        }

        [HttpGet("cart/{id}")]
        public ActionResult GetCart(string id, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id=$1;", new object[] { id });
                Cart cart = carts.Count > 0 ? carts.Last() : null;

                // Check if the cart id actually exists
                if (cart == null)
                    return NotFound();

                return Ok(cart);
            }
            else
                return Unauthorized();
        }
        
        [HttpGet("cart/{count}/{offset}")]
        public ActionResult<IEnumerable<Cart>> GetItemRange(int count, int offset, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart LIMIT $1 OFFSET $2;", new object[] { count, offset });

                if (carts.Count > 0)
                    return carts;
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPost("cart/{id}/items")]
        public ActionResult<IEnumerable<Cart>> UpdateCartItems(string id, [FromBody] string[] items, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id='{id}';");

                if (carts.Count > 0)
                {
                    Cart c = carts.First();
                    List<ItemProperty> cartItems = c.items;
                    
                    foreach (string item in items)
                    {
                        // check if the item is already in the cart
                        if (!cartItems.Any(c => c.id == Convert.ToInt32(item)))
                        {
                            //if not so get the item details and add them to the cart while also adding the correct amount
                            ItemProperty rawItem = Program.db.Query<ItemProperty>($"SELECT * FROM Item WHERE id='{item}';").First();

                            //if this is null it means we try adding an item that didn't exist
                            if (rawItem == null)
                                return NotFound($"Item with id {item} couldn't be found.");

                            //Create a new count property and add it to the cart items.
                            ItemProperty itemProp = rawItem;
                            itemProp.count = 1;
                            itemProp.multiplier = 1;

                            cartItems.Add(itemProp);
                        }
                    }

                    c.itemsBlob = JsonSerializer.Serialize(cartItems);

                    Program.db.Update(c);
                    return Ok(c);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpDelete("cart/{id}/items")]
        public ActionResult<IEnumerable<Cart>> DeleteItemsCart(string id, [FromBody] string[] items, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id='{id}';");

                if (carts.Count > 0)
                {
                    Cart c = carts.First();
                    List<ItemProperty> cartItems = c.items;
                    
                    foreach (string item in items)
                    {
                        // check if the item is already in the cart
                        if (cartItems.Any(c => c.id == Convert.ToInt32(item))) 
                        { 
                            cartItems.Remove(cartItems.FirstOrDefault(c => c.id == Convert.ToInt32(item)));
                        }
                    }

                    c.itemsBlob = JsonSerializer.Serialize(cartItems);

                    Program.db.Update(c);
                    return Ok(c);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPut("cart/{id}/items/count")]
        public ActionResult<IEnumerable<Cart>> UpdateCartItemsCount(string id, [FromBody] Dictionary<string, int> items, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id='{id}';");

                if (carts.Count > 0)
                {
                    Cart c = carts.First();
                    List<ItemProperty> cartItems = c.items;
                    
                    foreach (KeyValuePair<string, int> item in items)
                    {
                        // check if the item is already in the cart
                        if (cartItems.Any(c => c.id == Convert.ToInt32(item.Key))) 
                        { 
                            //if so just add/remove the amount
                            cartItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).count += item.Value;

                            //if we have 0 or less added at the moment we will remove it from the cart as we can't sell negatives in carts
                            if (cartItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).count <= 0)
                                cartItems.Remove(cartItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)));
                        }
                        else
                            return NotFound($"Cart does not contain item with id {item.Key}");
                    }

                    c.itemsBlob = JsonSerializer.Serialize(cartItems);

                    Program.db.Update(c);
                    return Ok(c);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }

        [HttpPut("cart/{id}/items/multiplier")]
        public ActionResult<IEnumerable<Cart>> UpdateCartItemsMultipliers(string id, [FromBody] Dictionary<string, int> items, [FromHeader] string token)
        {
            if (Logins.Verify(token) != null)
            {
                List<Cart> carts = Program.db.Query<Cart>($"SELECT * FROM Cart WHERE id='{id}';");

                if (carts.Count > 0)
                {
                    Cart c = carts.First();
                    List<ItemProperty> cartItems = c.items;
                    
                    foreach (KeyValuePair<string, int> item in items)
                    {
                        // check if the item is already in the cart
                        if (cartItems.Any(c => c.id == Convert.ToInt32(item.Key))) 
                        { 
                            //if so just add/remove the amount
                            cartItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).multiplier += item.Value;

                            //If the multiplier is 0 or below we will change it back to 1
                            if (cartItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).multiplier <= 0)
                                cartItems.FirstOrDefault(c => c.id == Convert.ToInt32(item.Key)).multiplier = 1;
                        }
                        else
                            return NotFound($"Cart does not contain item with id {item.Key}");
                    }

                    c.itemsBlob = JsonSerializer.Serialize(cartItems);

                    Program.db.Update(c);
                    return Ok(c);
                }
                else
                    return NotFound();
            }
            else
                return Unauthorized();
        }
    }
}