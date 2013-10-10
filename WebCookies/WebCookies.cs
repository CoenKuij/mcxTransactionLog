using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCookies
{
    public class Cookie
    {
        public enum Browsers { CHROME, IE, FIREFOX, SAFARI, OPERA };

        public static bool GetCookie (string strHost, string strField, ref string Value, Browsers browser = Browsers.CHROME)
        {
            bool ret_val = false;
            Value = null;
            switch (browser)
            {
                case Browsers.CHROME:
                    ret_val = GetCookie_Chrome(strHost, strField, ref Value);
                    break;

                case Browsers.FIREFOX:
                    ret_val = GetCookie_FireFox(strHost, strField, ref Value);
                    break;

                case Browsers.IE:
                    ret_val = GetCookie_InternetExplorer(strHost, strField, ref Value);
                    break;
            }
            return ret_val;
        }

        private static string GetChromeCookiePath()
        {
            string s = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            s += @"\Google\Chrome\User Data\Default\cookies";

            if (!File.Exists(s))
                return string.Empty;

            return s;
        }

        private static string GetFireFoxCookiePath()
        {
            string s = Environment.GetFolderPath(
                             Environment.SpecialFolder.ApplicationData);
            s += @"\Mozilla\"; 

            IEnumerable<string> list = Directory.GetDirectories(s, "*default*", SearchOption.AllDirectories);
            try
            {
                if (!list.Cast<string>().Any())
                    return string.Empty;

                s = list.ElementAt(0) + @"\" + "cookies.sqlite";
            }
            catch (Exception)
            {
                return string.Empty;
            }

            if (!File.Exists(s))
                return string.Empty;

            return s;
        }

        private static bool GetCookie_InternetExplorer (string strHost, string strField, ref string Value)
        {
            Value = string.Empty;
            bool fRtn = false;
            string strPath, strCookie;
            string[] fp;
            StreamReader r;
            int idx;

            try
            {
                strField = strField + "\n";
                strPath = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
                Version v = Environment.OSVersion.Version;

                strPath += @"\low";

                fp = Directory.GetFiles(strPath, "*.txt");

                foreach (string path in fp)
                {
                    idx = -1;
                    r = File.OpenText(path);
                    strCookie = r.ReadToEnd();
                    r.Close();

                    if (System.Text.RegularExpressions.Regex.IsMatch(strCookie, strHost))
                    {
                        idx = strCookie.ToUpper().IndexOf(strField.ToUpper());
                    }

                    if (-1 < idx)
                    {
                        idx += strField.Length;
                        Value = strCookie.Substring(idx, strCookie.IndexOf('\n', idx) - idx);
                        if (!Value.Equals(string.Empty))
                        {
                            fRtn = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception) //File not found, etc...
            {
                Value = string.Empty;
                fRtn = false;
            }

            return fRtn;
        }

        private static bool GetCookie_Chrome(string strHost, string strField, ref string Value)
        {
            Value = string.Empty;
            bool fRtn = false;
            string strPath, strDb;

            // Check to see if Chrome Installed
            strPath = GetChromeCookiePath();
            if (string.Empty == strPath) // Nope, perhaps another browser
                return false;

            try
            {
                strDb = "Data Source=" + strPath + ";pooling=false";

                using (SQLiteConnection conn = new SQLiteConnection(strDb))
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT value FROM cookies WHERE host_key LIKE '%" +
                            strHost + "%' AND name LIKE '%" + strField + "%';";

                        conn.Open();
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Value = reader.GetString(0);
                                if (!Value.Equals(string.Empty))
                                {
                                    fRtn = true;
                                    break;
                                }
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception)
            {
                Value = string.Empty;
                fRtn = false;
            }
            return fRtn;
        }

        private static bool GetCookie_FireFox(string strHost, string strField, ref string Value)
        {
            Value = string.Empty;
            bool fRtn = false;
            string strPath, strTemp, strDb;
            strTemp = string.Empty;

            // Check to see if FireFox Installed
            strPath = GetFireFoxCookiePath();
            if (string.Empty == strPath) // Nope, perhaps another browser
                return false;

            try
            {
                // First copy the cookie jar so that we can read the cookies 
                // from unlocked copy while
                // FireFox is running
                strTemp = strPath + ".temp";
                strDb = "Data Source=" + strTemp + ";pooling=false";

                File.Copy(strPath, strTemp, true);

                // Now open the temporary cookie jar and extract Value from the cookie if
                // we find it.
                using (SQLiteConnection conn = new SQLiteConnection(strDb))
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT value FROM moz_cookies WHERE host LIKE '%" +
                            strHost + "%' AND name LIKE '%" + strField + "%';";

                        conn.Open();
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Value = reader.GetString(0);
                                if (!Value.Equals(string.Empty))
                                {
                                    fRtn = true;
                                    break;
                                }
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception)
            {
                Value = string.Empty;
                fRtn = false;
            }

            // All done clean up
            if (string.Empty != strTemp)
            {
                File.Delete(strTemp);
            }
            return fRtn;
        }
    }
}
