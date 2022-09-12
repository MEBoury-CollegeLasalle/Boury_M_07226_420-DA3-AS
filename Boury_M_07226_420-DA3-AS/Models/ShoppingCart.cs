/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Models {
    internal class ShoppingCart : IModel<ShoppingCart> {

        private static readonly string DATABASE_TABLE_NAME = "dbo.ShoppingCart";

        public int Id { get; set; }
        public Customer Customer { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateOrdered { get; set; }
        public DateTime DateShipped { get; set; }


        public ShoppingCart(int id) {
            this.Id = id;
        }

        public ShoppingCart(Customer customer, string billingAddress, string shippingAddress) {
            this.Customer = customer;
            this.BillingAddress = billingAddress;
            this.ShippingAddress = shippingAddress;
        }


        #region Static Methods


        public static ShoppingCart GetById(int id) {
            ShoppingCart cart = new ShoppingCart(id);
            return cart.GetById();
        }
        public static ShoppingCart GetById(int id, SqlTransaction transaction, bool withExclusiveLock = false) {
            ShoppingCart cart = new ShoppingCart(id);
            return cart.GetById(transaction, withExclusiveLock);
        }


        #endregion



        #region Methods


        public void Delete() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                this.ExecuteDeleteCommand(connection.CreateCommand());
            }
        }

        public void Delete(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            this.ExecuteDeleteCommand(cmd);
        }

        private void ExecuteDeleteCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteDeleteCommand() : Id value is 0.");
            }

            string statement = $"DELETE FROM {DATABASE_TABLE_NAME} WHERE Id = @id;";
            cmd.CommandText = statement;

            SqlParameter param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.DbType = DbType.Int32;
            param.Value = this.Id;
            cmd.Parameters.Add(param);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            if (!(affectedRows > 0)) {
                // No affected rows: no deletion occured -> row with matching Id not found
                throw new Exception($"Failed to delete {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
            }
        }



        public ShoppingCart GetById() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteGetByIdCommand(connection.CreateCommand());
            }
        }

        public ShoppingCart GetById(SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteGetByIdCommand(cmd, withExclusiveLock);
        }

        private ShoppingCart ExecuteGetByIdCommand(SqlCommand cmd, bool withExclusiveLock = false) {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteGetByIdCommand() : Id value is 0.");
            }

            string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} " +
                (withExclusiveLock ? "WITH  (XLOCK, ROWLOCK) " : "") +
                $"WHERE Id = @id;";
            cmd.CommandText = statement;

            SqlParameter param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.DbType = DbType.Int32;
            param.Value = this.Id;
            cmd.Parameters.Add(param);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();

                // I do not deal with the ID at all since we already have it.
                this.Customer = new Customer(reader.GetInt32(1)).GetById();
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
                throw new Exception($"No database entry for {this.GetType().FullName} with id# {this.Id}.");
            }
        }



        public ShoppingCart Insert() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteInsertCommand(connection.CreateCommand());
            }
        }

        public ShoppingCart Insert(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteInsertCommand(cmd);
        }

        private ShoppingCart ExecuteInsertCommand(SqlCommand cmd) {
            if (this.Id != 0) {
                throw new Exception($"Cannot use method {this.GetType().FullName}.Insert() : Id value is not 0 [{this.Id}].");
            }

            // define the time of the creation (now). We will use it to set the value in the DB and
            // in the object here
            DateTime createTime = DateTime.Now;


            string insertCommandText = $"INSERT INTO {DATABASE_TABLE_NAME} (customerId, " +
                "billingAddress, shippingAddress, dateCreated) " +
                "VALUES (@customerId, @billingAddress, @shippingAddress, @dateCreated); " +
                "SELECT CAST(SCOPE_IDENTITY() AS int);";
            cmd.CommandText = insertCommandText;

            // Create and add parameters
            SqlParameter customerIdParam = cmd.CreateParameter();
            customerIdParam.ParameterName = "@customerId";
            customerIdParam.DbType = DbType.Int32;
            customerIdParam.Value = this.Customer.Id;
            cmd.Parameters.Add(customerIdParam);

            SqlParameter billingAddrParam = cmd.CreateParameter();
            billingAddrParam.ParameterName = "@billingAddress";
            billingAddrParam.DbType = DbType.String;
            billingAddrParam.Value = this.BillingAddress;
            cmd.Parameters.Add(billingAddrParam);

            SqlParameter shippingAddrParam = cmd.CreateParameter();
            shippingAddrParam.ParameterName = "@shippingAddress";
            shippingAddrParam.DbType = DbType.String;
            shippingAddrParam.Value = this.ShippingAddress;
            cmd.Parameters.Add(shippingAddrParam);

            SqlParameter dateUpdatedParam = cmd.CreateParameter();
            dateUpdatedParam.ParameterName = "@dateCreated";
            dateUpdatedParam.DbType = DbType.DateTime;
            dateUpdatedParam.Value = createTime;
            cmd.Parameters.Add(dateUpdatedParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            this.Id = (Int32)cmd.ExecuteScalar();
            this.DateCreated = createTime;

            return this;

        }



        public ShoppingCart Update() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteUpdateCommand(connection.CreateCommand());
            }
        }

        public ShoppingCart Update(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteUpdateCommand(cmd);
        }

        private ShoppingCart ExecuteUpdateCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, cannot update a product with no specific Id to track the correct db row.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteUpdateCommand() : Id value is 0.");
            }

            // define the time of the update (now). We will use it to set the value in the DB and
            // in the object here
            DateTime updateTime = DateTime.Now;

            // Create the Update statement.
            string updateCommandText = $"UPDATE {DATABASE_TABLE_NAME} SET " +
                "customerId = @customerId, " +
                "billingAddress = @billingAddress, " +
                "shippingAddress = @shippingAddress, " +
                "dateUpdated = @dateUpdated, " +
                "dateOrdered = @dateOrdered, " +
                "dateShipped = @dateShipped " +
                "WHERE Id = @id;";
            cmd.CommandText = updateCommandText;

            // Create and add parameters
            SqlParameter customerIdParam = cmd.CreateParameter();
            customerIdParam.ParameterName = "@customerId";
            customerIdParam.DbType = DbType.Int32;
            customerIdParam.Value = this.Customer.Id;
            cmd.Parameters.Add(customerIdParam);

            SqlParameter billingAddrParam = cmd.CreateParameter();
            billingAddrParam.ParameterName = "@billingAddress";
            billingAddrParam.DbType = DbType.String;
            billingAddrParam.Value = this.BillingAddress;
            cmd.Parameters.Add(billingAddrParam);

            SqlParameter shippingAddrParam = cmd.CreateParameter();
            shippingAddrParam.ParameterName = "@shippingAddress";
            shippingAddrParam.DbType = DbType.String;
            shippingAddrParam.Value = this.ShippingAddress;
            cmd.Parameters.Add(shippingAddrParam);

            SqlParameter dateUpdatedParam = cmd.CreateParameter();
            dateUpdatedParam.ParameterName = "@dateUpdated";
            dateUpdatedParam.DbType = DbType.DateTime;
            dateUpdatedParam.Value = updateTime;
            cmd.Parameters.Add(dateUpdatedParam);

            SqlParameter dateOrderedParam = cmd.CreateParameter();
            dateOrderedParam.ParameterName = "@dateOrdered";
            dateOrderedParam.DbType = DbType.DateTime;
            if (this.DateOrdered == DateTime.MinValue) {
                dateOrderedParam.Value = DBNull.Value;
            } else {
                dateOrderedParam.Value = this.DateOrdered;
            }
            cmd.Parameters.Add(dateOrderedParam);

            SqlParameter dateShippedParam = cmd.CreateParameter();
            dateShippedParam.ParameterName = "@dateShipped";
            dateShippedParam.DbType = DbType.DateTime;
            if (this.DateShipped == DateTime.MinValue) {
                dateOrderedParam.Value = DBNull.Value;
            } else {
                dateOrderedParam.Value = this.DateShipped;
            }
            cmd.Parameters.Add(dateShippedParam);

            SqlParameter wheteIdParam = cmd.CreateParameter();
            wheteIdParam.ParameterName = "@id";
            wheteIdParam.DbType = DbType.Int32;
            wheteIdParam.Value = this.Id;
            cmd.Parameters.Add(wheteIdParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            // Check that a row has been updated, if not, throw exception (no row with the id
            // value found in the database, thus no update done)
            if (affectedRows < 1) {
                throw new Exception($"Failed to update {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
            }

            // database update has completed successfully at this point. update the property in the
            // object to the dateUpdated value we sent to the database
            this.DateUpdated = updateTime;

            return this;

        }


        #endregion


    }
}
