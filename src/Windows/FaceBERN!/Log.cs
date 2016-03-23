using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class Log
    {
        internal Assembly csLog;
        internal Type csLogType;
        internal object csLogInstance;

        public Log()
        {
            /* Load the DLL.  --Kris */
            csLog = Assembly.LoadFile(Environment.CurrentDirectory + @"\csLog.dll");

            /* Retrieve the "Log" class definition.  --Kris */
            csLogType = csLog.GetType("csLog.Log");

            /* Instantiate the "Log" class.  --Kris */
            csLogInstance = Activator.CreateInstance(csLogType);
        }

        public void Init(string logname, string emutype = "string")
        {
            csLogType.InvokeMember("Init", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                new object[] { logname, emutype });
        }

        public void Append(string logname, string text, bool newline = false)
        {
            csLogType.InvokeMember("Append", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                new object[] { logname, text, newline });
        }

        public bool Increment(string logname, int count = 1)
        {
            return (bool)csLogType.InvokeMember("Increment", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                new object[] { logname, count });
        }

        public bool Decrement(string logname, int count = 1)
        {
            return (bool)csLogType.InvokeMember("Decrement", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                new object[] { logname, count });
        }

        public bool Toggle(string logname)
        {
            return (bool)csLogType.InvokeMember("Toggle", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                new object[] { logname });
        }

        public void SetBool(string logname, bool value = false)
        {
            csLogType.InvokeMember("SetBool", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                new object[] { logname, value });
        }

        public void Save(string logname, bool remove = true)
        {
            csLogType.InvokeMember("Save", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.OptionalParamBinding, null, csLogInstance,
                new object[] { logname, remove });
        }

        public void Save()
        {
            csLogType.InvokeMember("Save", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.OptionalParamBinding, null, csLogInstance,
                new object[] { });
        }

        public string GetData(string logname)
        {
            return (string)csLogType.InvokeMember("GetData", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                new object[] { logname });
        }

        public string Combine(Dictionary<int, Dictionary<string, string>> statements, Dictionary<int, string> times, Dictionary<int, string> timespans, string userclass, int statementkey)
        {
            return (string)csLogType.InvokeMember("Combine", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csLogInstance,
                new object[] { statements, times, timespans, userclass, statementkey });
        }
    }
}
