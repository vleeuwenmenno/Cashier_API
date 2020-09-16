using Cashier_API.Constructors;
using Microsoft.AspNetCore.Mvc;

namespace Cashier_API.Controllers
{
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        [HttpPost("item")]
        public IActionResult NewItem([FromBody] Invoice item, [FromHeader] string token)
        {
            if (Logins.Verify(token))
            {
                if (item.)
                {

                    return Ok(item);
                }
                else
                    return BadRequest("Item description, margin and price are required fields!");
            }
            else
                return Unauthorized();
        }
        
    }
}