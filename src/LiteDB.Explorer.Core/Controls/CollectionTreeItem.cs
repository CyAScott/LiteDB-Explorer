using System;
using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using LiteDB.Explorer.Core.Models;

namespace LiteDB.Explorer.Core.Controls
{
    /// <summary>
    /// An opened database collection tree item control.
    /// </summary>
    public class CollectionTreeItem : TreeItem, IDisposable
    {
        private void collectionOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Collection.Name))
            {
                Text = Collection.Name;
            }
        }

        /// <summary>
        /// Creates an opened database collection tree item control.
        /// </summary>
        public CollectionTreeItem(DatabaseModel db, CollectionModel collection)
        {
            Collection = collection;
            Database = db;
            Image = Icon.FromResource("LiteDB.Explorer.Core.Assets.Icons.Open.ico", typeof(CollectionTreeItem).Assembly);
            
            Text = collection.Name;
            collection.PropertyChanged += collectionOnPropertyChanged;
        }


        /// <summary>
        /// The collection for this item.
        /// </summary>
        public readonly CollectionModel Collection;

        /// <summary>
        /// The database for this item.
        /// </summary>
        public readonly DatabaseModel Database;

        public void Dispose()
        {
            Collection.PropertyChanged -= collectionOnPropertyChanged;
        } 
    }
}
