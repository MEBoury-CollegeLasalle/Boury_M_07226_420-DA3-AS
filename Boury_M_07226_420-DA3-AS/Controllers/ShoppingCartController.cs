/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Models;
using Boury_M_07226_420_DA3_AS.Utils;
using Boury_M_07226_420_DA3_AS.Views;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Boury_M_07226_420_DA3_AS.Controllers {
    internal class ShoppingCartController : IController {

        private ShoppingCartConsoleView consoleView;

        public ShoppingCartController() {
            this.consoleView = new ShoppingCartConsoleView();
        }

        public ShoppingCart CreateShoppingCart(int customerId, string billingAddress, string shippingAddress) {
            Customer customer = new Customer(customerId);
            customer.GetById();
            return CreateShoppingCart(customer, billingAddress, shippingAddress);
        }

        public ShoppingCart CreateShoppingCart(Customer customer, string billingAddress, string shippingAddress) {
            ShoppingCart newShoppingCart = new ShoppingCart(customer, billingAddress, shippingAddress);
            newShoppingCart.Insert();

            Console.WriteLine("CREATED SHOPPING CART:");
            this.DisplayShoppingCart(newShoppingCart);

            return newShoppingCart;
        }

        public void DisplayShoppingCart(int shoppingCartId) {
            ShoppingCart shoppingCart = new ShoppingCart(shoppingCartId);
            shoppingCart.GetById();
            this.DisplayShoppingCart(shoppingCart);
        }

        public void DisplayShoppingCart(ShoppingCart shoppingCart) {
            this.consoleView.Render(shoppingCart);
        }

        public ShoppingCart UpdateShoppingCart(int shoppingCartId, int customerId, string billingAddress, string shippingAddress) {
            Customer customer = new Customer(customerId);
            customer.GetById();
            return UpdateShoppingCart(shoppingCartId, customer, billingAddress, shippingAddress);
        }

        public ShoppingCart UpdateShoppingCart(int shoppingCartId, Customer customer, string billingAddress, string shippingAddress) {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try {
                    ShoppingCart shoppingCart = ShoppingCart.GetById(shoppingCartId, transaction, true);

                    Console.WriteLine("SHOPPING CART TO UPDATE:");
                    this.DisplayShoppingCart(shoppingCart);

                    shoppingCart.Customer = customer;
                    shoppingCart.BillingAddress = billingAddress;
                    shoppingCart.ShippingAddress = shippingAddress;
                    shoppingCart.Update();
                    transaction.Commit();

                    Console.WriteLine("UPDATED SHOPPING CART:");
                    this.DisplayShoppingCart(shoppingCart);

                    return shoppingCart;

                } catch (Exception e) {
                    transaction.Rollback();
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                    throw e;
                }
            }
        }

        public void DeleteShoppingCart(int shoppingCartId) {
            ShoppingCart shoppingCart = new ShoppingCart(shoppingCartId);
            this.DeleteShoppingCart(shoppingCart);
        }

        public void DeleteShoppingCart(ShoppingCart shoppingCart) {
            shoppingCart.Delete();

            Console.WriteLine("DELETED SHOPPING CART:");
            this.DisplayShoppingCart(shoppingCart);
        }
    }
}
