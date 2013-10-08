using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcxTrans
{
    public class TransactionHistory
    {
        public enum TransActionType { FEESHARE, INTEREST, EXCHANGE, LOTTO, DEPOSIT, WITHDRAW, TXID, RECEIVED, SENT, BAN, CREATED, UNKNOWN }

        public List<Transaction> transactions { get; set; }

        public TransactionHistory()
        {
            transactions = new List<Transaction>();
        }

        public class Transaction
        {
            public int timestamp { get; set; }
            public TransActionType transType { get; set; }
            public OpenOrders.OrderType orderType { get; set; }
            public decimal price { get; set; }
            public decimal quantity { get; set; }
            public decimal balance { get; set; }
            public string remark { get; set; }

            public Transaction()
            {

            }

            public Transaction(int timestamp,
                    TransActionType transType,
                    OpenOrders.OrderType orderType,
                    decimal price,
                    decimal quantity,
                    decimal balance,
                    string remark
                )
            {
                this.timestamp = timestamp;
                this.transType = transType;
                this.orderType = orderType;
                this.price = Math.Abs(price);
                this.quantity = Math.Abs(quantity);
                this.balance = balance;
                this.remark = remark;
            }
        }
    }
}
