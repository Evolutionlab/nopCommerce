using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Plugin.Evolutionlab.ExportFic.Data;
using Nop.Plugin.Evolutionlab.ExportFic.Domain;
using Nop.Plugin.Evolutionlab.ExportFic.Models;
using Nop.Plugin.Evolutionlab.ExportFic.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Evolutionlab.ExportFic
{
    public class ExportFicPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IConsumer<OrderPaidEvent>
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly FicSettings _ficSettings;
        private readonly LogExportFicObjectContext _objectContext;
        private readonly ILogExportFicService _ficLogService;
        private static HttpClient _httpClient;

        #endregion

        #region Ctor

        public ExportFicPlugin(
            IWebHelper webHelper,
            IPermissionService permissionService,
            IProductAttributeParser productAttributeParser,
            ICustomerAttributeParser customerAttributeParser,
            IOrderService orderService,
            ISettingService settingService,
            FicSettings ficSettings,
            LogExportFicObjectContext objectContext,
            ILogExportFicService ficLogService)
        {
            _webHelper               = webHelper;
            _permissionService       = permissionService;
            _productAttributeParser  = productAttributeParser;
            _customerAttributeParser = customerAttributeParser;
            _orderService            = orderService;
            _settingService          = settingService;
            _ficSettings             = ficSettings;
            _objectContext           = objectContext;
            _ficLogService           = ficLogService;
        }

        #endregion

        #region Events

        public void HandleEvent(OrderPaidEvent eventMessage)
        {
            try
            {
                var order = eventMessage.Order;
                if (order == null)
                    throw new ArgumentNullException(nameof(eventMessage));

                ExecuteExport(order);

            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(nameof(eventMessage));
            }
        }

        #endregion

        #region Methods

        public virtual void ExecuteExport(Order ordine)
        {
            if (_ficSettings.Attivo && !string.IsNullOrEmpty(_ficSettings.ApiUid) && !string.IsNullOrEmpty(_ficSettings.ApiKey) &&
                !string.IsNullOrEmpty(_ficSettings.BaseUrlPost))
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient();

                var adesso = DateTime.Now;
                var orderId           = ordine.Id;
                var orderIdFatInCloud = "";
                var modifica          = false;
                var rigaOutput        = $"Esportazione ordine {ordine.Id}: ";
                var totaleOrdine      = 0.00;
                var totaleSalvato     = 0.00;
                var prodotti          = ordine.OrderItems.ToList();

                var log = new LogExportFicRecord
                {
                    Data             = adesso,
                    OrdineId         = orderId,
                    OrdineNumero     = orderId,
                    OrdineFatInCloud = "",
                    DdtFatInCloud    = "",
                    TotaleDb         = 0.00,
                    TotaleEsportato  = 0.00,
                    Tipo             = "FATTURA"
                };

                var billingAddress = ordine.BillingAddress;

                totaleSalvato = ordine.OrderTotal.ToDbl();

                //ORDINE E CLIENTE

                var orderModel = new DocRequest
                {
                    ApiUid                = _ficSettings.ApiUid,
                    ApiKey                = _ficSettings.ApiKey,
                    Id                    = "",
                    Token                 = "",
                    Nome                  = billingAddress.Company.ToNotNull($"{billingAddress.FirstName} {billingAddress.LastName}"),
                    IndirizzoVia          = $"{billingAddress.Address1} {billingAddress.Address2}".Trim(),
                    IndirizzoCap          = billingAddress.ZipPostalCode,
                    IndirizzoCitta        = billingAddress.City,
                    IndirizzoProvincia    = billingAddress.StateProvince.Name,
                    PaeseIso              = billingAddress.Country.TwoLetterIsoCode,
                    Lingua                = "it",
                    Piva                  = ordine.VatNumber,
                    Cf                    = ordine.VatNumber,
                    AutocompilaAnagrafica = false,
                    SalvaAnagrafica       = true,
                    Numero                = _ficSettings.Numerazione, 
                    Data                  = ordine.CreatedOnUtc.ToString("dd/MM/yyyy"),
                    Valuta                = "EUR",
                    ValutaCambio          = 1,
                    PrezziIvati           = true,
                    OggettoVisibile       = $"Ordine sito: {ordine.Id}",
                    OggettoInterno        = $"Sito: www.topbuyer.it",
                    MostraInfoPagamento   = true,
                    MetodoPagamento       = GetPaymentMethodSlug(ordine.PaymentMethodSystemName),
                    ExtraAnagrafica = new ExtraAnagrafica
                    {
                        Tel  = billingAddress.PhoneNumber,
                        Mail = billingAddress.Email
                    }
                };

                var listaClienti = CheckEsistenzaCliente(orderModel.Nome, orderModel.Piva, orderModel.Cf);

                //Evito la duplicazione dei privati senza piva e cf
                if (listaClienti != null && listaClienti.Any() &&
                    string.IsNullOrWhiteSpace(orderModel.Piva) &&
                    string.IsNullOrWhiteSpace(orderModel.Cf))
                {
                    orderModel.SalvaAnagrafica = false;
                }

                var customAttributesXml = ordine.Customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomCustomerAttributes);
                var codDestinatario = _customerAttributeParser.ParseValues(customAttributesXml, 4).FirstOrDefault().ToNotNull().Trim();

                if (!string.IsNullOrWhiteSpace(codDestinatario))
                {
                    orderModel.Pa            = true;
                    orderModel.PaTipoCliente = "B2B";
                    orderModel.PaTipo        = "ordine";

                    if (codDestinatario.Contains("@"))
                        orderModel.PaPec = codDestinatario;
                    else
                        orderModel.PaCodice = codDestinatario;
                }

                //PRODOTTI

                var ordinamentoPRod = 0;
                var productModel    = new List<ListaArticoli>();
                var totProdotti     = 0.00;

                foreach (var prodotto in prodotti)
                {
                    productModel.Add(new ListaArticoli
                    {
                        Codice = prodotto.Product.FormatSku(prodotto.AttributesXml, _productAttributeParser),
                        Nome = prodotto.Product.Name + " " +
                               prodotto.AttributeDescription.Replace("<br />", " - "),
                        Quantita    = prodotto.Quantity,
                        Um          = "pezzi",
                        PrezzoLordo = Math.Round(prodotto.PriceInclTax.ToDbl(), 2),
                        CodIva      = 0,
                        Tassabile   = true,
                        Sconto      = 0.00,
                        InDdt       = true,
                        Magazzino   = false,
                        Ordine      = ordinamentoPRod

                    });

                    totProdotti += prodotto.Quantity * prodotto.PriceInclTax.ToDbl();
                    ordinamentoPRod++;
                }

                //OPZIONI

                if (ordine.OrderShippingInclTax > 0)
                {
                    productModel.Add(new ListaArticoli
                    {
                        Nome        = ordine.ShippingMethod,
                        Quantita    = 1,
                        PrezzoLordo = Math.Round(ordine.OrderShippingInclTax.ToDbl(), 2),
                        CodIva      = 0,
                        Tassabile   = true,
                        Sconto      = 0.00,
                        InDdt       = false,
                        Magazzino   = false,
                        Ordine      = ordinamentoPRod
                    });
                    ordinamentoPRod++;
                }

                if (ordine.PaymentMethodAdditionalFeeInclTax > 0)
                {
                    productModel.Add(new ListaArticoli
                    {
                        Nome        = "Pagamento",
                        Quantita    = 1,
                        PrezzoLordo = Math.Round(ordine.PaymentMethodAdditionalFeeInclTax.ToDbl(), 2),
                        CodIva      = 0,
                        Tassabile   = true,
                        Sconto      = 0.00,
                        InDdt       = false,
                        Magazzino   = false,
                        Ordine      = ordinamentoPRod
                    });
                    ordinamentoPRod++;
                }

                //SCONTI
                if (ordine.OrderDiscount > 0)
                {
                    productModel.Add(new ListaArticoli
                    {
                        Nome        = "SCONTO",
                        Quantita    = 1,
                        PrezzoLordo = -Math.Round(ordine.OrderDiscount.ToDbl(), 2),
                        CodIva      = 0,
                        Tassabile   = true,
                        Sconto      = 0.00,
                        InDdt       = false,
                        Magazzino   = false,
                        Ordine      = ordinamentoPRod
                    });
                    ordinamentoPRod++;
                }

                foreach (var prodotto in productModel)
                    totaleOrdine += prodotto.Quantita * prodotto.PrezzoLordo;

                totaleOrdine = Math.Round(totaleOrdine, 2);

                orderModel.ListaArticoli = productModel;


                // PAGAMENTI

                orderModel.ListaPagamenti = new List<ListaPagamenti>()
                {
                    new ListaPagamenti
                    {
                        DataScadenza = ordine.CreatedOnUtc.ToString("dd/MM/yyyy"),
                        Importo      = totaleOrdine,
                        Metodo       = orderModel.MetodoPagamento,
                        DataSaldo    = ordine.CreatedOnUtc.ToString("dd/MM/yyyy")
                    }
                };

                //Eseguo esportazione e scrivo esito

                var response  = ExecuteExport(orderModel, modifica, out string errore);

                if (!string.IsNullOrWhiteSpace(errore))
                {
                    rigaOutput += $" FALLITA - {errore}";
                }
                else if (response != null && response.NewOrderId > 0)
                {
                    //se non c'è il cliente, provo a ricercarlo dopo il salvataggio dell'ordine
                    if (listaClienti == null || listaClienti.Count == 0)
                        listaClienti = CheckEsistenzaCliente(orderModel.Nome, orderModel.Piva, orderModel.Cf);

                    //salvataggio cliente con dati fatturazione
                    var clienteFatInCloud = UpdateCliente(listaClienti, codDestinatario);

                    //if (clienteFatInCloud != null)
                    //    ordine.ClienteFatInCloud = clienteFatInCloud.Id;

                    ordine.InvoiceDate = adesso;
                    ordine.InvoiceId = GetNewInvoiceNumber(response.NewOrderId.ToString());
                    //ordine.TotaleFatInCloud = totaleOrdine;

                    try
                    {
                        _orderService.UpdateOrder(ordine);

                        var testoTotale = "";
                        if (Math.Abs(totaleOrdine - totaleSalvato) > 0.001)
                            testoTotale = $" - Totali diversi: Db={totaleSalvato} | Calcolato={totaleOrdine}";

                        log.OrdineFatInCloud = response.NewOrderId.ToString();
                        log.Token = response.Token;
                        log.TotaleDb = totaleSalvato;
                        log.TotaleEsportato = totaleOrdine;
                        log.Messaggio = "OK" + (!string.IsNullOrWhiteSpace(testoTotale) ? " - Totali diversi" : "");

                        rigaOutput += $" ESEGUITA{testoTotale}";

                    }
                    catch (Exception e)
                    {
                        rigaOutput += $" ERRORE - Esportazione riuscita ma errore salvataggio ordine su database";
                    }

                }
                else
                {
                    rigaOutput += " FALLITA - errore generale";
                }

                log.Errore = rigaOutput.Contains("FALLITA");

                if (string.IsNullOrEmpty(log.Messaggio))
                    log.Messaggio = rigaOutput;


                try
                {
                    _ficLogService.Add(log);
                }
                catch (Exception e)
                {

                }

            }
        }

        private DocNuovoResponse ExecuteExport(DocRequest docRequest, bool modifica, out string errore)
        {
            var content = new StringContent(docRequest.ToJson());
            errore = "";

            try
            {
                var response = _httpClient
                    .PostAsync($"{_ficSettings.BaseUrlPost}/fatture/{(modifica ? "modifica" : "nuovo")}", content)
                    .Result;
                var responseString = JsonConvert.DeserializeObject<DocNuovoResponse>(response.Content.ReadAsStringAsync().Result);

                if (responseString == null || responseString.Success == false)
                {
                    errore = response.Content.ReadAsStringAsync().Result;
                    return null;
                }

                return responseString;
            }
            catch (Exception e)
            {
                errore = e.Message;
                return null;
            }
        }

        private string GetNewInvoiceNumber(string newOrderId)
        {
            //recupero il numero fattura
            var richiesta = new DocDettagliRequest
            {
                ApiUid = _ficSettings.ApiUid,
                ApiKey = _ficSettings.ApiKey,
                Id     = newOrderId
            };

            var content = new StringContent(JsonConvert.SerializeObject(richiesta, Formatting.None));
            //errore = "";

            try
            {
                var response = _httpClient
                    .PostAsync($"{_ficSettings.BaseUrlPost}/fatture/dettagli", content)
                    .Result;

                var docResponseString = JsonConvert.DeserializeObject<DocDettagliResponse>(response.Content.ReadAsStringAsync().Result);

                if (docResponseString == null || docResponseString.Success == false)
                {
                    //errore = response.Content.ReadAsStringAsync().Result;
                    return "";
                }

                return docResponseString.DettagliDocumento.Numero;

            }
            catch (Exception e)
            {
                //errore = e.Message;
                return "";
            }
        }


        private List<ClientiResponse> CheckEsistenzaCliente(string nome, string piva, string cf)
        {
            var richiesta = new AnagraficaListaRequest()
            {
                ApiUid = _ficSettings.ApiUid,
                ApiKey = _ficSettings.ApiKey,
                Nome   = nome,
                Piva   = piva,
                Cf     = cf
            };

            var content = new StringContent(JsonConvert.SerializeObject(richiesta, Formatting.None));

            try
            {
                var response = _httpClient.PostAsync($"{_ficSettings.BaseUrlPost}/clienti/lista", content).Result;
                var responseString =
                    JsonConvert.DeserializeObject<AnagraficaListaResponse>(response.Content.ReadAsStringAsync().Result);

                if (responseString?.ListaClienti != null && responseString.Success)
                {
                    return responseString.ListaClienti;
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private ClientiResponse UpdateCliente(List<ClientiResponse> listaClienti, string codiceDestinatario)
        {
            try
            {
                if (listaClienti != null)
                {
                    var cliente = listaClienti.FirstOrDefault();

                    if (cliente != null && !string.IsNullOrWhiteSpace(codiceDestinatario) && cliente.PaCodice != codiceDestinatario)
                    {
                        cliente.ApiUid   = _ficSettings.ApiUid;
                        cliente.ApiKey   = _ficSettings.ApiKey;
                        cliente.Pa       = true;
                        cliente.PaCodice = codiceDestinatario;

                        return ExecuteExportCliente(cliente);
                    }

                    return cliente;
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private ClientiResponse ExecuteExportCliente(ClientiResponse docRequest)
        {
            var content = new StringContent(JsonConvert.SerializeObject(docRequest, Formatting.None));

            try
            {
                var response       = _httpClient.PostAsync($"{_ficSettings.BaseUrlPost}/clienti/modifica", content).Result;
                var responseString = JsonConvert.DeserializeObject<DocNuovoResponse>(response.Content.ReadAsStringAsync().Result);

                if (responseString == null || responseString.Success == false)
                {
                    return null;
                }

                return docRequest;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName     = "Evolutionlab.ExportFic",
                Title          = "Log export FatInCloud",
                IconClass      = "fa fa-dot-circle-o",
                ControllerName = "ExportFic",
                ActionName     = "LogExportFicDetails",
                Visible        = _permissionService.Authorize(StandardPermissionProvider.ManageOrders) || _permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings),
                RouteValues    = new RouteValueDictionary { { "Area", "Admin" } }
            };
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Sales");
            if (pluginNode != null)
                pluginNode.ChildNodes.Insert(1, menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);
        }

        private string GetPaymentMethodSlug(string paymentMethodSystemName)
        {
            switch (paymentMethodSystemName)
            {
                case "Opensoftware.Payments.Cartasi": return "Carta di credito";
                case "Payments.CheckMoneyOrder":      return "Bonifico bancario anticipato";
                case "Payments.PayPalStandard":       return "Carta di credito e Paypal";
                default:                              return "";

            }
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ExportFic/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new FicSettings
            {
                ApiUid      = "",
                ApiKey      = "",
                BaseUrlPost = "https://api.fattureincloud.it/v1",
                Numerazione = "",
                Attivo      = false
            };
            _settingService.SaveSetting(settings);

            //data
            _objectContext.Install();

            base.Install();
        }


        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<FicSettings>();

            //data
            _objectContext.Uninstall();

            base.Uninstall();
        }

        #endregion
    }
}
