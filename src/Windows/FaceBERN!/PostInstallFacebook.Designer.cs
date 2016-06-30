namespace FaceBERN_
{
    partial class PostInstallFacebook
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PostInstallFacebook));
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.labelVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.enableFacebankingCheckbox = new System.Windows.Forms.CheckBox();
            this.rememberUsernamePasswordCheckbox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.facebookUsernameTextbox = new System.Windows.Forms.TextBox();
            this.facebookPasswordTextbox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.weCanDoThisTogether = new System.Windows.Forms.PictureBox();
            this.label7 = new System.Windows.Forms.Label();
            this.browserModeComboBox = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.weCanDoThisTogether)).BeginInit();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(745, 366);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Next >";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(840, 366);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 22;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.richTextBox1.Location = new System.Drawing.Point(452, 51);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.ShortcutsEnabled = false;
            this.richTextBox1.Size = new System.Drawing.Size(457, 108);
            this.richTextBox1.TabIndex = 21;
            this.richTextBox1.TabStop = false;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(146, 353);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(0, 13);
            this.labelVersion.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 353);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "FaceBERN! Client Version:";
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(0, 349);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(916, 2);
            this.label2.TabIndex = 18;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Montserrat", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(447, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 25);
            this.label1.TabIndex = 17;
            this.label1.Text = "Facebook Settings";
            // 
            // enableFacebankingCheckbox
            // 
            this.enableFacebankingCheckbox.AutoSize = true;
            this.enableFacebankingCheckbox.Checked = true;
            this.enableFacebankingCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableFacebankingCheckbox.Location = new System.Drawing.Point(452, 177);
            this.enableFacebankingCheckbox.Name = "enableFacebankingCheckbox";
            this.enableFacebankingCheckbox.Size = new System.Drawing.Size(124, 17);
            this.enableFacebankingCheckbox.TabIndex = 24;
            this.enableFacebankingCheckbox.Text = "Enable Facebanking";
            this.enableFacebankingCheckbox.UseVisualStyleBackColor = true;
            this.enableFacebankingCheckbox.CheckedChanged += new System.EventHandler(this.enableFacebankingCheckbox_CheckedChanged);
            // 
            // rememberUsernamePasswordCheckbox
            // 
            this.rememberUsernamePasswordCheckbox.AutoSize = true;
            this.rememberUsernamePasswordCheckbox.Checked = true;
            this.rememberUsernamePasswordCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rememberUsernamePasswordCheckbox.Location = new System.Drawing.Point(452, 201);
            this.rememberUsernamePasswordCheckbox.Name = "rememberUsernamePasswordCheckbox";
            this.rememberUsernamePasswordCheckbox.Size = new System.Drawing.Size(230, 17);
            this.rememberUsernamePasswordCheckbox.TabIndex = 25;
            this.rememberUsernamePasswordCheckbox.Text = "Remember Facebook Username/Password";
            this.rememberUsernamePasswordCheckbox.UseVisualStyleBackColor = true;
            this.rememberUsernamePasswordCheckbox.CheckedChanged += new System.EventHandler(this.rememberUsernamePasswordCheckbox_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(468, 227);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(149, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Facebook Username or Email:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(510, 250);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(107, 13);
            this.label5.TabIndex = 27;
            this.label5.Text = "Facebook Password:";
            // 
            // facebookUsernameTextbox
            // 
            this.facebookUsernameTextbox.Location = new System.Drawing.Point(623, 224);
            this.facebookUsernameTextbox.Name = "facebookUsernameTextbox";
            this.facebookUsernameTextbox.Size = new System.Drawing.Size(286, 20);
            this.facebookUsernameTextbox.TabIndex = 0;
            this.facebookUsernameTextbox.TextChanged += new System.EventHandler(this.facebookUsernameTextbox_TextChanged);
            // 
            // facebookPasswordTextbox
            // 
            this.facebookPasswordTextbox.Location = new System.Drawing.Point(623, 247);
            this.facebookPasswordTextbox.Name = "facebookPasswordTextbox";
            this.facebookPasswordTextbox.Size = new System.Drawing.Size(286, 20);
            this.facebookPasswordTextbox.TabIndex = 1;
            this.facebookPasswordTextbox.UseSystemPasswordChar = true;
            this.facebookPasswordTextbox.TextChanged += new System.EventHandler(this.facebookPasswordTextbox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(468, 274);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(413, 13);
            this.label6.TabIndex = 30;
            this.label6.Text = "You can change or remove these login credentials at any time under Tools -> Setti" +
    "ngs.";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(452, 366);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 31;
            this.button3.Text = "< Back";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // weCanDoThisTogether
            // 
            this.weCanDoThisTogether.Image = global::FaceBERN_.Properties.Resources.together2;
            this.weCanDoThisTogether.Location = new System.Drawing.Point(0, 0);
            this.weCanDoThisTogether.Name = "weCanDoThisTogether";
            this.weCanDoThisTogether.Size = new System.Drawing.Size(436, 350);
            this.weCanDoThisTogether.TabIndex = 16;
            this.weCanDoThisTogether.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(449, 305);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 13);
            this.label7.TabIndex = 32;
            this.label7.Text = "Web Browser:";
            // 
            // browserModeComboBox
            // 
            this.browserModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.browserModeComboBox.FormattingEnabled = true;
            this.browserModeComboBox.Items.AddRange(new object[] {
            "-- Please Select a Web Browser --",
            "Google Chrome",
            "Mozilla Firefox"});
            this.browserModeComboBox.Location = new System.Drawing.Point(529, 302);
            this.browserModeComboBox.Name = "browserModeComboBox";
            this.browserModeComboBox.Size = new System.Drawing.Size(291, 21);
            this.browserModeComboBox.TabIndex = 33;
            this.browserModeComboBox.SelectedIndexChanged += new System.EventHandler(this.browserModeComboBox_SelectedIndexChanged);
            // 
            // PostInstallFacebook
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 401);
            this.Controls.Add(this.browserModeComboBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.facebookPasswordTextbox);
            this.Controls.Add(this.facebookUsernameTextbox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.rememberUsernamePasswordCheckbox);
            this.Controls.Add(this.enableFacebankingCheckbox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.weCanDoThisTogether);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PostInstallFacebook";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "First-Run Setup Wizard : Facebook";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PostInstallFacebook_FormClosed);
            this.Load += new System.EventHandler(this.PostInstallFacebook_Load);
            this.Shown += new System.EventHandler(this.PostInstallFacebook_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.weCanDoThisTogether)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox weCanDoThisTogether;
        private System.Windows.Forms.CheckBox enableFacebankingCheckbox;
        private System.Windows.Forms.CheckBox rememberUsernamePasswordCheckbox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox facebookUsernameTextbox;
        private System.Windows.Forms.TextBox facebookPasswordTextbox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label7;
        internal System.Windows.Forms.ComboBox browserModeComboBox;
    }
}