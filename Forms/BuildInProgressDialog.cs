using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubSonic.Core.VisualStudio.Forms
{
    public partial class BuildInProgressDialog 
        : Form
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        public BuildInProgressDialog()
        {
            this.cancellationTokenSource = new CancellationTokenSource(1000 * 120);

            InitializeComponent();
        }

        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        private void button1_Click(object sender, EventArgs e)
        {
            this.cancellationTokenSource.Cancel(true);
            this.DialogResult = DialogResult.Cancel;

            Close();
        }  
    }
}
