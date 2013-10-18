
namespace mcxTrans
{
    public abstract class Database
    {
        public enum DatabaseTypes { MYSQL, SQLITE }

        public struct DatabaseInitialiseData
        {
            public DatabaseTypes Type;
            public string DBName;
            public string Host;
            public string User;
            public string Password;
        }

        public static Database GetDatabaseHandle(DatabaseInitialiseData databaseInitialiseData)
        {
            if (databaseInitialiseData.Type == DatabaseTypes.MYSQL) 
                return new MySqlDatabase
                             (databaseInitialiseData.Password,
                              databaseInitialiseData.Host, 
                              databaseInitialiseData.User, 
                              databaseInitialiseData.DBName);
            else return new SQLiteDatabase(databaseInitialiseData.DBName);
        }

        public abstract void CreateTransactionTable(string tableName);

        public abstract bool Get1stTransaction(string tableName, ref int timestamp, ref string transType, ref string orderType, ref decimal price, ref decimal quantity, ref decimal balance, ref string remark);

        public abstract bool GetNextTransaction(ref int timestamp, ref string transType, ref string orderType, ref decimal price, ref decimal quantity, ref decimal balance, ref string remark);

        public abstract bool RecordExist(int timestamp, string orderType, decimal quantity, decimal balance, string tableName);

        public abstract bool AddTransaction(string tableName, int timestamp, string transType, string orderType, decimal price, decimal quantity, decimal balance, string remark);
    }
}
