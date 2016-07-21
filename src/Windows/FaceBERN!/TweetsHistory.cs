﻿using Newtonsoft.Json;
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
        private Form1 Main;
        private List<TweetsQueue> history;
        private Workflow workflow;

        public TweetsHistory(Form1 Main)
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
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                history = workflow.GetTweetsHistoryFromBirdie();
                List<TweetsQueue> history_remote = history;

                if (history == null || history.Count == 0)
                {
                    MessageBox.Show("No tweets history found.  Have you clicked 'START' yet?");
                    this.Close();
                }
                else
                {
                    tweetsLogListView.Items.Clear();

                    for (int ii = 0; ii < history.Count; ii++)
                    {
                        history[ii].SetStatusID(workflow.GetTwitterStatusId(history[ii].GetTweet()));
                    }

                    /* Use this opportunity to update Birdie API with the status IDs.  --Kris */
                    bool undoOk = workflow.UpdateBirdieTwitterStatusIDs(history);

                    int i = 0;
                    foreach (TweetsQueue entry in Enumerable.Reverse(history))
                    {
                        TweetsQueue entryRemote = null;  // If statusId update fails, only show Undo for tweets that already have a statusId in the database record.  --Kris
                        for (int ii = 0; ii < history_remote.Count; ii++)
                        {
                            if (history_remote[ii].GetTID() == entry.GetTID())
                            {
                                entryRemote = history_remote[ii];
                                break;
                            }
                        }

                        tweetsLogListView.Items.Add(new ListViewItem(new[] { entry.GetTweeted().Value.ToString(), entry.GetTweet(), Globals.CampaignName(entry.GetCampaignId()), 
                        entry.GetSource(), ( entry.GetStatusID() != null && (undoOk || (entryRemote != null && entryRemote.GetStatusID() != null)) ? "Undo" : "" ) }));
                        tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].UseItemStyleForSubItems = false;
                        tweetsLogListView.Items[tweetsLogListView.Items.Count - 1].SubItems[4].Tag = JsonConvert.SerializeObject(new List<string> { entry.GetStatusID(), entry.GetTweet() });
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
                workflow.ReportExceptionSilently(e, "Error loading history.");

                MessageBox.Show("Error loading history!");
                this.Close();
            }
        }

        private void tweetsLogListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = tweetsLogListView.HitTest(e.Location);

            if (hit.SubItem != null && hit.SubItem.Text.Equals("Undo") && hit.SubItem.Tag != null)
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

            if (hit.SubItem != null && hit.SubItem.Text.Equals("Undo") && hit.SubItem.Tag != null)
            {
                string twitterStatusId;
                string tweet;
                try
                {
                    List<string> tag = JsonConvert.DeserializeObject<List<string>>(hit.SubItem.Tag.ToString());
                    twitterStatusId = tag[0];
                    tweet = tag[1];
                }
                catch (Exception ex)
                {
                    workflow.ReportExceptionSilently(ex, "Error deserializing Tag string in TweetsHistory.tweetsLogListView_MouseUp.");

                    MessageBox.Show("Undo not available right now due to an error.");
                    return;
                }

                DialogResult dr = MessageBox.Show("Are you sure you want to delete tweet \"" + tweet + "\"?", "Confirm Tweet Deletion", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    workflow.DeleteTweet(twitterStatusId);

                    LoadHistory();
                }
            }
        }

        private void tweetsLogListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
