using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IntegrationApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InformationController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            Object data = new
            {
                connectionString = @"Data Source.\SQLEXPRESS;Initial Catalog=PiotrA132376;Integrated Security=True",
                integrationName = "bezposrednio.net.pl",
                studentIndex = "s132376",
                studentName = "Piotr Amilusik"
            };

            return Ok(data);
        }
    }
}
