using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    public partial class DebugForm : Form
    {
        private readonly Injector injector;
        private readonly Form parent;

        public DebugForm(Injector injector, Form parent)
        {
            InitializeComponent();

            this.injector = injector;
            this.parent = parent;
        }

        private void btnInject_Click(object sender, EventArgs e)
        {

        }

        private void btnDetach_Click(object sender, EventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            parent.Close();
        }

        private void DebugForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                Hide();
                parent.Show();
            }
        }
    }
}
