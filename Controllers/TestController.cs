using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sjam.Helpers;
using sjam.RabbitMQ.Common;

namespace sjam.Controllers
{
    [NoAuthorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMQueueProcessingService _mQueueProcessingService;

        public TestController (IMQueueProcessingService mQueueProcessingService)
        {
            _mQueueProcessingService = mQueueProcessingService;
        }

        [HttpPost("process-on-queue")]
        public async Task<IActionResult> ProcessQueue([FromQuery] string queueName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(queueName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Queue name is required."
                    });
                }

                await _mQueueProcessingService.ProcessQueueAsync(queueName);

                return Ok(new
                {
                    success = true,
                    message = $"Queue '{queueName}' processed successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while processing the queue.",
                    error = ex.Message
                });
            }
        }
    }
}
