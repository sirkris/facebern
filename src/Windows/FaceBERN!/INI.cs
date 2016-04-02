using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FaceBERN_
{
    public class INI
    {
        protected Assembly csINI;
        protected Type csINIType;
        protected object csINIInstance;

        public INI(Assembly csLogPass = null, Type csLogTypePass = null, object csLogInstancePass = null)
        {
            /* Load the DLL.  --Kris */
            csINI = Assembly.LoadFile(Environment.CurrentDirectory + @"\csINI.dll");

            /* Retrieve the "INI" class definition.  --Kris */
            csINIType = csINI.GetType("csINI.INI");

            /* Instantiate the "INI" class.  --Kris */
            csINIInstance = Activator.CreateInstance(csINIType, new object[] { true, true, csLogPass, csLogTypePass, csLogInstancePass });
        }

        public Dictionary<string, string> Load(string filename)
        {
            return (Dictionary<string, string>)csINIType.InvokeMember("Load",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csINIInstance,
                new object[] { filename });
        }

        public Dictionary<string, Dictionary<string, string>> LoadWithHeaders(string filename)
        {
            return (Dictionary<string, Dictionary<string, string>>)csINIType.InvokeMember("LoadWithHeaders",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csINIInstance,
                new object[] { filename });
        }

        public bool Save(string filename, Dictionary<string, string> directives)
        {
            return (bool)csINIType.InvokeMember("Save",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csINIInstance,
                new object[] { filename, directives });
        }

        public bool Save(string filename, Dictionary<string, Dictionary<string, string>> directives)
        {
            return (bool)csINIType.InvokeMember("Save",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csINIInstance,
                new object[] { filename, directives });
        }

        public bool Create(string INIPath, string title, string subtitle, Dictionary<string, Dictionary<string, string>> directives)
        {
            return (bool)csINIType.InvokeMember("Create", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csINIInstance,
                new object[] { INIPath, title, subtitle, directives });
        }

        public bool Clear(string INIPath)
        {
            return (bool)csINIType.InvokeMember("Clear", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, csINIInstance,
                new object[] { INIPath });
        }
    }
}
