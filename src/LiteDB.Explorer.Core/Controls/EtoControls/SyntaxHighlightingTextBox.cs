using System;
using Eto;
using Eto.Forms;

namespace LiteDB.Explorer.Core.Controls.EtoControls
{
    /// <summary>
    /// A handler for a text box control for syntax highlighting.
    /// </summary>
    public interface ISyntaxHighlightingTextBoxHandler : Control.IHandler, ISyntaxHighlightingTextBoxHandlerBase
    {
    }
    /// <summary>
    /// A handler for a text box control for syntax highlighting.
    /// </summary>
    public interface ISyntaxHighlightingTextBoxHandlerBase
    {
        /// <summary>
        /// Gets or sets the value if the control allows editing.
        /// </summary>
        bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the value if the line numbers should be shown.
        /// </summary>
        bool ShowLines { get; set; }

        /// <summary>
        /// The position of the cursor.
        /// </summary>
        int CaretOffset { get; set; }

        /// <summary>
        /// Gets or sets the plain text for the text box.
        /// </summary>
        string Text { get; set; }
    }
    /// <summary>
    /// A callback interface for a text box control for syntax highlighting.
    /// </summary>
    public interface ISyntaxHighlightingTextBoxCallback : Control.ICallback
    {
        /// <summary>
        /// Raises the text changed event.
        /// </summary>
        void OnTextChanged(SyntaxHighlightingTextBox widget, EventArgs e);
    }
    /// <summary>
    /// A text box control for syntax highlighting.
    /// </summary>
    [Handler(typeof(ISyntaxHighlightingTextBoxHandler))]
    public class SyntaxHighlightingTextBox : CommonControl, ISyntaxHighlightingTextBoxHandlerBase
    {
        private static readonly object callback = new SyntaxHighlightingTextBoxCallback();
        
        /// <summary>
        /// Callback implementation for the <see cref="Scrollable"/>
        /// </summary>
        protected class SyntaxHighlightingTextBoxCallback : Callback, ISyntaxHighlightingTextBoxCallback
        {
            /// <summary>
            /// Raises the text changed event.
            /// </summary>
            public void OnTextChanged(SyntaxHighlightingTextBox widget, EventArgs e)
            {
                widget.Platform.Invoke(() => widget.OnTextChanged(e));
            }
        }

        /// <summary>
        /// Gets an instance of an object used to perform callbacks to the widget from handler implementations
        /// </summary>
        /// <returns>The callback instance to use for this widget</returns>
        protected override object GetCallback()
        {
            return callback;
        }

        /// <summary>
        /// The handler for the native control.
        /// </summary>
        protected ISyntaxHighlightingTextBoxHandler TextBox => (ISyntaxHighlightingTextBoxHandler)Handler;

        /// <summary>
        /// Raises the <see cref="TextChanged"/> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnTextChanged(EventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when the <see cref="Text"/> changed.
        /// </summary>
        public event EventHandler<EventArgs> TextChanged;
        
        /// <summary>
        /// Gets or sets the value if the control allows editing.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return TextBox.ReadOnly;
            }
            set
            {
                TextBox.ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the value if the line numbers should be shown.
        /// </summary>
        public bool ShowLines
        {
            get
            {
                return TextBox.ShowLines;
            }
            set
            {
                TextBox.ShowLines = value;
            }
        }

        /// <summary>
        /// The position of the cursor.
        /// </summary>
        public int CaretOffset
        {
            get
            {
                return TextBox.CaretOffset;
            }
            set
            {
                TextBox.CaretOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the plain text for the text box.
        /// </summary>
        public string Text
        {
            get
            {
                return TextBox.Text;
            }
            set
            {
                TextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets the binding for the <see cref="Text"/> property.
        /// </summary>
        /// <value>The value binding.</value>
        public BindableBinding<SyntaxHighlightingTextBox, string> ValueBinding
        {
            get
            {
                return new BindableBinding<SyntaxHighlightingTextBox, string>(
                    this,
                    c => c.Text,
                    (c, v) => c.Text = v,
                    (c, h) => c.TextChanged += h,
                    (c, h) => c.TextChanged -= h
                );
            }
        }
    }
}
