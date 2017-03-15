using System.Linq;
using System.Threading.Tasks;
using LiteDB.Explorer.Core.Forms;
using LiteDB.Explorer.Core.Helpers;

namespace LiteDB.Explorer.Core.Models
{
    /// <summary>
    /// A model for a collection in a database file.
    /// </summary>
    public class CollectionModel : BaseModel
    {
        /// <summary>
        /// Creates a model for a collection in a database file.
        /// </summary>
        public CollectionModel(MainFormModel viewModel, DatabaseModel db, string name)
        {
            Database = db;
            Name = name;
            Parent = viewModel;
            
            Collection = db.Database.GetCollection(name);
        }

        /// <summary>
        /// The database this collection belongs to.
        /// </summary>
        public DatabaseModel Database { get; }

        /// <summary>
        /// The collection reference.
        /// </summary>
        public LiteCollection<BsonDocument> Collection { get; private set; } 

        /// <summary>
        /// The parent view model.
        /// </summary>
        public MainFormModel Parent { get; }

        /// <summary>
        /// The name of the collection.
        /// </summary>
        public string Name
        {
            get
            {
                return Get<string>();
            }
            private set
            {
                Set(value);
            }
        }

        /// <summary>
        /// Adds a document to the collection.
        /// </summary>
        public async Task AddDocument(BsonDocument doc)
        {
            Collection.Insert(doc);

            foreach (var query in Parent.Queries
                .Where(query => query.Collection == this))
            {
                await query.RunQuery().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Removes a document to the collection.
        /// </summary>
        public async Task RemoveDocument(BsonDocument doc)
        {
            Collection.Delete(doc["_id"]);

            foreach (var query in Parent.Queries
                .Where(query => query.Collection == this))
            {
                await query.RunQuery().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Saves a document to the collection.
        /// </summary>
        public async Task SaveDocument(BsonDocument doc)
        {
            Collection.Upsert(doc);

            foreach (var query in Parent.Queries
                .Where(query => query.Collection == this))
            {
                await query.RunQuery().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Renames the collection.
        /// </summary>
        public void Rename(string name)
        {
            Database.Database.RenameCollection(Name, name);
            
            Collection = Database.Database.GetCollection(name);
            Name = name;
        }
    }
}
