using Microsoft.AspNetCore.Mvc;
using ServiceStack.Host;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SC.DevChallenge.Api.DataBase
{
    public class DB
    {

        //singletone class to prevent creating more then 1 instance
        private DB()
        {
           
        }

        public static  DB db;

        public static DB GetDB()
        {
            if (db == null)
            {
                db = new DB();
            }
            return db;
        }

        public OleDbConnection getConnection()
        {
            string connectionString =
        @"provider=Microsoft.ACE.OLEDB.12.0;" +
           "data source=task.accdb";

            OleDbConnection myOleDbConnection
                = new OleDbConnection(connectionString);
            return myOleDbConnection;
        }


        //didn't use it
        private  double dtToTs(DateTime dt)
        {
            return (new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour,
                dt.Minute, dt.Second)
                - new DateTime(2018, 1, 1)).TotalSeconds;
        }

        private  DateTime tsToDt(int s)
        {
           long ticks = new DateTime(2018, 01, 01, 0, 0, 0,
    new CultureInfo("en-US", false).Calendar).Ticks;
            var t = (s*1000000000000 + ticks);
            return new DateTime(t);
        }

        
        public  JsonResult getAvarage(string p, string o, string i, string d)
        {
            //DateTime d1 = new DateTime();
            var myOleDbConnection = getConnection();
            OleDbCommand myOleDbCommand = myOleDbConnection.CreateCommand();
            myOleDbCommand.CommandText = "SELECT Avg(Price) as p " +
                    "FROM taskPrice2 ";

            string whereParam = "";
            if (p != null || o !=null || i != null || d != null)
            {
                whereParam = "Where ";
            }

            if (p != null)
            {
                whereParam += "portfolio = '" + p + "'";
                if (o!=null || i != null || d != null)
                {
                    whereParam += " And ";
                }
            }

            if (o != null)
            {
                whereParam += "oowner = '" + o +"'";
                if (i != null || d != null)
                {
                    whereParam += " And ";
                }
            }
            if (i != null)
            {
                whereParam += "instrument = '" + i + "'";
                if ( d != null)
                {
                    whereParam += " And ";
                }
            }
            DateTime mind = new DateTime();
            if (d != null)
            {
                
                d = d.Replace("%2F", "/");
                var parts = d.Split(" ");
                var dd = parts[0].Split("/");
                var time = parts[1].Split(":");
                var diffInSeconds = (new DateTime(Convert.ToInt32(dd[2].Substring(0, 4)),
                    Convert.ToInt32(dd[1]), Convert.ToInt32(dd[0]),
                    Convert.ToInt32(time[0]), Convert.ToInt32(time[1]),
                    Convert.ToInt32(time[2]))
                    - new DateTime( 2018,1,1)).TotalSeconds;
                var tointerval = (int)diffInSeconds % 10000;
                var frominterval = 10000 - tointerval;
                var parsedDate = DateTime.Parse(d);
                mind = parsedDate.AddSeconds(-tointerval);
                var maxd = parsedDate.AddSeconds(frominterval);
                Console.WriteLine(mind + " " + maxd);
                //var min = 10000 * interval;
                //var max = 10000 * (interval+1);
                //var d1 = tsToDt((int)min);
                //var d2 = tsToDt((int)max);
                string ddd = mind.ToString("dd/MM/yyyy HH:mm:ss").Replace(".", "/");
                //Console.WriteLine(ddd);
                string dddd = maxd.ToString("dd/MM/yyyy HH:mm:ss").Replace(".", "/");
                whereParam += "ddate >= #" + ddd + "# and ddate < #"+ dddd + "# ";
            }
            if (!String.IsNullOrEmpty(whereParam))
            {
                myOleDbCommand.CommandText += whereParam;
            }
           
            myOleDbConnection.Open();
            //OleDbDataReader myOleDbDataReader = myOleDbCommand.ExecuteReader();
            List<decimal> l = new List<decimal>();
            Console.WriteLine(myOleDbCommand.CommandText);
            OleDbDataReader myOleDbDataReader = myOleDbCommand.ExecuteReader();
           
            if (myOleDbDataReader.Read())
            {
                if (myOleDbDataReader[0] == DBNull.Value)
                {
                    throw new HttpException(404, "Not Found");
                }
                else
                {
                    l.Add((decimal)myOleDbDataReader[0]);
                }
            }
            else
            {
               throw new HttpException(404, "Not Found");
            }
            return new JsonResult(new MyResponce(l[0], mind));

        }


        //make request to db and returns result
        private  (decimal, DateTime) FindBenchmark(string p,
            (DateTime, DateTime) dateLimits, bool isAgg = false, 
            DateTime lastLim = new DateTime())
        {
            OleDbCommand myOleDbCommand = new OleDbCommand("SELECT Price " +
                    "FROM taskPrice2 Where portfolio = @port " +
                    "AND ddate >= @dd1 and ddate < @dd2");

            myOleDbCommand.Connection = getConnection();
            myOleDbCommand.Connection.Open();
            myOleDbCommand.Parameters.Add("port",
                OleDbType.VarChar, 255).Value = p;
            myOleDbCommand.Parameters.Add("dd1",
               OleDbType.Date).Value = dateLimits.Item1;
            myOleDbCommand.Parameters.Add("podd2rt",
               OleDbType.Date).Value = dateLimits.Item2;
            myOleDbCommand.Prepare();

            var reader = myOleDbCommand.ExecuteReader();
            List<decimal> prices = new List<decimal>();
            while (reader.Read())
            {
                if (reader[0] == DBNull.Value && !isAgg)
                {
                    throw new HttpException(404, "Not Found");
                }
                else
                {
                    if (reader[0] == DBNull.Value && isAgg)
                    {
                        prices.Add(0);
                    }
                    else
                    {
                        prices.Add((decimal)reader[0]);
                    }
                }
                }
               
               
            if (!isAgg && (prices == null || prices.Count == 0))
            {
                throw new HttpException(404, "Not Found");
            }

            //prices.Sort();
            var avgPrice = GetAvgBenchMark(prices, FindQs(prices));
            var l = isAgg ? lastLim : dateLimits.Item1;
            if (isAgg && (prices == null || prices.Count == 0))
            {
                return (0, l);
            }
            return (avgPrice, l);
        }

        //solve task1
        public (decimal, DateTime) GetBenchMark(string p, string d)
        {
            if (p == null || d == null)
            {
                throw new HttpException(404, "Not Found");
            }

            var dateLimits = GetDateTimeLimits(d);

            return FindBenchmark(p, dateLimits);
           
        }

        //finds quartiles
        public  (decimal, decimal) FindQs(List<decimal> prices)
        {
            prices.Sort();
            if (prices.Count == 2)
            {
                return (prices[0], prices[1]);
            }
            if (prices.Count == 1)
            {
                return (prices[0], prices[0]);
            }
            if (prices.Count == 0)
            {
                return (0, 0);
            }
            int len = prices.Count;
            //bool isLenOdd = len % 2 == 0 ? false : true ;
            decimal median = len / 2;
            decimal q1 = Math.Round(median) - median/ 2;
            decimal q3 = Math.Round(median) + median/ 2;

            decimal q1p = (prices[(int)Math.Floor(q1)]
                    + prices[(int)Math.Ceiling(q1)]) / 2;
            decimal q3p = (prices[(int)Math.Floor(q3)]
               + prices[(int)Math.Ceiling(q3)]) / 2;

            return (q1p, q3p);
        }

        //get avarage considering BenchMark
        private decimal GetAvgBenchMark(List<decimal> prices, 
            (decimal, decimal) q)
        {
            
            decimal iqr = q.Item2 - q.Item1;
            decimal ll = q.Item1 - (iqr + iqr / 2);
            decimal ul = q.Item2 + (iqr + iqr / 2);
            var suitablePrices = prices.FindAll(p => p >= ll && p <= ul);
            if (suitablePrices == null || suitablePrices.Count == 0)
            {
                return 0;
            }
            return Math.Round(suitablePrices.Average(),2);
        }

        //finds bottom and top limits of slot
        private  (DateTime, DateTime) GetDateTimeLimits(string d)
        {
            d = d.Replace("%2F", "/");
            var parts = d.Split(" ");
            var dd = parts[0].Split("/");
            var time = parts[1].Split(":");
            var diffInSeconds = (new DateTime(Convert.ToInt32(dd[2].Substring(0, 4)),
                    Convert.ToInt32(dd[1]), Convert.ToInt32(dd[0]),
                    Convert.ToInt32(time[0]), Convert.ToInt32(time[1]),
                    Convert.ToInt32(time[2]))
                    - new DateTime(2018, 1, 1)).TotalSeconds;
            var tointerval = (int)diffInSeconds % 10000;
            var frominterval = 10000 - tointerval;
            var parsedDate = DateTime.Parse(d);
            var mind = parsedDate.AddSeconds(-tointerval);
            var maxd = parsedDate.AddSeconds(frominterval);
            return (mind, maxd);
        }
        //solve task 2
        public List<(decimal, DateTime)> GetAggregate(string p,
             string sd, string ed, int intervals)
        {
            if (p == null || sd == null || ed == null || intervals == 0)
            {
                throw new HttpException(404, "Not Found");
            }
            var ll = GetDateTimeLimits(sd).Item1;
            var ul = GetDateTimeLimits(ed).Item2;
            int diff = (int)(ul - ll).TotalSeconds;
            int currIntervals = diff / 10000;
            //var res = new MyResponce[intervals];
            var res = new List<(decimal, DateTime)>();
            int fullGrSize = currIntervals / intervals + 1;
            int amountFull = currIntervals % intervals;
            int notFullGrSize = currIntervals / intervals;
            int amountMotFull = intervals - amountFull;
            int j = 0;
            for (int i = 0; i < intervals; i++)
            {
                if (amountFull != 0)
                {
                    var nm = 0;
                    decimal a = 0;
                    (decimal, DateTime) aa = (0, new DateTime());
                    for (int k = 0; k < fullGrSize; k++)
                    {
                        var currLL = ll.AddSeconds(j * 10000);
                        var currUL =
                            ll.AddSeconds((j + 1) * 10000);
                        aa =(FindBenchmark(p, (currLL, currUL),
                             true, currUL.AddSeconds(-10000)));
                        a += aa.Item1;
                        if (aa.Item1 == 0)
                        {
                            nm++;
                        }
                        j += 1;
                    }
                    res.Add((Math.Round(a/(fullGrSize-nm),2), aa.Item2));   
                    amountFull--;

                }
                else
                {
                    if (amountMotFull != 0)
                    {
                        var nmm = 0;
                        decimal a1 = 0;
                        (decimal, DateTime) aa1 = (0, new DateTime());
                        for (int k = 0; k < notFullGrSize; k++)
                        {
                            var currLL =
                        ll.AddSeconds(j * 10000);
                            var currUL =
                                ll.AddSeconds((j + 1) * 10000);
                            aa1 = (FindBenchmark(p, (currLL, currUL), true,
                                currUL.AddSeconds(-10000)));
                            a1 += aa1.Item1;
                            if (aa1.Item1 == 0)
                            {
                                nmm++;
                            }
                            j += 1;

                        }
                        res.Add((Math.Round(a1 /( fullGrSize-nmm),2),
                            aa1.Item2));
                        amountMotFull--;

                    }
                    else
                    {
                        res.Add((0,
                            ll.AddSeconds((j -1 + notFullGrSize) * 10000)));
                    }
                   
                    
                }


            }
            return res;
        }
    }


    //class to generate responce body
    public class MyResponce
    {
        public decimal Price;
        public DateTime Date;
        public MyResponce(decimal price = 0, DateTime dateTime = new DateTime())
        {
            Price = price;
            Date = dateTime;
        }
    }
}
