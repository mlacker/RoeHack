using System;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    public partial class InjectorForm : Form
    {
        private readonly Injector injector = new Injector();
        private readonly DebugForm debugForm;

        public InjectorForm()
        {
            InitializeComponent();

            debugForm = new DebugForm(injector, this);
        }

        private void btnInjectSwitch_Click(object sender, EventArgs e)
        {
            if (!injector.Injected)
            {
                //injector.Inject("Europa_Client");
                btnInjectSwitch.Text = "关闭";
            }
            else
            {
                //injector.Close();
                btnInjectSwitch.Text = "开启";
            }
        }

        private void InjectorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (injector.Injected)
            {
                injector.Close();
            }
        }

        private void InjectorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                Hide();
                debugForm.Show();
            }
        }
    }
}
