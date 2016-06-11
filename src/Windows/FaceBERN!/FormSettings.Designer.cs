namespace FaceBERN_
{
    partial class FormSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
            this.settingsTabControl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.autoUpdateCheckbox = new System.Windows.Forms.CheckBox();
            this.checkRememberPasswordByDefaultCheckbox = new System.Windows.Forms.CheckBox();
            this.useCustomEventsCheckbox = new System.Windows.Forms.CheckBox();
            this.useFTBEventsCheckbox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabStates = new System.Windows.Forms.TabPage();
            this.labelExpGOTV = new System.Windows.Forms.Label();
            this.enableGOTVCheckbox = new System.Windows.Forms.CheckBox();
            this.labelEnabledTasks = new System.Windows.Forms.Label();
            this.FTBEventIdTextBox = new System.Windows.Forms.TextBox();
            this.facebookIDTextBox = new System.Windows.Forms.TextBox();
            this.primaryAccessTextBox = new System.Windows.Forms.TextBox();
            this.primaryTypeTextBox = new System.Windows.Forms.TextBox();
            this.primaryDateTextBox = new System.Windows.Forms.TextBox();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.abbreviationTextBox = new System.Windows.Forms.TextBox();
            this.labelFTBEventID = new System.Windows.Forms.Label();
            this.labelFacebookID = new System.Windows.Forms.Label();
            this.labelPrimaryAccess = new System.Windows.Forms.Label();
            this.labelPrimaryType = new System.Windows.Forms.Label();
            this.labelPrimaryDate = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.labelAbbreviation = new System.Windows.Forms.Label();
            this.statesComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tweetRedditNewsCheckbox = new System.Windows.Forms.CheckBox();
            this.settingsTabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabStates.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingsTabControl
            // 
            this.settingsTabControl.Controls.Add(this.tabGeneral);
            this.settingsTabControl.Controls.Add(this.tabStates);
            this.settingsTabControl.Location = new System.Drawing.Point(12, 12);
            this.settingsTabControl.Name = "settingsTabControl";
            this.settingsTabControl.SelectedIndex = 0;
            this.settingsTabControl.Size = new System.Drawing.Size(760, 475);
            this.settingsTabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.tweetRedditNewsCheckbox);
            this.tabGeneral.Controls.Add(this.label6);
            this.tabGeneral.Controls.Add(this.button1);
            this.tabGeneral.Controls.Add(this.autoUpdateCheckbox);
            this.tabGeneral.Controls.Add(this.checkRememberPasswordByDefaultCheckbox);
            this.tabGeneral.Controls.Add(this.useCustomEventsCheckbox);
            this.tabGeneral.Controls.Add(this.useFTBEventsCheckbox);
            this.tabGeneral.Controls.Add(this.label4);
            this.tabGeneral.Controls.Add(this.label3);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.label1);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(752, 449);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            this.tabGeneral.Enter += new System.EventHandler(this.tabGeneral_Load);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(249, 151);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(157, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Clear Stored Credentials";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // autoUpdateCheckbox
            // 
            this.autoUpdateCheckbox.AutoSize = true;
            this.autoUpdateCheckbox.Checked = true;
            this.autoUpdateCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoUpdateCheckbox.Location = new System.Drawing.Point(391, 89);
            this.autoUpdateCheckbox.Name = "autoUpdateCheckbox";
            this.autoUpdateCheckbox.Size = new System.Drawing.Size(15, 14);
            this.autoUpdateCheckbox.TabIndex = 7;
            this.autoUpdateCheckbox.UseVisualStyleBackColor = true;
            this.autoUpdateCheckbox.CheckedChanged += new System.EventHandler(this.autoUpdateCheckbox_CheckChanged);
            // 
            // checkRememberPasswordByDefaultCheckbox
            // 
            this.checkRememberPasswordByDefaultCheckbox.AutoSize = true;
            this.checkRememberPasswordByDefaultCheckbox.Checked = true;
            this.checkRememberPasswordByDefaultCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRememberPasswordByDefaultCheckbox.Location = new System.Drawing.Point(391, 64);
            this.checkRememberPasswordByDefaultCheckbox.Name = "checkRememberPasswordByDefaultCheckbox";
            this.checkRememberPasswordByDefaultCheckbox.Size = new System.Drawing.Size(15, 14);
            this.checkRememberPasswordByDefaultCheckbox.TabIndex = 6;
            this.checkRememberPasswordByDefaultCheckbox.UseVisualStyleBackColor = true;
            this.checkRememberPasswordByDefaultCheckbox.CheckedChanged += new System.EventHandler(this.checkRememberPasswordByDefaultCheckbox_CheckChanged);
            // 
            // useCustomEventsCheckbox
            // 
            this.useCustomEventsCheckbox.AutoSize = true;
            this.useCustomEventsCheckbox.Checked = true;
            this.useCustomEventsCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useCustomEventsCheckbox.Location = new System.Drawing.Point(391, 39);
            this.useCustomEventsCheckbox.Name = "useCustomEventsCheckbox";
            this.useCustomEventsCheckbox.Size = new System.Drawing.Size(15, 14);
            this.useCustomEventsCheckbox.TabIndex = 5;
            this.useCustomEventsCheckbox.UseVisualStyleBackColor = true;
            this.useCustomEventsCheckbox.CheckedChanged += new System.EventHandler(this.useCustomEventsCheckbox_CheckChanged);
            // 
            // useFTBEventsCheckbox
            // 
            this.useFTBEventsCheckbox.AutoSize = true;
            this.useFTBEventsCheckbox.Checked = true;
            this.useFTBEventsCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useFTBEventsCheckbox.Location = new System.Drawing.Point(391, 14);
            this.useFTBEventsCheckbox.Name = "useFTBEventsCheckbox";
            this.useFTBEventsCheckbox.Size = new System.Drawing.Size(15, 14);
            this.useFTBEventsCheckbox.TabIndex = 4;
            this.useFTBEventsCheckbox.UseVisualStyleBackColor = true;
            this.useFTBEventsCheckbox.CheckedChanged += new System.EventHandler(this.useFTPEventsCheckbox_CheckChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(286, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Auto-Update:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(20, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(365, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Auto-Check Remember Password Box in Login Prompt:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(137, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(248, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Create Custom Events for Overflow:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(217, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Use feelthebern.events:";
            // 
            // tabStates
            // 
            this.tabStates.Controls.Add(this.labelExpGOTV);
            this.tabStates.Controls.Add(this.enableGOTVCheckbox);
            this.tabStates.Controls.Add(this.labelEnabledTasks);
            this.tabStates.Controls.Add(this.FTBEventIdTextBox);
            this.tabStates.Controls.Add(this.facebookIDTextBox);
            this.tabStates.Controls.Add(this.primaryAccessTextBox);
            this.tabStates.Controls.Add(this.primaryTypeTextBox);
            this.tabStates.Controls.Add(this.primaryDateTextBox);
            this.tabStates.Controls.Add(this.nameTextBox);
            this.tabStates.Controls.Add(this.abbreviationTextBox);
            this.tabStates.Controls.Add(this.labelFTBEventID);
            this.tabStates.Controls.Add(this.labelFacebookID);
            this.tabStates.Controls.Add(this.labelPrimaryAccess);
            this.tabStates.Controls.Add(this.labelPrimaryType);
            this.tabStates.Controls.Add(this.labelPrimaryDate);
            this.tabStates.Controls.Add(this.labelName);
            this.tabStates.Controls.Add(this.labelAbbreviation);
            this.tabStates.Controls.Add(this.statesComboBox);
            this.tabStates.Controls.Add(this.label5);
            this.tabStates.Location = new System.Drawing.Point(4, 22);
            this.tabStates.Name = "tabStates";
            this.tabStates.Padding = new System.Windows.Forms.Padding(3);
            this.tabStates.Size = new System.Drawing.Size(752, 449);
            this.tabStates.TabIndex = 1;
            this.tabStates.Text = "States";
            this.tabStates.UseVisualStyleBackColor = true;
            this.tabStates.Enter += new System.EventHandler(this.tabStates_Load);
            // 
            // labelExpGOTV
            // 
            this.labelExpGOTV.AutoSize = true;
            this.labelExpGOTV.Location = new System.Drawing.Point(303, 303);
            this.labelExpGOTV.Name = "labelExpGOTV";
            this.labelExpGOTV.Size = new System.Drawing.Size(353, 13);
            this.labelExpGOTV.TabIndex = 19;
            this.labelExpGOTV.Text = "Invite people to feelthebern.events get-out-the-vote events on Facebook.";
            this.labelExpGOTV.Click += new System.EventHandler(this.label7_Click);
            // 
            // enableGOTVCheckbox
            // 
            this.enableGOTVCheckbox.AutoSize = true;
            this.enableGOTVCheckbox.Checked = true;
            this.enableGOTVCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableGOTVCheckbox.Location = new System.Drawing.Point(306, 283);
            this.enableGOTVCheckbox.Name = "enableGOTVCheckbox";
            this.enableGOTVCheckbox.Size = new System.Drawing.Size(56, 17);
            this.enableGOTVCheckbox.TabIndex = 18;
            this.enableGOTVCheckbox.Text = "GOTV";
            this.enableGOTVCheckbox.UseVisualStyleBackColor = true;
            this.enableGOTVCheckbox.CheckedChanged += new System.EventHandler(this.StateFields_TextChanged);
            // 
            // labelEnabledTasks
            // 
            this.labelEnabledTasks.AutoSize = true;
            this.labelEnabledTasks.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEnabledTasks.Location = new System.Drawing.Point(184, 263);
            this.labelEnabledTasks.Name = "labelEnabledTasks";
            this.labelEnabledTasks.Size = new System.Drawing.Size(116, 16);
            this.labelEnabledTasks.TabIndex = 17;
            this.labelEnabledTasks.Text = "Enabled Tasks:";
            // 
            // FTBEventIdTextBox
            // 
            this.FTBEventIdTextBox.Location = new System.Drawing.Point(306, 225);
            this.FTBEventIdTextBox.Name = "FTBEventIdTextBox";
            this.FTBEventIdTextBox.Size = new System.Drawing.Size(284, 20);
            this.FTBEventIdTextBox.TabIndex = 16;
            this.FTBEventIdTextBox.TextChanged += new System.EventHandler(this.StateFields_TextChanged);
            // 
            // facebookIDTextBox
            // 
            this.facebookIDTextBox.Location = new System.Drawing.Point(306, 200);
            this.facebookIDTextBox.Name = "facebookIDTextBox";
            this.facebookIDTextBox.Size = new System.Drawing.Size(284, 20);
            this.facebookIDTextBox.TabIndex = 15;
            this.facebookIDTextBox.TextChanged += new System.EventHandler(this.StateFields_TextChanged);
            // 
            // primaryAccessTextBox
            // 
            this.primaryAccessTextBox.Cursor = System.Windows.Forms.Cursors.No;
            this.primaryAccessTextBox.Enabled = false;
            this.primaryAccessTextBox.Location = new System.Drawing.Point(306, 175);
            this.primaryAccessTextBox.Name = "primaryAccessTextBox";
            this.primaryAccessTextBox.ReadOnly = true;
            this.primaryAccessTextBox.Size = new System.Drawing.Size(284, 20);
            this.primaryAccessTextBox.TabIndex = 14;
            this.primaryAccessTextBox.TabStop = false;
            // 
            // primaryTypeTextBox
            // 
            this.primaryTypeTextBox.Cursor = System.Windows.Forms.Cursors.No;
            this.primaryTypeTextBox.Enabled = false;
            this.primaryTypeTextBox.Location = new System.Drawing.Point(306, 150);
            this.primaryTypeTextBox.Name = "primaryTypeTextBox";
            this.primaryTypeTextBox.ReadOnly = true;
            this.primaryTypeTextBox.Size = new System.Drawing.Size(284, 20);
            this.primaryTypeTextBox.TabIndex = 13;
            this.primaryTypeTextBox.TabStop = false;
            // 
            // primaryDateTextBox
            // 
            this.primaryDateTextBox.Cursor = System.Windows.Forms.Cursors.No;
            this.primaryDateTextBox.Enabled = false;
            this.primaryDateTextBox.Location = new System.Drawing.Point(306, 125);
            this.primaryDateTextBox.Name = "primaryDateTextBox";
            this.primaryDateTextBox.ReadOnly = true;
            this.primaryDateTextBox.Size = new System.Drawing.Size(284, 20);
            this.primaryDateTextBox.TabIndex = 12;
            this.primaryDateTextBox.TabStop = false;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Cursor = System.Windows.Forms.Cursors.No;
            this.nameTextBox.Enabled = false;
            this.nameTextBox.Location = new System.Drawing.Point(306, 100);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.ReadOnly = true;
            this.nameTextBox.Size = new System.Drawing.Size(284, 20);
            this.nameTextBox.TabIndex = 11;
            this.nameTextBox.TabStop = false;
            // 
            // abbreviationTextBox
            // 
            this.abbreviationTextBox.Cursor = System.Windows.Forms.Cursors.No;
            this.abbreviationTextBox.Enabled = false;
            this.abbreviationTextBox.Location = new System.Drawing.Point(306, 75);
            this.abbreviationTextBox.Name = "abbreviationTextBox";
            this.abbreviationTextBox.ReadOnly = true;
            this.abbreviationTextBox.Size = new System.Drawing.Size(62, 20);
            this.abbreviationTextBox.TabIndex = 10;
            this.abbreviationTextBox.TabStop = false;
            // 
            // labelFTBEventID
            // 
            this.labelFTBEventID.AutoSize = true;
            this.labelFTBEventID.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFTBEventID.Location = new System.Drawing.Point(8, 226);
            this.labelFTBEventID.Name = "labelFTBEventID";
            this.labelFTBEventID.Size = new System.Drawing.Size(292, 16);
            this.labelFTBEventID.TabIndex = 9;
            this.labelFTBEventID.Text = "Facebook Event ID for feelthebern.events:";
            // 
            // labelFacebookID
            // 
            this.labelFacebookID.AutoSize = true;
            this.labelFacebookID.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFacebookID.Location = new System.Drawing.Point(204, 201);
            this.labelFacebookID.Name = "labelFacebookID";
            this.labelFacebookID.Size = new System.Drawing.Size(96, 16);
            this.labelFacebookID.TabIndex = 8;
            this.labelFacebookID.Text = "Facebook ID:";
            // 
            // labelPrimaryAccess
            // 
            this.labelPrimaryAccess.AutoSize = true;
            this.labelPrimaryAccess.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPrimaryAccess.Location = new System.Drawing.Point(186, 176);
            this.labelPrimaryAccess.Name = "labelPrimaryAccess";
            this.labelPrimaryAccess.Size = new System.Drawing.Size(114, 16);
            this.labelPrimaryAccess.TabIndex = 7;
            this.labelPrimaryAccess.Text = "Primary Access:";
            // 
            // labelPrimaryType
            // 
            this.labelPrimaryType.AutoSize = true;
            this.labelPrimaryType.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPrimaryType.Location = new System.Drawing.Point(200, 151);
            this.labelPrimaryType.Name = "labelPrimaryType";
            this.labelPrimaryType.Size = new System.Drawing.Size(100, 16);
            this.labelPrimaryType.TabIndex = 6;
            this.labelPrimaryType.Text = "Primary Type:";
            // 
            // labelPrimaryDate
            // 
            this.labelPrimaryDate.AutoSize = true;
            this.labelPrimaryDate.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPrimaryDate.Location = new System.Drawing.Point(202, 126);
            this.labelPrimaryDate.Name = "labelPrimaryDate";
            this.labelPrimaryDate.Size = new System.Drawing.Size(98, 16);
            this.labelPrimaryDate.TabIndex = 5;
            this.labelPrimaryDate.Text = "Primary Date:";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(250, 101);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(50, 16);
            this.labelName.TabIndex = 4;
            this.labelName.Text = "Name:";
            // 
            // labelAbbreviation
            // 
            this.labelAbbreviation.AutoSize = true;
            this.labelAbbreviation.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAbbreviation.Location = new System.Drawing.Point(204, 76);
            this.labelAbbreviation.Name = "labelAbbreviation";
            this.labelAbbreviation.Size = new System.Drawing.Size(96, 16);
            this.labelAbbreviation.TabIndex = 3;
            this.labelAbbreviation.Text = "Abbreviation:";
            // 
            // statesComboBox
            // 
            this.statesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.statesComboBox.Font = new System.Drawing.Font("Montserrat", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statesComboBox.FormattingEnabled = true;
            this.statesComboBox.Location = new System.Drawing.Point(306, 14);
            this.statesComboBox.Name = "statesComboBox";
            this.statesComboBox.Size = new System.Drawing.Size(373, 24);
            this.statesComboBox.TabIndex = 2;
            this.statesComboBox.SelectedIndexChanged += new System.EventHandler(this.statesComboBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(134, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(166, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "Please Select a State:";
            // 
            // buttonApply
            // 
            this.buttonApply.Enabled = false;
            this.buttonApply.Location = new System.Drawing.Point(693, 493);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 6;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(612, 493);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(531, 493);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(129, 112);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(256, 16);
            this.label6.TabIndex = 9;
            this.label6.Text = "Tweet birdie news posts from Reddit:";
            // 
            // tweetRedditNewsCheckbox
            // 
            this.tweetRedditNewsCheckbox.AutoSize = true;
            this.tweetRedditNewsCheckbox.Checked = true;
            this.tweetRedditNewsCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tweetRedditNewsCheckbox.Location = new System.Drawing.Point(391, 114);
            this.tweetRedditNewsCheckbox.Name = "tweetRedditNewsCheckbox";
            this.tweetRedditNewsCheckbox.Size = new System.Drawing.Size(15, 14);
            this.tweetRedditNewsCheckbox.TabIndex = 10;
            this.tweetRedditNewsCheckbox.UseVisualStyleBackColor = true;
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 520);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.settingsTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.settingsTabControl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabStates.ResumeLayout(false);
            this.tabStates.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl settingsTabControl;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabStates;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox useFTBEventsCheckbox;
        private System.Windows.Forms.CheckBox useCustomEventsCheckbox;
        private System.Windows.Forms.CheckBox autoUpdateCheckbox;
        private System.Windows.Forms.CheckBox checkRememberPasswordByDefaultCheckbox;
        private System.Windows.Forms.ComboBox statesComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelFTBEventID;
        private System.Windows.Forms.Label labelFacebookID;
        private System.Windows.Forms.Label labelPrimaryAccess;
        private System.Windows.Forms.Label labelPrimaryType;
        private System.Windows.Forms.Label labelPrimaryDate;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelAbbreviation;
        private System.Windows.Forms.TextBox FTBEventIdTextBox;
        private System.Windows.Forms.TextBox facebookIDTextBox;
        private System.Windows.Forms.TextBox primaryAccessTextBox;
        private System.Windows.Forms.TextBox primaryTypeTextBox;
        private System.Windows.Forms.TextBox primaryDateTextBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.TextBox abbreviationTextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox enableGOTVCheckbox;
        private System.Windows.Forms.Label labelEnabledTasks;
        private System.Windows.Forms.Label labelExpGOTV;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox tweetRedditNewsCheckbox;
    }
}