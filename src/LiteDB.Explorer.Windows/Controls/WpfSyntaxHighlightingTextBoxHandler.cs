using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Eto.Wpf.Forms;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using LiteDB.Explorer.Core.Controls.EtoControls;
using Colors = System.Windows.Media.Colors;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace LiteDB.Explorer.Windows.Controls
{
    public class WpfSyntaxHighlightingTextBoxHandler : WpfControl<TextEditor, SyntaxHighlightingTextBox, ISyntaxHighlightingTextBoxCallback>, ISyntaxHighlightingTextBoxHandler
    {
        private void onTextChanged(object sender, EventArgs eventArgs)
        {
            Callback.OnTextChanged(Widget, eventArgs);
        }

        protected override void Initialize()
        {
            base.Initialize();

            Control.TextChanged += onTextChanged;
        }

        public WpfSyntaxHighlightingTextBoxHandler()
        {
            using (var stream = typeof(WpfSyntaxHighlightingTextBoxHandler).Assembly.GetManifestResourceStream("LiteDB.Explorer.Windows.Assets.Bson-Mode.xshd"))
            using (var xml = new XmlTextReader(stream ?? new MemoryStream()))
            {
                Control = new TextEditor
                {
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1),
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    SyntaxHighlighting = HighlightingLoader.Load(xml, HighlightingManager.Instance),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
            }
        }

        public bool ReadOnly
        {
            get
            {
                return Control.IsReadOnly;
            }
            set
            {
                Control.IsReadOnly = value;
            }
        }
        public bool ShowLines
        {
            get
            {
                return Control.ShowLineNumbers;
            }
            set
            {
                Control.ShowLineNumbers = value;
            }
        }
        public int CaretOffset
        {
            get
            {
                return Control.CaretOffset;
            }
            set
            {
                Control.CaretOffset = value;
            }
        }
        public string Text
        {
            get
            {
                return Control.Text;
            }
            set
            {
                Control.Text = value;
            }
        }
    }
}
