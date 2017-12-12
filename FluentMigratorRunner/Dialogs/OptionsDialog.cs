using EnvDTE;
using FluentMigratorRunner.Helpers;
using FluentMigratorRunner.Models;
using System;
using System.Windows.Forms;

namespace FluentMigratorRunner.Dialogs
{
    public partial class OptionsDialog : Form
    {
        private Options _options;

        public OptionsDialog(DTE dte)
        {
            InitializeComponent();

            OptionsHelper.Dte = dte;

            _options = OptionsHelper.GetOptions();
            ConnectionStringTextBox.Text = _options.Connection;

            DbTypeComboBox.Items.AddRange(Enum.GetNames(typeof(DbEnum)));
            DbTypeComboBox.Text = _options.DbType.ToString();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            _options.Connection = ConnectionStringTextBox.Text;
            _options.DbType = (DbEnum)Enum.Parse(typeof(DbEnum), DbTypeComboBox.Text);
            OptionsHelper.SaveOptions(_options);
            Close();
        }
    }
}
