using RoeHack.Library.Core.Logging;
using System;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    public partial class InjectorForm : Form
    {
        private readonly Injector injector;
        private readonly Form injectDebugForm;
        private readonly ILog logger;

        public InjectorForm(Injector injector, Form injectDebugForm, ILog logger)
        {
            InitializeComponent();

            this.injectDebugForm = injectDebugForm;
            this.injector = injector;
            this.logger = logger;
        }

        private void btnInjectSwitch_Click(object sender, EventArgs e)
        {
            if (!injector.Injected)
            {
                injector.Inject("Europa_Client");
                btnInjectSwitch.Text = "关闭";
            }
            else
            {
                injector.Close();
                btnInjectSwitch.Text = "开启";
            }
        }

        private void InjectorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            injectDebugForm.Close();
        }

        private void InjectorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                Hide();
                injectDebugForm.Show();
            }
        }

        private void InjectorForm_Shown(object sender, EventArgs e)
        {
            this.injectDebugForm.Hide();
        }
    }
}
