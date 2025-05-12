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

                        redeterminacionesCalculadas.Add(redetCalculada); // Agregar a la lista de resultados

                    }
                }



                foreach (var redet in redeterminacionesCalculadas)
                {

                    var autorizanteOriginal = listaAut.FirstOrDefault(a => a.CodigoAutorizante == redet.Autorizante.CodigoAutorizante);
                    // Check if redet.Autorizante and redet.Autorizante.Obra are not null
                    if (redet.Autorizante != null && redet.Autorizante.Obra != null)
                    {
                        // Create a new autorizante with the fields from the redeterminación
                        var nuevoAutorizante = new Autorizante
                        {
                            //Obra = new Obra
                            //{
                            //    Descripcion = redet.Autorizante.Obra.Descripcion,
                            //    Id = redet.Autorizante.Obra.Id,
                            //    Area = redet.Autorizante.Obra.Area != null ? new Area { Nombre = redet.Autorizante.Obra.Area.Nombre } : null,
                            //    Contrata = redet.Autorizante.Obra.Contrata != null ? new Contrata { Nombre = redet.Autorizante.Obra.Contrata.Nombre } : null
                            //},
                            Obra = autorizanteOriginal.Obra,
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


            public List<Certificado> listarCertReliq()
            {
                List<Autorizante> listaAut = new List<Autorizante>();
                List<Certificado> listaCert = new List<Certificado>();
                List<Redeterminacion> listaRedet = new List<Redeterminacion>();
                List<Certificado> listaCertificadosResultado = new List<Certificado>();

                AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
                CertificadoNegocio certificadoNegocio = new CertificadoNegocio();
                RedeterminacionNegocio redeterminacionNegocio = new RedeterminacionNegocio();

                listaAut = autorizanteNegocio.listar();
                listaCert = certificadoNegocio.listarFiltroAdmin();
                listaRedet = redeterminacionNegocio.listar();

                // Primero agregamos los certificados originales a la lista resultado
                listaCertificadosResultado.AddRange(listaCert);

                // Agrupamos los certificados por autorizante para facilitar el procesamiento
                var certificadosPorAutorizante = listaCert
                    .GroupBy(c => c.Autorizante.CodigoAutorizante)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Para cada redeterminación
                foreach (var redet in listaRedet)
                {
                    // Buscar el autorizante correspondiente
                    var autorizante = listaAut.FirstOrDefault(a => a.CodigoAutorizante == redet.Autorizante.CodigoAutorizante);

                    if (autorizante != null && redet.Salto.HasValue && redet.Porcentaje.HasValue)
                    {
                        // Lista para almacenar certificados procesados de esta redeterminación
                        List<Certificado> certificadosRedeterminacion = new List<Certificado>();

                        // Verificar si existen certificados para este autorizante
                        if (certificadosPorAutorizante.TryGetValue(autorizante.CodigoAutorizante, out var certificadosAutorizante))
                        {
                            // Filtrar certificados por código autorizante y fecha <= a salto (para cálculos)
                            var certificadosHastaSalto = certificadosAutorizante
                                .Where(c => c.MesAprobacion.HasValue && c.MesAprobacion <= redet.Salto)
                                .ToList();

                            // Para calcular el porcentaje de ejecución actual y acumulado
                            // Sumar montos totales de los certificados hasta el salto
                            decimal sumaMontosTotal = certificadosHastaSalto.Sum(c => c.MontoTotal);

                            // Calcular el porcentaje de ejecución acumulado
                            decimal porcentajeEjecucionAcumulado = 0;
                            if (autorizante.MontoAutorizado > 0)
                            {
                                porcentajeEjecucionAcumulado = (sumaMontosTotal / autorizante.MontoAutorizado) * 100;
                            }

                            decimal faltante = 100 - porcentajeEjecucionAcumulado;
                            faltante = Math.Max(0, faltante);

                            // Calcular monto de redeterminación
                            decimal montoCalculado = 0;

                            if (redet.Nro == 1)
                            {
                                montoCalculado = ((autorizante.MontoAutorizado * redet.Porcentaje.Value) / 100) * (faltante / 100);
                            }
                            else
                            {
                                // Obtener monto de redeterminaciones anteriores (simplificado para evitar dependencia circular)
                                decimal sumaMontosRedet = 0;
                                foreach (var redAnterior in listaRedet.Where(r =>
                                    r.Autorizante.CodigoAutorizante == redet.Autorizante.CodigoAutorizante &&
                                    r.Nro.HasValue &&
                                    r.Nro.Value < redet.Nro.Value &&
                                    r.MontoRedet.HasValue))
                                {
                                    sumaMontosRedet += redAnterior.MontoRedet.Value;
                                }

                                montoCalculado = (((autorizante.MontoAutorizado + sumaMontosRedet) * redet.Porcentaje.Value) / 100) * (faltante / 100);
                            }

                            // Ahora generamos certificados para esta redeterminación desde el mes del salto en adelante
                            var certificadosDesdeElSalto = certificadosAutorizante
                                .Where(c => c.MesAprobacion.HasValue && c.MesAprobacion >= redet.Salto)
                                .OrderBy(c => c.MesAprobacion)
                                .ToList();

                            // Para cada certificado desde el mes del salto, generar un certificado de redeterminación
                            foreach (var certificadoOriginal in certificadosDesdeElSalto)
                            {
                                // Calcular el porcentaje de ejecución del certificado original respecto al monto autorizado
                                decimal porcentajeEjecucionCertificado = 0;



                                if (autorizante.MontoAutorizado > 0)
                                {
                                    porcentajeEjecucionCertificado = (certificadoOriginal.MontoTotal / autorizante.MontoAutorizado) * 100;
                                }

                                decimal montoCertificadoRedet = 0;
                                decimal porcentajeCalculado = 0;
                                if (certificadoOriginal.MesAprobacion != redet.Salto) 
                                { 
                                    // Calcular el monto del certificado de redeterminación según ese mismo porcentaje
                                    montoCertificadoRedet = montoCalculado * (porcentajeEjecucionCertificado / 100);
                                    porcentajeCalculado = porcentajeEjecucionCertificado;
                                }
                                else
                                { 
                                    montoCertificadoRedet = montoCalculado * (porcentajeEjecucionAcumulado / 100);
                                    porcentajeCalculado = porcentajeEjecucionAcumulado;
                                }



                                // Crear el certificado de redeterminación
                                //Certificado certificadoRedet = new Certificado
                                //    {
                                //        Autorizante = new Autorizante
                                //        {
                                //            CodigoAutorizante = redet.CodigoRedet,
                                //            Obra = redet.Autorizante?.Obra != null ? new Obra
                                //            {
                                //                Descripcion = redet.Autorizante.Obra.Descripcion,
                                //                Id = redet.Autorizante.Obra.Id,
                                //                Area = redet.Autorizante.Obra.Area,
                                //                Contrata = redet.Autorizante.Obra.Contrata
                                //            } : null,
                                //            MontoAutorizado = montoCalculado
                                //        },
                                //        ExpedientePago = string.Empty,
                                //        MontoTotal = montoCertificadoRedet,
                                //        MesAprobacion = certificadoOriginal.MesAprobacion,
                                //        Tipo = new TipoPago { Id = 2, Nombre = "REDETERMINACION" },
                                //        Empresa = redet.Empresa,
                                //        Estado = "REDETERMINADO",
                                //        Porcentaje = porcentajeCalculado.ToString(),
                                //        FechaSade = redet.FechaSade,
                                //        BuzonSade = redet.BuzonSade
                                //    };
                                Certificado certificadoRedet = new Certificado
                                {
                                    Autorizante= new Autorizante
                                    {
                                        CodigoAutorizante = redet.CodigoRedet,
                                        Obra = certificadoOriginal.Autorizante.Obra,
                                        Concepto = certificadoOriginal.Autorizante.Concepto,
                                        Detalle = certificadoOriginal.Autorizante.Detalle,
                                        Estado = certificadoOriginal.Autorizante.Estado,
                                        Expediente = certificadoOriginal.Autorizante.Expediente,
                                        MontoAutorizado = certificadoOriginal.Autorizante.MontoAutorizado,
                                        AutorizacionGG = certificadoOriginal.Autorizante.AutorizacionGG,
                                        Fecha = certificadoOriginal.Autorizante.Fecha,
                                        Empresa = certificadoOriginal.Autorizante.Empresa,
                                        FechaSade = certificadoOriginal.Autorizante.FechaSade,
                                        BuzonSade = certificadoOriginal.Autorizante.BuzonSade
                                    },
                                  
                                    ExpedientePago = string.Empty,
                                    MontoTotal = montoCertificadoRedet,
                                    MesAprobacion = certificadoOriginal.MesAprobacion,
                                    Tipo = new TipoPago { Id = 2, Nombre = "RELIQUIDACION" },
                                    Empresa = redet.Empresa,
                                    Estado = redet.Etapa.Nombre,
                                    Porcentaje = porcentajeCalculado,
                                    FechaSade = redet.FechaSade,
                                    BuzonSade = redet.BuzonSade
                                };

                                certificadosRedeterminacion.Add(certificadoRedet);
                            }
                        }

                        // Agregar todos los certificados de redeterminación a la lista de resultados
                        listaCertificadosResultado.AddRange(certificadosRedeterminacion);
                    }
                }

                return listaCertificadosResultado;
            }



        }

    }

}
