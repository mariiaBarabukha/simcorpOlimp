using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SC.DevChallenge.Api.DataBase;
using ServiceStack.Host;
using System;
using System.Collections.Generic;

namespace SC.DevChallenge.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        //[HttpGet("average")]
        //public MyResponce Average(string portfolio, string owner,
        //    string instrument, string date)
        //{
        //    JsonResult json = null;
        //    var res = "";
        //    //DataBase.DB db = new DataBase.DB();
        //    try
        //    {
        //        json = DataBase.DB.GetDB().getAvarage(portfolio,
        //        owner, instrument, date);
        //        res = JsonConvert.SerializeObject(json.Value);
        //        return res;
        //    }
        //    catch (HttpException e)
        //    {
        //        //Response. = "404 Not Found";
        //        Response.StatusCode = 404;
        //        //res = JsonConvert.SerializeObject(json.Value);
        //    }

        //    return res;
        //}

        [HttpGet("benchmark")]
        public string Benchmark(string portfolio, string date)
        {
           
            var res = "";
            try
            {
                //json =
                //res = JsonConvert.SerializeObject(json.Value);
                var r = DataBase.DB.GetDB().GetBenchMark(portfolio, date);
                return $"{{\n price: {r.Item1},\n date: {r.Item2}\n}}";
            }
            catch (HttpException e)
            {
                //Response. = "404 Not Found";
                Response.StatusCode = 404;
                //res = JsonConvert.SerializeObject(json.Value);
            }

            return "";
        }
        //my answer do not match with answer in doc file
        //i tried to find out why but i failed
        [HttpGet("aggregate")]
        public string Aggregate(string portfolio, string startDate, 
            string endDate, int intervals)
        {
            List<(decimal, DateTime)> json = null;
            var res = "";
            try
            {
                //res += "[\n";
                //var js = DataBase.DB.GetDB().GetAggregate(portfolio, startDate,
                //    endDate, intervals);

                //foreach (var j in js)
                //{
                //    res += (JsonConvert.SerializeObject(j.Value)+"\n");
                //}
               
                //res = JsonConvert.SerializeObject(json.Value);
                //return res+"]";
                var r = DataBase.DB.GetDB().GetAggregate(portfolio, startDate,
                    endDate, intervals);
                var re = "[\n";
                foreach (var t in r)
                {
                    re += $"{{\n price: {r.Item1},\n date: {r.Item2}\n}}";
                }
                return re+"\n]";

            }
            catch (HttpException e)
            {
                //Response. = "404 Not Found";
                Response.StatusCode = 404;
                //res = JsonConvert.SerializeObject(json.Value);
            }
            

            return "";
        }
    }
}
