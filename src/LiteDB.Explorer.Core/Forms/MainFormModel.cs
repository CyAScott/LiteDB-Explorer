using Eto.Drawing;
using LiteDB.Explorer.Core.Helpers;
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
            Title = "Lite DB - Explorer";
        }

        /// <summary>
        /// The logger for the program.
        /// </summary>
        public ILogger Log { get; }

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
