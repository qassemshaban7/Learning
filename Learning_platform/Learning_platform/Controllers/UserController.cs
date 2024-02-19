using Learning_platform.DTO;
using Learning_platform.Models;
using Learning_platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                ApplicationUser user = await usermanager.FindByEmailAsync(userDto.Email);
                if (user != null)//email found
                {
                    bool found = await usermanager.CheckPasswordAsync(user, userDto.Password);
                    if (found)
                    {
                        //Claims Token
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Email, user.Email));
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName)); 
                        claims.Add(new Claim("Image", user.Image));
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

                        var response = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                            expiration = mytoken.ValidTo,
                            email = user.Email,
                            userName = user.UserName,
                            image = $"images/{user.Image}"

                        };

                        return Ok(response);
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
        public async Task<IActionResult> SendResetCode(SendResetPassDto model, [FromServices] IEmailProvider _emailProvider)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await usermanager.FindByEmailAsync(model.Email);
            if (user is null) return BadRequest("Email Not Found!");
            int pin = await _emailProvider.SendResetCode(model.Email);
            user.PasswordResetPin = pin;
            user.ResetExpires = DateTime.Now.AddMinutes(5);
            await usermanager.UpdateAsync(user);
            return Ok(new{
                ExpireAt = user.ResetExpires,
            });
        }
        [HttpPost("reset_code")]
        public async Task<IActionResult> SendResetCode(ResetPassDto model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await usermanager.FindByEmailAsync(model.Email);
            if (user is null || user.ResetExpires is null
               || user.ResetExpires < DateTime.Now || user.PasswordResetPin != model.pin)
                return BadRequest("Invalid Token!");

            //await usermanager.ChangePasswordAsync(user, model.Pass);
            var token = await usermanager.GeneratePasswordResetTokenAsync(user);
            var result = await usermanager.ResetPasswordAsync(user, token, model.Pass);
            if (result is null) return BadRequest();
            user.ResetExpires = null;
            user.PasswordResetPin = null;
            await usermanager.UpdateAsync(user);
            return Ok();
        }
        [HttpPost("check_code")]
        public async Task<IActionResult> CheckCode(CheckCodeDto model)  
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await usermanager.FindByEmailAsync(model.Email);
            if (user is null || user.ResetExpires is null
               || user.ResetExpires < DateTime.Now || user.PasswordResetPin != model.pin)
                return BadRequest(new {message =  "Invalid Token!"});
            return Ok(new { message = "Valid Token." });
        }
    }
}
