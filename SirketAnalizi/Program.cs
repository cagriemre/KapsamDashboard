using System;
using System.Windows.Forms;

namespace KapsamDashboard.UI
{
    /// <summary>
    /// Ana program giriş noktası
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktası.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}