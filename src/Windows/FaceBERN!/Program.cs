using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceBERN_
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0)
            {
                bool updated = false;
                bool logging = true;
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower().Trim())
                    {
                        case @"/nolog":
                            logging = false;
                            break;
                        case @"/updated":
                            updated = true;
                            break;
                    }
                }

                Application.Run(new Form1(updated, logging));
            }
            else
            {
                Application.Run(new Form1());
            }
        }
    }
}
