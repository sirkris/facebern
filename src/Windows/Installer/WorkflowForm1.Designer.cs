namespace Installer
{
    partial class WorkflowForm1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowForm1));
            this.weCanDoThisTogether = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.weCanDoThisTogether)).BeginInit();
            this.SuspendLayout();
            // 
            // weCanDoThisTogether
            // 
            this.weCanDoThisTogether.Image = global::Installer.Properties.Resources.together;
            this.weCanDoThisTogether.Location = new System.Drawing.Point(12, 12);
            this.weCanDoThisTogether.Name = "weCanDoThisTogether";
            this.weCanDoThisTogether.Size = new System.Drawing.Size(436, 350);
            this.weCanDoThisTogether.TabIndex = 0;
            this.weCanDoThisTogether.TabStop = false;
            // 
            // WorkflowForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(959, 374);
            this.ControlBox = false;
            this.Controls.Add(this.weCanDoThisTogether);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkflowForm1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FaceBERN! Installation Wizard";
            ((System.ComponentModel.ISupportInitialize)(this.weCanDoThisTogether)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox weCanDoThisTogether;
    }
}