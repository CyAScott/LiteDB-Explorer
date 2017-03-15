using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Eto.Forms;
using LiteDB.Explorer.Core.Forms;
using LiteDB.Explorer.Core.Models;

namespace LiteDB.Explorer.Core.Controls
{
    /// <summary>
    /// A tab control for queries, index updates, etc.
    /// </summary>
    public class MultiTabControl : TabControl
    {
        private ObservableCollection<DatabaseModel> openedDatabases;
        private readonly MainFormModel parentViewModel;
        private void autoCloseTabs()
        {
            try
            {
                SuspendLayout();

                if (openedDatabases == null)
                {
                    Pages.Clear();
                    return;
                }
                
                foreach (var page in Pages
                    .OfType<IAmAPage>()
                    .Where(page => 
                        !openedDatabases.Contains(page.Collection.Database) ||
                        !page.Collection.Database.Collections.Contains(page.Collection))
                    .Cast<TabPage>()
                    .ToArray())
                {
                    Pages.Remove(page);
                }
            }
            catch (Exception error)
            {
                parentViewModel.Log.Error(error);
                MessageBox.Show(error.Message, "Auto Close Tabs Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
            finally
            {
                ResumeLayout();
            }
        }
        private void onCollectionRemoved(object sender, CollectionModel collectionModel)
        {
            autoCloseTabs();
        }
        private void onOpenedDatabasesChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            autoCloseTabs();
        }
        private void viewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
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

            autoCloseTabs();
        }
        
        /// <summary>
        /// Creates a tab control for queries, index updates, etc.
        /// </summary>
        public MultiTabControl(MainFormModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            parentViewModel = viewModel;

            viewModel.CollectionRemoved += onCollectionRemoved;
            viewModel.PropertyChanged += viewModelOnPropertyChanged;
            if (viewModel.OpenedDatabases != null)
            {
                openedDatabases = viewModel.OpenedDatabases;
                openedDatabases.CollectionChanged += onOpenedDatabasesChanged;
            }
        }

        /// <summary>
        /// Opens a new tab for quering the selected collection.
        /// </summary>
        public void QuerySelectedCollection()
        {
            var query = new QueryModel
            {
                Collection = parentViewModel?.SelectedCollection
            };

            var page = new QueryPageControl(this, parentViewModel, query)
            {
                Collection = query.Collection
            };

            page.AttachNative();

            Pages.Add(page);

            SelectedPage = page;
            
            parentViewModel?.Queries.Add(query);
        }
    }

    /// <summary>
    /// An interface for a page for the multitab control.
    /// </summary>
    public interface IAmAPage
    {
        /// <summary>
        /// The collection for the page. 
        /// </summary>
        CollectionModel Collection { get; set; }
    }
}
