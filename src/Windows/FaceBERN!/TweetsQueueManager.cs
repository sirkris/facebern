using Newtonsoft.Json;
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
    public partial class TweetsQueueManager : Form
    {
        private Form1 Main;
        private List<TweetsQueue> queue;
        private Workflow workflow;

        public TweetsQueueManager(Form1 Main)
        {
            InitializeComponent();
            this.Main = Main;
            workflow = new Workflow(Main);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TweetsHistory_Load(object sender, EventArgs e)
        {
            LoadQueue();
        }

        private void LoadQueue()
        {
            try
            {
                workflow.UpdateLocalTweetsQueue(true);
                queue = workflow.GetTweetsQueue();
                List<TweetsQueue> queue_remote = queue;

                if (queue == null || queue.Count == 0)
                {
                    MessageBox.Show("The tweets queue is currently empty.  Please check back later.");
                    this.Close();
                }
                else
                {
                    tweetsLogListView.Items.Clear();

                    int i = 0;
                    foreach (TweetsQueue entry in Enumerable.Reverse(queue))
                    {
                        tweetsLogListView.Items.Add(new ListViewItem(new[] { entry.GetDiscovered().ToString(), entry.GetTweet(), Globals.CampaignName(entry.GetCampaignId()), 
                        entry.GetSource(), ((entry != null && entry.GetTweet() != null) ? "Remove" : "" ) }));
                        tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].UseItemStyleForSubItems = false;
                        tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].SubItems[4].Tag = JsonConvert.SerializeObject(new List<string> { entry.GetTweet() });
                        tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].SubItems[4].ForeColor = Color.Blue;
                        tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].SubItems[4].Font = new Font(tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].SubItems[4].Font, FontStyle.Underline);

                        i++;
                        if (i % 2 == 0)
                        {
                            for (int ii = 0; ii < tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].SubItems.Count; ii++)
                            {
                                tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].SubItems[ii].BackColor = Color.PowderBlue;
                            }
                        }
                    }

                    tweetsLogListView.MouseMove += tweetsLogListView_MouseMove;
                    tweetsLogListView.MouseUp += tweetsLogListView_MouseUp;
                }
            }
            catch (Exception e)
            {
                workflow.ReportExceptionSilently(e, "Error loading tweets queue.");

                MessageBox.Show("Error loading tweets queue!");
                this.Close();
            }
        }

        private void tweetsLogListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = tweetsLogListView.HitTest(e.Location);

            if (hit.SubItem != null && hit.SubItem.Text.Equals("Remove") && hit.SubItem.Tag != null)
            {
                tweetsLogListView.Cursor = Cursors.Hand;
            }
            else
            {
                tweetsLogListView.Cursor = Cursors.Default;
            }
        }

        private void tweetsLogListView_MouseUp(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = tweetsLogListView.HitTest(e.Location);

            if (hit.SubItem != null && hit.SubItem.Text.Equals("Remove") && hit.SubItem.Tag != null)
            {
                string tweet;
                try
                {
                    List<string> tag = JsonConvert.DeserializeObject<List<string>>(hit.SubItem.Tag.ToString());
                    tweet = tag[0];
                }
                catch (Exception ex)
                {
                    workflow.ReportExceptionSilently(ex, "Error deserializing Tag string in TweetsQueueManager.tweetsLogListView_MouseUp.");

                    MessageBox.Show("Remove not available right now due to an error.");
                    return;
                }

                DialogResult dr = MessageBox.Show("Are you sure you want to remove tweet \"" + tweet + "\" from the local queue?", "Confirm Queue Entry Deletion", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    workflow.RemoveFromLocalTweetsQueue(tweet);

                    LoadQueue();
                }
            }
        }
    }
}
