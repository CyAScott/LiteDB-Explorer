using LiteDB.Explorer.Core;
using LiteDB.Explorer.Core.Controls.EtoControls;
using LiteDB.Explorer.Windows.Controls;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace LiteDB.Explorer.Windows.IocPackages
{
    public class ControlsPackages : IPackage
    {
        public void RegisterServices(Container container)
        {
            var args = container.GetInstance<SetupArgs>();

            args.Application.Platform.Add<ISyntaxHighlightingTextBoxHandler>(() => new WpfSyntaxHighlightingTextBoxHandler());
        }
    }
}
