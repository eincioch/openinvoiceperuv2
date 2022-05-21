﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using OpenInvoicePeru.Comun.Dto.Intercambio;
using OpenInvoicePeru.Comun.Dto.Modelos;
using OpenInvoicePeru.Firmado;
using OpenInvoicePeru.Xml;
using Swashbuckle.Swagger.Annotations;

namespace OpenInvoicePeru.ApiRest.Controllers
{
    /// <inheritdoc />
    public class GenerarFacturaController : ApiController
    {
        private readonly IDocumentoXml _documentoXml;
        private readonly ISerializador _serializador;

        /// <inheritdoc />
        public GenerarFacturaController()
        {
            _serializador = new Serializador();
            _documentoXml = new FacturaXml();
            //_documentoXml = _documentoXml = UnityConfig.Container
            //    .Resolve<IDocumentoXml>(GetType().Name);
        }

        /// <summary>
        /// Generar el XML para una Factura o una Boleta.
        /// </summary>
        [HttpPost]
        [SwaggerResponse(200, "OK", typeof(DocumentoResponse))]
        [SwaggerResponse(400, "Bad Request", typeof(string))]
        [SwaggerResponse(209, "Conflicts", typeof(string))]
        public async Task<IHttpActionResult> Post([FromBody] DocumentoElectronico documento)
        {
            var response = new DocumentoResponse();
            try
            {
                var invoice = _documentoXml.Generar(documento);
                response.TramaXmlSinFirma = await _serializador.GenerarXml(invoice);
                var serieCorrelativo = documento.IdDocumento.Split('-');
                response.ValoresParaQr =
                    $"{documento.Emisor.NroDocumento}|{documento.TipoDocumento}|{serieCorrelativo[0]}|{serieCorrelativo[1]}|{documento.TotalIgv:N2}|{documento.TotalVenta:N2}|{Convert.ToDateTime(documento.FechaEmision):yyyy-MM-dd}|{documento.Receptor.TipoDocumento}|{documento.Receptor.NroDocumento}|";

                //await Task.Factory.StartNew(async () =>
                //{
                //    await _telegramService.EnviarMensaje($"{documento.Emisor.NombreLegal} => para {documento.Receptor.NombreLegal} | {documento.IdDocumento} con un Total de {documento.TotalVenta:N2}");
                //});

                response.Exito = true;
            }
            catch (Exception ex)
            {
                response.MensajeError = ex.Message;
                response.Pila = ex.StackTrace;
                response.Exito = false;
            }

            return Ok(response);
        }
    }
}
