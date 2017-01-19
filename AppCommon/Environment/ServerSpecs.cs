using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Data;
using System.Data.SqlClient;

namespace AppCommon.Environment
{
    public static class ServerSpecs
    {
        /// <summary>
        /// Get the operating system of the current server
        /// </summary>
        /// <returns></returns>
        public static string GetServerOS()
        {
            return System.Environment.OSVersion.ToString();
        }

        /// <summary>
        /// Get the amount of RAM capacity of the current server
        /// </summary>
        public static string GetServerMemory()
        {
            double ramAmount = -1;
            try
            {
                ManagementObjectSearcher search = new ManagementObjectSearcher("Select * From Win32_PhysicalMemory");
                foreach (ManagementObject ram in search.Get())
                {
                    ramAmount += Convert.ToDouble(ram.GetPropertyValue("Capacity")) / 1073741824;
                }
            }
            catch { }

            return string.Format("{0} GB", ramAmount);
        }

        /// <summary>
        /// Return the SQL Server database version 
        /// </summary>
        /// <returns></returns>
        public static string GetSqlServerDatabaseVersion(string connectionString)
        {
            string version = "Unknown";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SELECT @@VERSION AS Version", connection);
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                version = reader.GetString(reader.GetOrdinal("Version"));
                            }
                        }
                    }
                }
            }
            catch { }

            return version;
        }
    }
}
