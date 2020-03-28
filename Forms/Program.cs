using System;
using System.Configuration;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var logger = new TextboxLogger();
            var injector = new Injector(logger);

            var injectorForm = new InjectorForm(injector);
            Form injectorDebugForm = new InjectorDebugForm(injector, logger)
            {
                SwitchForm = injectorForm
            };
            injectorForm.SwitchForm = injectorDebugForm;

            var startedForm = ConfigurationManager.AppSettings["StartedForm"] == "debug"
                ? injectorDebugForm : injectorForm;
            startedForm.Show();

            Application.ApplicationExit += (object sender, EventArgs e) =>
            {
                if (injector.Injected)
                    injector.Close();
            };

            Application.Run();
        }
    }
}
