using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Evolutionlab.StockPortwest.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class StockPortwestController : BasePluginController
    {
        private readonly IDbContext _dbContext;
        private readonly IPermissionService _permissionService;

        public StockPortwestController(IDbContext dbContext, IPermissionService permissionService)
        {
            _dbContext = dbContext;
            _permissionService = permissionService;
        }

        public IActionResult UploadFile()
        {
            ViewBag.Aggiornate = 0;

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            return View("~/Plugins/Evolutionlab.StockPortwest/Views/UploadFile.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            ViewBag.Aggiornate = 0;

            if (file == null || file.Length == 0)
            {
                ViewBag.Errore = "Devi selezionare un file per continuare";
            } else if (!file.FileName.EndsWith(".csv"))
            {
                ViewBag.Errore = "Devi caricare il file .csv, nessun altro tipo di file è ammesso";
            }

            else
            {
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(), "FilePortwest", "magazzino_portwest.csv");

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                //lancio la stored procedure di aggiornamento, se restituisce errore vuol dire che non ho importato nu,,a

                try
                {
                    ViewBag.Aggiornate = _dbContext.ExecuteSqlCommand("AggiornaVarianti_Portwest", true, 900);

                    if (ViewBag.Aggiornate == 0)
                    {
                        ViewBag.Errore = "Nessuna variante trovata nel file caricato";
                    }

                }
                catch (Exception e)
                {
                    ViewBag.Errore = "Impossibile importare il file caricato:<br>" + e.Message;
                }

                //Elimino il file importato
                System.IO.File.Delete(path);

            }

           return View("~/Plugins/Evolutionlab.StockPortwest/Views/UploadFile.cshtml");


        }

    }
}
