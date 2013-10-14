using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text;
using System.Threading.Tasks;

namespace mcxTrans
{
    public class MySqlDatabase : Database
    {
        public string DBName {get; set;}
        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        private string cs = null;
        private MySqlDataReader iteratorTransReader = null;

        public MySqlDatabase (string password, string host = "localhost", string user = "root", string dbName = @"mcxTransactionDB")
        {
            this.DBName = dbName;
            this.Password = password;
            this.Host = host;
            this.User = user;
            using (MySqlConnection con = new MySqlConnection())
            {
                con.ConnectionString = string.Format("server={0};user={1};port=3306;password={2};pooling=false;", host, user, password);
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = string.Format("CREATE DATABASE IF NOT EXISTS {0};", dbName);
                    cmd.ExecuteNonQuery();
                }
            }
            this.cs = string.Format("server={0};user={1};database={2};port=3306;password={3};pooling=false;", host, user, dbName, password);
        }

        public override void CreateTransactionTable(string tableName)
        {
            using (MySqlConnection con = new MySqlConnection())
            {
                con.ConnectionString = cs;
                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText
                        = string.Format("CREATE TABLE IF NOT EXISTS {0}"
                                        + "(id INTEGER PRIMARY KEY AUTO_INCREMENT, trans_id INTEGER, timestamp INTEGER, trans_type TEXT, order_type TEXT, price DECIMAL(15,8), quantity DECIMAL(15,8), balance DECIMAL (20,8), remark TEXT);"
                                                    , tableName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public override bool Get1stTransaction(string tableName, ref int timestamp, ref string transType, ref string orderType, ref decimal price, ref decimal quantity, ref decimal balance, ref string remark)
        {
            using (MySqlConnection con = new MySqlConnection())
            {
                con.ConnectionString = cs;
                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT * from {0} ORDER BY timestamp ASC;", tableName);
                    iteratorTransReader = cmd.ExecuteReader();
                    if (iteratorTransReader.Read())
                    {
                        timestamp = Convert.ToInt32(iteratorTransReader["timestamp"]);
                        transType = iteratorTransReader["trans_type"].ToString();
                        orderType = iteratorTransReader["order_type"].ToString();
                        price = Convert.ToDecimal(iteratorTransReader["price"]);
                        quantity = Convert.ToDecimal(iteratorTransReader["quantity"]);
                        balance = Convert.ToDecimal(iteratorTransReader["balance"]);
                        remark = iteratorTransReader["remark"].ToString();
                        return true;
                    }
                    else return false;
                }
            }
        }

        public override bool GetNextTransaction(ref int timestamp, ref string transType, ref string orderType, ref decimal price, ref decimal quantity, ref decimal balance, ref string remark)
        {
            if (iteratorTransReader == null)
            {
                return false;
            }
            if (!iteratorTransReader.Read())
                return false;
            else
            {
                timestamp = Convert.ToInt32(iteratorTransReader["timestamp"]);
                transType = iteratorTransReader["trans_type"].ToString();
                orderType = iteratorTransReader["order_type"].ToString();
                price = Convert.ToDecimal(iteratorTransReader["price"]);
                quantity = Convert.ToDecimal(iteratorTransReader["quantity"]);
                balance = Convert.ToDecimal(iteratorTransReader["balance"]);
                remark = iteratorTransReader["remark"].ToString();
            }
            return true;
        }

        public override bool RecordExist(int timestamp, string orderType, decimal quantity, decimal balance, string tableName)
        {
            using (MySqlConnection con = new MySqlConnection())
            {
                con.ConnectionString = cs;
                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT * FROM {0} WHERE timestamp = {1} AND quantity = {2} AND order_type = \"{3}\" AND balance = {4};", tableName, timestamp, quantity, orderType, balance);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        public override bool AddTransaction(string tableName, int timestamp, string transType, string orderType, decimal price, decimal quantity, decimal balance, string remark)
        {
            if (!RecordExist(timestamp, orderType, quantity, balance, tableName))
            {
                using (MySqlConnection con = new MySqlConnection())
                {
                    con.ConnectionString = cs;
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = string.Format("INSERT INTO {0} (timestamp, trans_type, order_type, price, quantity, balance, remark) VALUES ({1}, \"{2}\", \"{3}\", {4}, {5}, {6}, \"{7}\")"
                                                        , tableName, timestamp, transType, orderType, price, quantity, balance, remark);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            else return false;
        }
    }
}
