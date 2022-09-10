using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Models {
    internal class ShoppingCart : IModel<ShoppingCart> {

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateOrdered { get; set; }
        public DateTime DateShipped { get; set; }


        public void Delete() {
            if (this.Id == 0) {
                throw new Exception("Deletion of objects requires an ID to be set already.");
            }
            using (SqlConnection conn = DbUtils<SqlConnection>.GetDefaultConnection()) {
                string deleteCommandText = $"DELETE FROM dbo.ShoppingCart WHERE Id = {this.Id};";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = deleteCommandText;

                int affectedRows = cmd.ExecuteNonQuery();
                if (affectedRows < 1) {
                    throw new Exception($"Deletion failed in the database: no row found for id# {this.Id}.");
                }
            }
        }

        public ShoppingCart GetById() {
            if (this.Id == 0) {
                throw new Exception("Fetching of objects require an ID to be set already.");
            }
            using (SqlConnection conn  = DbUtils<SqlConnection>.GetDefaultConnection()) {
                string fetchCommandText = $"SELECT * FROM dbo.ShoppingCart WHERE Id = {this.Id};";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = fetchCommandText;

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows) {
                    // Since we're limiting the instruction by the unique primary key,
                    // there can be only one row returned, no need for while()
                    reader.Read();

                    // I do not deal with the ID at all since we already have it.
                    this.CustomerId = reader.GetInt32(1);
                    this.BillingAddress = reader.GetString(2);
                    this.ShippingAddress = reader.GetString(3);
                    this.DateCreated = reader.GetDateTime(4);
                    if (!reader.IsDBNull(5)) {
                        this.DateUpdated = reader.GetDateTime(5);
                    }
                    if (!reader.IsDBNull(6)) {
                        this.DateOrdered = reader.GetDateTime(6);
                    }
                    if (!reader.IsDBNull(7)) {
                        this.DateShipped = reader.GetDateTime(7);
                    }

                    return this;

                } else {
                    throw new Exception("No data found in database for id# " + this.Id);
                }
            }
        }

        public ShoppingCart Insert() {
            if (this.Id != 0) {
                throw new Exception("Insertion of objects requires instances to NOT have an ID already.");
            }
            using (SqlConnection sqlConnection = DbUtils<SqlConnection>.GetDefaultConnection()) {
                string insertCommandText = "INSERT INTO dbo.ShoppingCart (customerId, " +
                    "billingAddress, shippingAddress, dateCreated) " +
                    $"VALUES ({this.CustomerId}, {this.BillingAddress}, {this.ShippingAddress}, {this.DateCreated.ToString()}); " +
                    "SELECT CAST(SCOPE_IDENTITY() AS int);";

                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = insertCommandText;
                sqlConnection.Open();
                int newId = (Int32) command.ExecuteScalar();
                this.Id = newId;

                return this;
            }
        }

        public ShoppingCart Update() {
            if (this.Id == 0) {
                throw new Exception("Update of objects requires an ID to be set already.");
            }
            using (SqlConnection conn = DbUtils<SqlConnection>.GetDefaultConnection()) {
                string updateCommandText = $"UPDATE dbo.ShoppingCart SET " +
                    $"customerId = {this.CustomerId}, " +
                    $"billingAddress = {this.BillingAddress}, " +
                    $"shippingAddress = {this.DateCreated.ToString()}";
                if (this.DateUpdated != DateTime.MinValue) {
                    updateCommandText += $", dateUpdated = {this.DateUpdated}";
                }
                if (this.DateOrdered != DateTime.MinValue) {
                    updateCommandText += $", dateOrdered = {this.DateOrdered}";
                }
                if (this.DateShipped != DateTime.MinValue) {
                    updateCommandText += $", dateShipped = {this.DateShipped}";
                }
                updateCommandText += $" WHERE Id = {this.Id};";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = updateCommandText;

                int affectedRows = cmd.ExecuteNonQuery();
                if (affectedRows < 1) {
                    throw new Exception($"Update failed in the database: no row found for id# {this.Id}.");
                }
            }
        }
    }
}
