using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    public partial class WorkflowForm2 : Form
    {
        public string installPath
        {
            get;
            private set;
        }

        public string branchName
        {
            get;
            private set;
        }

        public bool cancel
        {
            get;
            private set;
        }
        
        public WorkflowForm2()
        {
            InitializeComponent();
        }

        private void WorkflowForm2_Load(object sender, EventArgs e)
        {
            SetLocation(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + Path.DirectorySeparatorChar + @"FaceBERN!");
            branchName = "master";

            branchComboBox.SelectedIndex = 1;  // TODO - Set back to 0 (master) after the first production release.  --Kris
            branchComboBox.Enabled = false;  // TODO - Get rid of this line after the first production release.  --Kris

            cancel = true;
        }

        private void SetLocation(string path)
        {
            labelLocation.Text = path;
            installPath = path;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cancel = false;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cancel = true;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.SelectedPath = installPath;
            fbd.ShowNewFolderButton = true;
            fbd.Description = "Please specify where you would like to install Birdie on your computer.";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SetLocation(fbd.SelectedPath);
            }
        }

        private void branchComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> branches = new List<string>();
            branches.Add("master");
            branches.Add("develop");

            if (branchComboBox.SelectedIndex < branches.Count)
            {
                branchName = branches[branchComboBox.SelectedIndex];
            }
        }
    }
}
