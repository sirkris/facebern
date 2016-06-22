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

        public PostInstall(Form1 Main, string appVersion)
        {
            InitializeComponent();
            this.Main = Main;
            this.appVersion = appVersion;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            Main.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PostInstallFacebook postInstallFacebook = new PostInstallFacebook(Main, appVersion);
            postInstallFacebook.Show();

            this.Close();
        }

        private void PostInstall_Load(object sender, EventArgs e)
        {
            labelVersion.Text = appVersion;
            Main.UseWaitCursor = true;
        }
    }
}
