using Microsoft.Win32;
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
    public partial class PostInstallComplete : Form
    {
        private Form1 Main;
        private string appVersion;

        public PostInstallComplete(Form1 Main, string appVersion)
        {
            InitializeComponent();
            this.Main = Main;
            this.appVersion = appVersion;
        }

        private void PostInstallComplete_Load(object sender, EventArgs e)
        {
            labelVersion.Text = appVersion;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PostInstallTwitter postInstallTwitter = new PostInstallTwitter(Main, appVersion);
            postInstallTwitter.Show();

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (launchOnStartupCheckbox.Checked == true)
            {
                RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                runKey.SetValue(Globals.__APPNAME__, Application.ExecutablePath.ToString() + " /autoStart");
            }

            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey appKey = softwareKey.CreateSubKey("FaceBERN!");

            appKey.DeleteValue("postInstallNeeded", false);

            appKey.Close();
            softwareKey.Close();

            Main.UseWaitCursor = false;
            Main.Enabled = true;

            this.Close();
        }
    }
}
