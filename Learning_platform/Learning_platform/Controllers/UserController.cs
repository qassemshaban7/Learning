using Azure;
using Learning_platform.DTO;
using Learning_platform.Models;
using Learning_platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Learning_platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IWebHostEnvironment webHostEnvironment;

        private readonly UserManager<ApplicationUser> usermanager;
        private readonly RoleManager<IdentityRole> rolemanager;
        private readonly IConfiguration config;
        private readonly ApplicationDbContext context;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };

        public UserController(UserManager<ApplicationUser> usermanager, IConfiguration config, RoleManager<IdentityRole> rolemanager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.usermanager = usermanager;
            this.rolemanager = rolemanager;
            this.config = config;
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("register")]//api/account/register
        public async Task<IActionResult> Registration([FromForm] RegisterUserDto userDto)
        {
            if (ModelState.IsValid)
            {
                //save
                ApplicationUser user = new ApplicationUser();
                user.Email = userDto.Email;
                user.UserName = userDto.userName;
                if(await usermanager.FindByEmailAsync(userDto.Email) is not null)
                {
                    return BadRequest("User already exists!");
                }
                if (userDto.Image != null)
                {
                    string[] allowedExtensions = { ".png", ".jpg" };
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");

                    if (!allowedExtensions.Contains(Path.GetExtension(userDto.Image.FileName).ToLower()))
                    {
                        return BadRequest("Only .png and .jpg images are allowed!");
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + userDto.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userDto.Image.CopyToAsync(fileStream);
                    }
                    user.Image = uniqueFileName;
                }
                else
                {
                    user.Image = "default.png";
                }


                IdentityResult result = await usermanager.CreateAsync(user, userDto.Password);
                if (result.Succeeded)
                {
                    var role = "User";
                    await usermanager.AddToRoleAsync(user, role);
                    
                    var token = await usermanager.GenerateEmailConfirmationTokenAsync(user);
                    var response = new
                    {
                        Message = "Account added successfully",
                        UserName = user.UserName,
                        Email = user.Email,
                        ImageUrl = $"images/{user.Image}",
                        Token = token 
                    };
                    return Ok(response);
                }
                return BadRequest(result.Errors.FirstOrDefault());
            }
            return BadRequest(ModelState);
        }
        [HttpPost("login")]//api/account/login
        public async Task<IActionResult> Login(LoginUserDto userDto)
        {
            if (ModelState.IsValid == true)
            {
                //check - create token
                var user = await context.Users.FirstOrDefaultAsync(c => c.Email == userDto.Email);
                if (user != null)//email found
                {
                    bool found = await usermanager.CheckPasswordAsync(user, userDto.Password);
                    if (found)
                    {
                        //Claims Token
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Email, user.Email));
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName)); 
                        //claims.Add(new Claim("Image", user.Image));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                        //get role
                        var roles = await usermanager.GetRolesAsync(user);
                        foreach (var itemRole in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, itemRole/*.ToString()*/));
                        }
                        SecurityKey securityKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]));

                        //Create token
                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            issuer: config["JWT:ValidIssuer"],//url web api
                            audience: config["JWT:ValidAudiance"],//url consumer angular
                            expires: DateTime.Now.AddDays(double.Parse(config["JWT:DurationInDay"])),
                            claims: claims,
                            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                            );

                        if (user.Image == null)
                        {
                            var response = new
                            {
                                token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                                expiration = mytoken.ValidTo,
                                email = user.Email,
                                userName = user.UserName,
                                image = "null",
                            };
                            return Ok(response);
                        }
                        else
                        {
                            var response2 = new
                            {
                                userId = user.Id,
                                token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                                expiration = mytoken.ValidTo,
                                email = user.Email,
                                userName = user.UserName,
                                image = $"https://learningplatformv1.runasp.net/Images//{ user.Image}",
                            };
                            return Ok(response2);
                        }

                    }
                    return Ok("Email or password are invalid");
                }
                return Unauthorized();
            }
            return BadRequest();
        }

        [HttpPost("add_admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAdmin(AddAdminDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await usermanager.FindByEmailAsync(userDto.Email);
            if (user == null) return BadRequest();
            if (!await rolemanager.RoleExistsAsync("Admin"))
            {
                return BadRequest("role already exists!");
            }
            await usermanager.AddToRoleAsync(user, "Admin");

            return Ok();
        }

        [HttpPost("send_reset_code")]
        public async Task<IActionResult> SendResetCode(SendPINDto model, [FromServices] IEmailProvider _emailProvider)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid ModelState"
                });
            }
            var user = await usermanager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = "Email Not Found!"
                });
            }

            int pin = await _emailProvider.SendResetCode(model.Email);
            user.PasswordResetPin = pin;
            user.ResetExpires = DateTime.Now.AddMinutes(10);
            var expireTime = user.ResetExpires.Value.ToString("hh:mm tt");
            await usermanager.UpdateAsync(user);
            return Ok(new
            {
                status = 200,
                ExpireAt = "expired at " + expireTime,
                email = model.Email,
            });
        }
        [HttpPost("verify_pin/{email}")]
        public async Task<IActionResult> VerifyPin([FromBody] VerfiyPINDto model, [FromRoute] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid ModelState"
                });
            }
            var user = await usermanager.FindByEmailAsync(email);
            if (user is null)
            {
                return NotFound(new
                {
                    status = 404,
                    errorMessage = "Email Not Found!"
                });
            }
            if (user.ResetExpires < DateTime.Now || user.ResetExpires is null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Time Expired try to send new pin"
                });
            }
            if (user.PasswordResetPin != model.pin)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid pin"
                });
            }
            user.ResetExpires = null;
            user.PasswordResetPin = null;
            await usermanager.UpdateAsync(user);
            return Ok(new
            {
                status = 200,
                message = "PIN verified successfully",
                email = user.Email,
            });
        }

        [HttpPost("forget_password/{email}")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPassDto model, [FromRoute] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Invalid model state."
                });
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "New password and confirm new password do not match."
                });
            }

            var user = await usermanager.FindByEmailAsync(email);
            if (user is null)
            {
                return BadRequest(new
                {
                    status = 400,
                    errorMessage = "Email Not Found!"
                });
            }
            var token = await usermanager.GeneratePasswordResetTokenAsync(user);
            var result = await usermanager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                await usermanager.UpdateAsync(user);
                return Ok(new
                {
                    status = 200,
                    message = "Password changed successfully"
                });
            }
            return BadRequest(new
            {
                status = 400,
                errorMessage = "Invalid model state."
            });
            //return BadRequest(result.Errors.FirstOrDefault());
        }

    }
}
