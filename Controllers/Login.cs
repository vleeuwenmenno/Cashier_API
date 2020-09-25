using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashier_API.Constructors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TwoFactorAuthNet;
using TwoFactorAuthNetSkiaSharpQrProvider;

namespace Cashier_API.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase 
    { 
        [HttpPost("login")] 
        public IActionResult login([FromBody] string[] login)
        {
            if (login.Length != 3)
                return BadRequest("Missing login information, expected username, password and pincode.");

            // Get user with given username
            List<User> users = Program.db.Query<User>($"SELECT * FROM User WHERE username=$1;", new object[] { login[0] });

            // Check if we found any user with the query
            if (users.Count > 0)
            {
                User u = users[0];

                if (u == null)
                    return Unauthorized();

                // Check if the password matches the one stored as hashes and salts in the database
                if (Utilities.VerifyPassword(login[1], u.Hash, u.Salt))
                {
                    if (u.pinCode != login[2])
                        return Unauthorized("Incorrect pincode!");

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

        
        [HttpGet("login/2fa")] 
        public IActionResult setup2FA([FromHeader] string token)
        {
           // Check if the user managed to login with user and password but don't check 2fa here
            if (Logins.Verify(token, false, false) != null)
            {
                // Check if the user has 2fa enabled
                List<LoginSession> v = Program.db.Query<LoginSession>($"SELECT * FROM LoginSession WHERE id = '{token}';");
                LoginSession u = v.Count > 0 ? v.First() : null;

                // Get the user that owns this session
                List<User> users = Program.db.Query<User>($"SELECT * FROM User WHERE id='{u.userId}';");
                User user = users.Last();

                if (user.twoFactorConfirmed)
                    return BadRequest("2FA is already enabled for this account.");
                else
                {
                    TwoFactorAuth tfa = new TwoFactorAuth("Cashier API", qrcodeprovider: new SkiaSharpQrCodeProvider()); //TODO: Change org to company name from global settings (WIP)
                    string secret = tfa.CreateSecret(160);
                    
                    user.twoFactorSecret = secret;
                    user.twoFactorConfirmed = false;

                    Program.db.Update(user);
                    
                    return Ok(tfa.GetQrCodeImageAsDataUri("Cashier API", secret));
                }
            }
            else
                return Unauthorized();
        }

        [HttpDelete("login/2fa")] 
        public IActionResult removeTFA([FromBody] string tfaCode, [FromHeader] string token)
        {
            // Check if the user managed to login with user and password but don't check 2fa here
            if (Logins.Verify(token, false, false) != null)
            {
                // Check if the user has 2fa enabled
                List<LoginSession> v = Program.db.Query<LoginSession>($"SELECT * FROM LoginSession WHERE id = '{token}';");
                LoginSession u = v.Count > 0 ? v.First() : null;

                // Get the user that owns this session
                List<User> users = Program.db.Query<User>($"SELECT * FROM User WHERE id='{u.userId}';");
                User user = users.Last();

                if (!u.passed2FA)
                    return BadRequest("2FA must be validated before we can disable 2FA on this account.");

                if (users.Count > 0)
                {
                    if (!string.IsNullOrEmpty(user.twoFactorSecret))
                    {
                        var tfa = new TwoFactorAuth("Cashier API"); //TODO: Change org to company name from global settings (WIP)
                        
                        // Verify if 2FA code is valid
                        if (tfa.VerifyCode(user.twoFactorSecret, tfaCode))
                        {
                            // If 2FA was never confirmed let's make it confirmed as we validated a code.
                            if (user.twoFactorConfirmed)
                            {
                                user.twoFactorConfirmed = false;
                                user.twoFactorSecret = "";
                                Program.db.Update(user);
                            }
                            else
                                return BadRequest("2FA was not enabled on this account, therefore we cannot disable it either.");
                            
                            return Ok(u);
                        }
                        else
                            return Unauthorized("Incorrect 2FA code");
                    }
                    else
                        return BadRequest("2FA is not enabled for this account.");
                }
                else
                    return BadRequest("No user found with this login token.");
            }
            else
                return Unauthorized();
        }

        [HttpPost("login/2fa")] 
        public IActionResult confirmTFA([FromBody] string tfaCode, [FromHeader] string token)
        {
            // Check if the user managed to login with user and password but don't check 2fa here
            if (Logins.Verify(token, false, false) != null)
            {
                // Check if the user has 2fa enabled
                List<LoginSession> v = Program.db.Query<LoginSession>($"SELECT * FROM LoginSession WHERE id = '{token}';");
                LoginSession u = v.Count > 0 ? v.First() : null;

                // Get the user that owns this session
                List<User> users = Program.db.Query<User>($"SELECT * FROM User WHERE id='{u.userId}';");
                User user = users.Last();

                if (u.passed2FA)
                    return BadRequest("2FA is already validated for this session.");

                if (users.Count > 0)
                {
                    if (!string.IsNullOrEmpty(user.twoFactorSecret))
                    {
                        var tfa = new TwoFactorAuth("Cashier API"); //TODO: Change org to company name from global settings (WIP)
                        
                        // Verify if 2FA code is valid
                        if (tfa.VerifyCode(user.twoFactorSecret, tfaCode))
                        {
                            // Code seems legit, update db and return session info.
                            u.passed2FA = true;
                            Program.db.Update(u);

                            // If 2FA was never confirmed let's make it confirmed as we validated a code.
                            if (!user.twoFactorConfirmed)
                            {
                                user.twoFactorConfirmed = true;
                                Program.db.Update(user);
                            }
                            
                            return Ok(u);
                        }
                        else
                            return Unauthorized("Incorrect 2FA code");
                    }
                    else
                        return BadRequest("2FA is not enabled for this account.");
                }
                else
                    return BadRequest("No user found with this login token.");
            }
            else
                return Unauthorized();
        }
    }

    public class Logins
    {
        public static LoginSession Verify(string token, bool mustBeAdmin = false, bool checkTFA = true)
        {
            List<LoginSession> v = Program.db.Query<LoginSession>($"SELECT * FROM LoginSession WHERE id = '{token}';");
            LoginSession u = v.Count > 0 ? v.First() : null;

            //Check if we found a session matching our token.
            if (u == null)
                return null;
            
            //Check if the session is expired
            if (u.expiry < DateTime.Now)
            {
                //Session is not valid! We will delete it from the database.
                Program.db.Delete(u);
                return null;
            }
            
            // Get the user that owns this session
            List<User> users = Program.db.Query<User>($"SELECT * FROM User WHERE id='{u.userId}';");
            User user = users.Last();

            if (users.Count == 0)
            {
                // User does not exist, but there is a session for a user that doesn't exist!? Deleting the session ...
                Program.db.Delete(u);
                return null;
            }

            // Are we required to check for two factor auth?
            if (checkTFA && !string.IsNullOrEmpty(user.twoFactorSecret) && user.twoFactorConfirmed)
            {
                // Secret for 2FA is set, but user did not pass 2FA for current session.
                if (!u.passed2FA)
                    return null;
            }
            
            // If we must check if its admin we run the following code
            if (mustBeAdmin)
                return users.First().isAdmin == true ? u : null;
            else
                return u;
        }
    }
}
