using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DormCare.Data;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace DormCare.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var id))
                return RedirectToAction("Login");

            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password, string returnUrl = null)
        {
            // allow login by username or email
            var user = _db.Users.FirstOrDefault(u => (u.Email == login || u.Username == login) && u.Password == password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "Student"),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string fullName, string email, string phone, string password, string confirmPassword)
        {
            // Basic server-side validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("", "All fields are required");
                return View();
            }

            if (username.Length > 50)
            {
                ModelState.AddModelError("", "Username must be 50 characters or less");
                return View();
            }

            if (fullName.Length > 500)
            {
                ModelState.AddModelError("", "Full name must be 500 characters or less");
                return View();
            }

            if (_db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "Username already taken");
                return View();
            }

            if (_db.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email already registered");
                return View();
            }

            // email RFC-like validation
            try
            {
                var _ = new MailAddress(email);
            }
            catch
            {
                ModelState.AddModelError("", "Email is invalid");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View();
            }

            // password complexity: 1 digit, 1 special, 1 uppercase, length 8-16
            var pwdPattern = new Regex("^(?=.*\\d)(?=.*[A-Z])(?=.*\\W).{8,16}$");
            if (!pwdPattern.IsMatch(password))
            {
                ModelState.AddModelError("", "Password must be 8-16 chars, include at least 1 digit, 1 uppercase letter and 1 special character");
                return View();
            }

            // Ukrainian phone: require +380 followed by 9 digits
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var phonePattern = new Regex("^\\+380\\d{9}$");
                if (!phonePattern.IsMatch(phone))
                {
                    ModelState.AddModelError("", "Phone must be in Ukrainian format +380XXXXXXXXX");
                    return View();
                }
            }

            var user = new Models.User { Username = username, FullName = fullName, Email = email, Password = password, Phone = phone, Role = "Student" };
            _db.Users.Add(user);
            _db.SaveChanges();

            // create a Student record linked to this user
            var student = new Models.Student { UserId = user.Id, RoomId = null };
            _db.Students.Add(student);
            _db.SaveChanges();

            // sign in the new user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "Student"),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "StudentHome");
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            // Force Google to show account chooser each time
            // (adds `prompt=select_account` to the OAuth authorization request)
            properties.Parameters.Add("prompt", "select_account");
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            // In many setups external provider will sign in via cookie scheme, here we attempt to read external claims
            var external = await HttpContext.AuthenticateAsync();

            var principal = external.Principal ?? result?.Principal;
            if (principal == null)
            {
                return RedirectToAction("Login");
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                // create a minimal user record for external login
                user = new Models.User { Username = email, FullName = principal.FindFirst(ClaimTypes.Name)?.Value ?? email, Email = email, Password = "", Phone = "", Role = "Student" };
                _db.Users.Add(user);
                _db.SaveChanges();

                var student = new Models.Student { UserId = user.Id, RoomId = null };
                _db.Students.Add(student);
                _db.SaveChanges();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "Student"),
            };

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            // Sign out locally (cookie)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Attempt sign-out with external provider if supported; ignore if handler doesn't support sign-out
            try
            {
                await HttpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);
            }
            catch (InvalidOperationException)
            {
                // GoogleHandler doesn't support SignOutAsync; ignore and continue (local cookie cleared)
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
