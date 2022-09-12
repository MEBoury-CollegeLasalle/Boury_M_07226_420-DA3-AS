/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Models {
    internal class Customer : IModel<Customer> {

        private static readonly string DATABASE_TABLE_NAME = "dbo.Customer";

        private string _firstName;
        private string _lastName;
        private string _email;


        public int Id { get; set; } 
        public string FirstName { 
            get { return this._firstName; } 
            set {
                if (value.Length > 50) {
                    throw new Exception($"Field validation exception: length of value " +
                        $"for {this.GetType().FullName}.FirstName must be less or equal " +
                        $"than 50 charaters. Received: {value.Length}.");
                }
                this._firstName = value;
            }
        }
        public string LastName {
            get { return this._lastName; }
            set {
                if (value.Length > 50) {
                    throw new Exception($"Field validation exception: length of value " +
                        $"for {this.GetType().FullName}.LastName must be less or equal " +
                        $"than 50 charaters. Received: {value.Length}.");
                }
                this._lastName = value;
            }
        }
        public string Email {
            get { return this._email; }
            set {
                if (value.Length > 128) {
                    throw new Exception($"Field validation exception: length of value " +
                        $"for {this.GetType().FullName}.Email must be less or equal " +
                        $"than 128 charaters. Received: {value.Length}.");
                }
                this._email = value;
            }
        }
        public DateTime CreatedAt { get; set; }
        public DateTime DeletedAt { get; set; }


        public Customer(int id) {
            this.Id = id;
        }

        public Customer(string email) : this(email, "", "") {

        }

        public Customer(string email, string firstName, string lastName) {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
        }


        #region Static Methods


        public static Customer GetById(int id) {
            Customer customer = new Customer(id);
            return customer.GetById();
        }


        public static Customer GetById(int id, SqlTransaction transaction, bool withExclusiveLock = false) {
            Customer customer = new Customer(id);
            return customer.GetById(transaction, withExclusiveLock);
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



        public Customer GetById() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteGetByIdCommand(connection.CreateCommand());
            }
        }

        public Customer GetById(SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteGetByIdCommand(cmd, withExclusiveLock);
        }

        private Customer ExecuteGetByIdCommand(SqlCommand cmd, bool withExclusiveLock = false) {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteGetByIdCommand() : Id value is 0.");
            }

            string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} " +
                (withExclusiveLock ? "WITH  (XLOCK, ROWLOCK) " : "") +
                "WHERE Id = @id;";
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

                // firstName and lastName can be NULL in the database
                if (!reader.IsDBNull(1)) {
                    this.FirstName = reader.GetString(1);
                }
                if (!reader.IsDBNull(2)) {
                    this.LastName = reader.GetString(2);
                }
                this.Email = reader.GetString(3);
                this.CreatedAt = reader.GetDateTime(4);
                // deletedAt can be NULL in the database
                if (!reader.IsDBNull(5)) {
                    this.DeletedAt = reader.GetDateTime(5);
                }

                return this;

            } else {
                throw new Exception($"No database entry for {this.GetType().FullName} with id# {this.Id}.");
            }

        }



        public Customer Insert() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteInsertCommand(connection.CreateCommand());
            }
        }

        public Customer Insert(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteInsertCommand(cmd);
        }

        private Customer ExecuteInsertCommand(SqlCommand cmd) {
            if (this.Id > 0) {
                // Id has been set, cannot insert a product with a specific Id without risking
                // to mess up the database.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteInsertCommand() : Id value is not 0 [{this.Id}].");
            }

            // define the time of the creation (now). We will use it to set the value in the DB and
            // in the object here
            DateTime createTime = DateTime.Now;

            // Create the INSERT statement. We do not pass any Id value since this is insertion
            // and the id is auto-generated by the database on insertion (identity).
            string statement = $"INSERT INTO {DATABASE_TABLE_NAME} (firstName, lastName, email, createdAt) " +
                "VALUES (@firstName, @lastName, @email, @createdAt); SELECT CAST(SCOPE_IDENTITY() AS int);";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter firstNameParam = cmd.CreateParameter();
            firstNameParam.ParameterName = "@firstName";
            firstNameParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.FirstName)) {
                firstNameParam.Value = DBNull.Value;
            } else {
                firstNameParam.Value = this.FirstName;
            }
            cmd.Parameters.Add(firstNameParam);

            SqlParameter lastNameParam = cmd.CreateParameter();
            lastNameParam.ParameterName = "@lastName";
            lastNameParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.LastName)) {
                lastNameParam.Value = DBNull.Value;
            } else {
                lastNameParam.Value = this.LastName;
            }
            cmd.Parameters.Add(lastNameParam);

            SqlParameter emailParam = cmd.CreateParameter();
            emailParam.ParameterName = "@email";
            emailParam.DbType = DbType.String;
            emailParam.Value = this.Email;
            cmd.Parameters.Add(emailParam);

            SqlParameter createdAtParam = cmd.CreateParameter();
            createdAtParam.ParameterName = "@createdAt";
            createdAtParam.DbType = DbType.DateTime;
            createdAtParam.Value = createTime;
            cmd.Parameters.Add(createdAtParam);

            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            this.Id = (Int32)cmd.ExecuteScalar();
            this.CreatedAt = createTime;

            return this;

        }



        public Customer Update() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteUpdateCommand(connection.CreateCommand());
            }
        }

        public Customer Update(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteUpdateCommand(cmd);
        }

        private Customer ExecuteUpdateCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, cannot update a product with no specific Id to track the correct db row.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteUpdateCommand() : Id value is 0.");
            }

            // Create the Update statement.
            string statement = $"UPDATE {DATABASE_TABLE_NAME} SET " +
                "firstName = @firstName, " +
                "lastName = @lastName, " +
                "email = @email " +
                "WHERE Id = @id;";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter firstNameParam = cmd.CreateParameter();
            firstNameParam.ParameterName = "@firstName";
            firstNameParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.FirstName)) {
                firstNameParam.Value = DBNull.Value;
            } else {
                firstNameParam.Value = this.FirstName;
            }
            cmd.Parameters.Add(firstNameParam);

            SqlParameter lastNameParam = cmd.CreateParameter();
            lastNameParam.ParameterName = "@lastName";
            lastNameParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.LastName)) {
                firstNameParam.Value = DBNull.Value;
            } else {
                firstNameParam.Value = this.LastName;
            }
            cmd.Parameters.Add(lastNameParam);

            SqlParameter emailParam = cmd.CreateParameter();
            emailParam.ParameterName = "@email";
            emailParam.DbType = DbType.String;
            emailParam.Value = this.Email;
            cmd.Parameters.Add(emailParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            // Check that a row has been updated, if not, throw exception (no row with the id
            // value found in the database, thus no update done)
            if (!(affectedRows > 0)) {
                throw new Exception($"Failed to update {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
            }

            return this;
        }

        #endregion


    }
}
