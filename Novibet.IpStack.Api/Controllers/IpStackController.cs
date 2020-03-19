using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Novibet.IpStack.Abstractions;
using Novibet.IpStack.Business.Models;
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
            var ipDetail = await _ipStackService.GetIpCachedAsync(ip);
            if(ipDetail == null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return Ok(ipDetail);
        }
    }
}
