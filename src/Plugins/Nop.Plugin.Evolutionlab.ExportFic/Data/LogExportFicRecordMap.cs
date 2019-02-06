using Nop.Data.Mapping;
using Nop.Plugin.Evolutionlab.ExportFic.Domain;

namespace Nop.Plugin.Evolutionlab.ExportFic.Data
{
    public partial class LogExportFicRecordMap : NopEntityTypeConfiguration<LogExportFicRecord>
    {
        public LogExportFicRecordMap()
        {
            this.ToTable("LogExportFic");
            this.HasKey(x => x.Id);
        }
    }
}