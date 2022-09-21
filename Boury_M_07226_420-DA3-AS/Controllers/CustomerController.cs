/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Models;
using Boury_M_07226_420_DA3_AS.Utils;
using Boury_M_07226_420_DA3_AS.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Controllers {
    internal class CustomerController : IController {

        private CustomerConsoleView consoleView;


        public CustomerController() {
            this.consoleView = new CustomerConsoleView();
        }


        public Customer CreateCustomer(string email) {
            return CreateCustomer(email, "", "");
        }

        public Customer CreateCustomer(string email, string firstName, string lastName) {
            Customer customer = new Customer(email, firstName, lastName);
            customer.Insert();

            Console.WriteLine("CREATED CUSTOMER:");
            this.DisplayCustomer(customer);
            return customer;
        }

        public void DisplayCustomer(int customerId) {
            Customer customer = new Customer(customerId);
            customer.GetById();
            this.DisplayCustomer(customer);
        }

        public void DisplayCustomer(Customer customer) {
            this.consoleView.Render(customer);
        }

        public Customer UpdateCustomer(int customerId, string firstName, string lastName, string email) {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try {
                    Customer customer = Customer.GetById(customerId, transaction, true);

                    Console.WriteLine("CUSTOMER TO UPDATE:");
                    this.DisplayCustomer(customer);

                    customer.FirstName = firstName;
                    customer.LastName = lastName;
                    customer.Email = email;
                    customer.Update(transaction);
                    transaction.Commit();

                    Console.WriteLine("UPDATED CUSTOMER:");
                    this.DisplayCustomer(customer);

                    return customer;

                } catch (Exception e) {
                    transaction.Rollback();
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                    throw e;
                }
            }
        }

        public void DeleteCustomer(int customerId) {
            Customer customer = new Customer(customerId);
            this.DeleteCustomer(customer);
        }

        public void DeleteCustomer(Customer customer) {
            customer.Delete();

            Console.WriteLine("DELETED CUSTOMER:");
            this.DisplayCustomer(customer);
        }
    }
}
