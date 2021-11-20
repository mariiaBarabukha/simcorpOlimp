using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceStack.Host;
using System;

namespace SC.DevChallenge.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        [HttpGet("average")]
        public string Average(string portfolio, string owner,
            string instrument, string date)
        {
            JsonResult json = null;
            var res = "";
            //DataBase.DB db = new DataBase.DB();
            try
            {
                json = DataBase.DB.GetDB().getAvarage(portfolio,
                owner, instrument, date);
                res = JsonConvert.SerializeObject(json.Value);
                return res;
            }
            catch (HttpException e)
            {
                //Response. = "404 Not Found";
                Response.StatusCode = 404;
                //res = JsonConvert.SerializeObject(json.Value);
            }

            return res;
        }

        [HttpGet("benchmark")]
        public string Benchmark(string portfolio, string date)
        {
            JsonResult json = null;
            var res = "";
            try
            {
                json = DataBase.DB.GetDB().GetBenchMark(portfolio, date);
                res = JsonConvert.SerializeObject(json.Value);
                return res;
            }
            catch (HttpException e)
            {
                //Response. = "404 Not Found";
                Response.StatusCode = 404;
                //res = JsonConvert.SerializeObject(json.Value);
            }

            return res;
        }
        //my answer do not match with answer in doc file
        //i tried to find out why but i failed
        [HttpGet("aggregate")]
        public string Aggregate(string portfolio, string startDate, 
            string endDate, int intervals)
        {
            JsonResult json = null;
            var res = "";
            try
            {
                res += "[\n";
                var js = DataBase.DB.GetDB().GetAggregate(portfolio, startDate,
                    endDate, intervals);

                foreach (var j in js)
                {
                    res += (JsonConvert.SerializeObject(j.Value)+"\n");
                }
               
                //res = JsonConvert.SerializeObject(json.Value);
                return res+"]";
            }
            catch (HttpException e)
            {
                //Response. = "404 Not Found";
                Response.StatusCode = 404;
                //res = JsonConvert.SerializeObject(json.Value);
            }
            

            return res;
        }
    }
}
