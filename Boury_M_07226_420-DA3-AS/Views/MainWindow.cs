/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Controllers;
using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boury_M_07226_420_DA3_AS.Views {

    public partial class MainWindow : Form {

        // We hold the connection object and all the required controllers in this main-menu style class
        // They could be held in some other service-type or manager class, but this will do for now.
        private readonly SqlConnection connection;
        private readonly CustomerController customerController;
        private readonly ProductController productController;
        private readonly ShoppingCartController shoppingCartController;

        public MainWindow() {
            // create a connection object and set it in the appropriate field for future use.
            this.connection = DbUtils.GetDefaultConnection();
            // create controller objects and set them in their respective fields for future use.
            this.customerController = new CustomerController(this.connection);
            this.productController = new ProductController(this.connection);
            this.shoppingCartController = new ShoppingCartController(this.connection);
            InitializeComponent();
        }

        /// <summary>
        /// Manage Customers button click event handler. Calls the <see cref="CustomerController.OpenCustomerGridViewWindow"/>
        /// method to open the customer data grid view modal window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonManageCustomers_Click(object sender, EventArgs e) {
            this.customerController.OpenCustomerGridViewWindow();
        }

        /// <summary>
        /// Manage products button click event handler. Calls the <see cref="ProductController.OpenProductGridViewWindow"/>
        /// method to open the product data grid view modal window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonManageProducts_Click(object sender, EventArgs e) {
            this.productController.OpenProductGridViewWindow();
        }

        /// <summary>
        /// Manage Shopping Carts button click event handler. Calls the 
        /// <see cref="ShoppingCartController.OpenShoppingCartGridViewWindow"/> method to open 
        /// the shopping cart data grid view modal window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonManageShoppingCarts_Click(object sender, EventArgs e) {
            this.shoppingCartController.OpenShoppingCartGridViewWindow();
        }

        /// <summary>
        /// Exit button click event handler. Shuts down the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e) {
            Application.Exit();
        }
    }
}
