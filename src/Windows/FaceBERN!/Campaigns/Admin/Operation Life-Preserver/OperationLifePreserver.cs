using Newtonsoft.Json;
using OpenQA.Selenium;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_.Campaigns.Admin
{
    public class OperationLifePreserver : Generic
    {
        public OperationLifePreserver(WorkflowFacebook workflowFacebook, WorkflowTwitter workflowTwitter, Workflow workflow, bool refresh = true)
            : base(workflowFacebook, workflowTwitter, workflow, refresh)
        { }

        public override bool ExecuteFacebook()
        {
            return true;
        }

        public override bool ExecuteTwitter()
        {
            return true;
        }

        /* To iterate a date range backwards, simply pass end as start and start as end.  --Kris */
        public IEnumerable<DateTime> IterateDays(DateTime start, DateTime end)
        {
            if (start == end)
            {
                yield return start;
            }

            int increment = (start < end ? 1 : -1);
            for (DateTime t = start.Date; t.Date != end.Date; t = t.AddDays(increment))
            {
                yield return t;
            }
        }
    }
}
