using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Evolutionlab.ExportFic.Domain;
using Nop.Plugin.Evolutionlab.ExportFic.Models;
using Nop.Plugin.Evolutionlab.ExportFic.Services;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Evolutionlab.ExportFic.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ExportFicController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly FicSettings _ficSettings;
        private readonly ILogExportFicService _logExportFicService;

        public ExportFicController(
            IPermissionService permissionService, 
            ISettingService settingService,
            FicSettings ficSettings,
            ILogExportFicService logExportFicService)
        {
            _permissionService   = permissionService;
            _settingService      = settingService;
            _ficSettings         = ficSettings;
            _logExportFicService = logExportFicService;
        }

        public IActionResult Configure()
        {
            if (UserNotEnabled())
                return AccessDeniedView();

            CommonHelper.SetTelerikCulture();
            return View("~/Plugins/Evolutionlab.ExportFic/Views/Configure.cshtml", _ficSettings);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult Configure(FicSettings model)
        {
            if (UserNotEnabled())
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _settingService.SaveSetting(model);

            //redisplay the form
            return Configure();
        }

        public IActionResult LogExportFicDetails()
        {
            return View("~/Plugins/Evolutionlab.ExportFic/Views/LogExportFicDetails.cshtml", null);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult LogExportFicList(DataSourceRequest command)
        {
            if (UserNotEnabled())
                return AccessDeniedKendoGridJson();

            var lista = _logExportFicService.GetAll();

            var pagedList = new PagedList<LogExportFicRecord>(_logExportFicService.GetAll(), command.Page - 1, command.PageSize);

            DataSourceResult dataSourceResult = new DataSourceResult { Data = pagedList, Total = pagedList.TotalCount };
            return Json(dataSourceResult);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult LogExportFicDelete(int id)
        {
            if (UserNotEnabled())
                return AccessDeniedKendoGridJson();

            _logExportFicService.Delete(id);

            return new NullJsonResult();
        }

        private bool UserNotEnabled()
        {
            return !_permissionService.Authorize(StandardPermissionProvider.ManageSettings) &&
                   !_permissionService.Authorize(StandardPermissionProvider.ManageOrders) &&
                   !_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings);
        }

    }
}
