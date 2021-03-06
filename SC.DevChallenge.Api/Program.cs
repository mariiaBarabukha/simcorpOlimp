using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Data;
using System;

using System.Linq;
using System.Text;

using System.IO;


namespace SC.DevChallenge.Api
{

    //if you cannot connect to db connect it via server explorer
    //if it's still not working you need to install Microsoft.ACE.OLEDB.12.0
    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

       

        public static  IHostBuilder CreateHostBuilder(string[] args)
        {
           // DataBase.DB db = new DataBase.DB();
            return  Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>());
        }

        
    }
}
