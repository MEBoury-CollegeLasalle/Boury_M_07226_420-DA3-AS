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
    public partial class CustomerGridView : Form, IView {

        private SqlConnection connection;
        private CustomerController customerController;

        public CustomerGridView(SqlConnection connection, CustomerController parentCustomerController) {
            this.connection = connection;
            this.customerController = parentCustomerController;
            InitializeComponent();
        }

        public void OpenWindow() {
            this.dataGridView1.DataSource = Customer.GetDataTable(this.connection);
            this.ShowDialog();
        }

        public void CloseWindow() {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonClose_Click(object sender, EventArgs e) {
            this.CloseWindow();
        }

        private void buttonSaveChanges_Click(object sender, EventArgs e) {
            this.customerController.UpdateCustomerDataSet();
        }
    }
}
