using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace sjam.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        public AuthController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        [Authorize]
        [HttpGet("Login")]
        public IActionResult Login()
        {
            try
            {
                return Ok(new
                {
                    status = "ValidToken"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, msg = ex.Message.ToString() });
            }
        }

        [Authorize]
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            try
            {
                return Ok(new
                {
                    status = "ValidToken"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, msg = ex.Message.ToString() });
            }
        }

    }
}
