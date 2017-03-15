using System;
using Eto.Drawing;
using Eto.Forms;
using LiteDB.Explorer.Core.Controls;
using LiteDB.Explorer.Core.Dialogs;
using LiteDB.Explorer.Core.Models;
// ReSharper disable VirtualMemberCallInContructor

namespace LiteDB.Explorer.Core.Forms
{
    /// <summary>
    /// The first form that loads when the program starts.
    /// </summary>
    public sealed class MainForm : Form
    {
        private class AddDocumentCommand : BaseCommand<MainForm, MainFormModel>
        {
            private void onCollectionSelectionChanged(object sender, CollectionModel collection)
            {
                Enabled = collection != null;
            }

            protected override async void OnExecuted(EventArgs e)
            {
                base.OnExecuted(e);

                try
                {
                    var collection = ViewModel?.SelectedCollection;
                    if (collection == null)
                    {
                        return;
                    }

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
                        await collection.AddDocument(newDoc).ConfigureAwait(false);
                    }
                }
                catch (Exception error)
                {
                    ViewModel?.Log.Error(error);
                    MessageBox.Show(error.Message, "Add Document Error", MessageBoxButtons.OK, MessageBoxType.Error);
                }
            }

            public AddDocumentCommand(MainForm parent, MainFormModel viewModel)
                : base(parent, viewModel)
            {
                Enabled = false;
                Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.New.ico", typeof(MainForm).Assembly);
                MenuText = "Add Document";
                ToolTip = "Add Document";
                viewModel.CollectionSelectionChanged += onCollectionSelectionChanged;
            }
        }
        private class AddCollectionCommand : BaseCommand<MainForm, MainFormModel>
        {
            private void onDatabaseSelectionChanged(object sender, DatabaseModel databaseModel)
            {
                Enabled = databaseModel != null;
            }

            protected override void OnExecuted(EventArgs e)
            {
                base.OnExecuted(e);

                try
                {
                    using (var addCollection = new AddCollectionDialog())
                    {
                        var name = addCollection.ShowModal();
                        if (addCollection.DialogResult == DialogResult.Ok)
                        {
                            ViewModel.SelectedDatabase?.AddCollection(name);
                        }
                    }
                }
                catch (Exception error)
                {
                    ViewModel.Log.Error(error);
                    MessageBox.Show(error.Message, "Add Collection Error", MessageBoxButtons.OK, MessageBoxType.Error);
                }
            }

            public AddCollectionCommand(MainForm parent, MainFormModel viewModel)
                : base(parent, viewModel)
            {
                Enabled = false;
                Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Open.ico", typeof(MainForm).Assembly);
                MenuText = "Add Collection";
                ToolTip = "Add Collection";
                viewModel.DatabaseSelectionChanged += onDatabaseSelectionChanged;
            }
        }
        private class CopyCommand : BaseCommand<MainForm, MainFormModel>
        {
            protected override void OnExecuted(EventArgs e)
            {
                base.OnExecuted(e);

                try
                {
                    ViewModel.ActiveClipboardController?.Copy();
                }
                catch (Exception error)
                {
                    ViewModel.Log.Error(error);
                    MessageBox.Show(error.Message, "Copy Error", MessageBoxButtons.OK, MessageBoxType.Error);
                }
            }

            public CopyCommand(MainForm parent, MainFormModel viewModel)
                : base(parent, viewModel)
            {
                this.Bind(control => control.Enabled, viewModel, model => model.ClipboardEnabled, DualBindingMode.OneWay);
                Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Copy.ico", typeof(MainForm).Assembly);
                MenuText = "&Copy";
                Shortcut = Application.Instance.CommonModifier | Keys.C;
                ToolTip = "Copy";
            }
        }
        private class CutCommand : BaseCommand<MainForm, MainFormModel>
        {
            protected override void OnExecuted(EventArgs e)
            {
                base.OnExecuted(e);

                try
                {
                    ViewModel.ActiveClipboardController?.Cut();
                }
                catch (Exception error)
                {
                    ViewModel.Log.Error(error);
                    MessageBox.Show(error.Message, "Cut Error", MessageBoxButtons.OK, MessageBoxType.Error);
                }
            }

            public CutCommand(MainForm parent, MainFormModel viewModel)
                : base(parent, viewModel)
            {
                this.Bind(control => control.Enabled, viewModel, model => model.ClipboardEnabled, DualBindingMode.OneWay);
                Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Cut.ico", typeof(MainForm).Assembly);
                MenuText = "Cut";
                Shortcut = Application.Instance.CommonModifier | Keys.X;
                ToolTip = "Cut";
            }
        }
        private class NewCommand : BaseCommand<MainForm, MainFormModel>
        {
            protected override void OnExecuted(EventArgs e)
            {
                base.OnExecuted(e);

                LiteDatabase db = null;
                try
                {
                    using (var saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filters.Add(new FileDialogFilter("Lite Db", ".db"));
                        saveFileDialog.Filters.Add(new FileDialogFilter("Any", ".*"));

                        if (saveFileDialog.ShowDialog(ParentForm) == DialogResult.Ok)
                        {
                            ViewModel.Open(db = new LiteDatabase(saveFileDialog.FileName), saveFileDialog.FileName);
                        }
                    }
                }
                catch (Exception error)
                {
                    db?.Dispose();
                    ViewModel.Log.Error(error);
                    MessageBox.Show(error.Message, "New Error", MessageBoxButtons.OK, MessageBoxType.Error);
                }
            }

