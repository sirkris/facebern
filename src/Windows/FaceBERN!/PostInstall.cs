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
    public partial class PostInstall : Form
    {
        private Form1 Main;
        private string appVersion;

        bool next = false;

        public PostInstall(Form1 Main, string appVersion)
        {
            InitializeComponent();
            this.Main = Main;
            this.appVersion = appVersion;

            Main.UseWaitCursor = true;
            Main.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PostInstallFacebook postInstallFacebook = new PostInstallFacebook(Main, appVersion);
            postInstallFacebook.Show();

            next = true;
            this.Close();
        }

        private void PostInstall_Load(object sender, EventArgs e)
        {
            labelVersion.Text = appVersion;
            Main.UseWaitCursor = true;
        }

        private void PostInstall_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (next == false)
            {
                Main.Exit();
            }
        }
    }
}
