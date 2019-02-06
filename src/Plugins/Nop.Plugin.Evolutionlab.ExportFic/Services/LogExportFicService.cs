using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Plugin.Evolutionlab.ExportFic.Domain;

namespace Nop.Plugin.Evolutionlab.ExportFic.Services
{
    public partial class LogExportFicService : ILogExportFicService
    {
        #region Fields

        private readonly IRepository<LogExportFicRecord> _logRepository;

        #endregion

        #region Ctor

        public LogExportFicService(IRepository<LogExportFicRecord> logRepository)
        {
            this._logRepository = logRepository;
        }

        #endregion

        #region Methods

        public virtual void Delete(int logExportFicId)
        {
            Delete(GetById(logExportFicId));
        }

        public virtual void Delete(LogExportFicRecord logExportFic)
        {
            if (logExportFic == null)
                throw new ArgumentNullException(nameof(logExportFic));

            _logRepository.Delete(logExportFic);
        }

        public virtual IList<LogExportFicRecord> GetAll()
        {
            var query = from logs in _logRepository.Table
                        orderby logs.Id
                        select logs;
            var records = query.ToList();
            return records;
        }

        public virtual LogExportFicRecord GetById(int logExportFicId)
        {
            if (logExportFicId == 0)
                return null;

            return _logRepository.GetById(logExportFicId);
        }

        public virtual void Add(LogExportFicRecord logExportFic)
        {
            if (logExportFic == null)
                throw new ArgumentNullException(nameof(logExportFic));

            _logRepository.Insert(logExportFic);
        }

        public virtual void Update(LogExportFicRecord logExportFic)
        {
            if (logExportFic == null)
                throw new ArgumentNullException(nameof(logExportFic));

            _logRepository.Update(logExportFic);
        }

        #endregion
    }
}