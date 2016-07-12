namespace FaceBERN_
{
    partial class TweetsHistory
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TweetsHistory));
            this.tweetsLogListView = new System.Windows.Forms.ListView();
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTweet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCampaign = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tweetsLogListView
            // 
            this.tweetsLogListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDate,
            this.colTweet,
            this.colCampaign,
            this.colSource});
            this.tweetsLogListView.Location = new System.Drawing.Point(12, 12);
            this.tweetsLogListView.Name = "tweetsLogListView";
            this.tweetsLogListView.Size = new System.Drawing.Size(960, 607);
            this.tweetsLogListView.TabIndex = 0;
            this.tweetsLogListView.UseCompatibleStateImageBehavior = false;
            this.tweetsLogListView.View = System.Windows.Forms.View.Details;
            // 
            // colDate
            // 
            this.colDate.Text = "Date";
            this.colDate.Width = 134;
            // 
            // colTweet
            // 
            this.colTweet.Text = "Tweet";
            this.colTweet.Width = 376;
            // 
            // colCampaign
            // 
            this.colCampaign.Text = "Campaign";
            this.colCampaign.Width = 291;
            // 
            // colSource
            // 
            this.colSource.Text = "Source";
            this.colSource.Width = 136;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(450, 625);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 25);
            this.button1.TabIndex = 1;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TweetsHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 662);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tweetsLogListView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TweetsHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Birdie Tweets History";
            this.Load += new System.EventHandler(this.TweetsHistory_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView tweetsLogListView;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colTweet;
        private System.Windows.Forms.ColumnHeader colCampaign;
        private System.Windows.Forms.ColumnHeader colSource;
        private System.Windows.Forms.Button button1;
    }
}