using Boury_M_07226_420_DA3_AS.Utils;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Boury_M_07226_420_DA3_AS.Views {
    partial class MainWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.buttonManageCustomers = new System.Windows.Forms.Button();
            this.buttonManageProducts = new System.Windows.Forms.Button();
            this.buttonManageShoppingCarts = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonManageCustomers
            // 
            this.buttonManageCustomers.Location = new System.Drawing.Point(302, 66);
            this.buttonManageCustomers.Name = "buttonManageCustomers";
            this.buttonManageCustomers.Size = new System.Drawing.Size(189, 32);
            this.buttonManageCustomers.TabIndex = 0;
            this.buttonManageCustomers.Text = "Manage Customers";
            this.buttonManageCustomers.UseVisualStyleBackColor = true;
            this.buttonManageCustomers.Click += new System.EventHandler(this.buttonManageCustomers_Click);
            // 
            // buttonManageProducts
            // 
            this.buttonManageProducts.Location = new System.Drawing.Point(302, 104);
            this.buttonManageProducts.Name = "buttonManageProducts";
            this.buttonManageProducts.Size = new System.Drawing.Size(189, 32);
            this.buttonManageProducts.TabIndex = 1;
            this.buttonManageProducts.Text = "Manage Products";
            this.buttonManageProducts.UseVisualStyleBackColor = true;
            this.buttonManageProducts.Click += new System.EventHandler(this.buttonManageProducts_Click);
            // 
            // buttonManageShoppingCarts
            // 
            this.buttonManageShoppingCarts.Location = new System.Drawing.Point(302, 142);
            this.buttonManageShoppingCarts.Name = "buttonManageShoppingCarts";
            this.buttonManageShoppingCarts.Size = new System.Drawing.Size(189, 32);
            this.buttonManageShoppingCarts.TabIndex = 2;
            this.buttonManageShoppingCarts.Text = "Manage Shopping Carts";
            this.buttonManageShoppingCarts.UseVisualStyleBackColor = true;
            this.buttonManageShoppingCarts.Click += new System.EventHandler(this.buttonManageShoppingCarts_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(302, 210);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(189, 32);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Exit";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonManageShoppingCarts);
            this.Controls.Add(this.buttonManageProducts);
            this.Controls.Add(this.buttonManageCustomers);
            this.Name = "MainWindow";
            this.Text = "Iterative Lab window";
            this.ResumeLayout(false);

        }

        #endregion

        private Button buttonManageCustomers;
        private Button buttonManageProducts;
        private Button buttonManageShoppingCarts;
        private Button buttonClose;
    }
}

