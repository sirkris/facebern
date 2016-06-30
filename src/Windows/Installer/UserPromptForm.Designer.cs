namespace Installer
{
    partial class UserPromptForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserPromptForm));
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.weCanDoThisTogether = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.labelInstalledPath = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.updateRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.uninstallRadioButton = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.weCanDoThisTogether)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(749, 372);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 15;
            this.button2.Text = "Next >";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(844, 372);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 14;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(99, 359);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(0, 13);
            this.labelVersion.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 359);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Installer Version:";
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(4, 355);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(916, 2);
            this.label2.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Montserrat", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(451, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(406, 25);
            this.label1.TabIndex = 9;
            this.label1.Text = "Welcome to the Birdie Setup Wizard!";
            // 
            // weCanDoThisTogether
            // 
            this.weCanDoThisTogether.Image = global::Installer.Properties.Resources.together;
            this.weCanDoThisTogether.Location = new System.Drawing.Point(4, 6);
            this.weCanDoThisTogether.Name = "weCanDoThisTogether";
            this.weCanDoThisTogether.Size = new System.Drawing.Size(436, 350);
            this.weCanDoThisTogether.TabIndex = 8;
            this.weCanDoThisTogether.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(453, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(174, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Birdie is currently installed in:";
            // 
            // labelInstalledPath
            // 
            this.labelInstalledPath.AutoSize = true;
            this.labelInstalledPath.Location = new System.Drawing.Point(456, 76);
            this.labelInstalledPath.Name = "labelInstalledPath";
            this.labelInstalledPath.Size = new System.Drawing.Size(0, 13);
            this.labelInstalledPath.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(453, 108);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(192, 16);
            this.label5.TabIndex = 18;
            this.label5.Text = "What would you like to do?";
            // 
            // updateRadioButton
            // 
            this.updateRadioButton.AutoSize = true;
            this.updateRadioButton.Checked = true;
            this.updateRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updateRadioButton.ForeColor = System.Drawing.Color.Green;
            this.updateRadioButton.Location = new System.Drawing.Point(0, 3);
            this.updateRadioButton.Name = "updateRadioButton";
            this.updateRadioButton.Size = new System.Drawing.Size(102, 17);
            this.updateRadioButton.TabIndex = 19;
            this.updateRadioButton.TabStop = true;
            this.updateRadioButton.Text = "Update Birdie";
            this.updateRadioButton.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.uninstallRadioButton);
            this.panel1.Controls.Add(this.updateRadioButton);
            this.panel1.Location = new System.Drawing.Point(456, 142);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(456, 210);
            this.panel1.TabIndex = 20;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 108);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(160, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Remove Birdie from your system.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(361, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Check for a newer version of Birdie.  If one is available, perform the update.";
            // 
            // uninstallRadioButton
            // 
            this.uninstallRadioButton.AutoSize = true;
            this.uninstallRadioButton.ForeColor = System.Drawing.Color.Red;
            this.uninstallRadioButton.Location = new System.Drawing.Point(0, 85);
            this.uninstallRadioButton.Name = "uninstallRadioButton";
            this.uninstallRadioButton.Size = new System.Drawing.Size(94, 17);
            this.uninstallRadioButton.TabIndex = 20;
            this.uninstallRadioButton.Text = "Uninstall Birdie";
            this.uninstallRadioButton.UseVisualStyleBackColor = true;
            // 
            // UserPromptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 401);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.labelInstalledPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.weCanDoThisTogether);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserPromptForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Birdie Installation Wizard";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.UserPromptForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.weCanDoThisTogether)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox weCanDoThisTogether;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelInstalledPath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.RadioButton updateRadioButton;
        public System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.RadioButton uninstallRadioButton;
    }
}