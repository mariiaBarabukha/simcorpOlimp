using Microsoft.AspNetCore.Mvc;
using System;

namespace SC.DevChallenge.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        [HttpGet("average/{portfolio}/{owner}/{instrument}/{date?}")]
        public string Average(string portfolio, string owner,
            string instrument, string date)
        {
            //DataBase.DB db = new DataBase.DB();
            string res = DataBase.DB.getAvarage(portfolio, owner, instrument, date);
            return res;
        }
        
    }
}
