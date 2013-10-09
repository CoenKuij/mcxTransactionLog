using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Threading;
using mcxNOW;

namespace mcxTrans
{
    class TransactionData
    {
        private Database db = null;
        private Api tradeAPI = null;
        private DispatcherTimer fetchTransactionTimer = null;

        public TransactionData(Api tradeAPI, Database.DatabaseInitialiseData databaseInitialiseData)
        {
            this.tradeAPI = tradeAPI;
            /// Make a connection with the CryptoBot database, or create it when it does not exist
            db = Database.GetDatabaseHandle(databaseInitialiseData);
            
            foreach (Currency currency in Currency.GetAll())
            {
                string tableName = currency.Code + "Transactions";
                db.CreateTransactionTable(tableName);
            }
        }


        /// <summary>
        /// Starts the fetching of price data
        /// </summary>
        public void StartFetchingPrice()
        {
            //To gather transaction data start a timed function every 60s
            fetchTransactionTimer = new System.Windows.Threading.DispatcherTimer();
            fetchTransactionTimer.Tick += new EventHandler(FetchTransactionHistory);
            fetchTransactionTimer.Interval = new TimeSpan(0, 0, 1, 0, 0);
            fetchTransactionTimer.Start();
        }

        public void Export(string filename)
        {
            string tableName = null;
            int timestamp = 0;
            string transType = null;
            string orderType = null;
            decimal price = 0;
            decimal quantity = 0;
            decimal balance = 0;
            string remark = null;

            if (fetchTransactionTimer != null) fetchTransactionTimer.Stop();
            using(StreamWriter writer = new StreamWriter(filename, true))
            {
                writer.WriteLine("Currency,Date/Time,Transaction type,Order type,Price,Quantity,Balance,Remark");
                foreach (Currency currency in Currency.GetAll())
                {
                    tableName = currency.Code + "Transactions";
                    if (db.Get1stTransaction(tableName, ref timestamp, ref transType, ref orderType, ref price, ref quantity, ref balance, ref remark))
                    {
                        writer.Write(currency.Code);
                        writer.Write(",");
                        writer.Write(UnixTimestampToDateTime((double)timestamp));
                        writer.Write(",");
                        writer.Write(transType);
                        writer.Write(",");
                        writer.Write(orderType);
                        writer.Write(",");
                        writer.Write(decimal.Round(price,8,MidpointRounding.AwayFromZero));
                        writer.Write(",");
                        writer.Write(decimal.Round(quantity,8,MidpointRounding.AwayFromZero));
                        writer.Write(",");
                        writer.Write(decimal.Round(balance, 8, MidpointRounding.AwayFromZero));
                        writer.Write(",");
                        writer.Write(remark);
                        writer.WriteLine("");
                        while (db.GetNextTransaction(ref timestamp, ref transType, ref orderType, ref price, ref quantity, ref balance, ref remark))
                        {
                            writer.Write(currency.Code);
                            writer.Write(",");
                            writer.Write(UnixTimestampToDateTime((double)timestamp));
                            writer.Write(",");
                            writer.Write(transType);
                            writer.Write(",");
                            writer.Write(orderType);
                            writer.Write(",");
                            writer.Write(decimal.Round(price, 8, MidpointRounding.AwayFromZero));
                            writer.Write(",");
                            writer.Write(decimal.Round(quantity, 8, MidpointRounding.AwayFromZero));
                            writer.Write(",");
                            writer.Write(decimal.Round(balance, 8, MidpointRounding.AwayFromZero));
                            writer.Write(",");
                            writer.Write(remark);
                            writer.WriteLine("");
                        }
                    }
                }
            }
            if (fetchTransactionTimer != null) fetchTransactionTimer.Start();
        }

        private void FetchTransactionHistory(Object sender, EventArgs e)
        {
            foreach (Currency currency in Currency.GetAll())
            {
                /// Read the order book to load the historical trades
                try
                {
                    TransactionHistory transactionHistory = getTransactionHistory(currency);

                    string tableName = currency.Code + "Transactions";

                    foreach (TransactionHistory.Transaction trans in transactionHistory.transactions)
                    {
                            /// only store the hisotry in the database when it is newer that the latest tick in the database
                            if (!db.AddTransaction(tableName, Convert.ToInt32(trans.timestamp),
                                           trans.transType.ToString(),
                                           trans.orderType.ToString(),
                                           trans.price,
                                           trans.quantity,
                                           trans.balance,
                                           trans.remark)) break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != typeof(mcxNOW.ConnectionErrorException)) throw (ex);
                }
            }
        }

