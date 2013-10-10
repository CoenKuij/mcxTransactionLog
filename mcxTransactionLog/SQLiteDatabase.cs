﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcxTrans
{
    class SQLiteDatabase : Database
    {
        private SQLiteConnection con = new SQLiteConnection();
        private string cs = null;
        public SQLiteDataReader iteratorTickReader = null;
        public SQLiteDataReader iteratorTransReader = null;

        public SQLiteDatabase(string dbName = @"mcxTransactionDB")
        {
            string path = Path.Combine(Environment.GetFolderPath(
                            Environment.SpecialFolder.ApplicationData), "CryptoDB");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            cs = string.Format("Data Source={0};pooling=false", Path.Combine(path, dbName));
            con.ConnectionString = cs;
            con.Open();
        }

        public override void CreateTransactionTable(string tableName)
        {
            using (SQLiteConnection con = new SQLiteConnection())
            {
                con.ConnectionString = cs;
                con.Open();

                using (SQLiteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText
                        = string.Format("CREATE TABLE IF NOT EXISTS {0}"
                                        + "(id INTEGER PRIMARY KEY AUTOINCREMENT, trans_id INTEGER, timestamp INTEGER, trans_type STRING, order_type STRING, price REAL, quantity REAL, balance REAL, remark STRING);"
                                                    , tableName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public override bool Get1stTransaction(string tableName, ref int timestamp, ref string transType, ref string orderType, ref decimal price, ref decimal quantity, ref decimal balance, ref string remark)
        {
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = cs;
            con.Open();

            using (SQLiteCommand cmd = con.CreateCommand())
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

        public override bool RecordExist(int timestamp, decimal quantity, string tableName)
        {
            bool result = false;
            using (SQLiteConnection con = new SQLiteConnection())
            {
                con.ConnectionString = cs;
                con.Open();

                using (SQLiteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT * FROM {0} WHERE timestamp = {1} AND quantity = {2};", tableName, timestamp, quantity);
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result = true;
                    }
                    else result = false;
                }
            }
            return result;
        }

        public override bool AddTransaction(string tableName, int timestamp, string transType, string orderType, decimal price, decimal quantity, decimal balance, string remark)
        {
            if (!RecordExist(timestamp, quantity, tableName))
            {
                using (SQLiteConnection con = new SQLiteConnection())
                {
                    con.ConnectionString = cs;
                    con.Open();
                    using (SQLiteCommand cmd = con.CreateCommand())
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
