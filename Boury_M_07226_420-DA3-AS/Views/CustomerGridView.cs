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

        /// <summary>
        /// This field holds the <see cref="SqlConnection"/> object received in the 
        /// view's constructor for future use inside this class.
        /// </summary>
        private readonly SqlConnection connection;
        /// <summary>
        /// This field holds the <see cref="CustomerController"/> object received in the 
        /// controller's constructor for future use inside this class.
        /// </summary>
        private readonly CustomerController customerController;

        public CustomerGridView(SqlConnection connection, CustomerController parentCustomerController) {
            this.connection = connection;
            this.customerController = parentCustomerController;
            InitializeComponent();
        }

        /// <summary>
        /// Binds the <see cref="DataGridView"/> component to the customer class's <see cref="DataTable"/>
        /// object and thens open the window as a modal window.
        /// </summary>
        public void OpenWindow() {
            this.dataGridView1.DataSource = Customer.GetDataTable(this.connection);
            this.ShowDialog();
        }

        /// <summary>
        /// Closes the current modal window.
        /// </summary>
        public void CloseWindow() {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Close button click event handler: calls <see cref="CustomerGridView.CloseWindow"/> to close the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e) {
            this.CloseWindow();
        }

        /// <summary>
        /// Save Changes button click event handler: calls <see cref="CustomerController.UpdateCustomerDataSet"/> to
        /// trigger the push of modifications to the data source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveChanges_Click(object sender, EventArgs e) {
            this.customerController.UpdateCustomerDataSet();
        }
    }
}
