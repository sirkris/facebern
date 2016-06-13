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
            this.label4 = new System.Windows.Forms.Label();
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
            this.tabFacebook = new System.Windows.Forms.TabPage();
            this.enableFacebankingCheckbox = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.checkRememberPasswordByDefaultCheckbox = new System.Windows.Forms.CheckBox();
            this.useCustomEventsCheckbox = new System.Windows.Forms.CheckBox();
            this.useFTBEventsCheckbox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabTwitter = new System.Windows.Forms.TabPage();
            this.twitterAccessTokenTextbox = new System.Windows.Forms.TextBox();
            this.twitterUserIdTextbox = new System.Windows.Forms.TextBox();
            this.twitterUsernameTextbox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.enableTwitterCheckbox = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tweetRedditNewsCheckbox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.settingsTabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabStates.SuspendLayout();
            this.tabFacebook.SuspendLayout();
            this.tabTwitter.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingsTabControl
            // 
            this.settingsTabControl.Controls.Add(this.tabGeneral);
            this.settingsTabControl.Controls.Add(this.tabStates);
            this.settingsTabControl.Controls.Add(this.tabFacebook);
            this.settingsTabControl.Controls.Add(this.tabTwitter);
            this.settingsTabControl.Location = new System.Drawing.Point(12, 12);
            this.settingsTabControl.Name = "settingsTabControl";
            this.settingsTabControl.SelectedIndex = 0;
            this.settingsTabControl.Size = new System.Drawing.Size(760, 475);
            this.settingsTabControl.TabIndex = 0;
            this.settingsTabControl.Enter += new System.EventHandler(this.tabTwitter_Load);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.button1);
            this.tabGeneral.Controls.Add(this.autoUpdateCheckbox);
            this.tabGeneral.Controls.Add(this.label4);
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
            this.button1.Location = new System.Drawing.Point(249, 48);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(157, 23);
            this.button1.TabIndex = 16;
            this.button1.Text = "Clear Stored Credentials";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // autoUpdateCheckbox
            // 
            this.autoUpdateCheckbox.AutoSize = true;
            this.autoUpdateCheckbox.Checked = true;
            this.autoUpdateCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoUpdateCheckbox.Location = new System.Drawing.Point(391, 14);
            this.autoUpdateCheckbox.Name = "autoUpdateCheckbox";
            this.autoUpdateCheckbox.Size = new System.Drawing.Size(15, 14);
            this.autoUpdateCheckbox.TabIndex = 7;
            this.autoUpdateCheckbox.UseVisualStyleBackColor = true;
            this.autoUpdateCheckbox.CheckedChanged += new System.EventHandler(this.autoUpdateCheckbox_CheckChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(286, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Auto-Update:";
            this.label4.Click += new System.EventHandler(this.label4_Click);
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
            // tabFacebook
            // 
            this.tabFacebook.Controls.Add(this.button3);
            this.tabFacebook.Controls.Add(this.enableFacebankingCheckbox);
            this.tabFacebook.Controls.Add(this.label7);
            this.tabFacebook.Controls.Add(this.checkRememberPasswordByDefaultCheckbox);
            this.tabFacebook.Controls.Add(this.useCustomEventsCheckbox);
            this.tabFacebook.Controls.Add(this.useFTBEventsCheckbox);
            this.tabFacebook.Controls.Add(this.label3);
            this.tabFacebook.Controls.Add(this.label2);
            this.tabFacebook.Controls.Add(this.label1);
            this.tabFacebook.Location = new System.Drawing.Point(4, 22);
            this.tabFacebook.Name = "tabFacebook";
            this.tabFacebook.Size = new System.Drawing.Size(752, 449);
            this.tabFacebook.TabIndex = 2;
            this.tabFacebook.Text = "Facebook";
            this.tabFacebook.UseVisualStyleBackColor = true;
            this.tabFacebook.Enter += new System.EventHandler(this.tabFacebook_Load);
            // 
            // enableFacebankingCheckbox
            // 
            this.enableFacebankingCheckbox.AutoSize = true;
            this.enableFacebankingCheckbox.Checked = true;
            this.enableFacebankingCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableFacebankingCheckbox.Location = new System.Drawing.Point(391, 14);
            this.enableFacebankingCheckbox.Name = "enableFacebankingCheckbox";
            this.enableFacebankingCheckbox.Size = new System.Drawing.Size(15, 14);
            this.enableFacebankingCheckbox.TabIndex = 17;
            this.enableFacebankingCheckbox.UseVisualStyleBackColor = true;
            this.enableFacebankingCheckbox.CheckedChanged += new System.EventHandler(this.enableFacebankingCheckbox_CheckChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(241, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(144, 16);
            this.label7.TabIndex = 16;
            this.label7.Text = "Enable Facebanking:";
            this.label7.Click += new System.EventHandler(this.label7_Click_1);
            // 
            // checkRememberPasswordByDefaultCheckbox
            // 
            this.checkRememberPasswordByDefaultCheckbox.AutoSize = true;
            this.checkRememberPasswordByDefaultCheckbox.Checked = true;
            this.checkRememberPasswordByDefaultCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRememberPasswordByDefaultCheckbox.Location = new System.Drawing.Point(391, 89);
            this.checkRememberPasswordByDefaultCheckbox.Name = "checkRememberPasswordByDefaultCheckbox";
            this.checkRememberPasswordByDefaultCheckbox.Size = new System.Drawing.Size(15, 14);
            this.checkRememberPasswordByDefaultCheckbox.TabIndex = 14;
            this.checkRememberPasswordByDefaultCheckbox.UseVisualStyleBackColor = true;
            // 
            // useCustomEventsCheckbox
            // 
            this.useCustomEventsCheckbox.AutoSize = true;
            this.useCustomEventsCheckbox.Location = new System.Drawing.Point(391, 64);
            this.useCustomEventsCheckbox.Name = "useCustomEventsCheckbox";
            this.useCustomEventsCheckbox.Size = new System.Drawing.Size(15, 14);
            this.useCustomEventsCheckbox.TabIndex = 13;
            this.useCustomEventsCheckbox.UseVisualStyleBackColor = true;
            // 
            // useFTBEventsCheckbox
            // 
            this.useFTBEventsCheckbox.AutoSize = true;
            this.useFTBEventsCheckbox.Checked = true;
            this.useFTBEventsCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useFTBEventsCheckbox.Location = new System.Drawing.Point(391, 39);
            this.useFTBEventsCheckbox.Name = "useFTBEventsCheckbox";
            this.useFTBEventsCheckbox.Size = new System.Drawing.Size(15, 14);
            this.useFTBEventsCheckbox.TabIndex = 12;
            this.useFTBEventsCheckbox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(20, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(365, 16);
            this.label3.TabIndex = 11;
            this.label3.Text = "Auto-Check Remember Password Box in Login Prompt:";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(137, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(248, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "Create Custom Events for Overflow:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(217, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 16);
            this.label1.TabIndex = 9;
            this.label1.Text = "Use feelthebern.events:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // tabTwitter
            // 
            this.tabTwitter.Controls.Add(this.twitterAccessTokenTextbox);
            this.tabTwitter.Controls.Add(this.twitterUserIdTextbox);
            this.tabTwitter.Controls.Add(this.twitterUsernameTextbox);
            this.tabTwitter.Controls.Add(this.label9);
            this.tabTwitter.Controls.Add(this.label11);
            this.tabTwitter.Controls.Add(this.label12);
            this.tabTwitter.Controls.Add(this.button2);
            this.tabTwitter.Controls.Add(this.label10);
            this.tabTwitter.Controls.Add(this.enableTwitterCheckbox);
            this.tabTwitter.Controls.Add(this.label8);
            this.tabTwitter.Controls.Add(this.tweetRedditNewsCheckbox);
            this.tabTwitter.Controls.Add(this.label6);
            this.tabTwitter.Location = new System.Drawing.Point(4, 22);
            this.tabTwitter.Name = "tabTwitter";
            this.tabTwitter.Size = new System.Drawing.Size(752, 449);
            this.tabTwitter.TabIndex = 3;
            this.tabTwitter.Text = "Twitter";
            this.tabTwitter.UseVisualStyleBackColor = true;
            // 
            // twitterAccessTokenTextbox
            // 
            this.twitterAccessTokenTextbox.Cursor = System.Windows.Forms.Cursors.No;
            this.twitterAccessTokenTextbox.Enabled = false;
            this.twitterAccessTokenTextbox.Location = new System.Drawing.Point(306, 200);
            this.twitterAccessTokenTextbox.Name = "twitterAccessTokenTextbox";
            this.twitterAccessTokenTextbox.ReadOnly = true;
            this.twitterAccessTokenTextbox.Size = new System.Drawing.Size(284, 20);
            this.twitterAccessTokenTextbox.TabIndex = 26;
            this.twitterAccessTokenTextbox.TabStop = false;
            // 
            // twitterUserIdTextbox
            // 
            this.twitterUserIdTextbox.Cursor = System.Windows.Forms.Cursors.No;
            this.twitterUserIdTextbox.Enabled = false;
            this.twitterUserIdTextbox.Location = new System.Drawing.Point(306, 175);
            this.twitterUserIdTextbox.Name = "twitterUserIdTextbox";
            this.twitterUserIdTextbox.ReadOnly = true;
            this.twitterUserIdTextbox.Size = new System.Drawing.Size(284, 20);
            this.twitterUserIdTextbox.TabIndex = 25;
            this.twitterUserIdTextbox.TabStop = false;
            // 
            // twitterUsernameTextbox
            // 
            this.twitterUsernameTextbox.Cursor = System.Windows.Forms.Cursors.No;
            this.twitterUsernameTextbox.Enabled = false;
            this.twitterUsernameTextbox.Location = new System.Drawing.Point(306, 150);
            this.twitterUsernameTextbox.Name = "twitterUsernameTextbox";
            this.twitterUsernameTextbox.ReadOnly = true;
            this.twitterUsernameTextbox.Size = new System.Drawing.Size(284, 20);
            this.twitterUsernameTextbox.TabIndex = 24;
            this.twitterUsernameTextbox.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(194, 201);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(106, 16);
            this.label9.TabIndex = 23;
            this.label9.Text = "Access Token:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(238, 176);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(62, 16);
            this.label11.TabIndex = 22;
            this.label11.Text = "User ID:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(222, 151);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(78, 16);
            this.label12.TabIndex = 21;
            this.label12.Text = "Username:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(219, 79);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(187, 23);
            this.button2.TabIndex = 20;
            this.button2.Text = "Associate Twitter Account";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(190, 117);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(110, 16);
            this.label10.TabIndex = 19;
            this.label10.Text = "Account Data:";
            // 
            // enableTwitterCheckbox
            // 
            this.enableTwitterCheckbox.AutoSize = true;
            this.enableTwitterCheckbox.Checked = true;
            this.enableTwitterCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableTwitterCheckbox.Location = new System.Drawing.Point(391, 14);
            this.enableTwitterCheckbox.Name = "enableTwitterCheckbox";
            this.enableTwitterCheckbox.Size = new System.Drawing.Size(15, 14);
            this.enableTwitterCheckbox.TabIndex = 14;
            this.enableTwitterCheckbox.UseVisualStyleBackColor = true;
            this.enableTwitterCheckbox.CheckedChanged += new System.EventHandler(this.enableTwitterCheckbox_CheckChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(275, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(110, 16);
            this.label8.TabIndex = 13;
            this.label8.Text = "Enable Twitter:";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // tweetRedditNewsCheckbox
            // 
            this.tweetRedditNewsCheckbox.AutoSize = true;
            this.tweetRedditNewsCheckbox.Checked = true;
            this.tweetRedditNewsCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tweetRedditNewsCheckbox.Location = new System.Drawing.Point(391, 39);
            this.tweetRedditNewsCheckbox.Name = "tweetRedditNewsCheckbox";
            this.tweetRedditNewsCheckbox.Size = new System.Drawing.Size(15, 14);
            this.tweetRedditNewsCheckbox.TabIndex = 12;
            this.tweetRedditNewsCheckbox.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(129, 37);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(256, 16);
            this.label6.TabIndex = 11;
            this.label6.Text = "Tweet birdie news posts from Reddit:";
            this.label6.Click += new System.EventHandler(this.label6_Click);
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
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(208, 132);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(198, 23);
            this.button3.TabIndex = 18;
            this.button3.Text = "Clear Stored Facebook Credentials";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
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
            this.tabFacebook.ResumeLayout(false);
            this.tabFacebook.PerformLayout();
            this.tabTwitter.ResumeLayout(false);
            this.tabTwitter.PerformLayout();
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
        private System.Windows.Forms.CheckBox autoUpdateCheckbox;
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
        private System.Windows.Forms.CheckBox enableGOTVCheckbox;
        private System.Windows.Forms.Label labelEnabledTasks;
        private System.Windows.Forms.Label labelExpGOTV;
        private System.Windows.Forms.TabPage tabFacebook;
        private System.Windows.Forms.CheckBox checkRememberPasswordByDefaultCheckbox;
        private System.Windows.Forms.CheckBox useCustomEventsCheckbox;
        private System.Windows.Forms.CheckBox useFTBEventsCheckbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabTwitter;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox enableFacebankingCheckbox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox enableTwitterCheckbox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox tweetRedditNewsCheckbox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox twitterAccessTokenTextbox;
        private System.Windows.Forms.TextBox twitterUserIdTextbox;
        private System.Windows.Forms.TextBox twitterUsernameTextbox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button3;
    }
}