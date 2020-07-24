using Microsoft.VisualStudio.Data.Services;
using System;
using System.Windows.Forms;

namespace SubSonic.Core.VisualStudio.Forms
{
    public partial class DataContextForm
        : Form
    {
        private TabControl tabControl1;
        private TabPage connection;
        private Button create;
        private ComboBox connections;
        private Label connectionsLabel;
        private Button addNewConnection;
        private TabPage schema;

        public int TabControlPaddingBottom { get; private set; }

        public DataContextForm(IVsDataExplorerConnectionManager vsDataExplorerConnectionManager)
        {
            this.connectionManager = vsDataExplorerConnectionManager;

            InitializeComponent();
            BindDataSources();
        }
        
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.connection = new System.Windows.Forms.TabPage();
            this.connections = new System.Windows.Forms.ComboBox();
            this.connectionsLabel = new System.Windows.Forms.Label();
            this.schema = new System.Windows.Forms.TabPage();
            this.create = new System.Windows.Forms.Button();
            this.addNewConnection = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.connection.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.connection);
            this.tabControl1.Controls.Add(this.schema);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(325, 202);
            this.tabControl1.TabIndex = 0;
            // 
            // connection
            // 
            this.connection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.connection.Controls.Add(this.addNewConnection);
            this.connection.Controls.Add(this.connections);
            this.connection.Controls.Add(this.connectionsLabel);
            this.connection.Location = new System.Drawing.Point(4, 22);
            this.connection.Name = "connection";
            this.connection.Padding = new System.Windows.Forms.Padding(3);
            this.connection.Size = new System.Drawing.Size(317, 176);
            this.connection.TabIndex = 0;
            this.connection.Text = "Connection";
            this.connection.UseVisualStyleBackColor = true;
            // 
            // connections
            // 
            this.connections.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connections.FormattingEnabled = true;
            this.connections.Location = new System.Drawing.Point(131, 3);
            this.connections.Name = "connections";
            this.connections.Size = new System.Drawing.Size(126, 21);
            this.connections.TabIndex = 1;
            // 
            // connectionsLabel
            // 
            this.connectionsLabel.AutoSize = true;
            this.connectionsLabel.Location = new System.Drawing.Point(7, 7);
            this.connectionsLabel.Name = "connectionsLabel";
            this.connectionsLabel.Size = new System.Drawing.Size(118, 13);
            this.connectionsLabel.TabIndex = 0;
            this.connectionsLabel.Text = "Sql Server Connections";
            // 
            // schema
            // 
            this.schema.Location = new System.Drawing.Point(4, 22);
            this.schema.Name = "schema";
            this.schema.Padding = new System.Windows.Forms.Padding(3);
            this.schema.Size = new System.Drawing.Size(317, 176);
            this.schema.TabIndex = 1;
            this.schema.Text = "Schema";
            this.schema.UseVisualStyleBackColor = true;
            // 
            // create
            // 
            this.create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.create.Location = new System.Drawing.Point(257, 221);
            this.create.Name = "create";
            this.create.Size = new System.Drawing.Size(75, 23);
            this.create.TabIndex = 1;
            this.create.Text = "Create";
            this.create.UseVisualStyleBackColor = true;
            this.create.Click += new System.EventHandler(this.Create_Click);
            // 
            // addNewConnection
            // 
            this.addNewConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addNewConnection.Location = new System.Drawing.Point(263, 2);
            this.addNewConnection.Name = "addNewConnection";
            this.addNewConnection.Size = new System.Drawing.Size(48, 23);
            this.addNewConnection.TabIndex = 2;
            this.addNewConnection.Text = "Add";
            this.addNewConnection.UseMnemonic = false;
            this.addNewConnection.UseVisualStyleBackColor = true;
            this.addNewConnection.Click += new System.EventHandler(this.AddNewConnection_Click);
            // 
            // DataContextForm
            // 
            this.AcceptButton = this.create;
            this.ClientSize = new System.Drawing.Size(347, 260);
            this.Controls.Add(this.create);
            this.Controls.Add(this.tabControl1);
            this.Name = "DataContextForm";
            this.tabControl1.ResumeLayout(false);
            this.connection.ResumeLayout(false);
            this.connection.PerformLayout();
            this.ResumeLayout(false);

        }

        private void Create_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void AddNewConnection_Click(object sender, EventArgs e)
        {
            connectionManager.PromptAndAddConnection();
            BindDataSources();
        }
    }
}
