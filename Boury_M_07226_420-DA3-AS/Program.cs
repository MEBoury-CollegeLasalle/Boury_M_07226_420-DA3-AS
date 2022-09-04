using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boury_M_07226_420_DA3_AS {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            DbUtils<SqlConnection> connector = new DbUtils<SqlConnection>();
            using (DbConnection connection = connector.GetDefaultFileDbConnection()) {
                connection.Open();
                Debug.WriteLine("Connection to lab.mdf opened successfully!");

            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
