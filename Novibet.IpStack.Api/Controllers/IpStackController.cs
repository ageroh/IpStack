using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Novibet.IpStack.Business.Services;

namespace Novibet.IpStack.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IpStackController : ControllerBase
    {
        private readonly ILogger<IpStackController> _logger;
        private readonly IIpStackService _ipStackService;
        
        public IpStackController(ILogger<IpStackController> logger, IIpStackService ipStackService)
        {
            _logger = logger;
            _ipStackService = ipStackService;
        }

        [HttpGet("{ip}")]
        public async Task<IActionResult> Get(string ip)
        {
            try
            {
                var ipDetail = await _ipStackService.GetIpCachedAsync(ip);

                if (ipDetail == null)
                {
                    _logger.LogWarning("Ip {ip} does not exists.", ip);
                    return StatusCode((int)HttpStatusCode.InternalServerError);
                }

                return Ok(ipDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Some error occurred.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
