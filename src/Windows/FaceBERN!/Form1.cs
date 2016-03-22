using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    public partial class Form1 : Form
    {
        protected const string Logname = "FaceBERN!";
        protected string logLibDir = Environment.CurrentDirectory;

        protected Assembly csLog = null;
        protected Type csLogType = null;
        protected object csLogInstance = null;

        protected bool csLogEnabled = false;

        private Icon trayIcon;

        public Form1(bool logging = true, bool failSilently = true, Assembly csLogPass = null, Type csLogTypePass = null, object csLogInstancePass = null)
        {
            InitializeComponent();
            labelVersion.Text = Globals.__VERSION__;
            this.Resize += Form1_Resize;

            /* Initialize the log.  --Kris */
            if (logging == true)
            {
                csLogEnabled = InitLog(failSilently, csLogPass, csLogTypePass, csLogInstancePass);
            }
        }

        [DllImport("user32.dll", EntryPoint = "ShowCaret")]
        public static extern long ShowCaret(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern long HideCaret(IntPtr hwnd);

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.MouseDoubleClick += notifyIcon1_DoubleClick;
            trayIcon = notifyIcon1.Icon;
            browserModeComboBox.SelectedIndex = 0;
            notifyIcon1.Visible = false;

            HideCaret(outBox.Handle);

            LogW("Ready.");
        }

        internal bool InitLog(bool failSilently, Assembly csLogPass, Type csLogTypePass, object csLogInstancePass)
        {
            if (csLogPass == null)
            {
                try
                {
                    csLog = Assembly.LoadFile(logLibDir + @"\csLog.dll");
                }
                catch (Exception e)
                {
                    if (failSilently == false)
                    {
                        throw e;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                csLog = csLogPass;
            }

            if (csLog != null
                && csLogTypePass == null)
            {
                try
                {
                    csLogType = csLog.GetType("csLog.Log");
                }
                catch (Exception e)
                {
                    if (failSilently == false)
                    {
                        throw e;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                csLogType = csLogTypePass;
            }

            if (csLog != null && csLogType != null
                && csLogInstancePass == null)
            {
                try
                {
                    csLogInstance = Activator.CreateInstance(csLogType);
                }
                catch (Exception e)
                {
                    if (failSilently == false)
                    {
                        throw e;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                csLogInstance = csLogInstancePass;
            }

            /* Initialize the log.  --Kris */
            try
            {
                csLogType.InvokeMember("Init", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                    new object[] { Logname, "string" });
            }
            catch (Exception e)
            {
                if (failSilently == false)
                {
                    throw e;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /* Interact with the log handler.  --Kris */
        internal void Log(string text = null, string action = "append", bool newline = true)
        {
            if (csLogEnabled == true)
            {
                switch (action.ToLower())
                {
                    default:
                    case "append":
                        csLogType.InvokeMember("Append", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                            null, csLogInstance, new object[] { Logname, text, newline });
                        break;
                    case "increment":
                        csLogType.InvokeMember("Increment", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                            null, csLogInstance, new object[] { Logname, Int32.Parse(text) });
                        break;
                    case "decrement":
                        csLogType.InvokeMember("Decrement", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                            null, csLogInstance, new object[] { Logname, Int32.Parse(text) });
                        break;
                    case "save":
                        csLogType.InvokeMember("Save", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                            null, csLogInstance, new object[] { Logname });
                        break;
                }
            }
        }

        /* Save the log buffer to file.  --Kris */
        internal void saveLog()
        {
            Log(null, "save");
        }

        /* Append to log and optionally display in log window (should only include log entries that are relevant/useful to the end-user).  --Kris */
        internal void LogW(string text, bool show = true, bool appendW = true, bool newline = true, bool timestamp = true)
        {
            string logText = (timestamp ? "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " : "") + text;

            Log(logText, "append", newline);

            if (show == true)
            {
                this.outBox.Text = (appendW ? this.outBox.Text : "") + text + (newline ? Environment.NewLine : "");
                this.outBox.SelectionStart = this.outBox.Text.Length;
                this.outBox.ScrollToCaret();

                HideCaret(outBox.Handle);
            }

            saveLog();
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

        private void donateToBernieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessStartInfo pso = new ProcessStartInfo("https://secure.actblue.com/contribute/page/reddit-for-bernie");
            Process.Start(pso);
        }
    }
}
