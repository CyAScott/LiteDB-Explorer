using System;
using System.Collections.ObjectModel;
using Eto.Drawing;
using LiteDB.Explorer.Core.Helpers;
using LiteDB.Explorer.Core.Models;
using NLog;

namespace LiteDB.Explorer.Core.Forms
{
    /// <summary>
    /// The view model for the first form that loads when the program starts.
    /// </summary>
    public class MainFormModel : BaseModel
    {
        /// <summary>
        /// Creates the view model for the first form that loads when the program starts.
        /// </summary>
        public MainFormModel(ILogger log)
        {
            ClientSize = new Size(1280, 720);
            Log = log;
            OpenedDatabases = new ObservableCollection<DatabaseModel>();
            Queries = new ObservableCollection<QueryModel>();
            Title = "Lite DB - Explorer";
        }
        
        /// <summary>
        /// The currently selected clipboard controller.
        /// </summary>
        public IClipboardController ActiveClipboardController
        {
            get
            {
                return Get<IClipboardController>();
            }
            set
            {
                Set(value);
                ClipboardEnabled = value != null;
            }
        }

        /// <summary>
        /// If the clip controller is enabled.
        /// </summary>
        public bool ClipboardEnabled
        {
            get
            {
                return Get<bool>();
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// The selected collection in the tree.
        /// </summary>
        public CollectionModel SelectedCollection
        {
            get
            {
                return Get<CollectionModel>();
            }
            set
            {
                Set(value);

                SelectedDatabase = value?.Database;

                try
                {
                    CollectionSelectionChanged?.Invoke(this, value);
                }
                catch (Exception error)
                {
                    Log.Error(error);
                }
            }
        }

        /// <summary>
        /// Triggered when the collection selection changes.
        /// </summary>
        public event EventHandler<CollectionModel> CollectionSelectionChanged;

        /// <summary>
        /// The selected database in the tree.
        /// </summary>
        public DatabaseModel SelectedDatabase
        {
            get
            {
                return Get<DatabaseModel>();
            }
            set
            {
                Set(value);

                try
                {
                    DatabaseSelectionChanged?.Invoke(this, value);
                }
                catch (Exception error)
                {
                    Log.Error(error);
                }
            }
        }

        /// <summary>
        /// Triggered when the database selection changes.
        /// </summary>
        public event EventHandler<DatabaseModel> DatabaseSelectionChanged;

        /// <summary>
        /// The logger for the program.
        /// </summary>
        public ILogger Log { get; }

        /// <summary>
        /// A collection of all the opened databases.
        /// </summary>
        public ObservableCollection<DatabaseModel> OpenedDatabases
        {
            get
            {
                return Get<ObservableCollection<DatabaseModel>>();
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// All the queries for the user.
        /// </summary>
        public ObservableCollection<QueryModel> Queries
        {
            get
            {
                return Get<ObservableCollection<QueryModel>>();
            }
            set
            {
                Set(value);
            }
        } 

        /// <summary>
        /// The window title.
        /// </summary>
        public Size ClientSize
        {
            get
            {
                return Get<Size>();
            }
            set
            {
                Set(value);
            }
        }
        
        /// <summary>
        /// Triggered when a collection was removed from a database.
        /// </summary>
        public event EventHandler<CollectionModel> CollectionRemoved;

        /// <summary>
        /// Removes a collection from the database.
        /// </summary>
        public void RemoveCollection(CollectionModel collection)
        {
            collection.Database.Database.DropCollection(collection.Name);
            collection.Database.Collections.Remove(collection);

            if (SelectedCollection == collection)
            {
                SelectedCollection = null;
            }

            CollectionRemoved?.Invoke(this, collection);
        }

        /// <summary>
        /// Closes the currently selected database.
        /// </summary>
        public void CloseDatabase()
        {
            if (SelectedDatabase == null)
            {
                throw new ArgumentNullException(nameof(SelectedDatabase));
            }

            using (SelectedDatabase)
            {
                OpenedDatabases.Remove(SelectedDatabase);
            }

            SelectedDatabase = null;
        }
        
        /// <summary>
        /// Opens an existing file.
        /// </summary>
        public void Open(LiteDatabase db, string location)
        {
            var newItem = new DatabaseModel(this, db, location);
            OpenedDatabases.Add(newItem);
            SelectedDatabase = newItem;
        }
        
        /// <summary>
        /// The window title.
        /// </summary>
        public string Title
        {
            get
            {
                return Get<string>();
            }
            set
            {
                Set(value);
            }
        }
    }
}
