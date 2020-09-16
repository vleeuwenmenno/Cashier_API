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
    public class LoginController : ControllerBase 
    { 
        [HttpPost("login")] 
        public IActionResult login([FromBody] string[] login)
        {
            // Get user with given username
            List<User> users = Program.db.Query<User>($"SELECT * FROM User WHERE username = '{login[0]}';");

            // Check if we found any user with the query
            if (users.Count > 0)
            {
                User u = users[0];

                if (u == null)
                    return Unauthorized();

                // Check if the password matches the one stored as hashes and salts in the database
                if (LoginCrypto.VerifyPassword(login[1], u.Hash, u.Salt))
                {
                    // Password matched! Create a new session for the user to use.
                    LoginSession session = new LoginSession();

                    session.id = Guid.NewGuid().ToString();
                    session.expiry = DateTime.Now.AddHours(1);
                    session.userId = u.id;

                    // Store the new session in the database and return it to the requester.
                    Program.db.Insert(session);
                    return Ok(session);
                }

                // Password did not match!
                return Unauthorized();
            }
            else
                return NotFound();
        }

        [HttpPost("logout")]
        public IActionResult logout([FromHeader] string token)
        {
            // Find session by id, if we find any put it in the variable
            List<LoginSession> v = Program.db.Query<LoginSession>($"SELECT * FROM LoginSession WHERE id = '{token}';");
            LoginSession u = v.Count > 0 ? v.First() : null;

            // If null it means we didn't find any session, so its unauthorized.
            if (u == null)
                return Unauthorized();
            
            // Authorized, delete the session and return ok.
            Program.db.Delete(u);            
            return Ok();
        }
    }

    public class Logins
    {
        public static bool Verify(string token, bool mustBeAdmin = false)
        {
            List<LoginSession> v = Program.db.Query<LoginSession>($"SELECT * FROM LoginSession WHERE id = '{token}';");
            LoginSession u = v.Count > 0 ? v.First() : null;

            //Check if we found a session matching our token.
            if (u == null)
                return false;
            
            //Check if the session is expired
            if (u.expiry < DateTime.Now)
            {
                //Session is not valid! We will delete it from the database.
                Program.db.Delete(u);
                return false;
            }
            
            // If we must check if its admin we run the following code
            if (mustBeAdmin)
            {
                // Get the user that owns this session
                List<User> users = Program.db.Query<User>($"SELECT * FROM User WHERE id='{u.userId}';");

                if (users.Count > 0)
                    return users.First().isAdmin;
                
                // User does not exist, but there is a session for a user that doesn't exist!? Deleting the session ...
                Program.db.Delete(u);
                return false;
            }       
            else
                return true;     
        }
    }
}
