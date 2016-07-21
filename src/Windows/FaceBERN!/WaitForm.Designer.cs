namespace FaceBERN_
{
    partial class WaitForm
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
            this.messageRichTextbox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // messageRichTextbox
            // 
            this.messageRichTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.messageRichTextbox.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.messageRichTextbox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageRichTextbox.Location = new System.Drawing.Point(12, 12);
            this.messageRichTextbox.Name = "messageRichTextbox";
            this.messageRichTextbox.ReadOnly = true;
            this.messageRichTextbox.ShortcutsEnabled = false;
            this.messageRichTextbox.Size = new System.Drawing.Size(360, 18);
            this.messageRichTextbox.TabIndex = 9999;
            this.messageRichTextbox.TabStop = false;
            this.messageRichTextbox.Text = "Please wait....";
            this.messageRichTextbox.UseWaitCursor = true;
            // 
            // WaitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 42);
            this.ControlBox = false;
            this.Controls.Add(this.messageRichTextbox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WaitForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Please Wait....";
            this.TopMost = true;
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.WaitForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox messageRichTextbox;

    }
}