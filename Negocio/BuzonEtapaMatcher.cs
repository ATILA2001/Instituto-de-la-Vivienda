using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    /// <summary>
    /// Proporciona lógica para verificar la correspondencia entre etapas y buzones de SADE.
    /// </summary>
    public static class BuzonEtapaMatcher
    {
        // El diccionario se inicializa una sola vez gracias a que es estático.
        private static readonly Dictionary<string, List<string>> _correspondencias = new Dictionary<string, List<string>>
        {
            {"RD-01/11-Subsanacion Empresa", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
            {"RD-02/11-Análisis Tecnica", new List<string>{
                "IVC-33000 DG GESTION DE INTERVENCION SOCIAL",
                "IVC-2600 GO PLANIFICACION Y CONTROL",
                "IVC-3300 GO PLANEAMIENTO Y EVALUACION DE OBRAS",
                "IVC-3400 GO INSPECCION Y AUDITORIA DE OBRAS",
                "IVC-3000 DG DESARROLLO DE OBRAS",
                "IVC-3500 GO PROYECTOS URBANOS E INFRAESTRUCTURA",
                "IVC-2000 DG DESARROLLO HABITACIONAL",
                "IVC-2240 DEPTO OBRAS",
                "IVC-3430 DEPTO OBRAS1",
                "IVC-2400 GO CONJUNTOS URBANOS"
            }},
            {"RD-03/11-Análisis DGAyF", new List<string>{
                "IVC-4010 DEPTO REDETERMINACIONES",
                "IVC-4100 GO ECONOMICA FINANCIERA",
                "IVC-4000 DG ADMINISTRACION Y FINANZAS"
            }},
            {"RD-04/11-Dgtal-Dictamen", new List<string>{
                "IVC-5200 GO ASUNTOS LEGALES",
                "IVC-1400 DG TECNICO ADMINISTRATIVA Y LEGAL",
                "IVC-5120 DEPTO ASUNTOS JUDICIALES"
            }},
            {"RD-04a/11-Tecnica Rta Dictamen", new List<string>{"IVC-1000 GERENCIA GENERAL"}},
            {"RD-05/11-Firma AA Empresa", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
            {"RD-06/11Firma AA cd", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
            {"RD-07/11-Aprobacion Anterior", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
            {"RD-08/11-Reser. Presup", new List<string>{
                "IVC-4120 DEPTO PRESUPUESTOS",
                "IVC-12400 GO LOGISTICA"
            }},
            {"RD-09/11-Dgtal-Dispo", new List<string>{
                "IVC-1850 DEPTO DESPACHOS Y PROYECTOS",
                "IVC-1400 DG TECNICO ADMINISTRATIVA Y LEGAL"
            }},
            {"RD-10/11-Notificar a la empresa", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
            {"RD-11/11-Notificada", new List<string>{
                "IVC-4010 DEPTO REDETERMINACIONES",
                "IVC-1860 DEPTO REGISTRO Y GUARDA DE DOCUMENTACION",
                "MGEYA-D.G.MESA DE ENTRADAS SALIDAS Y ARCHIVO"
            }},
            {"Rectificacion", new List<string>{"IVC-3160 DEPTO PROYECTOS"}},
            {"RO-01/11-Creacion Expediente", new List<string>{"IVC-2320 DEPTO ORGANIZACION Y CAPACITACION CONSORCIAL"}},
            {"RO-02/11-Análisis Tecnica", new List<string>{
                "IVC-2600 GO PLANIFICACION Y CONTROL",
                "IVC-3300 GO PLANEAMIENTO Y EVALUACION DE OBRAS",
                "IVC-3400 GO INSPECCION Y AUDITORIA DE OBRAS",
                "IVC-3000 DG DESARROLLO DE OBRAS",
                "IVC-3500 GO PROYECTOS URBANOS E INFRAESTRUCTURA",
                "IVC-2000 DG DESARROLLO HABITACIONAL",
                "IVC-2240 DEPTO OBRAS",
                "IVC-3430 DEPTO OBRAS1",
                "IVC-2400 GO CONJUNTOS URBANOS"
            }},
            {"RO-03/11-Análisis DGAyF", new List<string>{
                "IVC-4010 DEPTO REDETERMINACIONES",
                "IVC-4100 GO ECONOMICA FINANCIERA",
                "IVC-4000 DG ADMINISTRACION Y FINANZAS"
            }},
            {"RO-04/11-Dgtal-Dictamen", new List<string>{
                "IVC-5200 GO ASUNTOS LEGALES",
                "IVC-1400 DG TECNICO ADMINISTRATIVA Y LEGAL",
                "IVC-1000 GERENCIA GENERAL",
                "IVC-5120 DEPTO ASUNTOS JUDICIALES"
            }},
            {"RO-05/11-Firma AA Empresa", new List<string>{"HRR-HOSP.DE REHABILITACION MANUEL ROCCA"}},
            {"RO-06/11Firma AA cd", new List<string>{"IVC-6000 DG CREDITOS Y ALQUILERES"}},
            {"RO-07/11-Aprobacion Anterior", new List<string>{"IVC-02 DESPACHO (AAG)"}},
            {"RO-08/11-Reser. Presup", new List<string>{
                "IVC-4120 DEPTO PRESUPUESTOS",
                "IVC-12400 GO LOGISTICA"
            }},
            {"RO-09/11-Dgtal-Dispo", new List<string>{
                "IVC-1850 DEPTO DESPACHOS Y PROYECTOS",
                "IVC-1400 DG TECNICO ADMINISTRATIVA Y LEGAL"
            }},
            {"RO-10/11-Notificar a la empresa", new List<string>{"IVC-6230 DEPTO SOCIAL Y ADMINISTRATIVO"}},
            {"RO-11/11-Notificada", new List<string>{
                "IVC-9510 DEPTO TECNOLOGIA",
                "IVC-1860 DEPTO REGISTRO Y GUARDA DE DOCUMENTACION",
                "MGEYA-D.G.MESA DE ENTRADAS SALIDAS Y ARCHIVO"
            }},
            {"RP-01/09-Subsanacion Empresa", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
            {"RP-02/09-Análisis Tecnica", new List<string>{
                "IVC-2600 GO PLANIFICACION Y CONTROL",
                "IVC-3300 GO PLANEAMIENTO Y EVALUACION DE OBRAS",
                "IVC-3400 GO INSPECCION Y AUDITORIA DE OBRAS",
                "IVC-3000 DG DESARROLLO DE OBRAS",
                "IVC-3500 GO PROYECTOS URBANOS E INFRAESTRUCTURA",
                "IVC-2000 DG DESARROLLO HABITACIONAL",
                "IVC-2240 DEPTO OBRAS",
                "IVC-3430 DEPTO OBRAS1",
                "IVC-2400 GO CONJUNTOS URBANOS"
            }},
            {"RP-03/09-Análisis DGAyF", new List<string>{
                "IVC-4010 DEPTO REDETERMINACIONES",
                "IVC-4100 GO ECONOMICA FINANCIERA",
                "IVC-4000 DG ADMINISTRACION Y FINANZAS"
            }},
            {"RP-04/09-Dgtal-Dictamen", new List<string>{
                "IVC-5200 GO ASUNTOS LEGALES",
                "IVC-1400 DG TECNICO ADMINISTRATIVA Y LEGAL",
                "IVC-5120 DEPTO ASUNTOS JUDICIALES"
            }},
            {"RP-04a/09-Tecnica Rta Dictamen", new List<string>{"IVC-1000 GERENCIA GENERAL"}},
            {"RP-05/09-Aprobacion Anterior", new List<string>{"IVC-8320 DEPTO REGULARIZACION DOMINIAL"}},
            {"RP-06/09-Reser. Presup", new List<string>{
                "IVC-4120 DEPTO PRESUPUESTOS",
                "IVC-12400 GO LOGISTICA"
            }},
            {"RP-07/09-Dgtal-Dispo", new List<string>{
                "IVC-1850 DEPTO DESPACHOS Y PROYECTOS",
                "IVC-1400 DG TECNICO ADMINISTRATIVA Y LEGAL"
            }},
            {"RP-08/09-Notificar a la empresa", new List<string>{"IVC-ALS_MIGRADO_EX_UGIS ADMINISTRADOR LOCAL SUME"}},
            {"RP-09/09-Notificada", new List<string>{
                "IVC-1260 DEPTO GESTION DE ALQUILERES",
                "IVC-1860 DEPTO REGISTRO Y GUARDA DE DOCUMENTACION",
                "MGEYA-D.G.MESA DE ENTRADAS SALIDAS Y ARCHIVO"
            }}
        };

        /// <summary>
        /// Verifica si un buzón de SADE no corresponde a la etapa actual.
        /// </summary>
        /// <param name="etapaNombre">El nombre de la etapa.</param>
        /// <param name="buzonSade">El nombre del buzón de SADE.</param>
        /// <returns>True si hay una discrepancia (mismatch); de lo contrario, false.</returns>
        public static bool IsMismatch(string etapaNombre, string buzonSade)
        {
            if (string.IsNullOrEmpty(etapaNombre) || string.IsNullOrEmpty(buzonSade))
            {
                return false;
            }

            if (_correspondencias.ContainsKey(etapaNombre))
            {
                // Retorna true si NINGUNO de los buzones permitidos está contenido en el buzonSade.
                bool coincide = _correspondencias[etapaNombre].Any(b => buzonSade.Contains(b));
                return !coincide; // Hay mismatch si no coincide.
            }

            // Si la etapa no está en el diccionario, se asume que no hay mismatch.
            return false;
        }
    }
}
