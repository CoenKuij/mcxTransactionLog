#region license
//   Please read and agree to license.md contents before using this SDK.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mcxNOW
{
    public class Info
    {
        /**
         * Amount of base currency in users account
         */
        public decimal base_bal { get; set; }

        /**
         * Amount of info currency in users account
         */
        public decimal cur_bal { get; set; }

        /**
         * Users buy and sell orders
         */
        public List<Order> orders { get; set; }
    }


    public class Order
    {
        /**
         * Id of order
         */
        public int id { get; set; }

        /**
         * buy or sell  
         * false = sell
         * true = buy
         * {ck}
         */
        public bool b { get; set; }

        /**
         * Was executed 1 yes 0 no
         */
        public bool e { get; set; }
        
        /**
         * Time order was placed
         * DateTime not working {ck}
         */
        public int t { get; set; }
        
        /**
         * Volume of the order
         * {ck}
         */
        public decimal a1 { get; set; }

        /**
         * Price of the order
         * {ck}
         */
        public decimal p { get; set; }
    }
 }
