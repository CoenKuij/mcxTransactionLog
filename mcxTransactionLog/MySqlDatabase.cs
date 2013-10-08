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
    class MySqlDatabase
    {
        private readonly string dbName = @"mcxTransactionDB";
        private readonly string host = @"localhost";
        private readonly string user = @"root";
        private MySqlDataReader iteratorTransReader = null;
        private string password;

        public MySqlDatabase(string host, string user, string password, string dbName = @"mcxTransactionDB")
        {
            this.dbName = dbName;
            this.password = password;
            this.host = host;
            this.user = user;
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
        }

        public void CreateTransactionTable(string tableName)
        {
            using (MySqlConnection con = new MySqlConnection())
            {
                con.ConnectionString = string.Format("server={0};user={1};database={2};port=3306;password={3};pooling=false;", host, user, dbName, password);
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

        public bool Get1stTransaction(string tableName, ref int timestamp, ref string transType, ref string orderType, ref decimal price, ref decimal quantity, ref decimal balance, ref string remark)
        {
            MySqlConnection con = new MySqlConnection();
            con.ConnectionString = string.Format("server={0};user={1};database={2};port=3306;password={3};pooling=false;", host, user, dbName, password);
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

        public bool GetNextTransaction(ref int timestamp, ref string transType, ref string orderType, ref decimal price, ref decimal quantity, ref decimal balance, ref string remark)
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

        public bool RecordExist(int timestamp, decimal quantity, string tableName)
        {
            using (MySqlConnection con = new MySqlConnection())
            {
                con.ConnectionString = string.Format("server={0};user={1};database={2};port=3306;password={3};pooling=false;", host, user, dbName, password);
                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT * FROM {0} WHERE timestamp = {1} AND quantity = {2};", tableName, timestamp, quantity);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        public bool AddTransaction(string tableName, int timestamp, string transType, string orderType, decimal price, decimal quantity, decimal balance, string remark)
        {
            if (!RecordExist(timestamp, quantity, tableName))
            {
                using (MySqlConnection con = new MySqlConnection())
                {
                    con.ConnectionString = string.Format("server={0};user={1};database={2};port=3306;password={3};pooling=false;", host, user, dbName, password);
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
