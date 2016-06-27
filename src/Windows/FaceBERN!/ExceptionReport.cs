using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    public class ExceptionReport
    {
        private string logName = "ExceptionReport";
        public Log ExceptionReportLog;

        private Form1 Main;

        internal Exception ex;

        internal string message = null;
        internal string stackTrace = null;
        internal string source = null;
        internal string type = null;
        internal DateTime discovered;

        internal string logMsg = null;

        bool error = false;

        private string lastLogMsg = null;

        public ExceptionReport(Form1 Main, Exception ex, string logMsg = null, bool autoSend = true)
        {
            this.Main = Main;
            this.ex = ex;

            try
            {
                this.message = ex.Message;
                this.stackTrace = ex.StackTrace;
                this.source = ex.Source;
                this.type = ex.GetType().ToString();
                this.discovered = DateTime.Now;

                this.logMsg = logMsg;
            }
            catch (Exception e)
            {
                Log("Warning:  Unable to parse Exception : " + e.ToString());

                error = true;

                return;
            }

            if (autoSend)
            {
                Send();
            }
        }

        public void SetLogMsg(string logMsg)
        {
            this.logMsg = logMsg;
        }

        public bool Send()
        {
            try
            {
                Workflow workflow = new Workflow(Main);

                Dictionary<string, dynamic> body = new Dictionary<string, dynamic>();
                body.Add("exType", type);
                body.Add("exMessage", message);
                body.Add("exStackTrace", stackTrace);
                body.Add("exSource", source);
                body.Add("exToString", ex.ToString());
                body.Add("logMsg", logMsg);
                body.Add("appName", @"FaceBERN!");
                body.Add("clientId", workflow.GetAppID());

                IRestResponse res = workflow.BirdieQuery(@"/exceptions", "POST", null, JsonConvert.SerializeObject(body));

                if (res == null || res.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    Log("Warning:  BirdieQuery failed (code=" + (res != null ? res.StatusCode.ToString() : "" ) + ") for exception report on exception : " + ex.ToString());

                    error = true;

                    return false;
                }
            }
            catch (Exception e)
            {
                Log("Warning:  Unable to report Exception : " + e.ToString());

                error = true;

                return false;
            }

            Log("Exception reported successfully.");

            return true;
        }

        private void Log(string text, bool show = true, bool appendW = true, bool newline = true, bool timestamp = true, bool suppressDups = true)
        {
            if (Main.InvokeRequired)
            {
                Main.BeginInvoke(
                    new MethodInvoker(
                        delegate() { Log(text, show, appendW, newline, timestamp); }));
            }
            else
            {
                if (suppressDups == true && text.Equals(lastLogMsg))
                {
                    return;
                }

                lastLogMsg = text;

                Main.LogW(text, show, appendW, newline, timestamp, logName, ExceptionReportLog);

                Main.Refresh();
            }
        }
    }
}
