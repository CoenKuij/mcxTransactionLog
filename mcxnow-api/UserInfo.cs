using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace mcxNOW
{
    public class UserInfo
    {
        public UserInfo(string xmlTree)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xmlTree);
            XmlNodeList slog = xDoc.GetElementsByTagName("slog");
            AddSecurityLog(slog);

            XmlNodeList theme = xDoc.GetElementsByTagName("theme");
            if (theme.Count > 0) this.theme = Convert.ToInt32(theme[0].InnerText);

            XmlNodeList sounds = xDoc.GetElementsByTagName("sounds");
            if (sounds.Count > 0) this.sounds = Convert.ToInt32(sounds[0].InnerText);

            XmlNodeList iplogout = xDoc.GetElementsByTagName("iplogout");
            if (iplogout.Count > 0) this.iplogout = Convert.ToInt32(iplogout[0].InnerText);

            XmlNodeList showads = xDoc.GetElementsByTagName("showads");
            if (showads.Count > 0) this.showads = Convert.ToInt32(showads[0].InnerText);

            XmlNodeList idlelogout = xDoc.GetElementsByTagName("idlelogout");
            if (idlelogout.Count > 0) this.idlelogout = Convert.ToInt32(idlelogout[0].InnerText);

            XmlNodeList richlist = xDoc.GetElementsByTagName("richlist");
            if (richlist.Count > 0) this.richlist = richlist[0].InnerText;

            XmlNodeList gauthstatus = xDoc.GetElementsByTagName("gauthstatus");
            if (gauthstatus.Count > 0) this.gauthstatus = Convert.ToInt32(gauthstatus[0].InnerText);

            XmlNodeList cur = xDoc.GetElementsByTagName("cur");
            AddCurrency(cur);

            XmlNodeList totalbtcworth = xDoc.GetElementsByTagName("totalbtcworth");
            if (totalbtcworth.Count > 0) this.totalbtcworth = Convert.ToDecimal(totalbtcworth[0].InnerText);
        }

        private void AddSecurityLog(XmlNodeList nodeList)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                int t = 0;
                string d = null;

                if (nodeList[i].ChildNodes.Count > 0)
                {
                    t = Convert.ToInt32(nodeList[i].ChildNodes[0].InnerText);
                    d = nodeList[i].ChildNodes[1].InnerText;
                    this.slog.Add(new SecurityLog(t, d));
                }
            }
        }

        private void AddCurrency(XmlNodeList nodeList)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                string name = null;
                string tla = null;
                decimal balavail = 0;
                decimal baltotal = 0;
                decimal btctotal = 0;
                string addr = null;
                int sellorders = 0;
                int buyorders = 0;
                List<Log> log = new List<Log>();

                if (nodeList[i].ChildNodes.Count > 0)
                {
                    name = nodeList[i].ChildNodes[0].InnerText;
                    tla = nodeList[i].ChildNodes[1].InnerText;
                    balavail = Convert.ToDecimal(nodeList[i].ChildNodes[2].InnerText);
                    baltotal = Convert.ToDecimal(nodeList[i].ChildNodes[3].InnerText);
                    btctotal = Convert.ToDecimal(nodeList[i].ChildNodes[4].InnerText);
                    addr = nodeList[i].ChildNodes[5].InnerText;
                    int startJ;
                    if (tla != "BTC")
                    {
                        sellorders = Convert.ToInt32(nodeList[i].ChildNodes[6].InnerText);
                        buyorders = Convert.ToInt32(nodeList[i].ChildNodes[7].InnerText);
                        startJ = 8;
                    }
                    else startJ = 6;
                    for (int j = startJ; j < nodeList[i].ChildNodes.Count; j++ )
                    {
                        string q = nodeList[i].ChildNodes[j].OuterXml;
                        if (q.Substring(0, 5) == "<utx>") continue;
                        log.Add(AddLog(nodeList[i].ChildNodes[j].ChildNodes));
                    }
                    this.cur.Add(new CurrencyInfo(name, tla, balavail, baltotal, btctotal, addr, sellorders, buyorders, log));
                }
            }
        }

        private Log AddLog(XmlNodeList nodeList)
        {
            int timestamp = 0;
            string d = null;
            decimal a = 0m;
            decimal b = 0m;

            if (nodeList.Count > 0)
            {
                timestamp = Convert.ToInt32(nodeList[0].InnerText);
                d = nodeList[1].InnerText;
                a = Convert.ToDecimal(nodeList[2].InnerText);
                b = Convert.ToDecimal(nodeList[3].InnerText);
                new Log(timestamp, d, a, b);
            }
            return new Log(timestamp, d, a, b);
        }

        /**
         * Security log
         */
        public List<SecurityLog> slog = new List<SecurityLog>();

        /**
         * Theme
         */
        public int theme { get; set; }

        /**
         * Sounds
         */
        public int sounds { get; set; }

        /**
         * ip change logout
         */
        public int iplogout { get; set; }

        /**
         * show adds
         */
        public int showads { get; set; }

        /**
         * log out on idle
         */
        public int idlelogout { get; set; }

        /**
         * show on richlist
         */
        public string richlist { get; set; }

        /**
         * Google auth status
         */
        public int gauthstatus { get; set; }

        /**
         * info about the different currencies
         */
        public List<CurrencyInfo> cur = new List<CurrencyInfo>();

        /**
         * Total value of all currencies in BTC
         */
        public decimal totalbtcworth { get; set; }
    }

    public class SecurityLog
    {
        public SecurityLog(int t, string d)
        {
            this.t = t;
            this.d = d;
        }

        /* time stamp */
        public int t { get; set; }

        /* event */
        public string d { get; set; }
    }

    public class CurrencyInfo
    {
        public CurrencyInfo
                    (string name, 
                     string tla, 
                     decimal balavail, 
                     decimal baltotal, 
                     decimal btctotal, 
                     string addr, 
                     int sellorders, 
                     int buyorders, 
                     List<Log> log)
        {
            this.name = name;
            this.tla = tla;
            this.balavail = balavail;
            this.baltotal = baltotal;
            this.btctotal = btctotal;
            this.addr = addr;
            this.sellorders = sellorders;
            this.buyorders = buyorders;
            this.log = log;
        }

        /* currency name */
        public string name { get; set; }

        /* currency abbreviation */
        public string tla { get; set; }

        /* Available balance */
        public decimal balavail { get; set; }

        /* Total balance */
        public decimal baltotal { get; set; }

        /* Balance expressed in btc */
        public decimal btctotal { get; set; }

        /* Deposit address */
        public string addr { get; set; }

        /* Number of sell orders in order book */
        public int sellorders { get; set; }

        /* Number of buy orders in order book */
        public int buyorders { get; set; }

        /* Transaction history */
        public List<Log> log { get; set; }
    }

    public class Log
    {
        public Log(int timestamp, string d, decimal a, decimal b)
        {
            this.timestamp = timestamp;
            this.d = d;
            this.a = a;
            this.b = b;
        }

        /* timestamp */
        public int timestamp { get; set; }

        /* Reason */
        public string d { get; set; }

        /* amount */
        public decimal a { get; set; }

        /* new balance */
        public decimal b { get; set; }
    }
}