            public NewCommand(MainForm parent, MainFormModel viewModel)
                : base(parent, viewModel)
            {
                Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.New.ico", typeof(MainForm).Assembly);
                MenuText = "&New";
                Shortcut = Application.Instance.CommonModifier | Keys.N;
                ToolTip = "New";
            }
        }
        private class OpenCommand : BaseCommand<MainForm, MainFormModel>
        {
            protected override void OnExecuted(EventArgs e)
            {
                base.OnExecuted(e);

                LiteDatabase db = null;
                try
                {
                    using (var openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filters.Add(new FileDialogFilter("Lite Db", ".db"));
                        openFileDialog.Filters.Add(new FileDialogFilter("Any", ".*"));

                        if (openFileDialog.ShowDialog(ParentForm) == DialogResult.Ok)
                        {
                            ViewModel.Open(db = new LiteDatabase(openFileDialog.FileName), openFileDialog.FileName);
                        }
                    }
                }
                catch (Exception error)
                {
                    db?.Dispose();
                    ViewModel.Log.Error(error);
                    MessageBox.Show(error.Message, "Open Error", MessageBoxButtons.OK, MessageBoxType.Error);
                }
            }

            public OpenCommand(MainForm parent, MainFormModel viewModel)
                : base(parent, viewModel)
            {
                Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Open.ico", typeof(MainForm).Assembly);
                MenuText = "&Open";
                Shortcut = Application.Instance.CommonModifier | Keys.O;
                ToolTip = "Open";
            }
        }
        private class QueryCommand : BaseCommand<MainForm, MainFormModel>
        {
            private void onCollectionSelectionChanged(object sender, CollectionModel collection)
            {
                Enabled = collection != null;
            }

            protected override void OnExecuted(EventArgs e)
            {
                base.OnExecuted(e);

                try
                {
                    ParentForm.MainSplitter.MultiTabControl.QuerySelectedCollection();
                }
                catch (Exception error)
                {
                    ViewModel.Log.Error(error);
                    MessageBox.Show(error.Message, "Query Error", MessageBoxButtons.OK, MessageBoxType.Error);
                }
            }

            public QueryCommand(MainForm parent, MainFormModel viewModel)
                : base(parent, viewModel)
            {
                Enabled = false;
                Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Search.ico", typeof(MainForm).Assembly);
                MenuText = "&Query";
                Shortcut = Application.Instance.CommonModifier | Keys.Q;
                ToolTip = "Query";
                viewModel.CollectionSelectionChanged += onCollectionSelectionChanged;
            }
        }
        private class PasteCommand : BaseCommand<MainForm, MainFormModel>
        {
            protected override void OnExecuted(EventArgs e)
            {
                base.OnExecuted(e);

                try
                {
                    ViewModel.ActiveClipboardController?.Paste();
                }
                catch (Exception error)
                {
                    ViewModel.Log.Error(error);
                    MessageBox.Show(error.Message, "Paste Error", MessageBoxButtons.OK, MessageBoxType.Error);
                }
            }

            public PasteCommand(MainForm parent, MainFormModel viewModel)
                : base(parent, viewModel)
            {
                this.Bind(control => control.Enabled, viewModel, model => model.ClipboardEnabled, DualBindingMode.OneWay);
                Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Paste.ico", typeof(MainForm).Assembly);
                MenuText = "Paste";
                Shortcut = Application.Instance.CommonModifier | Keys.V;
                ToolTip = "Paste";
            }
        }

        /// <summary>
        /// Creates the first form that loads when the program starts.
        /// </summary>
        public MainForm(MainFormModel viewModel)
        {
            DataContext = viewModel;

            Icon = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Db.ico", typeof(MainForm).Assembly);

            this.Bind(form => form.ClientSize, viewModel, model => model.ClientSize, DualBindingMode.OneWay);
            this.Bind(form => form.Title, viewModel, model => model.Title, DualBindingMode.OneWay);
            
            Content = MainSplitter = new MainSplitter(viewModel);
            Menu = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem
                    {
                        Text = "&File",
                        Items =
                        {
                            new NewCommand(this, viewModel),
                            new OpenCommand(this, viewModel)
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "&Edit",
                        Items =
                        {
                            new AddCollectionCommand(this, viewModel),
                            new AddDocumentCommand(this, viewModel),
                            new QueryCommand(this, viewModel),
                            new CutCommand(this, viewModel),
                            new CopyCommand(this, viewModel),
                            new PasteCommand(this, viewModel)
                        }
                    }
                }
            };
            ToolBar = new ToolBar
            {
                Dock = ToolBarDock.Top,
                Items =
                {
                    new NewCommand(this, viewModel),
                    new OpenCommand(this, viewModel),
                    new SeparatorToolItem(),
                    new AddCollectionCommand(this, viewModel),
                    new AddDocumentCommand(this, viewModel),
                    new QueryCommand(this, viewModel),
                    new SeparatorToolItem(),
                    new CutCommand(this, viewModel),
                    new CopyCommand(this, viewModel),
                    new PasteCommand(this, viewModel)
                }
            };
        }

        /// <summary>
        /// The split panel the houses the multitab control and opened database tree.
        /// </summary>
        public MainSplitter MainSplitter { get; }
    }
}
