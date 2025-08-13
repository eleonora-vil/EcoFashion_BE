using EcoFashionBackEnd.Dtos.User;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginTokenController : ControllerBase
    {
        private readonly UserService _userService;
        public LoginTokenController(UserService userService) {
            _userService = userService;
        }
        [HttpPost("supplier")]
        public async Task<IActionResult> GetSupplierToken()
        {
            var request = new UserLoginRequest
            {
                Email = "supplier@example.com",
                PasswordHash = "supplier"
            };
            try
            {
                var result = await _userService.LoginAsync(request);
                return Ok(result.Token);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
        [HttpPost("designer")]
        public async Task<IActionResult> GetDesignerToken()
        {
            var request = new UserLoginRequest
            {
                Email = "designer@example.com",
                PasswordHash = "designer"
            };
            try
            {
                var result = await _userService.LoginAsync(request);
                return Ok(result.Token);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
    }
}
