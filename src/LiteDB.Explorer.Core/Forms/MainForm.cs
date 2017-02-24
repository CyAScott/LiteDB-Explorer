using Eto.Forms;

namespace LiteDB.Explorer.Core.Forms
{
    /// <summary>
    /// The first form that loads when the program starts.
    /// </summary>
    public class MainForm : Form
    {
        /// <summary>
        /// Creates the first form that loads when the program starts.
        /// </summary>
        public MainForm(MainFormModel viewModel)
        {
            DataContext = viewModel;

            Icon = Eto.Drawing.Icon.FromResource("LiteDB.Explorer.Core.Db.ico", typeof(MainForm).Assembly);

            this.Bind(form => form.ClientSize, viewModel, model => model.ClientSize, DualBindingMode.OneWay);
            this.Bind(form => form.Title, viewModel, model => model.Title, DualBindingMode.OneWay);
        }
    }
}
