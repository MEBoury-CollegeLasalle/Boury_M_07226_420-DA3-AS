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
    public partial class ProductGridView : Form, IView {

        private SqlConnection connection;
        private ProductController productController;

        public ProductGridView(SqlConnection connection, ProductController parentProductController) {
            this.connection = connection;
            this.productController = parentProductController;
            InitializeComponent();
        }

        public void OpenWindow() {
            this.dataGridView1.DataSource = Product.GetDataTable(this.connection);
            this.ShowDialog();
        }

        public void CloseWindow() {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonClose_Click(object sender, EventArgs e) {
            this.CloseWindow();
        }

        private void buttonSaveData_Click(object sender, EventArgs e) {
            this.productController.UpdateProductDataSet();
        }
    }
}