        /// <summary>
        /// Gets the history of own transactions on the exchange
        /// </summary>
        /// <param name="currencyPair">Pair to get the transactions for</param>
        /// <returns>TransactionHistory</returns>
        public TransactionHistory getTransactionHistory(Currency currency)
        {
            TransactionHistory trans = new TransactionHistory();

            Regex feeshare = new Regex(@"^Feeshare payment for having ([0-9]{1,2})MCX");
            Regex interest = new Regex(@"^Interest Payment on deposit of ([0-9]+\.[0-9]+)");
            Regex exchanged = new Regex(@"^Exchanged for ([0-9]+\.[0-9]+)([A-Z]{2,3}) at ([0-9]+\.[0-9]+)[A-Z]{2,3} each");
            Regex lotto = new Regex(@"^Lotto Win");
            Regex deposit = new Regex(@"^Deposit - TXID: (.*)");
            Regex withdraw = new Regex(@"^Withdraw to (.*)");
            Regex txid = new Regex(@"^Network TXID: (.*)");
            Regex received = new Regex(@"^Received from (.*)");
            Regex sent = new Regex(@"^Sent to (.*)");
            Regex oldTrans = new Regex(@"^Exchanged for ([0-9]+\.[0-9]+)([A-Z]{2,3})");
            Regex ban = new Regex(@"^Banned (.*)");
            Regex created = new Regex(@"^Account Created");

            try
            {
                UserInfo ui = tradeAPI.UserInfo();
                foreach (CurrencyInfo ci in ui.cur)
                {
                    if (ci.tla != currency.Code) continue;
                    foreach (mcxNOW.Log log in ci.log)
                    {
                        TransactionHistory.TransActionType transactionType = TransactionHistory.TransActionType.UNKNOWN;
                        decimal quantity = 0;
                        decimal price = 0;
                        string remark = "";

                        Match regexMatch = feeshare.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.FEESHARE;
                            quantity = Convert.ToDecimal(regexMatch.Groups[1].Value);
                        }

                        regexMatch = interest.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.INTEREST;
                            quantity = Convert.ToDecimal(regexMatch.Groups[1].Value);
                        }

                        regexMatch = exchanged.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.EXCHANGE;
                            quantity = Convert.ToDecimal(regexMatch.Groups[1].Value);
                            price = Convert.ToDecimal(regexMatch.Groups[3].Value);
                            remark = regexMatch.Groups[2].Value;
                        }

                        regexMatch = lotto.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.LOTTO;
                        }

                        regexMatch = received.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.RECEIVED;
                            remark = regexMatch.Groups[1].Value;
                        }

                        regexMatch = sent.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.SENT;
                            remark = regexMatch.Groups[1].Value;
                        }

                        regexMatch = deposit.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.DEPOSIT;
                            remark = regexMatch.Groups[1].Value;
                        }

                        regexMatch = withdraw.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.WITHDRAW;
                            remark = regexMatch.Groups[1].Value;
                        }

                        regexMatch = txid.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.TXID;
                            remark = regexMatch.Groups[1].Value;
                        }

                        regexMatch = ban.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.BAN;
                            remark = regexMatch.Groups[1].Value;
                        }

                        regexMatch = created.Match(log.d);
                        if (regexMatch.Groups[0].Value != "")
                        {
                            transactionType = TransactionHistory.TransActionType.CREATED;
                        }

                        if (transactionType == TransactionHistory.TransActionType.UNKNOWN)
                        {
                            regexMatch = oldTrans.Match(log.d);
                            if (regexMatch.Groups[0].Value != "")
                            {
                                transactionType = TransactionHistory.TransActionType.EXCHANGE;
                                quantity = Convert.ToDecimal(regexMatch.Groups[1].Value);
                                price = quantity / log.a;
                                remark = regexMatch.Groups[2].Value;
                            }
                        }

                        OpenOrders.OrderType orderType = log.a >= 0 ? OpenOrders.OrderType.BUY : OpenOrders.OrderType.SELL;
                        trans.transactions.Add(new TransactionHistory.Transaction(log.timestamp, transactionType, orderType, price, log.a, log.b, remark));
                    }
                }
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(mcxNOW.ConnectionErrorException)) throw e;
            }
            return trans;
        }

        private static DateTime UnixTimestampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

    }
}
