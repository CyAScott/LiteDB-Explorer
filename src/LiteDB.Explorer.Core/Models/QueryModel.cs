using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Explorer.Core.Extensions;
using LiteDB.Explorer.Core.Helpers;

namespace LiteDB.Explorer.Core.Models
{
    /// <summary>
    /// The model for a query.
    /// </summary>
    public class QueryModel : BaseModel
    {
        private List<Tuple<int, BsonValue>> indices = new List<Tuple<int, BsonValue>>();
        private int queryRunning;

        public BsonDocument DocumentAtPosition(int position)
        {
            var id = indices
                .Where(index => position <= index.Item1)
                .Select(index => index.Item2)
                .FirstOrDefault();
            
            return Results?.FirstOrDefault(doc => doc["_id"] == id);
        }

        /// <summary>
        /// The query results.
        /// </summary>
        public BsonDocument[] Results { get; set; }

        /// <summary>
        /// The collection for the query. 
        /// </summary>
        public CollectionModel Collection { get; set; }
        
        /// <summary>
        /// The current query.
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// The total number of items per page.
        /// </summary>
        public double ItemsPerPage
        {
            get
            {
                return Get<double>();
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public double PageCount
        {
            get
            {
                return Get<double>();
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// The page index (starting with 1).
        /// </summary>
        public double PageIndex
        {
            get
            {
                return Get<double>();
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// The total number of items for the query.
        /// </summary>
        public int QueryCount
        {
            get
            {
                return Get<int>();
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// The query results as a string.
        /// </summary>
        public string ResultsAsString
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

        /// <summary>
        /// The query in the form of a string.
        /// </summary>
        public string QueryAsString
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

        /// <summary>
        /// Runs the query.
        /// </summary>
        public async Task RunQuery(bool asCountQuery = false)
        {
            if (Interlocked.CompareExchange(ref queryRunning, 1, 0) == 1)
            {
                return;
            }

            try
            {
                var queryDoc = QueryAsString.ParseDocument();

                var query = queryDoc.ToQuery() ?? Query.All();

                QueryCount = await Task.Run(() => Collection.Collection.Count(query)).ConfigureAwait(false);
                if (asCountQuery)
                {
                    return;
                }

                PageCount = Math.Ceiling(QueryCount / ItemsPerPage);

                Results = await Task.Run(() => Collection.Collection.Find(query,
                    Convert.ToInt32(ItemsPerPage * (PageIndex - 1)),
                    Convert.ToInt32(ItemsPerPage)).ToArray()).ConfigureAwait(false);

                var sb = new StringBuilder();
                indices.Clear();
                foreach (var doc in Results)
                {
                    sb.AppendLine(doc.ToText());
                    indices.Add(new Tuple<int, BsonValue>(sb.Length, doc["_id"]));
                }

                ResultsAsString = sb.ToString();
            }
            finally
            {
                queryRunning = 0;
            }
        }
    }
}
