using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;

namespace SC.DevChallenge.Api.DataBase
{
    public class DB
    {
        public DB()
        {
           
        }

        public static OleDbConnection getConnection()
        {
            string connectionString =
        @"provider=Microsoft.ACE.OLEDB.12.0;" +
           "data source=task.accdb";

            OleDbConnection myOleDbConnection
                = new OleDbConnection(connectionString);
            return myOleDbConnection;
        }

        //public static string getAvarage()
        //{
        //    var myOleDbConnection = getConnection();
        //    OleDbCommand myOleDbCommand = myOleDbConnection.CreateCommand();
        //    myOleDbCommand.CommandText =
        //        "SELECT Avg(Price) as p " +
        //         "FROM taskPrice2 ";
        //    myOleDbConnection.Open();
        //    List<decimal> l = new List<decimal>();
        //    OleDbDataReader myOleDbDataReader = myOleDbCommand.ExecuteReader();
        //    while (myOleDbDataReader.Read())
        //    {
        //        l.Add((decimal)myOleDbDataReader[0]);
        //    }
        //    return l == null ? "" : l[0].ToString();
        //}

        private double dtToTs(DateTime dt)
        {
            return (new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour,
                dt.Minute, dt.Second)
                - new DateTime(2018, 1, 1)).TotalSeconds;
        }

        private DateTime tsToDt(int s)
        {
            return new DateTime((s+DateTime.Now.Second)*1000000000);
        }
        public static string getAvarage(string p, string o, string i, string d)
        {
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
            if (d != null)
            {
                
                d = d.Replace("%2F", "/");
                var dd = d.Split("/");
                var diffInSeconds = (new DateTime(Convert.ToInt32(dd[2]),
                    Convert.ToInt32(dd[1]), Convert.ToInt32(dd[0]))
                    - new DateTime( 2018,1,1)).TotalSeconds;
                var interval = diffInSeconds / 10000;
                var min = 10000 * interval;
                var max = 10000 * (interval+1);
                //DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
                whereParam += "ddate = DateAdd('s', #" + d + "#";
            }
            if (!String.IsNullOrEmpty(whereParam))
            {
                myOleDbCommand.CommandText += whereParam;
            }
           
            myOleDbConnection.Open();
            //OleDbDataReader myOleDbDataReader = myOleDbCommand.ExecuteReader();
            List<decimal> l = new List<decimal>();
            try
            {
                OleDbDataReader myOleDbDataReader = myOleDbCommand.ExecuteReader();
                while (myOleDbDataReader.Read())

                {

                    if (myOleDbDataReader[0] == DBNull.Value)
                    {
                        l.Add(0);
                    }
                    else
                    {
                        l.Add((decimal)myOleDbDataReader[0]);
                    }
                }
                return l == null ? "" : l[0].ToString();
            }catch(Exception e)
            {
                return "Something went wrong!";
            }
            
        }
    }
}
