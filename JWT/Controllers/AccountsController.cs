using Edu_plat.DTO;
using Edu_plat.Model;
using Edu_plat.Model.Interfaces;
using Edu_plat.Model.OTP;
using Edu_plat.Services;
using Google.Api.Gax;
using JWT.DATA;
using JWT.DTO;
using JWT.Model.OTP;
using JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;


namespace JWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IblackListService _blacklistService;

        private readonly IMailingServices _mailService;
        private readonly ApplicationDbContext _context;
        
        private static readonly Dictionary<string, (string Otp, DateTime ExpirationTime, TemporaryUserDTO TempUser)> _otpStore = new Dictionary<string, (string, DateTime, TemporaryUserDTO)>();

        
        private static readonly Dictionary<string, (string Otp, DateTime ExpirationTime)> _otpStoreFR = new Dictionary<string, (string, DateTime)>();
       
        #region Dependency Injection
        public AccountsController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            IMailingServices mailService,
             ApplicationDbContext context,
             IblackListService iblackListService

            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _mailService = mailService;
            _context = context;
            _blacklistService = iblackListService;
        }
        #endregion

        #region Register 
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        {
            if (!ModelState.IsValid)
                return Ok(new { success = false, message = "Invalid inputs Data" });
			
            if (dto.Email.Any(c => c > 127))  
			{
				return Ok(new { success = false, message = "Email must be written in English characters only." });
			}

			var existingUser = await _userManager.Users.AnyAsync(u => u.Email == dto.Email);
            if (existingUser)
                return Ok(new { success = false, message = "Email already registered." });
			
            var existingUsername = await _userManager.Users.AnyAsync(u => u.UserName == dto.UserName);
			if (existingUsername)
				return Ok(new { success = false, message = " UserName already registered." });

			var existingTempUser = await _context.TemporaryUsers.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingTempUser != null)
            {
                var existingOtp = await _context.OtpVerification
                    .FirstOrDefaultAsync(o => o.Email == dto.Email && DateTime.UtcNow < o.ExpirationTime);

                if (existingOtp != null)
                {
                
                    await _mailService.SendEmailAsync(dto.Email, "Your OTP Is On Its Way!",
                    $@"
               <p>Hello {dto.UserName},</p>
              <p>We noticed that you requested an OTP again. Don't worry, your previous code is still valid!</p>
              <p>Here is your One-Time Password (OTP):</p>
              <h1 style='color: #00bfff;'>{existingOtp.Otp}</h1>
              <p><strong>Note:</strong> This code is valid for the next <strong>5 minutes</strong>.</p>
               <p>If you did not request this, please ignore this email. Your account is secure.</p>
                <p>Take care,</p>
              <p><strong>The EduPlat Team</strong></p>");

                    return Ok(new { success = true, message = "OTP has been resent to your email." });
                }
                else
                {
                    
                    _context.TemporaryUsers.Remove(existingTempUser);
                    await _context.SaveChangesAsync();
                }
            }
			var hasher = new PasswordHasher<TemporaryUser>();
			var tempUser = new TemporaryUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PasswordHash =hasher.HashPassword(null, dto.Password)
			};
            await _context.TemporaryUsers.AddAsync(tempUser);

            string newOtp = GenerateOTP.GenerateOtp();
            var otpVerificationNew = new OtpVerification
            {
                Email = dto.Email,
                Otp = newOtp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(5)
            };
            await _context.OtpVerification.AddAsync(otpVerificationNew);
            await _context.SaveChangesAsync();

            await _mailService.SendEmailAsync(dto.Email, "Welcome to Our Platform!",
                $"<p>Hi {dto.UserName},</p>" +
                $"<p>We’re thrilled to have you here! To complete your registration, please verify your email using the code below:</p>" +
                $"<h2 style='color: #00bfff;'>{newOtp}</h2>" +
                $"<p>This code is valid for the next 5 minutes, so make sure to use it soon.</p>" +
                $"<p>Need help? Feel free to reach out—we’re here for you!</p>" +
                $"<p>Warm regards,</p>" +
                $"<p>The EduPlat Team</p>");

            return Ok(new { success = true, message = "Registration successful. A verification code has been sent to your email." });
        }
        #endregion 

        #region VerifyAccount

        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpDTO dto)
        {
			if (!ModelState.IsValid)
				return Ok(new { success = false, message = "Invalid inputs Data" });
			// check otp & email
			var otpRecord = await _context.OtpVerification
               .FirstOrDefaultAsync(o => o.Otp == dto.Otp && o.Email == dto.email);

            if (otpRecord == null)
                return Ok(new { success = false, message = "Invalid OTP ." });

            // check expire Time 
            if (DateTime.UtcNow > otpRecord.ExpirationTime)
                return Ok(new { success = false, message = "OTP has expired." });

            // find tempUser 
            var tempUser = await _context.TemporaryUsers.FirstOrDefaultAsync(u => u.Email == otpRecord.Email);
            if (tempUser == null)
                return Ok(new { success = false, message = "Incorrect Email" });

            // delete otp 
            _context.OtpVerification.Remove(otpRecord);
            await _context.SaveChangesAsync();


            // create new User 
            var newUser = new ApplicationUser
            {
                UserName = tempUser.UserName,
                Email = tempUser.Email,
                PasswordHash = tempUser.PasswordHash,

                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
                return Ok(new
                {
                    success = false,
                    message = "Failed to create User",
                    errors = result.Errors.Select(e => e.Description)
                });

            if (result.Succeeded)
            {
                newUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(newUser); // Update the user in the database
                var roleExist = await _roleManager.RoleExistsAsync("Student");
                if (!roleExist)
                {
                    // If "Student" role doesn't exist, create it
                    await _roleManager.CreateAsync(new IdentityRole("Student"));
                }
                var StudentObj = new Student()
                {
                    UserId = newUser.Id,
                    applicationUser = newUser,
                };
                _context.Set<Student>().Add(StudentObj);
                await _context.SaveChangesAsync();
                await _userManager.AddToRoleAsync(newUser, "Student");
                _context.TemporaryUsers.Remove(tempUser);
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true, message = "Email verified successfully and user created." });
        }


        #endregion

        #region Login

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO dto)
        {
            if (ModelState.IsValid)
            {

                var account = await _userManager.FindByEmailAsync(dto.email);
                if (account == null)
                    return Ok(new { success = false, message = "Invalid password or email" });


                var checkPass = await _userManager.CheckPasswordAsync(account, dto.Password);
                if (checkPass)
                {
                   
					#region User claims
					var UserClaims = new List<Claim>();
                    UserClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                    UserClaims.Add(new Claim("ApplicationUserId", account.Id));
					UserClaims.Add(new Claim("ApplicationUserName", account.UserName));

                    var Roles = await _userManager.GetRolesAsync(account);
					var role = Roles.FirstOrDefault();
					foreach (var RoleName in Roles)
                        UserClaims.Add(new Claim(ClaimTypes.Role, RoleName));
                   
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

                    SigningCredentials signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    #endregion

                    #region register Device
                    if (!string.IsNullOrWhiteSpace(dto.DeviceToken))
                    {
                        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == account.Id);
                        if (student != null)
                        {
                            var exists = await _context.userDevice
                                .AnyAsync(d => d.DeviceToken == dto.DeviceToken && d.StudentId == student.StudentId);

                            if (!exists)
                            {
                                var device = new userDevice
                                {
                                    DeviceToken = dto.DeviceToken,
                                    StudentId = student.StudentId
                                };
                               
                                student.userDevices.Add(device);
                              
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == account.Id);
                            if (doctor != null)
                            {
                                var exists = await _context.userDevice
                                    .AnyAsync(d => d.DeviceToken == dto.DeviceToken && d.DoctorId == doctor.DoctorId);

                                if (!exists)
                                {
                                    var device = new userDevice
                                    {
                                        DeviceToken = dto.DeviceToken,
                                        DoctorId = doctor.DoctorId
                                    };
                                    doctor.userDevices.Add(device);

                                    
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                    }

                    #endregion

                    JwtSecurityToken jwtSecurityToken = new JwtSecurityToken
                    (
                         issuer: _configuration["JWT:IssuerIP"],
                        audience: _configuration["JWT:audienceIP"],
                        expires: DateTime.Now.AddYears(1),
                        claims: UserClaims,
                        signingCredentials: signingCred

                    );
                    var roles = await _userManager.GetRolesAsync(account);

       
                    return Ok(new
                    {
                        success=true,
                        message= "Login successful",
                        userData = new {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                         roles,
                        expiration = DateTime.Now.AddYears(1)
                    }

                    });
                }
                else
                {

                    return Ok(new { success = false, message = "Email or Password inValid" });
                }
            }
            else
                return Ok(new { success = false, message = "Invalid Input Data" });
        }
        #endregion

        #region RegisterAdmin

        
        [HttpPost("RegisterAdmin")]
        [Authorize(Roles= "SuperAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false,message="Model state is invalid" });
            }

            var admin = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(admin, dto.Password);
            if (result.Succeeded)
            {
                
                var roleExist = await _roleManager.RoleExistsAsync("Admin");
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                await _userManager.AddToRoleAsync(admin, "Admin");
                return Ok("Admin user created successfully.");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Ok(new { success=false ,message="model state is invalid"});
        }
        #endregion

        #region RegisterDoctor 
        
        [HttpPost("RegisterDoctor")]
        [Authorize(Roles = "Admin,SuperAdmin")] 
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new {success=false,message="Model state is invalid"});
            }


			var existinguser = await _userManager.Users.AnyAsync(u => u.Email == dto.Email);
			if (existinguser)
				return Ok(new { success = false, message = "Email is already registered" });
			var existingUsername = await _userManager.Users.AnyAsync(u => u.UserName == dto.UserName);
			if (existingUsername)
				return Ok(new { success = false, message = " UserName already registered." });
			
            var doctor = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };
           
           var result = await _userManager.CreateAsync(doctor, dto.Password);
            
            var userId = doctor.Id;
            
            var DoctorObj = new Doctor
            {
                UserId = userId, 
                applicationUser = doctor
                
            };
          
             if (result.Succeeded)
            {
				_context.Set<Doctor>().Add(DoctorObj);
				await _context.SaveChangesAsync();
				
				var roleResult = await _userManager.AddToRoleAsync(doctor, "Doctor");
                if (!roleResult.Succeeded)
                {
                    return Ok(new { success = false, message = "Failed to assign Doctor role." });
                }

                return Ok(new {success=true , Message = "Doctor registered successfully." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Ok(new { success=false ,message="Faild Register Doctor"});
        }


        #endregion

        #region SendEmail
        //[HttpPost("SendEmail")]
        //public async Task<IActionResult> sendMail([FromForm] MailRequsetDTO dto)
        //{

        //    await _mailService.SendEmailAsync(dto.ToEmail, dto.Subject, dto.Body);
        //    return Ok();
        //}
        #endregion

        #region ForgetPassword
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
                return Ok(new { success = false, message = "Invalid Input." });

            
            var existingOtps = _context.OtpVerification
                .Where(o => o.Email == model.Email && o.Purpose == "ResetPassword");
            _context.OtpVerification.RemoveRange(existingOtps);


            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(new { success = false, message = "Incorrect Email" });

            
            string otp = GenerateOTP.GenerateOtp();
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(5);

            var otpVerification = new OtpVerification
            {
                Email = model.Email,
                Otp = otp,
                Purpose = "ResetPassword",
                ExpirationTime = expirationTime,
                IsVerified = false
            };

            await _context.OtpVerification.AddAsync(otpVerification);
            await _context.SaveChangesAsync();

            //Sending the otp to email
            var emailBody = $@"
<p>Hi {user.UserName},</p>
<p>We received a request to reset your password, and we're here to help!</p>
<p>Your OTP (One-Time Password) to reset your password is:</p>
<h2 style='color: #2E86C1;'>{otp}</h2>
<p>This code is valid for the next <strong>5 minutes</strong>.</p>
<p>If you didn't request a password reset, no worries—your account is still safe. You can simply ignore this email.</p>
<p>Take care,</p>
<p><strong>The Support Team</strong></p>";

            await _mailService.SendEmailAsync(model.Email, "Password Reset OTP", emailBody);

            return Ok(new { success = true, message = "A password OTP has been sent to your email." });
        }

        #endregion


        #region ValidateOtp
        [HttpPost("validate-otp")]
        public async Task<IActionResult> ValidateOtp([FromBody] VerifyOtpDTO model)
        {
            if (!ModelState.IsValid)
                return Ok(new { success = false, message = "Invalid request." });

            var otpRecord = await _context.OtpVerification
                .FirstOrDefaultAsync(o => o.Otp == model.Otp && o.Email==model.email && o.Purpose == "ResetPassword");

            if (otpRecord == null)
                return Ok(new { success = false, message = "Invalid OTP." });

            if (DateTime.UtcNow > otpRecord.ExpirationTime)
                return Ok(new { success = false, message = "OTP has expired." });

            
            otpRecord.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "OTP is valid" });
        }

        #endregion

        #region ResetPassword 

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
                return Ok(new { success = false, message = "Invalid request." });

           
            var otpRecord = await _context.OtpVerification
                .FirstOrDefaultAsync(o => o.IsVerified == true && o.Email==model.email && o.Purpose == "ResetPassword");

            if (otpRecord == null)
                return Ok(new { success = false, message = "Unauthorized or expired request." });

          
            var user = await _userManager.FindByEmailAsync(otpRecord.Email);
            if (user == null)
                return Ok(new { success = false, message = "Password or email is invalid" });
            

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetResult = await _userManager.ResetPasswordAsync(user, passwordResetToken, model.NewPassword);
            if (!resetResult.Succeeded)
                return Ok(new { success = false, message = "Password reset failed." });

            _context.OtpVerification.Remove(otpRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Password reset successful." });
        }
        #endregion

        #region Logout 
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { success = false, message = "Token is required" });

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "ApplicationUserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { success = false, message = "Invalid token" });
            }
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; 
            if (role == "Doctor")
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor != null)
                {
                    var device = await _context.userDevice
                        .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId );
                    if (device != null)
                    {
                        _context.userDevice.Remove(device);
                        await _context.SaveChangesAsync();
                    }
                }
                return Ok(new { success = true, message = "Logged out successfully and Doctor device token deleted " });

            }
            if (role == "Student")
            {
                var student = _context.Students.FirstOrDefault(d => d.UserId == userId);
                if (student != null)
                {
                    var device = await _context.userDevice
                        .FirstOrDefaultAsync(d => d.StudentId == student.StudentId );
                    if (device != null)
                    {
                        _context.userDevice.Remove(device);
                        await _context.SaveChangesAsync();
                    }
                }
                return Ok(new { success = true, message = "Logged out successfully and student device token deleted " });

            }
            if (string.IsNullOrEmpty(jti))
                return Ok(new { success = false, message = "Invalid token" });

            await _blacklistService.AddTokenToBlackListAsync(jti, jwtToken.ValidTo);

            return Ok(new { success = true, message = "Logged out successfully" });

        }

        #endregion

    }
}
