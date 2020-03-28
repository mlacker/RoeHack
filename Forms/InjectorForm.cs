﻿using RoeHack.Library.Core.Logging;
using System;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    public partial class InjectorForm : Form
    {
        private readonly Injector injector;

        public InjectorForm(Injector injector)
        {
            InitializeComponent();

            this.injector = injector;
        }

        public Form SwitchForm { get; set; }

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
            Application.Exit();
        }

        private void InjectorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                Hide();
                SwitchForm.Show();
            }
        }
    }
}
