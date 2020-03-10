using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoeHack.Forms
{
    public partial class InjectorForm : Form
    {
        private readonly Injector injector = new Injector();

        public InjectorForm()
        {
            InitializeComponent();
        }

        private void btnEnable_Click(object sender, EventArgs e)
        {
            injector.Inject("hl");
            //injector.Inject("Europa_Client");
        }
    }
}
