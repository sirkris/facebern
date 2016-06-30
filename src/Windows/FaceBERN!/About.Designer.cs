namespace FaceBERN_
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.label1 = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.dontclickme = new System.Windows.Forms.LinkLabel();
            this.bernieBirdieBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.bernieBirdieBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Montserrat", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(406, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 33);
            this.label1.TabIndex = 1;
            this.label1.Text = "Birdie";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.ForeColor = System.Drawing.Color.Black;
            this.labelVersion.Location = new System.Drawing.Point(543, 46);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(0, 13);
            this.labelVersion.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(412, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "FaceBERN! Client Version";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.richTextBox1.Location = new System.Drawing.Point(412, 81);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.Size = new System.Drawing.Size(460, 196);
            this.richTextBox1.TabIndex = 27;
            this.richTextBox1.TabStop = false;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            this.richTextBox1.Enter += new System.EventHandler(this.richTextBox1_Enter);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(0, 0);
            this.button1.TabIndex = 28;
            this.button1.TabStop = false;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // dontclickme
            // 
            this.dontclickme.AutoSize = true;
            this.dontclickme.Font = new System.Drawing.Font("Arial Narrow", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dontclickme.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.dontclickme.LinkColor = System.Drawing.Color.Navy;
            this.dontclickme.Location = new System.Drawing.Point(823, 295);
            this.dontclickme.Name = "dontclickme";
            this.dontclickme.Size = new System.Drawing.Size(59, 13);
            this.dontclickme.TabIndex = 29;
            this.dontclickme.TabStop = true;
            this.dontclickme.Text = "About the Author";
            this.dontclickme.VisitedLinkColor = System.Drawing.Color.Navy;
            this.dontclickme.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.dontclickme_LinkClicked);
            // 
            // bernieBirdieBox
            // 
            this.bernieBirdieBox.Image = global::FaceBERN_.Properties.Resources.berniebirdie;
            this.bernieBirdieBox.Location = new System.Drawing.Point(0, 0);
            this.bernieBirdieBox.Name = "bernieBirdieBox";
            this.bernieBirdieBox.Size = new System.Drawing.Size(400, 310);
            this.bernieBirdieBox.TabIndex = 0;
            this.bernieBirdieBox.TabStop = false;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 310);
            this.Controls.Add(this.dontclickme);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bernieBirdieBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About Birdie";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.About_Load);
            this.Click += new System.EventHandler(this.About_Close);
            ((System.ComponentModel.ISupportInitialize)(this.bernieBirdieBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox bernieBirdieBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.LinkLabel dontclickme;
    }
}