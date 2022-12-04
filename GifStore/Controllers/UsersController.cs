using GifStore.Data.Dtos;
using GifStore.Data.ViewModels;
using GifStore.Entities;
using GifStore.Repositories;
using GifStore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace GifStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository repo;
        private readonly IPasswordHasher<User> harsher;
        private readonly ITokenProvider tokenProvider;
        private readonly DatabaseSeeder dbSeeder;

        public UsersController(IUserRepository _repo, 
            IPasswordHasher<User> _harsher, 
            ITokenProvider _tokenProvider,
            DatabaseSeeder _dbSeeder)
        {
            repo = _repo;
            harsher = _harsher;
            tokenProvider = _tokenProvider;
            dbSeeder = _dbSeeder;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto userInfo)
        {
            if(! ModelState.IsValid)
            {
                return BadRequest(new ResponseVM { Status=ResponseStatus.ERROR, Message="Inputs are invalid", Data=ModelState});
            }

            var user = await repo.Get(userInfo.Email);
            if(user != null)
            {
                return BadRequest(new ResponseVM { Status=ResponseStatus.ERROR, Message="User with this email is already registered"});
            }

            user = new User
            {
                Email = userInfo.Email,
                Fullname = userInfo.Fullname,
            };
            user.PasswordHash = harsher.HashPassword(user, userInfo.Password);

            var result = await repo.Save(user);
            if(! result)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "An error has been encountered while registering user" });
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message="User is registered successfully"});
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto userInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Inputs are invalid", Data = ModelState.Values });
            }

            var user = await repo.Get(userInfo.Email);
            if (user == null)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Invalid login credentials" });
            }

            var verifiedPassword = harsher.VerifyHashedPassword(user, user.PasswordHash, userInfo.Password);

            if (verifiedPassword == PasswordVerificationResult.Failed)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Invalid login credentials" }); 
            }

            string token = tokenProvider.GenerateToken(user);

            var loginTokenVM = new LoginTokenVM { Id = user.Id, Email = user.Email, Fullname = user.Fullname, Token = token };

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "User is logged in successfully", Data = loginTokenVM });
        }


        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateFullname(SingleFieldDto<string> userInfo)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Inputs are invalid", Data = ModelState.Values });
            }

            var user = await repo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Invalid login credentials", Data = HttpContext.User.FindFirstValue(ClaimTypes.Email) });
            }

            user.Fullname = userInfo.Data;
            var result = await repo.Update(user);

            if (! result)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Error encountered while processing the request" });
            }

            string token = tokenProvider.GenerateToken(user);
            var loginTokenVM = new LoginTokenVM { Id = user.Id, Email = user.Email, Fullname = user.Fullname, Token = token };

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Information is updated successfully", Data = loginTokenVM });
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(SingleFieldDto<string> userInfo)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Inputs are invalid", Data = ModelState.Values });
            }

            var user = await repo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Invalid login credentials", Data = HttpContext.User.FindFirstValue(ClaimTypes.Email) });
            }

            user.PasswordHash = harsher.HashPassword(user, userInfo.Data);
            var result = await repo.Update(user);

            if (!result)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Error encountered while processing the request" });
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Password is updated successfully" });
        }


        [HttpOptions("seed-data")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                await dbSeeder.Run();
                return Ok("Database seeded");
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Seeding failed: {e.Message}");
            }
        }

    }
}
