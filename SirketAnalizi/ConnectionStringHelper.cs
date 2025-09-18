using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KapsamDashboard.UI
{
    public static class ConnectionStringHelper
    {
        public static string GetConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SQLSERVER"].ConnectionString;
            return connectionString;
        }
    }
}
