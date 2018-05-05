using System;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Evolutionlab.StockPortwest
{
    public class StockPortwestPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;

        public StockPortwestPlugin(IWebHelper webHelper, IPermissionService permissionService)
        {
            _webHelper = webHelper;
            _permissionService = permissionService;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/StockPortwest/UploadFile";
        }



        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName     = "Evolutionlab.StockPortwest",
                Title          = "Carica magazzino Porwtest",
                IconClass      = "fa fa-dot-circle-o",
                ControllerName = "StockPortwest",
                ActionName     = "UploadFile",
                Visible        = _permissionService.Authorize(StandardPermissionProvider.ManageMaintenance),
                RouteValues    = new RouteValueDictionary { { "Area", "Admin" } }
            };
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Catalog");
            if (pluginNode != null)
                pluginNode.ChildNodes.Insert(1, menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);
        }

    }
}
