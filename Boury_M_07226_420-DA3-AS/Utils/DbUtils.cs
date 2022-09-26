/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Utils {
    internal class DbUtils {

        private static readonly string DEFAULT_DATABASE_NAME = "db_lab";


        public static SqlConnection GetDefaultConnection() {

            // option 1 : connection string as a literal string
            string connectionStringLiteral = $"Server=.\\SQL2019EXPRESS; " +
                $"Integrated security=true; " +
                $"Database={DEFAULT_DATABASE_NAME};";


            SqlConnection connection = new SqlConnection(connectionStringLiteral);
            return connection;
        }
    }
}
