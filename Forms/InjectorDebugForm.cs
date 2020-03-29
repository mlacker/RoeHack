using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RoeHack.Library.Core;
using RoeHack.Library.Core.Logging;

namespace RoeHack.Forms
{
    public partial class InjectorDebugForm : Form
    {
        private readonly Injector injector;
        private readonly ILog logger;

        public InjectorDebugForm(Injector injector, TextboxLogger logger)
        {
            InitializeComponent();

            this.injector = injector;
            this.logger = logger;

            logger.Textbox = txtOutput;

            RefreshProcesses();
        }

        public Form SwitchForm { get; set; }

        private void btnInject_Click(object sender, EventArgs e)
        {
            try
            {
                injector.Parameter.DirectXVersion = (DirectXVersion)cbxVersion.SelectedIndex;
                injector.Inject(cbxProcess.SelectedValue.ToString());

                btnInject.Enabled = false;
                btnDetach.Enabled = true;
            }
            catch (AppException ex)
            {
                logger.Error(ex.Message);
            }
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
            Application.Exit();
        }

        private void InjectorDebugForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                Hide();
                SwitchForm.Show();
            }
        }
    }
}
