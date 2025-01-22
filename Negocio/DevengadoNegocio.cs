using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class DevengadoNegocio
    {
        public List<Devengado> ListarDevengados()
        {
            var lista = new List<Devengado>();
            var datos = new AccesoDatos();

            try
            {
                string query = @"
            SELECT ID, EJERCICIO, TIPO_DEV, NUMERO_DEVENGADO, ESTADO_FIRMA, EXPEDIENTE, ENTE, CUIT,
                   DESCRIPCION, OBRA, IMPORTE_DEVENGADO, JURISDICCION, SUB_JURISDICCION, ENTIDAD,
                   PROGRAMA, SUB_PROGRAMA, PROYECTO, ACTIVIDAD, OBRA2, INCISO, PRINCIPAL, PARCIAL,
                   SUB_PARCIAL, FUENTE_FINANCIERA, UBICACION_GEO, CUENTA_ESCRITURAL, CUENTA_PAGADORA,
                   IMPORTE_PP, IMPORTE_PAGADO, SALDO_A_PAGAR_PP, TOTAL_DEVENGADO, TOTAL_PAGADO,
                   DEUDA_DEV, FECHA_IMPUTACION, EE_FINANCIERA, EE_AUTORIZANTE, MONTO_DEVENGADO,CONCATENADO_SF,TIPO
            FROM DEVENGADOS
            WHERE 1 = 1";

                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    var devengado = new Devengado
                    {
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        Ejercicio = datos.Lector["EJERCICIO"] != DBNull.Value ? (int?)datos.Lector["EJERCICIO"] : null,
                        TipoDev = datos.Lector["TIPO_DEV"]?.ToString(),
                        NumeroDevengado = datos.Lector["NUMERO_DEVENGADO"] != DBNull.Value ? Convert.ToInt32(datos.Lector["NUMERO_DEVENGADO"]) : 0,
                        EstadoFirma = datos.Lector["ESTADO_FIRMA"]?.ToString(),
                        Expediente = datos.Lector["EXPEDIENTE"]?.ToString(),
                        Ente = datos.Lector["ENTE"] != DBNull.Value ? (int?)datos.Lector["ENTE"] : null,
                        Cuit = datos.Lector["CUIT"] != DBNull.Value ? (long?)datos.Lector["CUIT"] : null,
                        Descripcion = datos.Lector["DESCRIPCION"]?.ToString(),
                        Obra = datos.Lector["OBRA"] != DBNull.Value ? (int?)datos.Lector["OBRA"] : null,
                        ImporteDevengado = datos.Lector["IMPORTE_DEVENGADO"] != DBNull.Value ? (decimal?)datos.Lector["IMPORTE_DEVENGADO"] : null,
                        Jurisdiccion = datos.Lector["JURISDICCION"] != DBNull.Value ? (int?)datos.Lector["JURISDICCION"] : null,
                        SubJurisdiccion = datos.Lector["SUB_JURISDICCION"] != DBNull.Value ? (int?)datos.Lector["SUB_JURISDICCION"] : null,
                        Entidad = datos.Lector["ENTIDAD"] != DBNull.Value ? (int?)datos.Lector["ENTIDAD"] : null,
                        Programa = datos.Lector["PROGRAMA"] != DBNull.Value ? (int?)datos.Lector["PROGRAMA"] : null,
                        SubPrograma = datos.Lector["SUB_PROGRAMA"] != DBNull.Value ? (int?)datos.Lector["SUB_PROGRAMA"] : null,
                        Proyecto = datos.Lector["PROYECTO"] != DBNull.Value ? (int?)datos.Lector["PROYECTO"] : null,
                        Actividad = datos.Lector["ACTIVIDAD"] != DBNull.Value ? (int?)datos.Lector["ACTIVIDAD"] : null,
                        Obra2 = datos.Lector["OBRA2"] != DBNull.Value ? (int?)datos.Lector["OBRA2"] : null,
                        Inciso = datos.Lector["INCISO"] != DBNull.Value ? (int?)datos.Lector["INCISO"] : null,
                        Principal = datos.Lector["PRINCIPAL"] != DBNull.Value ? (int?)datos.Lector["PRINCIPAL"] : null,
                        Parcial = datos.Lector["PARCIAL"] != DBNull.Value ? (int?)datos.Lector["PARCIAL"] : null,
                        SubParcial = datos.Lector["SUB_PARCIAL"] != DBNull.Value ? (int?)datos.Lector["SUB_PARCIAL"] : null,
                        FuenteFinanciera = datos.Lector["FUENTE_FINANCIERA"] != DBNull.Value ? (int?)datos.Lector["FUENTE_FINANCIERA"] : null,
                        UbicacionGeo = datos.Lector["UBICACION_GEO"] != DBNull.Value ? (int?)datos.Lector["UBICACION_GEO"] : null,
                        CuentaEscritural = datos.Lector["CUENTA_ESCRITURAL"] != DBNull.Value ? (int?)datos.Lector["CUENTA_ESCRITURAL"] : null,
                        CuentaPagadora = datos.Lector["CUENTA_PAGADORA"]?.ToString(),
                        ImportePP = datos.Lector["IMPORTE_PP"] != DBNull.Value ? (decimal?)datos.Lector["IMPORTE_PP"] : null,
                        ImportePagado = datos.Lector["IMPORTE_PAGADO"] != DBNull.Value ? (decimal?)datos.Lector["IMPORTE_PAGADO"] : null,
                        SaldoAPagarPP = datos.Lector["SALDO_A_PAGAR_PP"] != DBNull.Value ? (decimal?)datos.Lector["SALDO_A_PAGAR_PP"] : null,
                        TotalDevengado = datos.Lector["TOTAL_DEVENGADO"] != DBNull.Value ? (decimal?)datos.Lector["TOTAL_DEVENGADO"] : null,
                        TotalPagado = datos.Lector["TOTAL_PAGADO"] != DBNull.Value ? (decimal?)datos.Lector["TOTAL_PAGADO"] : null,
                        DeudaDev = datos.Lector["DEUDA_DEV"] != DBNull.Value ? (decimal?)datos.Lector["DEUDA_DEV"] : null,
                        FechaImputacion = datos.Lector["FECHA_IMPUTACION"] != DBNull.Value ? (DateTime?)datos.Lector["FECHA_IMPUTACION"] : null,
                        EeFinanciera = datos.Lector["EE_FINANCIERA"]?.ToString(),
                        EeAutorizante = datos.Lector["EE_AUTORIZANTE"]?.ToString(),
                        MontoDevengado = datos.Lector["MONTO_DEVENGADO"] != DBNull.Value ? (decimal?)datos.Lector["MONTO_DEVENGADO"] : null,
                        Concatenado = datos.Lector["CONCATENADO_SF"]?.ToString(),
                        Tipo = datos.Lector["TIPO"]?.ToString()
                    };

                    lista.Add(devengado);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar devengados.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

    }
}
