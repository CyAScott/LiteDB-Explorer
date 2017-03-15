using System;
using Eto.Drawing;
using Eto.Forms;
// ReSharper disable VirtualMemberCallInContructor

namespace LiteDB.Explorer.Core.Dialogs
{
    /// <summary>
    /// A dialog box for creating or renaming a collection.
    /// </summary>
    public class AddCollectionDialog : Dialog<string>
    {
        private readonly TextBox nameTextBox;
        private void onCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close(null);
        }
        private void onOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Ok;
            Close(nameTextBox.Text);
        }

        /// <summary>
        /// Creating a dialog box for creating or renaming a collection.
        /// </summary>
        public AddCollectionDialog(string name = null)
        {
            Icon = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Open.ico", typeof(AddCollectionDialog).Assembly);
            Resizable = false;
            Result = null;
            Size = new Size(400, 100);
            Title = string.IsNullOrEmpty(name) ? "Add Collection" : "Rename Collection";
            
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
                            new TableRow(
                                "Name:",
                                new TableCell(nameTextBox = new TextBox
                                {
                                    Text = name ?? ""
                                }, true)
                            )
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
                                new TableCell("", true),
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
                        ScaleHeight = true
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
