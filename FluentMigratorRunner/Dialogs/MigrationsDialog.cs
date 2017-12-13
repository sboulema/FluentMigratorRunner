using EnvDTE;
using FluentMigratorRunner.Helpers;
using System;
using System.Windows.Forms;

namespace FluentMigratorRunner.Dialogs
{
    public partial class MigrationsDialog : Form
    {
        public string SelectedVersion;

        public MigrationsDialog(DTE dte)
        {
            InitializeComponent();

            MigrationsComboBox.Items.AddRange(ProjectHelper.GetMigrations(dte).ToArray());
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            SelectedVersion = MigrationsComboBox.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
