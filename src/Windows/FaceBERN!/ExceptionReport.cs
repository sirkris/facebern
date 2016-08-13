using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    [Serializable]
    public class ExceptionReport
    {
        private string logName = "ExceptionReport";
        internal Log ExceptionReportLog;

        internal Form1 Main;

        public Exception ex = null;

        public string message = null;
        public string stackTrace = null;
        public string source = null;
        public string type = null;
        public DateTime discovered;
        public Dictionary<dynamic, dynamic> data;

        public string logMsg = null;

        internal bool error = false;

        private string lastLogMsg = null;

        public ExceptionReport(Form1 Main, Exception ex, string logMsg = null, bool autoSend = true)
        {
            ExInit(Main, ex, logMsg, autoSend);
        }

        public void ExInit(Form1 Main, Exception ex, string logMsg = null, bool autoSend = true)
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
                if (ex.Data != null && ex.Data.Count > 0)
                {
                    this.data = new Dictionary<dynamic, dynamic>();
                    foreach (DictionaryEntry pair in ex.Data)
                    {
                        this.data.Add(pair.Key, pair.Value);
                    }
                }

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
            if (Main == null)
            {
                return false;
            }

            try
            {
                Workflow workflow = new Workflow(Main);

                Dictionary<string, dynamic> body = new Dictionary<string, dynamic>();
                body.Add("exType", type);
                body.Add("exMessage", message);
                body.Add("exStackTrace", stackTrace);
                body.Add("exSource", source);
                body.Add("exToString", ex.ToString());
                if (data != null)
                {
                    try
                    {
                        body.Add("exData", JsonConvert.SerializeObject(data));
                    }
                    catch (Exception e)
                    {
                        Log("Warning:  Unable to serialize data for exception report : " + e.ToString());
                    }
                }
                body.Add("logMsg", logMsg);
                body.Add("appName", @"FaceBERN!");
                body.Add("appVersion", Globals.__VERSION__);
                body.Add("clientId", workflow.GetAppID());

                IRestResponse res = workflow.BirdieQuery(@"/exceptions", "POST", null, JsonConvert.SerializeObject(body));

                if (res == null || res.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    Log("Warning:  BirdieQuery failed (code=" + (res != null ? res.StatusCode.ToString() : "" ) + ") for exception report on exception : " + ex.ToString());
#if (DEBUG)
                    if (res != null)
                    {
                        Log("DEBUG:  StatusDescription=" + res.StatusDescription);
                        Log("DEBUG:  Body=" + res.Content);
                    }
                    else
                    {
                        Log("DEBUG:  Res is null.");
                    }
#endif

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
            if (Main == null)
            {
                return;
            }
            
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
