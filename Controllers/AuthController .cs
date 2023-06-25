using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SitBackTradeAPI.Data;
using SitBackTradeAPI.Model;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SitBackTradeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ApplicationDbContext? _context;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<JwtSettings> jwtSettings, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Email = model.Email,
                    FirstName = model.FirstName!,
                    LastName = model.LastName!,
                    Role = (Role)Enum.Parse(typeof(Role), model.Role),
                    Wallet = 0,
                    UserName = model.UserN
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded && user.Role == Role.Seller)
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role.ToString()!)
                    };

                    var store = new Store
                    {
                        StoreName = model.StoreName, // Make sure to add StoreName to your RegisterModel
                        UserId = user.Id,
                        // Add any additional properties for the Store model here
                    };

                    await _context!.Stores!.AddAsync(store);
                    await _context.SaveChangesAsync();
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(new { IsSeller = true });
                }
                else if(result.Succeeded && user.Role == Role.Buyer)
                {
                    return Ok(new { IsSeller = false });
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    if (result.Succeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.Role.ToString()!)
                        };

                        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret!));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            issuer: null,
                            audience: null,
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                            signingCredentials: creds
                        );

                        return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
                    }
                }

                return Unauthorized();
            }

            return BadRequest(ModelState);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "User logged out successfully" });
        }

        [Authorize(Roles = nameof(Role.Seller) + "," + nameof(Role.Admin))]
        [HttpPost("create-store")]
        public async Task<IActionResult> CreateStore(Store model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingStore = await _context!.Stores!.FirstOrDefaultAsync(s => s.UserId == userId);
            if (existingStore != null)
            {
                return BadRequest("A store already exists for this user." + existingStore.StoreName);
            }

            var store = new Store
            {
                StoreName = model.StoreName,
                Description = model.Description,
                LogoUrl = model.LogoUrl,
                UserId = userId
            };

            await _context!.Stores!.AddAsync(store);
            await _context!.SaveChangesAsync();

            return Ok("Store created successfully.");
        }

    }
}