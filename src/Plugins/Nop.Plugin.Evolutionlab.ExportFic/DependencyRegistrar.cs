using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Evolutionlab.ExportFic.Data;
using Nop.Plugin.Evolutionlab.ExportFic.Domain;
using Nop.Plugin.Evolutionlab.ExportFic.Services;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Evolutionlab.ExportFic
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<LogExportFicService>().As<ILogExportFicService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<LogExportFicObjectContext>(builder, "nop_object_context_export_fic");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<LogExportFicRecord>>()
                .As<IRepository<LogExportFicRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_export_fic"))
                .InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 1;
    }
}