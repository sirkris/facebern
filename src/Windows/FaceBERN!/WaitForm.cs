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
    public partial class WaitForm : Form
    {
        private string waitMsg;

        public WaitForm(string waitMsg = null)
        {
            InitializeComponent();
            this.waitMsg = waitMsg;
        }

        private void WaitForm_Load(object sender, EventArgs e)
        {
            if (waitMsg != null)
            {
                messageRichTextbox.Text = waitMsg;
            }

            messageRichTextbox.SelectAll();
            messageRichTextbox.SelectionAlignment = HorizontalAlignment.Center;
        }
    }
}
