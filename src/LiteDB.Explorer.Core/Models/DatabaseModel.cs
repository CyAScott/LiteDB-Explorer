using System;
using System.Collections.ObjectModel;
using System.Linq;
using LiteDB.Explorer.Core.Forms;
using LiteDB.Explorer.Core.Helpers;

namespace LiteDB.Explorer.Core.Models
{
    /// <summary>
    /// A model for an opened database.
    /// </summary>
    public class DatabaseModel : BaseModel, IDisposable
    {
        /// <summary>
        /// Creates a model for an opened database.
        /// </summary>
        public DatabaseModel(MainFormModel viewModel, LiteDatabase db, string filePath)
        {
            Database = db;
            FilePath = filePath;
            Parent = viewModel;

            Collections = new ObservableCollection<CollectionModel>(Database
                .GetCollectionNames()
                .Select(name => new CollectionModel(viewModel, this, name)));
        }

        /// <summary>
        /// The collections in the database file.
        /// </summary>
        public ObservableCollection<CollectionModel> Collections { get; }

        /// <summary>
        /// The Db Lite database.
        /// </summary>
        public LiteDatabase Database { get; }

        /// <summary>
        /// The parent view model.
        /// </summary>
        public MainFormModel Parent { get; }

        /// <summary>
        /// The file path for the database.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Adds a collection to the database.
        /// </summary>
        public void AddCollection(string collectionName)
        {
            if (Collections.Any(collection => collection.Name == collectionName))
            {
                throw new ArgumentException("Duplicate collection name.");
            }
            Collections.Add(new CollectionModel(Parent, this, collectionName));
        }

        /// <summary>
        /// Disposes the database.
        /// </summary>
        public void Dispose()
        {
            Collections.Clear();
            Database.Dispose();
        }
    }
}
