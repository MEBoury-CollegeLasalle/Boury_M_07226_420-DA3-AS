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
            cart.GetById();
            return cart;
        }


        #endregion



        #region Methods


        public void Delete() {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.Delete() : Id value is 0.");
            }

            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                string statement = $"DELETE FROM {DATABASE_TABLE_NAME} WHERE Id = @id;";
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = statement;

                SqlParameter param = cmd.CreateParameter();
                param.ParameterName = "@id";
                param.DbType = DbType.Int32;
                param.Value = this.Id;
                cmd.Parameters.Add(param);

                connection.Open();
                int affectedRows = cmd.ExecuteNonQuery();

                if (!(affectedRows > 0)) {
                    // No affected rows: no deletion occured -> row with matching Id not found
                    throw new Exception($"Failed to delete {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
                }
            }
        }

        public ShoppingCart GetById() {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.GetById() : Id value is 0.");
            }

            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} WHERE Id = @id;";
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = statement;

                SqlParameter param = cmd.CreateParameter();
                param.ParameterName = "@id";
                param.DbType = DbType.Int32;
                param.Value = this.Id;
                cmd.Parameters.Add(param);

                connection.Open();
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
            
        }

        public ShoppingCart Insert() {
            if (this.Id != 0) {
                throw new Exception($"Cannot use method {this.GetType().FullName}.Insert() : Id value is not 0 [{this.Id}].");
            }
            using (SqlConnection sqlConnection = DbUtils.GetDefaultConnection()) {

                // define the time of the creation (now). We will use it to set the value in the DB and
                // in the object here
                DateTime createTime = DateTime.Now;


                string insertCommandText = $"INSERT INTO {DATABASE_TABLE_NAME} (customerId, " +
                    "billingAddress, shippingAddress, dateCreated) " +
                    "VALUES (@customerId, @billingAddress, @shippingAddress, @dateCreated); " +
                    "SELECT CAST(SCOPE_IDENTITY() AS int);";

                SqlCommand cmd = sqlConnection.CreateCommand();
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


                sqlConnection.Open();
                this.Id = (Int32) cmd.ExecuteScalar();
                this.DateCreated = createTime;

                return this;
            }
        }

        public ShoppingCart Update() {
            if (this.Id == 0) {
                // Id has not been set, cannot update a product with no specific Id to track the correct db row.
                throw new Exception($"Cannot use method {this.GetType().FullName}.Update() : Id value is 0.");
            }

            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {

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

                SqlCommand cmd = connection.CreateCommand();
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

                connection.Open();
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
        }


        #endregion


    }
}
