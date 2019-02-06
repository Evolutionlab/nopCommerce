using System.Collections.Generic;
using Nop.Plugin.Evolutionlab.ExportFic.Domain;

namespace Nop.Plugin.Evolutionlab.ExportFic.Services
{
    public partial interface ILogExportFicService
    {
        void Delete(LogExportFicRecord logExportFic);

        void Delete(int logExportFicId);

        IList<LogExportFicRecord> GetAll();

        LogExportFicRecord GetById(int logExportFicId);

        void Add(LogExportFicRecord logExportFic);

        void Update(LogExportFicRecord logExportFic);

    }
}