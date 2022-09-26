using Boury_M_07226_420_DA3_AS.Controllers;
using Boury_M_07226_420_DA3_AS.Models;
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
    public partial class ShoppingCartGridView : Form, IView {

        private readonly SqlConnection connection;
        private readonly ShoppingCartController shoppingCartController;

        public ShoppingCartGridView(SqlConnection connection, ShoppingCartController parentShoppingCartController) {
            this.connection = connection;
            this.shoppingCartController = parentShoppingCartController;
            InitializeComponent();
        }

        public void OpenWindow() {
            this.dataGridView1.DataSource = Product.GetDataTable(this.connection);
            this.ShowDialog();
        }

        public void CloseWindow() {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonSaveData_Click(object sender, EventArgs e) {
            this.shoppingCartController.UpdateShoppingCartDataSet();
        }

        private void buttonClose_Click(object sender, EventArgs e) {
            this.CloseWindow();
        }
    }
}
