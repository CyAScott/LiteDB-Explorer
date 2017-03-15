using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using LiteDB.Explorer.Core.Forms;
using LiteDB.Explorer.Core.Models;

namespace LiteDB.Explorer.Core.Controls
{
    /// <summary>
    /// An opened database tree item control.
    /// </summary>
    public class OpenedDatabaseTreeItem : TreeItem, IDisposable
    {
        private readonly MainFormModel parentViewModel;
        private readonly OpenedDatabaseTree openedDatabaseTree;

        private void onCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            try
            {
                var databaseCollections = Database.Collections;
                var changesMade = false;

                //remove deleted collections
                foreach (var child in Children
                    .OfType<CollectionTreeItem>()
                    .Where(child => !databaseCollections.Contains(child.Collection))
                    .ToArray())
                {
                    changesMade = true;
                    Children.Remove(child);
                    child.Dispose();
                }

                //add newly created collections
                var children = Children.OfType<CollectionTreeItem>().ToArray();
                foreach (var item in databaseCollections
                    .Where(collection => children.All(child => child.Collection != collection))
                    .Select(collection => new CollectionTreeItem(Database, collection)))
                {
                    changesMade = true;
                    Children.Add(item);
                }

                if (changesMade)
                {
                    Expanded = true;
                    openedDatabaseTree.RefreshItem(this);
                }
            }
            catch (Exception error)
            {
                parentViewModel.Log.Error(error);
                MessageBox.Show(error.Message, "Database Collection Resync Error", MessageBoxButtons.OK, MessageBoxType.Error);
            }
        }

        /// <summary>
        /// Creates an opened database tree item control.
        /// </summary>
        public OpenedDatabaseTreeItem(MainFormModel viewModel, OpenedDatabaseTree parentTree, DatabaseModel db)
        {
            Database = db;
            Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Db.ico", typeof(OpenedDatabaseTreeItem).Assembly);
            openedDatabaseTree = parentTree;
            parentViewModel = viewModel;
            Text = Path.GetFileNameWithoutExtension(db.FilePath);
            
            db.Collections.CollectionChanged += onCollectionChanged;
            
            Children.AddRange(db.Collections.Select(collection => new CollectionTreeItem(db, collection)));
        }


        /// <summary>
        /// The database for this item.
        /// </summary>
        public readonly DatabaseModel Database;

        /// <summary>
        /// Disposes this control.
        /// </summary>
        public void Dispose()
        {
            Database.Collections.CollectionChanged -= onCollectionChanged;

            foreach (var child in Children.OfType<IDisposable>())
            {
                child.Dispose();
            }
            Children.Clear();

            Image?.Dispose();
        }
    }
}
