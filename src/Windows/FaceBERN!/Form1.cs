using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            labelVersion.Text = Globals.__VERSION__;
            this.Resize += Form1_Resize;
        }

        private Icon trayIcon;

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.MouseDoubleClick += notifyIcon1_DoubleClick;
            trayIcon = notifyIcon1.Icon;
            browserModeComboBox.SelectedIndex = 0;
            notifyIcon1.Visible = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(10000);
                this.Hide();
            }
            else
            {
                notifyIcon1.Visible = false;
            }
        }

        private void openControlCenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            openControlCenterToolStripMenuItem_Click(sender, e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
