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
            public List<Autorizante> listarAutRedet()


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

                        decimal montoCalculado = 0;

                        if (redet.Nro == 1)
                        {
                            // Calcular monto según la fórmula: ((montoAutorizado * porcentaje) / 100) * faltante / 100
                            montoCalculado = ((autorizante.MontoAutorizado * redet.Porcentaje.Value) / 100) * (faltante / 100);
                        }
                        else
                        {
                            // Obtener redeterminaciones anteriores para este mismo autorizante
                            var redeterminacionesAnteriores = redeterminacionesCalculadas
                                .Where(r => r.Autorizante.CodigoAutorizante == redet.Autorizante.CodigoAutorizante
                                       && r.Nro.HasValue
                                       && r.Nro.Value < redet.Nro.Value
                                       && r.MontoRedet.HasValue)
                                .ToList();


                            // Sumar los montos de redeterminaciones anteriores
                            decimal sumaMontosRedet = redeterminacionesAnteriores.Sum(r => r.MontoRedet.Value);

                            // Calcular monto según la fórmula: ((montoAutorizado * porcentaje) / 100) * faltante / 100 + sumaMontosRedet
                            montoCalculado = (((autorizante.MontoAutorizado + sumaMontosRedet) * redet.Porcentaje.Value) / 100) * (faltante / 100);
                        }



                        // Agregar el monto calculado a la redeterminación
                        var redetCalculada = new Redeterminacion();
                        redetCalculada = redet;
                        redetCalculada.MontoRedet = montoCalculado;

                        redeterminacionesCalculadas.Add(redet); // Agregar a la lista de resultados

                    }
                }



                foreach (var redet in redeterminacionesCalculadas)
                {
                    // Check if redet.Autorizante and redet.Autorizante.Obra are not null
                    if (redet.Autorizante != null && redet.Autorizante.Obra != null)
                    {
                        // Create a new autorizante with the fields from the redeterminación
                        var nuevoAutorizante = new Autorizante
                        {
                            Obra = new Obra
                            {
                                Descripcion = redet.Autorizante.Obra.Descripcion,
                                Id = redet.Autorizante.Obra.Id,
                                Area = redet.Autorizante.Obra.Area != null ? new Area { Nombre = redet.Autorizante.Obra.Area.Nombre } : null,
                                Contrata = redet.Autorizante.Obra.Contrata != null ? new Contrata { Nombre = redet.Autorizante.Obra.Contrata.Nombre } : null
                            },
                            CodigoAutorizante = redet.CodigoRedet,
                            Concepto = new Concepto { Id = 11, Nombre = "REDETERMINACION" }, // Assign a default concept
                            Detalle = redet.Observaciones,
                            Expediente = redet.Expediente,
                            Estado = new EstadoAutorizante { Id = redet.Etapa.Id, Nombre = redet.Etapa.Nombre },
                            MontoAutorizado = redet.MontoRedet.HasValue ? redet.MontoRedet.Value : 0,
                            Fecha = redet.Salto,
                            Empresa = redet.Empresa,
                            FechaSade = redet.FechaSade,
                            BuzonSade = redet.BuzonSade,
                            AutorizacionGG = true
                        };

                        listaAut.Add(nuevoAutorizante);
                    }
                }


                return listaAut;
            }

        }
    }

}
