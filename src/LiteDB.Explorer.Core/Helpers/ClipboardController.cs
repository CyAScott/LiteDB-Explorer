namespace LiteDB.Explorer.Core.Helpers
{
    /// <summary>
    /// An interface for a control that implements clipboard commands.
    /// </summary>
    public interface IClipboardController
    {
        /// <summary>
        /// Executes the copy command.
        /// </summary>
        void Copy();

        /// <summary>
        /// Executes the cut command.
        /// </summary>
        void Cut();

        /// <summary>
        /// Executes the paste command.
        /// </summary>
        void Paste();
    }
}
