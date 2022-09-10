using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boury_M_07226_420_DA3_AS.Views {

    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public class TestView1 : Panel, IView {
        private Button switchToPanel2Button;
        private Label testLabel;

        public TestView1() : base() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            this.Location = new System.Drawing.Point(13, 13);
            this.Name = "TestView1";
            this.Size = new System.Drawing.Size(775, 425);
            this.TabIndex = 0;
            this.Visible = true;
            this.testLabel = new System.Windows.Forms.Label();
            this.switchToPanel2Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // testLabel
            // 
            this.testLabel.AutoSize = true;
            this.testLabel.Location = new System.Drawing.Point(0, 0);
            this.testLabel.Name = "testLabel";
            this.testLabel.Size = new System.Drawing.Size(150, 13);
            this.testLabel.TabIndex = 0;
            this.testLabel.Text = "Hello, I am a test label in panel 1.";
            // 
            // switchToPanel1Button
            // 
            this.switchToPanel2Button.Location = new System.Drawing.Point(0, 20);
            this.switchToPanel2Button.Name = "switchToPanel2Button";
            this.switchToPanel2Button.Size = new System.Drawing.Size(150, 23);
            this.switchToPanel2Button.TabIndex = 0;
            this.switchToPanel2Button.Text = "Switch to panel 2";
            this.switchToPanel2Button.UseVisualStyleBackColor = true;
            this.switchToPanel2Button.Click += new EventHandler(SwitchToPanel2Button_Click);
            // 
            // TestView1
            // 
            this.Controls.Add(this.testLabel);
            this.Controls.Add(this.switchToPanel2Button);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void SwitchToPanel2Button_Click(object sender, System.EventArgs e) {
            MainWindow mainWindow = (MainWindow) this.Parent;
            mainWindow.Controls.Remove(mainWindow.displayedPanel);
            mainWindow.displayedPanel = new TestView2();
            mainWindow.Controls.Add(mainWindow.displayedPanel);
        }
    }
}
