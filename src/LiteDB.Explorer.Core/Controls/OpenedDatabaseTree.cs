using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Eto.Forms;
using LiteDB.Explorer.Core.Forms;
using LiteDB.Explorer.Core.Helpers;
using LiteDB.Explorer.Core.Models;
// ReSharper disable UnusedParameter.Local

namespace LiteDB.Explorer.Core.Controls
{
    /// <summary>
    /// A tree control for a summary view of each opened database.
    /// </summary>
    public class OpenedDatabaseTree : TreeView, IClipboardController
    {
        private ObservableCollection<DatabaseModel> openedDatabases;
        private readonly MainFormModel parentViewModel;
        private readonly TreeItemCollection tree;
        private bool ignoreSelectionChanged;
        private void onMouseDoubleClick(object sender, MouseEventArgs mouseEventArgs)
        {
            try
            {
                var collection = SelectedItem as CollectionTreeItem;

                if (collection == null)
                {
                    return;
                }

                var parent = Parent.Parent as MainForm;

                parent?.MainSplitter.MultiTabControl.QuerySelectedCollection();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }
        private void onOpenedDatabasesChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            resyncDatabases();
        }
        private void onSelectionChanged(object sender, EventArgs eventArgs)
        {
            onSelectionChanged(true);
        }
        private void onSelectionChanged(bool fromUser)
        {
            if (ignoreSelectionChanged)
            {
                return;
            }

            try
            {
                ignoreSelectionChanged = true;

                if (fromUser)
                {
                    var selectedCollection = SelectedItem as CollectionTreeItem;
                    if (selectedCollection != null)
                    {
                        parentViewModel.SelectedCollection = selectedCollection.Collection;
                    }
                    else
                    {
                        parentViewModel.SelectedCollection = null;

                        var selectedDb = SelectedItem as OpenedDatabaseTreeItem;
                        parentViewModel.SelectedDatabase = selectedDb?.Database;
                    }
                }
                else
                {
                    var selectedItem = parentViewModel.SelectedDatabase;

                    SelectedItem = tree
                        .OfType<OpenedDatabaseTreeItem>()
                        .FirstOrDefault(item => string.Equals(item.Database.FilePath, selectedItem?.FilePath));
                }
            }
            catch (Exception error)
            {
                parentViewModel.Log.Error(error);
                MessageBox.Show(error.Message, "Selection Changed Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
            finally
            {
                ignoreSelectionChanged = false;
            }
        }
        private void resyncDatabases()
        {
            try
            {
                ignoreSelectionChanged = true;
                //SuspendLayout();
                
                if (openedDatabases == null)
                {
                    return;
                }
                
                //remove closed databases
                foreach (var child in tree.OfType<OpenedDatabaseTreeItem>()
                    .Where(child => !openedDatabases.Contains(child.Database))
                    .ToArray())
                {
                    tree.Remove(child);
                }

                //add newly opened databases
                var children = tree.OfType<OpenedDatabaseTreeItem>().ToArray();
                foreach (var item in openedDatabases
                    .Where(db => children.All(child => child.Database != db))
                    .Select(db => new OpenedDatabaseTreeItem(parentViewModel, this, db)
                    {
                        Expanded = true
                    }))
                {
                    tree.Add(item);
                    RefreshItem(item);
                }
                
                var selectedItem = parentViewModel.SelectedDatabase;

                SelectedItem = tree
                    .OfType<OpenedDatabaseTreeItem>()
                    .FirstOrDefault(item => string.Equals(item.Database.FilePath, selectedItem?.FilePath));
            }
            catch (Exception error)
            {
                parentViewModel.Log.Error(error);
                MessageBox.Show(error.Message, "Opened Database Resync Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
            finally
            {
                ignoreSelectionChanged = false;
                //ResumeLayout();
            }
        }
        private void viewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.Equals(propertyChangedEventArgs.PropertyName, nameof(parentViewModel.SelectedDatabase)))
            {
                onSelectionChanged(false);
                return;
            }

            if (!string.Equals(propertyChangedEventArgs.PropertyName, nameof(parentViewModel.OpenedDatabases)))
            {
                return;
            }

            if (openedDatabases != null)
            {
                openedDatabases.CollectionChanged -= onOpenedDatabasesChanged;
            }

            openedDatabases = parentViewModel.OpenedDatabases;

            if (openedDatabases != null)
            {
                openedDatabases.CollectionChanged += onOpenedDatabasesChanged;
            }

            resyncDatabases();
        }

        /// <summary>
        /// Creates a tree control for a summary view of each opened database.
        /// </summary>
        public OpenedDatabaseTree(MainFormModel viewModel)
        {
            DataContext = parentViewModel = viewModel;
            DataStore = tree = new TreeItemCollection();
            SelectionChanged += onSelectionChanged;
            
            parentViewModel.PropertyChanged += viewModelOnPropertyChanged;
            if (parentViewModel.OpenedDatabases != null)
            {
                openedDatabases = parentViewModel.OpenedDatabases;
                openedDatabases.CollectionChanged += onOpenedDatabasesChanged;
            }

            MouseDoubleClick += onMouseDoubleClick;
            viewModel.ActiveClipboardController = this;
            GotFocus += (sender, args) => viewModel.ActiveClipboardController = this;
        }
        
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
