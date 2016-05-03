namespace Installer
{
    partial class WorkflowForm2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowForm2));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.labelLocation = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.branchComboBox = new System.Windows.Forms.ComboBox();
            this.includeSrcCheckbox = new System.Windows.Forms.CheckBox();
            this.createStartMenuFolderCheckbox = new System.Windows.Forms.CheckBox();
            this.createDesktopShortcutCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Setup";
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(2, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(480, 2);
            this.label2.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label3.Location = new System.Drawing.Point(2, 260);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(480, 2);
            this.label3.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(302, 277);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "Install";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(397, 277);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Location:";
            // 
            // labelLocation
            // 
            this.labelLocation.AutoSize = true;
            this.labelLocation.Location = new System.Drawing.Point(68, 50);
            this.labelLocation.Name = "labelLocation";
            this.labelLocation.Size = new System.Drawing.Size(0, 13);
            this.labelLocation.TabIndex = 11;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(397, 45);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 12;
            this.button3.Text = "Browse";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 90);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Branch:";
            // 
            // branchComboBox
            // 
            this.branchComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.branchComboBox.FormattingEnabled = true;
            this.branchComboBox.Items.AddRange(new object[] {
            "master (recommended)",
            "develop (stability not guaranteed)"});
            this.branchComboBox.Location = new System.Drawing.Point(67, 87);
            this.branchComboBox.Name = "branchComboBox";
            this.branchComboBox.Size = new System.Drawing.Size(310, 21);
            this.branchComboBox.TabIndex = 14;
            this.branchComboBox.SelectedIndexChanged += new System.EventHandler(this.branchComboBox_SelectedIndexChanged);
            // 
            // includeSrcCheckbox
            // 
            this.includeSrcCheckbox.AutoSize = true;
            this.includeSrcCheckbox.Location = new System.Drawing.Point(67, 115);
            this.includeSrcCheckbox.Name = "includeSrcCheckbox";
            this.includeSrcCheckbox.Size = new System.Drawing.Size(330, 17);
            this.includeSrcCheckbox.TabIndex = 15;
            this.includeSrcCheckbox.Text = "Include Source Files (leave unchecked if you\'re not a developer)";
            this.includeSrcCheckbox.UseVisualStyleBackColor = true;
            // 
            // createStartMenuFolderCheckbox
            // 
            this.createStartMenuFolderCheckbox.AutoSize = true;
            this.createStartMenuFolderCheckbox.Enabled = false;
            this.createStartMenuFolderCheckbox.Location = new System.Drawing.Point(67, 155);
            this.createStartMenuFolderCheckbox.Name = "createStartMenuFolderCheckbox";
            this.createStartMenuFolderCheckbox.Size = new System.Drawing.Size(153, 17);
            this.createStartMenuFolderCheckbox.TabIndex = 16;
            this.createStartMenuFolderCheckbox.Text = "Create Start Menu shortcut";
            this.createStartMenuFolderCheckbox.UseVisualStyleBackColor = true;
            // 
            // createDesktopShortcutCheckbox
            // 
            this.createDesktopShortcutCheckbox.AutoSize = true;
            this.createDesktopShortcutCheckbox.Enabled = false;
            this.createDesktopShortcutCheckbox.Location = new System.Drawing.Point(67, 179);
            this.createDesktopShortcutCheckbox.Name = "createDesktopShortcutCheckbox";
            this.createDesktopShortcutCheckbox.Size = new System.Drawing.Size(141, 17);
            this.createDesktopShortcutCheckbox.TabIndex = 17;
            this.createDesktopShortcutCheckbox.Text = "Create Desktop shortcut";
            this.createDesktopShortcutCheckbox.UseVisualStyleBackColor = true;
            // 
            // WorkflowForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 304);
            this.ControlBox = false;
            this.Controls.Add(this.createDesktopShortcutCheckbox);
            this.Controls.Add(this.createStartMenuFolderCheckbox);
            this.Controls.Add(this.includeSrcCheckbox);
            this.Controls.Add(this.branchComboBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.labelLocation);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkflowForm2";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FaceBERN! Installation Wizard";
            this.Load += new System.EventHandler(this.WorkflowForm2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelLocation;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox branchComboBox;
        public System.Windows.Forms.CheckBox includeSrcCheckbox;
        public System.Windows.Forms.CheckBox createStartMenuFolderCheckbox;
        public System.Windows.Forms.CheckBox createDesktopShortcutCheckbox;
    }
}