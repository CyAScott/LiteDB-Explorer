using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using LiteDB.Explorer.Core.Controls.EtoControls;
using LiteDB.Explorer.Core.Extensions;

// ReSharper disable VirtualMemberCallInContructor

namespace LiteDB.Explorer.Core.Dialogs
{
    /// <summary>
    /// Gets a BSON document from the user.
    /// </summary>
    public class GetDocumentDialog : Dialog<BsonDocument>
    {
        private readonly SyntaxHighlightingTextBox docTextBox;
        private void onCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close(null);
        }
        private void onOkClick(object sender, EventArgs e)
        {
            try
            {
                var doc = docTextBox.Text.ParseBsonDocument();
                
                if (!doc.ContainsKey("_id"))
                {
                    throw new ArgumentException("The provided document does not have an \"_id\" field.");
                }

                DialogResult = DialogResult.Ok;
                Close(doc);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Document Parse Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
        }

        private GetDocumentDialog(BsonDocument originalDocument, string title)
        {
            if (originalDocument == null)
            {
                throw new ArgumentNullException(nameof(originalDocument));
            }

            Icon = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.New.ico", typeof(AddCollectionDialog).Assembly);
            Resizable = true;
            Result = null;
            MinimumSize = Size = new Size(600, 400);
            Title = title;
            
            Content = new TableLayout
            {
                Padding = new Padding(5),
                Rows =
                {
                    new TableRow(new TableCell(docTextBox = new SyntaxHighlightingTextBox
                    {
                        ReadOnly = false,
                        ShowLines = true,
                        Text = originalDocument.ToText()
                    }, true))
                    {
                        ScaleHeight = true
                    },
                    new TableRow(new TableLayout
                    {
                        Padding = new Padding(0, 5, 0, 0),
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
                }
            };
        }

        /// <summary>
        /// Creates a dialog box that let's the user modify a BSON document.
        /// </summary>
        public GetDocumentDialog(BsonDocument originalDocument)
            : this(originalDocument, "Edit Document")
        {
        }
        /// <summary>
        /// Creates a dialog box that let's the user create a BSON document.
        /// </summary>
        public GetDocumentDialog()
            : this(new BsonDocument(new Dictionary<string, BsonValue>
            {
                { "_id", ObjectId.NewObjectId() }
            }), "Add Document")
        {
        }

        /// <summary>
        /// Gets the dialog results for the dialog box.
        /// </summary>
        public DialogResult DialogResult { get; private set; } = DialogResult.Cancel;
    }
}
