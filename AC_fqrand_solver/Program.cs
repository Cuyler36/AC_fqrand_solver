using System;
using System.Windows.Forms;

namespace AC_fqrand_solver
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AllDebtsForm()); // new Form1()
        }
    }
}
