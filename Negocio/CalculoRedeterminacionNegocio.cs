using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    using Dominio;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Negocio
    {
        public class CalculoRedeterminacionNegocio
        {
            public List<Redeterminacion> listarAutRedet()
            {
                List<Autorizante> listaAut = new List<Autorizante>();
                List<Certificado> listaCert = new List<Certificado>();
                List<Redeterminacion> listaRedet = new List<Redeterminacion>();

                AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
                CertificadoNegocio certificadoNegocio = new CertificadoNegocio();
                RedeterminacionNegocio redeterminacionNegocio = new RedeterminacionNegocio();

                listaAut = autorizanteNegocio.listar();
                listaCert = certificadoNegocio.listarFiltroAdmin();
                listaRedet = redeterminacionNegocio.listar();

                // Lista resultante con cálculos adicionales
                var redeterminacionesCalculadas = new List<Redeterminacion>();

                foreach (var redet in listaRedet)
                {
                    // Buscar el autorizante correspondiente
                    var autorizante = listaAut.FirstOrDefault(a => a.CodigoAutorizante == redet.Autorizante.CodigoAutorizante);

                    if (autorizante != null && redet.Salto.HasValue && redet.Porcentaje.HasValue)
                    {
                        // Filtrar certificados por código autorizante y fecha <= a salto
                        var certificadosRelacionados = listaCert
                            .Where(c => c.Autorizante.CodigoAutorizante == autorizante.CodigoAutorizante
                                   && c.MesAprobacion.HasValue
                                   && c.MesAprobacion <= redet.Salto)
                            .ToList();

                        // Sumar montos totales de los certificados
                        decimal sumaMontosTotal = certificadosRelacionados.Sum(c => c.MontoTotal);

                        // Calcular el faltante (100 - porcentajeUtilizado)
                        decimal porcentajeUtilizado = 0;
                        if (autorizante.MontoAutorizado > 0)
                        {
                            porcentajeUtilizado = (sumaMontosTotal / autorizante.MontoAutorizado) * 100;
                        }

                        decimal faltante = 100 - porcentajeUtilizado;

                        // Asegurar que el faltante no sea negativo
                        faltante = Math.Max(0, faltante);

                        // Calcular monto según la fórmula: ((montoAutorizado * porcentaje) / 100) * faltante / 100
                        decimal montoCalculado = ((autorizante.MontoAutorizado * redet.Porcentaje.Value) / 100) * (faltante / 100);

                        // Agregar el monto calculado a la redeterminación
                        var redetCalculada = new Redeterminacion(); 
                        redetCalculada = redet;
                        redetCalculada.MontoRedet = montoCalculado;
                                           

                        redeterminacionesCalculadas.Add(redet); // Agregar a la lista de resultados
                    }
                }

                return redeterminacionesCalculadas;
            }
        }
    }

}
