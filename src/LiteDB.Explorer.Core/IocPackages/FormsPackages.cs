using System.Linq;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace LiteDB.Explorer.Core.IocPackages
{
    /// <summary>
    /// Registers all the forms and view models in the IOC container.
    /// </summary>
    public class FormsPackages : IPackage
    {
        /// <summary>
        /// Registers all the forms and view models in the IOC container.
        /// </summary>
        public void RegisterServices(Container container)
        {
            foreach (var type in typeof(FormsPackages)
                .Assembly
                .GetExportedTypes()
                .Where(type => type.Namespace?.StartsWith("LiteDB.Explorer.Core.Forms") ?? false))
            {
                container.Register(type, type, Lifestyle.Singleton);
            }
        }
    }
}
