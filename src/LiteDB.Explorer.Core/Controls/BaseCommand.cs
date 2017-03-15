using Eto.Forms;
using LiteDB.Explorer.Core.Helpers;

namespace LiteDB.Explorer.Core.Controls
{
    /// <summary>
    /// The base class for all command controls.
    /// </summary>
    /// <typeparam name="TForm">The parent form.</typeparam>
    /// <typeparam name="TViewModel">The view model for the parent form.</typeparam>
    public abstract class BaseCommand<TForm, TViewModel> : Command
        where TForm : Form
        where TViewModel : BaseModel
    {
        /// <summary>
        /// The parent form.
        /// </summary>
        protected readonly TForm ParentForm;

        /// <summary>
        /// The view model for the parent form.
        /// </summary>
        protected readonly TViewModel ViewModel;

        /// <summary>
        /// Creates the base class for all command controls.
        /// </summary>
        /// <param name="parent">The parent form.</param>
        /// <param name="viewModel">The view model for the parent form.</param>
        protected BaseCommand(TForm parent, TViewModel viewModel)
        {
            ParentForm = parent;
            ViewModel = viewModel;
        }
    }
}
