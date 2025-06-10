using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

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
                                   && c.MesAprobacion < redet.Salto && c.Tipo.Id != 1)
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
                            Detalle = redet.Tipo + " - " + redet.Etapa,
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

            public List<Autorizante> listarAutRedet(Usuario usuario)
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
                            Obra = autorizanteOriginal.Obra,
                            CodigoAutorizante = redet.CodigoRedet,
                            Concepto = new Concepto { Id = 11, Nombre = "REDETERMINACION" }, // Assign a default concept
                            Detalle = redet.Tipo + " - " + redet.Etapa,
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

                // Validar que el área del autorizante coincide con el área del usuario
                // Esta validación se hace sólo al final, después de todos los cálculos
                if (usuario != null && usuario.Area != null && usuario.Area.Id > 0)
                {
                    // Filtrar la lista para incluir solo los autorizantes cuya área coincide con la del usuario
                    listaAut = listaAut.Where(a =>
                        a != null &&
                        a.Obra != null &&
                        a.Obra.Area != null &&
                        a.Obra.Area.Id == usuario.Area.Id).ToList();
                }

                return listaAut;
            }

            public List<Certificado> listarCertReliq()
            {
                List<Autorizante> listaAut = new List<Autorizante>();
                List<Certificado> listaCert = new List<Certificado>();
                List<Redeterminacion> listaRedet = new List<Redeterminacion>();
                List<Certificado> listaCertificadosResultado = new List<Certificado>();
                ExpedienteReliqNegocio expedienteReliqNegocio = new ExpedienteReliqNegocio();

                AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
                CertificadoNegocio certificadoNegocio = new CertificadoNegocio();
                RedeterminacionNegocio redeterminacionNegocio = new RedeterminacionNegocio();
                LegitimoNegocio legitimoNegocio = new LegitimoNegocio();

                listaAut = autorizanteNegocio.listar();
                listaCert = certificadoNegocio.listarFiltroAdmin();
                listaRedet = redeterminacionNegocio.listar();

                // Primero agregamos los certificados originales a la lista resultado, pero sin SIGAF aún
                // (lo recalcularemos más tarde para incluir las reliquidaciones)
                foreach (var cert in listaCert)
                {
                    // Crear copia del certificado, pero sin el valor SIGAF
                    var certificadoCopia = new Certificado
                    {
                        Id = cert.Id,
                        Porcentaje = cert.Porcentaje,
                        Autorizante = cert.Autorizante,
                        ExpedientePago = cert.ExpedientePago,
                        Tipo = cert.Tipo,
                        MontoTotal = cert.MontoTotal,
                        // No copiamos el Sigaf porque lo recalcularemos
                        MesAprobacion = cert.MesAprobacion,
                        FechaSade = cert.FechaSade,
                        BuzonSade = cert.BuzonSade,
                        Empresa = cert.Empresa,
                        Estado = cert.Estado
                    };
                    listaCertificadosResultado.Add(certificadoCopia);
                }

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
                                .Where(c => c.MesAprobacion.HasValue && c.MesAprobacion < redet.Salto && c.Tipo.Id != 1)
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
                                    montoCertificadoRedet = montoCalculado * ((porcentajeEjecucionCertificado+porcentajeEjecucionAcumulado) / 100);
                                    porcentajeCalculado = porcentajeEjecucionCertificado+porcentajeEjecucionAcumulado ;
                                }

                                Certificado certificadoRedet = new Certificado
                                {
                                    Autorizante = new Autorizante
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
                                    MontoTotal = montoCertificadoRedet,
                                    MesAprobacion = certificadoOriginal.MesAprobacion,
                                    Tipo = new TipoPago { Id = 2, Nombre = "RELIQUIDACION" },
                                    Empresa = redet.Empresa,
                                    Porcentaje = porcentajeCalculado
                                };

                                // Buscar si existe un expediente en la tabla EXPEDIENTES_RELIQ
                                if (certificadoRedet.MesAprobacion.HasValue)
                                {
                                    try
                                    {
                                        // Verificar si ya hay un expediente asignado en la tabla EXPEDIENTES_RELIQ
                                        var expedienteReliq = expedienteReliqNegocio.ObtenerPorCodigoYMes(
                                            certificadoRedet.Autorizante.CodigoAutorizante,
                                            certificadoRedet.MesAprobacion.Value);

                                        // Si existe, asignar el expediente
                                        if (expedienteReliq != null)
                                        {
                                            certificadoRedet.ExpedientePago = expedienteReliq.Expediente;

                                            // Obtener información SADE
                                            var sadeInfo = SADEHelper.ObtenerInfoSADE(certificadoRedet.ExpedientePago);
                                            certificadoRedet.FechaSade = sadeInfo.FechaUltimoPase ?? null;
                                            certificadoRedet.BuzonSade = sadeInfo.BuzonDestino ?? null;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Capturar errores pero continuar el proceso
                                        System.Diagnostics.Debug.WriteLine($"Error al buscar expediente reliquidación: {ex.Message}");
                                    }
                                }

                                // Determinar el estado del expediente utilizando el helper
                                certificadoRedet.Estado = SIGAFHelper.DeterminarEstadoExpediente(certificadoRedet.ExpedientePago);

                                certificadosRedeterminacion.Add(certificadoRedet);
                            }
                        }

                        // Agregar todos los certificados de redeterminación a la lista de resultados
                        listaCertificadosResultado.AddRange(certificadosRedeterminacion);
                    }
                }


                // Obtener los legítimos abonos directamente de la base de datos
                Dictionary<string, List<decimal>> legitimosPorExpediente = new Dictionary<string, List<decimal>>();
                try
                {
                    // Consultamos directamente la tabla LEGITIMOS_ABONOS
                    var datosLegitimos = new AccesoDatos();
                    datosLegitimos.setearConsulta(@"
            SELECT EXPEDIENTE, CERTIFICADO 
            FROM LEGITIMOS_ABONOS 
            WHERE EXPEDIENTE IS NOT NULL AND EXPEDIENTE <> '' AND CERTIFICADO IS NOT NULL");

                    datosLegitimos.ejecutarLectura();

                    while (datosLegitimos.Lector.Read())
                    {
                        string expediente = datosLegitimos.Lector["EXPEDIENTE"]?.ToString();
                        if (!string.IsNullOrEmpty(expediente) && datosLegitimos.Lector["CERTIFICADO"] != DBNull.Value)
                        {
                            decimal certificado = Convert.ToDecimal(datosLegitimos.Lector["CERTIFICADO"]);

                            if (!legitimosPorExpediente.ContainsKey(expediente))
                                legitimosPorExpediente[expediente] = new List<decimal>();

                            legitimosPorExpediente[expediente].Add(certificado);
                        }
                    }

                    datosLegitimos.cerrarConexion();
                }
                catch (Exception ex)
                {
                    // Si hay error al obtener los legítimos, continuamos solo con los certificados
                    System.Diagnostics.Debug.WriteLine($"Error al obtener legítimos abonos: {ex.Message}");
                }

                // Compartir los certificados resultantes para uso en otros servicios
                DatosCompartidosHelper.SetCertificados(listaCertificadosResultado);

                // Crear diccionario de certificados por expediente para el cálculo final
                var certificadosPorExpediente = listaCertificadosResultado
                    .Where(c => !string.IsNullOrEmpty(c.ExpedientePago))
                    .GroupBy(c => c.ExpedientePago)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Combinar los montos de certificados y legítimos
                Dictionary<string, List<decimal>> montosCombinados = new Dictionary<string, List<decimal>>();

                // Primero agregamos los montos de certificados
                foreach (var kvp in certificadosPorExpediente)
                {
                    string expediente = kvp.Key;
                    var certificados = kvp.Value;

                    montosCombinados[expediente] = certificados.Select(c => c.MontoTotal).ToList();
                }

                // Luego agregamos los montos de legítimos
                foreach (var kvp in legitimosPorExpediente)
                {
                    string expediente = kvp.Key;
                    List<decimal> montos = kvp.Value;

                    if (!montosCombinados.ContainsKey(expediente))
                        montosCombinados[expediente] = new List<decimal>();

                    montosCombinados[expediente].AddRange(montos);
                }

                // Calcular el SIGAF para todos los certificados usando los montos combinados
                foreach (var certificado in listaCertificadosResultado)
                {
                    // Si el certificado tiene un expediente, calcular su SIGAF
                    if (!string.IsNullOrEmpty(certificado.ExpedientePago) &&
                        montosCombinados.TryGetValue(certificado.ExpedientePago, out var montosExpediente))
                    {
                        // Obtener total de devengados
                        decimal totalDevengado = SIGAFHelper.ObtenerTotalImporteDevengados(certificado.ExpedientePago);

                        if (totalDevengado > 0)
                        {
                            // Calcular la suma total de montos para este expediente
                            decimal sumaTotalMontos = montosExpediente.Sum();

                            // Calcular la proporción correspondiente al certificado actual
                            if (sumaTotalMontos > 0)
                            {
                                certificado.Sigaf = totalDevengado * certificado.MontoTotal / sumaTotalMontos;
                            }
                        }
                    }

                    // Reevaluar el estado basado en el SIGAF calculado
                    if (string.IsNullOrEmpty(certificado.ExpedientePago))
                    {
                        certificado.Estado = "NO INICIADO";
                    }
                    else if (certificado.Sigaf.HasValue && certificado.Sigaf > 0)
                    {
                        certificado.Estado = "DEVENGADO";
                    }
                    else
                    {
                        certificado.Estado = "EN TRAMITE";
                    }
                }

                return listaCertificadosResultado;
            }


            public List<Certificado> listarCertReliq(Usuario usuario)
            {
                List<Autorizante> listaAut = new List<Autorizante>();
                List<Certificado> listaCert = new List<Certificado>();
                List<Redeterminacion> listaRedet = new List<Redeterminacion>();
                List<Certificado> listaCertificadosResultado = new List<Certificado>();
                ExpedienteReliqNegocio expedienteReliqNegocio = new ExpedienteReliqNegocio();

                AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
                CertificadoNegocio certificadoNegocio = new CertificadoNegocio();
                RedeterminacionNegocio redeterminacionNegocio = new RedeterminacionNegocio();
                LegitimoNegocio legitimoNegocio = new LegitimoNegocio();

                listaAut = autorizanteNegocio.listar();
                listaCert = certificadoNegocio.listarFiltroAdmin();
                listaRedet = redeterminacionNegocio.listar();

                // Primero agregamos los certificados originales a la lista resultado, pero sin SIGAF aún
                // (lo recalcularemos más tarde para incluir las reliquidaciones)
                foreach (var cert in listaCert)
                {
                    // Crear copia del certificado, pero sin el valor SIGAF
                    var certificadoCopia = new Certificado
                    {
                        Id = cert.Id,
                        Porcentaje = cert.Porcentaje,
                        Autorizante = cert.Autorizante,
                        ExpedientePago = cert.ExpedientePago,
                        Tipo = cert.Tipo,
                        MontoTotal = cert.MontoTotal,
                        // No copiamos el Sigaf porque lo recalcularemos
                        MesAprobacion = cert.MesAprobacion,
                        FechaSade = cert.FechaSade,
                        BuzonSade = cert.BuzonSade,
                        Empresa = cert.Empresa,
                        Estado = cert.Estado
                    };
                    listaCertificadosResultado.Add(certificadoCopia);
                }

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

                                Certificado certificadoRedet = new Certificado
                                {
                                    Autorizante = new Autorizante
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
                                    MontoTotal = montoCertificadoRedet,
                                    MesAprobacion = certificadoOriginal.MesAprobacion,
                                    Tipo = new TipoPago { Id = 2, Nombre = "RELIQUIDACION" },
                                    Empresa = redet.Empresa,
                                    Porcentaje = porcentajeCalculado
                                };

                                // Buscar si existe un expediente en la tabla EXPEDIENTES_RELIQ
                                if (certificadoRedet.MesAprobacion.HasValue)
                                {
                                    try
                                    {
                                        // Verificar si ya hay un expediente asignado en la tabla EXPEDIENTES_RELIQ
                                        var expedienteReliq = expedienteReliqNegocio.ObtenerPorCodigoYMes(
                                            certificadoRedet.Autorizante.CodigoAutorizante,
                                            certificadoRedet.MesAprobacion.Value);

                                        // Si existe, asignar el expediente
                                        if (expedienteReliq != null)
                                        {
                                            certificadoRedet.ExpedientePago = expedienteReliq.Expediente;

                                            // Obtener información SADE
                                            var sadeInfo = SADEHelper.ObtenerInfoSADE(certificadoRedet.ExpedientePago);
                                            certificadoRedet.FechaSade = sadeInfo.FechaUltimoPase ?? null;
                                            certificadoRedet.BuzonSade = sadeInfo.BuzonDestino ?? null;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Capturar errores pero continuar el proceso
                                        System.Diagnostics.Debug.WriteLine($"Error al buscar expediente reliquidación: {ex.Message}");
                                    }
                                }

                                // Determinar el estado del expediente utilizando el helper
                                certificadoRedet.Estado = SIGAFHelper.DeterminarEstadoExpediente(certificadoRedet.ExpedientePago);

                                certificadosRedeterminacion.Add(certificadoRedet);
                            }
                        }

                        // Agregar todos los certificados de redeterminación a la lista de resultados
                        listaCertificadosResultado.AddRange(certificadosRedeterminacion);
                    }
                }


                // Obtener los legítimos abonos directamente de la base de datos
                Dictionary<string, List<decimal>> legitimosPorExpediente = new Dictionary<string, List<decimal>>();
                try
                {
                    // Consultamos directamente la tabla LEGITIMOS_ABONOS
                    var datosLegitimos = new AccesoDatos();
                    datosLegitimos.setearConsulta(@"
            SELECT EXPEDIENTE, CERTIFICADO 
            FROM LEGITIMOS_ABONOS 
            WHERE EXPEDIENTE IS NOT NULL AND EXPEDIENTE <> '' AND CERTIFICADO IS NOT NULL");

                    datosLegitimos.ejecutarLectura();

                    while (datosLegitimos.Lector.Read())
                    {
                        string expediente = datosLegitimos.Lector["EXPEDIENTE"]?.ToString();
                        if (!string.IsNullOrEmpty(expediente) && datosLegitimos.Lector["CERTIFICADO"] != DBNull.Value)
                        {
                            decimal certificado = Convert.ToDecimal(datosLegitimos.Lector["CERTIFICADO"]);

                            if (!legitimosPorExpediente.ContainsKey(expediente))
                                legitimosPorExpediente[expediente] = new List<decimal>();

                            legitimosPorExpediente[expediente].Add(certificado);
                        }
                    }

                    datosLegitimos.cerrarConexion();
                }
                catch (Exception ex)
                {
                    // Si hay error al obtener los legítimos, continuamos solo con los certificados
                    System.Diagnostics.Debug.WriteLine($"Error al obtener legítimos abonos: {ex.Message}");
                }

                // Compartir los certificados resultantes para uso en otros servicios
                DatosCompartidosHelper.SetCertificados(listaCertificadosResultado);

                // Crear diccionario de certificados por expediente para el cálculo final
                var certificadosPorExpediente = listaCertificadosResultado
                    .Where(c => !string.IsNullOrEmpty(c.ExpedientePago))
                    .GroupBy(c => c.ExpedientePago)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Combinar los montos de certificados y legítimos
                Dictionary<string, List<decimal>> montosCombinados = new Dictionary<string, List<decimal>>();

                // Primero agregamos los montos de certificados
                foreach (var kvp in certificadosPorExpediente)
                {
                    string expediente = kvp.Key;
                    var certificados = kvp.Value;

                    montosCombinados[expediente] = certificados.Select(c => c.MontoTotal).ToList();
                }

                // Luego agregamos los montos de legítimos
                foreach (var kvp in legitimosPorExpediente)
                {
                    string expediente = kvp.Key;
                    List<decimal> montos = kvp.Value;

                    if (!montosCombinados.ContainsKey(expediente))
                        montosCombinados[expediente] = new List<decimal>();

                    montosCombinados[expediente].AddRange(montos);
                }

                // Calcular el SIGAF para todos los certificados usando los montos combinados
                foreach (var certificado in listaCertificadosResultado)
                {
                    // Si el certificado tiene un expediente, calcular su SIGAF
                    if (!string.IsNullOrEmpty(certificado.ExpedientePago) &&
                        montosCombinados.TryGetValue(certificado.ExpedientePago, out var montosExpediente))
                    {
                        // Obtener total de devengados
                        decimal totalDevengado = SIGAFHelper.ObtenerTotalImporteDevengados(certificado.ExpedientePago);

                        if (totalDevengado > 0)
                        {
                            // Calcular la suma total de montos para este expediente
                            decimal sumaTotalMontos = montosExpediente.Sum();

                            // Calcular la proporción correspondiente al certificado actual
                            if (sumaTotalMontos > 0)
                            {
                                certificado.Sigaf = totalDevengado * certificado.MontoTotal / sumaTotalMontos;
                            }
                        }
                    }

                    // Reevaluar el estado basado en el SIGAF calculado
                    if (string.IsNullOrEmpty(certificado.ExpedientePago))
                    {
                        certificado.Estado = "NO INICIADO";
                    }
                    else if (certificado.Sigaf.HasValue && certificado.Sigaf > 0)
                    {
                        certificado.Estado = "DEVENGADO";
                    }
                    else
                    {
                        certificado.Estado = "EN TRAMITE";
                    }
                }
                if (usuario != null && usuario.Area != null && usuario.Area.Id > 0)
                {
                    // Filtrar la lista para incluir solo los certificados cuya área coincide con la del usuario
                    listaCertificadosResultado = listaCertificadosResultado.Where(c =>
                        c != null &&
                        c.Autorizante != null &&
                        c.Autorizante.Obra != null &&
                        c.Autorizante.Obra.Area != null &&
                        c.Autorizante.Obra.Area.Id == usuario.Area.Id).ToList();
                }

                return listaCertificadosResultado;
            }


    }
}