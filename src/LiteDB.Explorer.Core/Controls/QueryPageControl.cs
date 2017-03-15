using System;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using LiteDB.Explorer.Core.Controls.EtoControls;
using LiteDB.Explorer.Core.Dialogs;
using LiteDB.Explorer.Core.Forms;
using LiteDB.Explorer.Core.Helpers;
using LiteDB.Explorer.Core.Models;

namespace LiteDB.Explorer.Core.Controls
{
    /// <summary>
    /// A control for quering a collection.
    /// </summary>
    public class QueryPageControl : TabPage, IAmAPage, IClipboardController
    {
        private SyntaxHighlightingTextBox resultsTextBox;
        private async Task runQuery(bool asCountQuery = false)
        {
            try
            {
                await Query.RunQuery(asCountQuery).ConfigureAwait(false);

                if (asCountQuery)
                {
                    MessageBox.Show($"{Query.QueryCount} documents found.", "Count Results", MessageBoxButtons.OK);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Query Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
        }
        private async void addDoc(object sender, EventArgs eventArgs)
        {
            try
            {
                var newDoc = Application.Instance.Invoke(() =>
                {
                    using (var getDocumentDialog = new GetDocumentDialog())
                    {
                        var doc = getDocumentDialog.ShowModal();
                        if (getDocumentDialog.DialogResult == DialogResult.Ok)
                        {
                            return doc;
                        }
                    }
                    return null;
                });
                if (newDoc != null)
                {
                    await Collection.AddDocument(newDoc).ConfigureAwait(false);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Add Document Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
        }
        private async void editDoc(object sender, EventArgs eventArgs)
        {
            try
            {
                var doc = Query.DocumentAtPosition(Application.Instance.Invoke(() => resultsTextBox.CaretOffset));

                if (doc == null)
                {
                    throw new ArgumentException("No document found at the caret.");
                }

                var newDoc = Application.Instance.Invoke(() =>
                {
                    using (var getDocumentDialog = new GetDocumentDialog(doc))
                    {
                        var returnValue = getDocumentDialog.ShowModal();
                        if (getDocumentDialog.DialogResult == DialogResult.Ok)
                        {
                            return returnValue;
                        }
                    }
                    return null;
                });
                if (newDoc != null)
                {
                    await Collection.SaveDocument(newDoc).ConfigureAwait(false);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Edit Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
        }
        private async void removeDoc(object sender, EventArgs eventArgs)
        {
            try
            {
                var doc = Query.DocumentAtPosition(Application.Instance.Invoke(() => resultsTextBox.CaretOffset));

                if (doc == null)
                {
                    throw new ArgumentException("No document found at the caret.");
                }
                
                if (MessageBox.Show($"Are sure you want to remove document ({doc["_id"]})?", 
                    "Remove Document", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxType.Question) == DialogResult.Yes)
                {
                    await Collection.RemoveDocument(doc).ConfigureAwait(false);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Edit Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
        }

        /// <summary>
        /// Creates a control for quering a collection.
        /// </summary>
        public QueryPageControl(MultiTabControl parent, MainFormModel viewModel, QueryModel query)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (query.Collection == null)
            {
                throw new ArgumentNullException(nameof(query.Collection));
            }

            Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Search.ico", typeof(QueryPageControl).Assembly);
            Query = query;

            this.Bind(page => page.Text, query.Collection, model => model.Name, DualBindingMode.OneWay);

            NumericUpDown itemsPerPage, pageCount, pageIndex;
            SyntaxHighlightingTextBox queryTextBox;
            Content = new TableLayout
            {
                Padding = new Padding(5),
                Rows =
                {
                    #region Query Row
                    new TableRow(new TableLayout
                    {
                        Padding = new Padding(0, 0, 0, 5),
                        Spacing = new Size(5, 5),
                        Rows =
                        {
                            new TableRow(
                            new TableCell(new Label
                            {
                                Text = "Query:"
                            }),
                            new TableCell(queryTextBox = new SyntaxHighlightingTextBox
                            {
                                ReadOnly = false,
                                ShowLines = false,
                                Text = Query.QueryAsString = "{}"
                            }, true),
                            new TableCell(new Button(async (sender, e) => await runQuery(true).ConfigureAwait(false))
                            {
                                Text = "Count"
                            }),
                            new TableCell(new Button(async (sender, e) => await runQuery().ConfigureAwait(false))
                            {
                                Text = "Search"
                            }),
                            new TableCell(new Button((sender, e) => FindParent<MultiTabControl>()?.Remove(this))
                            {
                                Text = "Close"
                            }))
                        }
                    }),
                    #endregion

                    #region Query Paramters Row
                    new TableRow(new TableLayout
                    {
                        Padding = new Padding(0, 0, 0, 5),
                        Spacing = new Size(5, 5),
                        Rows =
                        {
                            new TableRow(
                            new TableCell(new Label
                            {
                                Text = "Items per Page:"
                            }),
                            new TableCell(itemsPerPage = new NumericUpDown
                            {
                                DecimalPlaces = 0,
                                Increment = 1,
                                MinValue = 1,
                                Value = Query.ItemsPerPage = 50
                            }),
                            new TableCell(new Label
                            {
                                Text = "Page:"
                            }),
                            new TableCell(pageIndex = new NumericUpDown
                            {
                                DecimalPlaces = 0,
                                Increment = 1,
                                MinValue = 1,
                                Value = Query.PageIndex = 1
                            }),
                            new TableCell(new Label
                            {
                                Text = "out of"
                            }),
                            new TableCell(pageCount = new NumericUpDown
                            {
                                DecimalPlaces = 0,
                                Increment = 1,
                                MinValue = 0,
                                ReadOnly = true,
                                Value = Query.PageCount = 0
                            }),
                            null,
                            new TableCell(new Button(addDoc)
                            {
                                Text = "Add"
                            }),
                            new TableCell(new Button(removeDoc)
                            {
                                Text = "Remove"
                            }),
                            new TableCell(new Button(editDoc)
                            {
                                Text = "Edit"
                            }))
                        }
                    }),
                    #endregion

                    #region Query Results Row
                    new TableRow(new TableCell(resultsTextBox = new SyntaxHighlightingTextBox
                    {
                        ReadOnly = true,
                        ShowLines = true,
                        Text = Query.ResultsAsString = ""
                    }, true))
                    {
                        ScaleHeight = true
                    }
                    #endregion
                }
            };
            
            itemsPerPage.Bind(control => control.Value, query, model => model.ItemsPerPage, DualBindingMode.OneWayToSource);
            pageCount.Bind(control => control.Value, query, model => model.PageCount, DualBindingMode.OneWay);
            pageIndex.Bind(control => control.Value, query, model => model.PageIndex, DualBindingMode.OneWayToSource);
            queryTextBox.Bind(control => control.Text, query, model => model.QueryAsString, DualBindingMode.OneWayToSource);
            resultsTextBox.Bind(control => control.Text, query, model => model.ResultsAsString, DualBindingMode.OneWay);

            queryTextBox.KeyUp += async (sender, e) =>
            {
                if (e.Control && e.Key == Keys.Enter)
                {
                    await runQuery().ConfigureAwait(false);
                    e.Handled = true;
                }
            };
            itemsPerPage.ValueChanged += async (sender, e) => await runQuery().ConfigureAwait(false);
            pageIndex.ValueChanged += async (sender, e) => await runQuery().ConfigureAwait(false);

            viewModel.ActiveClipboardController = this;
            GotFocus += (sender, args) =>
            {
                viewModel.ActiveClipboardController = this;
                viewModel.SelectedDatabase = Collection.Database;
                viewModel.SelectedCollection = Collection;
            };

#pragma warning disable 4014
            runQuery();
#pragma warning restore 4014
        }


        /// <summary>
        /// The query for this tab.
        /// </summary>
        public QueryModel Query { get; }

        /// <summary>
        /// The collection for the page. 
        /// </summary>
        public CollectionModel Collection { get; set; }
        
        public void Copy()
        {

        }
        public void Cut()
        {

        }
        public void Paste()
        {

        }
    }
}
