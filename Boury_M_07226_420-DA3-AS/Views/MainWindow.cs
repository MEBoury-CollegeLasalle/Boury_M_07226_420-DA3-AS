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

        private readonly SqlConnection connection;
        private readonly CustomerController customerController;
        private readonly ProductController productController;
        private readonly ShoppingCartController shoppingCartController;

        public MainWindow() {
            this.connection = DbUtils.GetDefaultConnection();
            this.customerController = new CustomerController(this.connection);
            this.productController = new ProductController(this.connection);
            this.shoppingCartController = new ShoppingCartController(this.connection);
            InitializeComponent();
        }

        private void buttonManageCustomers_Click(object sender, EventArgs e) {
            this.customerController.OpenCustomerGridViewWindow();
        }

        private void buttonManageProducts_Click(object sender, EventArgs e) {
            this.productController.OpenProductGridViewWindow();
        }

        private void buttonManageShoppingCarts_Click(object sender, EventArgs e) {
            this.shoppingCartController.OpenShoppingCartGridViewWindow();
        }

        private void buttonClose_Click(object sender, EventArgs e) {
            Application.Exit();
        }
    }
}
