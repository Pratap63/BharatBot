using System.Security.Claims;
using ChatBot.Data;
using ChatBot.Helpers;
using ChatBot.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;


        private readonly OtpService _otpService;

        public AccountController(AppDbContext context, OtpService otpService)
        {
            _context = context;
            _otpService = otpService;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            // 🔒 Always show same message (security)
            ViewBag.Message = "If the email exists, a reset link has been sent.";

            if (user == null)
                return View();

            // Generate secure token
            user.ResetToken = Guid.NewGuid().ToString("N");
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);

            await _context.SaveChangesAsync();

            string resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token = user.ResetToken },
                Request.Scheme
            );

            var emailService = new EmailService();

            await emailService.SendEmailAsync(
                user.Email,

                "Reset Your BharatBot Password",
                $"<h1>Thanks for visiting us! </h1>Click here to reset your password: <a href='{resetLink}'>Reset Password</a>"
            );


            // 📧 Send email (pseudo for now)
            Console.WriteLine("Reset link: " + resetLink);

            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(
            string token,
            string password,
            string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.Token = token;
                return View();
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.ResetToken == token &&
                u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                ViewBag.Error = "Invalid or expired reset link.";
                return View();
            }

            user.PasswordHash = PasswordHelper.HashPassword(password);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Password reset successful. Please login.";
            return RedirectToAction("Login");
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Register(string Name, string email, string password, string confirmPassword)
        {
            // 1. Basic validation
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // 2. Check existing user
            bool userExists = _context.Users.Any(u =>
                 u.Email == email);

            if (userExists)
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            // 3. Hash password
            string passwordHash = PasswordHelper.HashPassword(password);

            // 4. Create user
            var user = new User
            {
                Username = Name,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. Success → Login
            TempData["Success"] = "Account created successfully.";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var identity = new ClaimsIdentity(claims, "ManualAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("ManualAuth", principal);

            return RedirectToAction("Index", "Chat");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null || !PasswordHelper.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var identity = new ClaimsIdentity(claims, "ManualAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("ManualAuth", principal);

            return RedirectToAction("Index", "Chat");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("ManualAuth");
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Message = "If the email exists, OTP has been sent.";
                return View("ForgotPassword");
            }

            await _otpService.GenerateAndSaveOtp(email);

            return RedirectToAction("VerifyOtp", new { email });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            var isValid = await _otpService.ValidateOtp(email, otp);

            if (!isValid)
            {
                ViewBag.Error = "Invalid or Expired OTP";
                ViewBag.Email = email;
                return View();
            }

            return RedirectToAction("ResetPasswordWithOtp", new { email });
        }


        [HttpGet]
        public IActionResult ResetPasswordWithOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordWithOtp(
            string email,
            string password,
            string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.Email = email;
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            user.PasswordHash = PasswordHelper.HashPassword(password);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Password reset successful.";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult ResetUsingOtp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetUsingOtp(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            ViewBag.Message = "If the email exists, OTP has been sent.";

            if (user == null)
                return View();

            await _otpService.GenerateAndSaveOtp(email);

            return RedirectToAction("VerifyOtp", new { email });
        }



    }
}
