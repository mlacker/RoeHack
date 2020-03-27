using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RoeHack.Library.Core.Logging;

namespace RoeHack.Forms
{
    public partial class InjectorDebugForm : Form
    {
        private readonly Injector injector;
        private readonly Form injectForm;
        private readonly ILog logger;

        public InjectorDebugForm()
        {
            InitializeComponent();

            this.logger = new TextboxLogger(txtOutput);
            this.injector = new Injector(logger);
            this.injectForm = new InjectorForm(injector, this, logger);

            this.injectForm.Show();

            RefreshProcesses();
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            injector.Inject(cbxProcess.SelectedValue.ToString());

            btnInject.Enabled = false;
            btnDetach.Enabled = true;
        }

        private void btnDetach_Click(object sender, EventArgs e)
        {
            injector.Close();

            btnInject.Enabled = true;
            btnDetach.Enabled = false;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshProcesses();
        }

        private void RefreshProcesses()
        {
            cbxProcess.DataSource = Process.GetProcesses().Where(m => !string.IsNullOrEmpty(m.MainWindowTitle)).ToList();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
        }

        private void InjectorDebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (injector.Injected)
            {
                injector.Close();

                Thread.Sleep(500);
            }
        }

        private void InjectorDebugForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                Hide();
                injectForm.Show();
            }
        }
    }
}
