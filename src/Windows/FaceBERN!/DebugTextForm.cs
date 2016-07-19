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
    public partial class DebugTextForm : Form
    {
        private string desc;

        public DebugTextForm(string desc = null)
        {
            InitializeComponent();
            this.desc = desc;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void DebugTextForm_Load(object sender, EventArgs e)
        {
            if (desc != null)
            {
                labelDescription.Text = desc;
            }
        }
    }
}
