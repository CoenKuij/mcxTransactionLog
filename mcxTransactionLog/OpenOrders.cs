using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcxTrans
{
    public class OpenOrders
    {
        public enum OrderType {
                                [Description("Buy")]
                                BUY,
                                [Description("Sell")]
                                SELL
                              };

        public List<Order> orders { get; set; }

        public OpenOrders()
        {
            orders = new List<Order>();
        }

        public class Order
        {
            public int id { get; set; }
            public OrderType orderType { get; set; }
            public int timestamp { get; set; }
            public decimal price { get; set; }
            public decimal quantity { get; set; }
            public decimal total { get; set; }

            public Order()
            {

            }

            public Order(int id,
                    OrderType orderType,
                    int timestamp,
                    decimal price,
                    decimal quantity,
                    decimal total)
            {
                this.id = id;
                this.orderType = orderType;
                this.timestamp = timestamp;
                this.price = price;
                this.quantity = quantity;
                this.total = total;
            }
        }
    }
}
