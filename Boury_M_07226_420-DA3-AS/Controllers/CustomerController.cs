/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Controllers {
    internal class CustomerController : IController {


        public void CreateCustomer(string email) {
            CreateCustomer(email, "", "");
        }

        public void CreateCustomer(string email, string firstName, string lastName) {
            Customer customer = new Customer(email, firstName, lastName);
            customer.Insert();
        }

        public void DisplayCustomer(int customerId) {
            Customer customer = new Customer(customerId);
            customer.GetById();
            this.DisplayCustomer(customer);
        }

        public void DisplayCustomer(Customer customer) {
            // do something later in the course when we have views, or, if you want,
            // dump the data to the console.
        }

        public void UpdateCustomer(int customerId, string firstName, string lastName, string email) {
            Customer customer = Customer.GetById(customerId);
            customer.FirstName = firstName;
            customer.LastName = lastName;
            customer.Email = email;
            customer.Update();
        }

        public void DeleteCustomer(int customerId) {
            Customer customer = new Customer(customerId);
            this.DeleteCustomer(customer);
        }

        public void DeleteCustomer(Customer customer) {
            customer.Delete();
        }
    }
}
