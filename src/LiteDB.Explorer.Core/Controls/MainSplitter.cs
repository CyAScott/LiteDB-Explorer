using Eto.Forms;
using LiteDB.Explorer.Core.Forms;

namespace LiteDB.Explorer.Core.Controls
{
    /// <summary>
    /// The main table layout for the main form.
    /// </summary>
    public class MainSplitter : Splitter
    {
        /// <summary>
        /// Creates the main table layout for the main form.
        /// </summary>
        public MainSplitter(MainFormModel viewModel)
        {
            DataContext = viewModel;
            Panel1 = OpenedDatabaseTree = new OpenedDatabaseTree(viewModel);
            Panel2 = MultiTabControl = new MultiTabControl(viewModel);
            Position = 300;
        }

        /// <summary>
        /// The tab control for queries, index updates, etc.
        /// </summary>
        public MultiTabControl MultiTabControl { get; }

        /// <summary>
        /// The tre control for opened databases.
        /// </summary>
        public OpenedDatabaseTree OpenedDatabaseTree { get; }
    }
}
