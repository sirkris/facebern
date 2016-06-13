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
    public partial class TwitPin : Form
    {
        internal string pin
        {
            get;
            private set;
        }

        public TwitPin(string pin = "")
        {
            InitializeComponent();
            this.pin = pin;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.pin = pinTextbox.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void TwitPin_Shown(object sender, EventArgs e)
        {
            pinTextbox.Text = pin;
        }
    }
}
