using System;
using Eto.Drawing;
using Eto.Forms;
// ReSharper disable VirtualMemberCallInConstructor

namespace LiteDB.Explorer.Core.Dialogs
{
    /// <summary>
    /// A class for building a connection string.
    /// </summary>
    public class ConnectionStringDialog : Dialog<ConnectionString>
    {
        private readonly CheckBox journal, upgrade;
        private readonly NumericUpDown timeOutInSeconds;
        private readonly PasswordBox passwordTextBox;
        private readonly string filePath;
        private void onCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close(null);
        }
        private void onOkClick(object sender, EventArgs e)
        {   
            try
            {
                DialogResult = DialogResult.Ok;

                Close(new ConnectionString
                {
                    Filename = filePath,
                    Journal = journal.Checked ?? true,
                    Password = string.IsNullOrEmpty(passwordTextBox.Text) ? null : passwordTextBox.Text,
                    Timeout = TimeSpan.FromSeconds(timeOutInSeconds.Value),
                    Upgrade = upgrade.Checked ?? false
                });
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Connection String Creation Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
        }

        /// <summary>
        /// Creates a class for building a connection string.
        /// </summary>
        public ConnectionStringDialog(string path)
        {
            Icon = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Open.ico", typeof(AddCollectionDialog).Assembly);
            //Resizable = false;
            Result = null;
            Size = new Size(600, 200);
            Title = "Open Database";
            
            Content = new TableLayout
            {
                Padding = new Padding(5),
                Rows =
                {
                    new TableRow(new TableCell(new TableLayout
                    {
                        Padding = new Padding(0, 0, 0, 5),
                        Spacing = new Size(5, 5),
                        Rows =
                        {
                            new TableRow("Filename:", new TableCell(filePath = path, true)),
                            new TableRow("Password:", new TableCell(passwordTextBox = new PasswordBox
                            {
                                PasswordChar = '•'
                            }, true)),
                            new TableRow("Lock Timeout (in seconds):", new TableCell(timeOutInSeconds = new NumericUpDown
                            {
                                DecimalPlaces = 1,
                                MinValue = 0,
                                Value = 30
                            }, true)),
                            new TableRow("Journal:", new TableCell(journal = new CheckBox
                            {
                                Checked = true,
                                Text = "Enabled or disable double write check to ensure durability"
                            }, true)),
                            new TableRow("Upgrade:", new TableCell(upgrade = new CheckBox
                            {
                                Checked = false,
                                Text = "If true, try upgrade datafile from old version (v2)"
                            }, true))
                        }
                    }, true))
                    {
                        ScaleHeight = true
                    },
                    new TableRow(new TableLayout
                    {
                        Spacing = new Size(5, 5),
                        Rows =
                        {
                            new TableRow(
                                null,
                                new Button(onCancelClick)
                                {
                                    Text = "Cancel"
                                },
                                new Button(onOkClick)
                                {
                                    Text = "OK"
                                }
                            )
                        }
                    })
                    {
                        ScaleHeight = false
                    }
                }
            };
        }

        /// <summary>
        /// Gets the dialog results for the dialog box.
        /// </summary>
        public DialogResult DialogResult { get; private set; } = DialogResult.Cancel;
    }
}
