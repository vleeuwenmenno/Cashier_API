using System.Collections.Generic;
using System.Linq;
using Cashier_API.Constructors;
using Microsoft.AspNetCore.Mvc;
using SQLite;

namespace Cashier_API.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpPost("users")] 
        public ActionResult<IEnumerable<User>> NewUser([FromBody] User user, [FromHeader] string token)
        {
            // Check if user login
            if (Logins.Verify(token, true))
            {
                HashSalt hs = Utilities.GenerateSaltedHash(64, user.Hash);
                    
                user.Hash = hs.Hash;
                user.Salt = hs.Salt;
                
                try 
                {
                    Program.db.Insert(user);

                    user.Hash = "Hidden";
                    user.Salt = "Hidden";

                    return Ok(user);
                }
                catch (SQLiteException sqlex)
                {
                    if (sqlex.Message.StartsWith("UNIQUE constraint failed:"))
                        return BadRequest("A user with this username already exists!");
                    else
                        return BadRequest("Unexpected error: " + sqlex);
                }
            }
            else 
                return Unauthorized();
        }

        [HttpGet("users")] 
        public ActionResult<IEnumerable<User>> GetAll([FromHeader] string token)
        {
            // Check if user login
            if (Logins.Verify(token, true))
            {
                return Program.db.Query<User>("SELECT id, displayName, username, isAdmin FROM User WHERE 1;");
            }
            else 
                return Unauthorized();
        }

        [HttpDelete("users/{id}")]
        public ActionResult DeleteUser(int id, [FromHeader] string token)
        {
            // Check if user login
            if (Logins.Verify(token, true))
            {
                // Get the user to delete
                User u = Users.getUserById(id);

                // Make sure the user has been found
                if (u == null)
                    return NotFound();

                List<LoginSession> v = Program.db.Query<LoginSession>($"SELECT * FROM LoginSession WHERE id = '{token}';");
                LoginSession uSession = v.Count > 0 ? v.First() : null;

                if (u.id == uSession.userId)
                    return BadRequest("Cannot delete yourself.");

                Program.db.Delete(u);
                return Ok();
            }
            else 
                return Unauthorized();
        }

        [HttpPut("users/{id}")]
        public ActionResult UpdateUser(int id, [FromBody] User raw, [FromHeader] string token)
        {
            // Check if user login
            if (Logins.Verify(token, true))
            {
                // Get the user to delete
                User u = Users.getUserById(id);

                // Make sure the user has been found
                if (u == null)
                    return NotFound();

                // Update values of user if they are not null
                if (raw.displayName != null)
                    u.displayName = raw.displayName;

                if (raw.username != null)
                    u.username = raw.username;

                if (raw.isAdmin != null)
                    u.isAdmin = raw.isAdmin;

                if (raw.Hash != null)
                {
                    // Update the password
                    HashSalt hs = Utilities.GenerateSaltedHash(64, raw.Hash);

                    u.Hash = hs.Hash;
                    u.Salt = hs.Salt;
                }

                Program.db.Update(u);
                return Ok();
            }
            else 
                return Unauthorized();
        }
    }

    public class Users 
    {
        public static User getUserById(int id)
        {
            // Get user from id
            List<User> users = Program.db.Query<User>($"SELECT * FROM User WHERE id='{id}';");
            return users.Count > 0 ? users.First() : null;
        }
    }
}

///TEST USER:
// INSERT INTO "main"."User" ("id", "displayName", "username", "Hash", "Salt", "isAdmin") VALUES ('1', 'Menno van Leeuwen', 'menno', '4Swrw0N8a21fPmf0SoUGAh71s9Va63m1PeQQFT2XShjyWoQ2uIikqt2222Sn0Du1s411sSwINIO+UQ4E8HuWJY/6SSqZrLHhEi3pb+psWUnl9mkMN0v9HR6c/om73f/p00Lm6/0VHbX6PPDFlu+5pmCPx1HGEdg+ONYLB3IJ8mGZVbn2YIOuLfMJ3bAEGv6DLB01wjeacxEMQbFCbSig7+v4mRR4B1XIXen5e0fHr1eChNIiYoUxOWrDufeGtZppyxojkFogcUrKTU+/V5ZHPBgQQsCNLxlIDA4ad1NZ/ZTKkK1Kdz9H4wrd9+zjstoxXhZmij8Ijl7Uaz5BLwxINA==', 'tAS1Ky3P8iM+rR4QYYFdglE0T+PxH7Qv8SjsiORsZv8GMDqAkT26t0089XrR/WhQ1FovaJ47mr2QvH0a5BlkIw==', '1');