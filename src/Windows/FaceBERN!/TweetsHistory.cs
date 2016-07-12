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
    public partial class TweetsHistory : Form
    {
        Form1 Main;
        List<TweetsQueue> history;

        public TweetsHistory(Form1 Main)
        {
            InitializeComponent();
            this.Main = Main;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TweetsHistory_Load(object sender, EventArgs e)
        {
            Workflow workflow = new Workflow(Main);
            history = workflow.GetTweetsHistoryFromBirdie();

            if (history == null || history.Count == 0)
            {
                MessageBox.Show("No tweets history found.  Have you clicked 'START' yet?");
                this.Close();
            }
            else
            {
                foreach (TweetsQueue entry in Enumerable.Reverse(history))
                {
                    tweetsLogListView.Items.Add(new ListViewItem(new[] { entry.GetTweeted().Value.ToString(), entry.GetTweet(), Globals.CampaignName(entry.GetCampaignId()), entry.GetSource() }));
                }
            }
        }
    }
}
