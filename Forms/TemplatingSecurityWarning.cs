using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubSonic.Core.VisualStudio.Forms
{
    public partial class TemplatingSecurityWarning : Form
    {
        public TemplatingSecurityWarning()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Close();
        }

        private void Cancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            Close();
        }
    }
}
