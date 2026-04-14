using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ChatBot.Data;

namespace ChatBot.Services
{
    public class OtpService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public OtpService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }



        private string GenerateOtp()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);

            int value = BitConverter.ToInt32(bytes, 0);
            value = Math.Abs(value % 1000000);

            return value.ToString("D6");
        }


        public async Task GenerateAndSaveOtp(string email)
        {
            var otp = GenerateOtp();

            var resetOtp = new PasswordResetOtp
            {
                Email = email,
                OtpCode = otp,
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var userName = user.Username;

            var expiryTime = DateTime.UtcNow.AddMinutes(5).ToString("hh:mm tt");

            _context.PasswordResetOtps.Add(resetOtp);
            await _context.SaveChangesAsync();   // 🔥 ADD THIS

            await _emailService.SendEmailAsync(
            email,
            "Reset Your BharatBot Password - OTP Code",
            $@"
                <div style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:40px 0;'>
                    <table align='center' width='600' cellpadding='0' cellspacing='0' 
                           style='background:#ffffff; border-radius:8px; padding:30px; box-shadow:0 4px 10px rgba(0,0,0,0.05);'>
            
                        <tr>
                            <td style='text-align:center; padding-bottom:20px;'>
                                <h1 style='color:#1f2937; margin:0;'>BharatBot</h1>
                                <p style='color:#6b7280; margin:5px 0 0;'>Secure Account Recovery</p>
                            </td>
                        </tr>

                        <tr>
                            <td style='padding:20px 0; color:#374151; font-size:15px; line-height:1.6;'>
                                Hello, {userName}

                                <br/><br/>
                                We received a request to reset your BharatBot account password.
                                Please use the One-Time Password (OTP) below to proceed.
                            </td>
                        </tr>

                        <tr>
                            <td style='text-align:center; padding:20px 0;'>
                                <div style='display:inline-block; padding:15px 30px; 
                                            font-size:28px; letter-spacing:4px; 
                                            background:#2563eb; color:#ffffff; 
                                            border-radius:6px; font-weight:bold;'>
                                    {otp}
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td style='color:#dc2626; font-size:14px; text-align:center; padding-bottom:20px;'>
                                This OTP will expire in 5 minutes.
                            </td>
                        </tr>

                        <tr>
                            <td style='color:#6b7280; font-size:14px; line-height:1.6; padding-top:10px;'>
                                If you did not request a password reset, please ignore this email.
                                <br/><br/>
                                For security reasons, do not share this OTP with anyone.
                            </td>
                        </tr>

                        <tr>
                            <td style='border-top:1px solid #e5e7eb; padding-top:20px; text-align:center; 
                                       color:#9ca3af; font-size:12px;'>
                                © {DateTime.Now.Year} BharatBot. All rights reserved.
                            </td>
                        </tr>

                    </table>
                </div>
                "
            );

        }


        public async Task<bool> ValidateOtp(string email, string otp)
        {
            var record = await _context.PasswordResetOtps
                .Where(x => x.Email == email &&
                            !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null)
                return false;

            if (record.ExpiryTime < DateTime.UtcNow)
                return false;

            if (record.OtpCode != otp)
                return false;

            record.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
