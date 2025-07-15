using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class FormulacionNegocio
    {
        public void agregar(Formulacion formulacion)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string consulta = "INSERT INTO FORMULACION (ID_BASE, MONTO_26, MONTO_27, MONTO_28, PPI, ID_UNIDAD_MEDIDA, VALOR_MEDIDA, TECHOS, MES_BASE,PRIORIDAD,OBSERVACIONES) " +
                                 "VALUES (@IdBase, @Monto26, @Monto27, @Monto28, @Ppi, @IdUnidadMedida, @ValorMedida,@Techos,@Mes,@Prioridad,@observaciones)";

                datos.setearConsulta(consulta);
                datos.setearParametros("@IdBase", formulacion.Obra.Id);
                datos.setearParametros("@Monto26", formulacion.Monto_26);
                datos.setearParametros("@Monto27", formulacion.Monto_27);
                datos.setearParametros("@Monto28", formulacion.Monto_28);

                if (formulacion.Ppi != null)
                    datos.setearParametros("@Ppi", formulacion.Ppi);
                else
                    datos.setearParametros("@Ppi", DBNull.Value);

                if (formulacion.Techos2026 != null)
                    datos.setearParametros("@Techos", formulacion.Techos2026);
                else
                    datos.setearParametros("@Techos", DBNull.Value);

                if (formulacion.MesBase != null)
                    datos.setearParametros("@Mes", formulacion.MesBase);
                else
                    datos.setearParametros("@Mes", DBNull.Value);

                if (formulacion.UnidadMedida != null)
                    datos.setearParametros("@IdUnidadMedida", formulacion.UnidadMedida.Id);
                else
                    datos.setearParametros("@IdUnidadMedida", DBNull.Value);

                datos.setearParametros("@ValorMedida", formulacion.ValorMedida);
                if (formulacion.Prioridad != null)
                    datos.setearParametros("@Prioridad", formulacion.Prioridad.Id);
                else
                    datos.setearParametros("@Prioridad", DBNull.Value);
                if (formulacion.Observacion != null)
                    datos.setearParametros("@observaciones", formulacion.Observacion);
                else
                    datos.setearParametros("@observaciones", DBNull.Value);


                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void agregarUser(Usuario usuario, Formulacion formulacion)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string consulta = "INSERT INTO FORMULACION (ID_BASE, MONTO_26, MONTO_27, MONTO_28,  ID_UNIDAD_MEDIDA, VALOR_MEDIDA,  MES_BASE,PRIORIDAD,OBSERVACIONES) " +
                                 "VALUES (@IdBase, @Monto26, @Monto27, @Monto28,  @IdUnidadMedida, @ValorMedida,@Mes,@Prioridad,@observaciones)";

                datos.setearConsulta(consulta);
                datos.setearParametros("@IdBase", formulacion.Obra.Id);
                datos.setearParametros("@Monto26", formulacion.Monto_26);
                datos.setearParametros("@Monto27", formulacion.Monto_27);
                datos.setearParametros("@Monto28", formulacion.Monto_28);
                if (formulacion.MesBase != null)
                    datos.setearParametros("@Mes", formulacion.MesBase);
                else
                    datos.setearParametros("@Mes", DBNull.Value);

                if (formulacion.UnidadMedida != null)
                    datos.setearParametros("@IdUnidadMedida", formulacion.UnidadMedida.Id);
                else
                    datos.setearParametros("@IdUnidadMedida", DBNull.Value);

                datos.setearParametros("@ValorMedida", formulacion.ValorMedida);
                if (formulacion.Prioridad != null)
                    datos.setearParametros("@Prioridad", formulacion.Prioridad.Id);
                else
                    datos.setearParametros("@Prioridad", DBNull.Value);
                if (formulacion.Observacion != null)
                    datos.setearParametros("@observaciones", formulacion.Observacion);
                else
                    datos.setearParametros("@observaciones", DBNull.Value);


                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void modificar(Formulacion formulacion)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string consulta = "UPDATE FORMULACION SET MONTO_26 = @Monto26, " +
                                 "MONTO_27 = @Monto27, MONTO_28 = @Monto28, PPI = @Ppi, " +
                                 "ID_UNIDAD_MEDIDA = @IdUnidadMedida, VALOR_MEDIDA = @ValorMedida, TECHOS = @Techos, MES_BASE = @mes,OBSERVACIONES = @observacion, PRIORIDAD = @prioridad " +
                                 "WHERE ID = @Id";

                datos.setearConsulta(consulta);
                datos.setearParametros("@Id", formulacion.Id);
                datos.setearParametros("@Monto26", formulacion.Monto_26);
                datos.setearParametros("@Monto27", formulacion.Monto_27);
                datos.setearParametros("@Monto28", formulacion.Monto_28);

                if (formulacion.Ppi != null)
                    datos.setearParametros("@Ppi", formulacion.Ppi);
                else
                    datos.setearParametros("@Ppi", DBNull.Value);

                if (formulacion.UnidadMedida != null)
                    datos.setearParametros("@IdUnidadMedida", formulacion.UnidadMedida.Id);
                else
                    datos.setearParametros("@IdUnidadMedida", DBNull.Value);

                if (formulacion.Techos2026 != null)
                    datos.setearParametros("@Techos", formulacion.Techos2026);
                else
                    datos.setearParametros("@Techos", DBNull.Value);

                if (formulacion.MesBase != null)
                    datos.setearParametros("@mes", formulacion.MesBase);
                else
                    datos.setearParametros("@mes", DBNull.Value);

                datos.setearParametros("@ValorMedida", formulacion.ValorMedida);

                if (formulacion.Prioridad != null)
                    datos.setearParametros("@prioridad", formulacion.Prioridad.Id);
                else
                    datos.setearParametros("@prioridad", DBNull.Value);

                if (formulacion.Observacion != null)
                    datos.setearParametros("@observacion", formulacion.Observacion);
                else
                    datos.setearParametros("@observacion", DBNull.Value);

                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public void modificarUser(Formulacion formulacion)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string consulta = "UPDATE FORMULACION SET MONTO_26 = @Monto26, " +
                                 "MONTO_27 = @Monto27, MONTO_28 = @Monto28,  " +
                                 "ID_UNIDAD_MEDIDA = @IdUnidadMedida, VALOR_MEDIDA = @ValorMedida,  MES_BASE = @mes,OBSERVACIONES = @observacion, PRIORIDAD = @prioridad " +
                                 "WHERE ID = @Id";

                datos.setearConsulta(consulta);
                datos.setearParametros("@Id", formulacion.Id);
                datos.setearParametros("@Monto26", formulacion.Monto_26);
                datos.setearParametros("@Monto27", formulacion.Monto_27);
                datos.setearParametros("@Monto28", formulacion.Monto_28);

                if (formulacion.UnidadMedida != null)
                    datos.setearParametros("@IdUnidadMedida", formulacion.UnidadMedida.Id);
                else
                    datos.setearParametros("@IdUnidadMedida", DBNull.Value);
              
                if (formulacion.MesBase != null)
                    datos.setearParametros("@mes", formulacion.MesBase);
                else
                    datos.setearParametros("@mes", DBNull.Value);

                datos.setearParametros("@ValorMedida", formulacion.ValorMedida);

                if (formulacion.Prioridad != null)
                    datos.setearParametros("@prioridad", formulacion.Prioridad.Id);
                else
                    datos.setearParametros("@prioridad", DBNull.Value);

                if (formulacion.Observacion != null)
                    datos.setearParametros("@observacion", formulacion.Observacion);
                else
                    datos.setearParametros("@observacion", DBNull.Value);

                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public bool eliminar(int id)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM FORMULACION WHERE ID = @ID");
                datos.setearParametros("@ID", id);
                datos.ejecutarAccion();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public List<Formulacion> listar(Usuario usuario)
        {
            List<Formulacion> lista = new List<Formulacion>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = @"SELECT 
                                F.ID, F.ID_BASE, F.MONTO_26, F.MONTO_27, F.MONTO_28, F.PPI, 
                                F.ID_UNIDAD_MEDIDA, F.VALOR_MEDIDA, 
                                UM.NOMBRE AS NOMBRE_UNIDAD,
                                O.ID AS OBRA_ID, 
                                A.NOMBRE AS AREA,
                                A.ID AS AREA_ID,
                                E.NOMBRE AS EMPRESA,
                                E.ID AS EMPRESA_ID,
                                O.NUMERO, 
CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA,
                                C.ID AS CONTRATA_ID, 
                                O.AÑO,
                                O.ETAPA,
                                O.OBRA AS OBRA_NUMERO,
                                B.NOMBRE AS BARRIO,
                                B.ID AS BARRIO_ID,
                                O.DESCRIPCION,
                                LG.NOMBRE AS LINEA_GESTION,
                                LG.ID AS LINEA_GESTION_ID,
                                BD.PROYECTO AS PROYECTO,
                                BD.ID AS PROYECTO_ID,
 (
                    SELECT ISNULL(SUM(CERT.MONTO_TOTAL), 0) 
                    FROM CERTIFICADOS CERT 
                    INNER JOIN AUTORIZANTES AUT ON CERT.CODIGO_AUTORIZANTE = AUT.CODIGO_AUTORIZANTE
                    WHERE AUT.OBRA = O.ID 
                    AND YEAR(CERT.MES_APROBACION) IN (2026, 2027, 2028)
                ) + 
                (
                    SELECT ISNULL(SUM(LA.CERTIFICADO), 0) 
                    FROM LEGITIMOS_ABONOS LA 
                    WHERE LA.Obra = O.ID 
                    AND YEAR(LA.MES_APROBACION) IN (2026, 2027, 2028)
                ) AS PLURIANUAL,
                F.TECHOS,
                F.MES_BASE AS MES_BASE,
                F.OBSERVACIONES AS OBSERVACIONES,
                P.NOMBRE AS PRIORIDAD,
                P.ID AS ID_PRIORIDAD
                                FROM FORMULACION F
                                LEFT JOIN UNIDADES_MEDIDA UM ON F.ID_UNIDAD_MEDIDA = UM.ID
                                INNER JOIN OBRAS O ON F.ID_BASE = O.ID
                                INNER JOIN EMPRESAS E ON O.EMPRESA = E.ID
                                INNER JOIN AREAS A ON O.AREA = A.ID
                                INNER JOIN CONTRATA C ON O.CONTRATA = C.ID
                                INNER JOIN BARRIOS B ON O.BARRIO = B.ID
                                LEFT JOIN BD_PROYECTOS BD ON O.ID = BD.ID_BASE
                                LEFT JOIN LINEA_DE_GESTION LG ON BD.LINEA_DE_GESTION = LG.ID
                                LEFT JOIN PRIORIDADES P ON F.PRIORIDAD = P.ID
                                WHERE O.AREA = @area";



                query += " ORDER BY F.ID";



                datos.setearConsulta(query);

                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();


                while (datos.Lector.Read())
                {
                    Formulacion aux = new Formulacion();
                    aux.Id = (int)datos.Lector["ID"];

                    // Cargar datos de Obra
                    aux.Obra = new Obra
                    {
                        Id = (int)datos.Lector["OBRA_ID"],
                        Area = new Area
                        {
                            Id = datos.Lector["AREA_ID"] != DBNull.Value ? (int)datos.Lector["AREA_ID"] : 0,
                            Nombre = datos.Lector["AREA"] != DBNull.Value ? (string)datos.Lector["AREA"] : string.Empty
                        },
                        Empresa = new Empresa
                        {
                            Id = datos.Lector["EMPRESA_ID"] != DBNull.Value ? (int)datos.Lector["EMPRESA_ID"] : 0,
                            Nombre = datos.Lector["EMPRESA"] != DBNull.Value ? (string)datos.Lector["EMPRESA"] : string.Empty
                        },
                        Numero = datos.Lector["NUMERO"] != DBNull.Value ? (int?)datos.Lector["NUMERO"] : null,
                        Contrata = new Contrata
                        {
                            Id = datos.Lector["CONTRATA_ID"] != DBNull.Value ? (int)datos.Lector["CONTRATA_ID"] : 0,
                            Nombre = datos.Lector["CONTRATA"] != DBNull.Value ? (string)datos.Lector["CONTRATA"] : string.Empty
                        },
                        Año = datos.Lector["AÑO"] != DBNull.Value ? (int?)datos.Lector["AÑO"] : null,
                        Etapa = datos.Lector["ETAPA"] != DBNull.Value ? (int?)datos.Lector["ETAPA"] : null,
                        ObraNumero = datos.Lector["OBRA_NUMERO"] != DBNull.Value ? (int?)datos.Lector["OBRA_NUMERO"] : null,
                        Barrio = new Barrio
                        {
                            Id = datos.Lector["BARRIO_ID"] != DBNull.Value ? (int)datos.Lector["BARRIO_ID"] : 0,
                            Nombre = datos.Lector["BARRIO"] != DBNull.Value ? (string)datos.Lector["BARRIO"] : string.Empty
                        },
                        Descripcion = datos.Lector["DESCRIPCION"] != DBNull.Value ? (string)datos.Lector["DESCRIPCION"] : string.Empty,
                        LineaGestion = new LineaGestion
                        {
                            Id = datos.Lector["LINEA_GESTION_ID"] != DBNull.Value ? (int)datos.Lector["LINEA_GESTION_ID"] : 0,
                            Nombre = datos.Lector["LINEA_GESTION"] != DBNull.Value ? (string)datos.Lector["LINEA_GESTION"] : string.Empty
                        },
                        Proyecto = new BdProyecto
                        {
                            Id = datos.Lector["PROYECTO_ID"] != DBNull.Value ? (int)datos.Lector["PROYECTO_ID"] : 0,
                            Proyecto = datos.Lector["PROYECTO"] != DBNull.Value ? (string)datos.Lector["PROYECTO"] : string.Empty
                        }
                    };

                    // Cargar datos de Formulacion
                    aux.Monto_26 = datos.Lector["MONTO_26"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["MONTO_26"]) : 0;

                    aux.Monto_27 = datos.Lector["MONTO_27"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["MONTO_27"]) : 0;

                    aux.Monto_28 = datos.Lector["MONTO_28"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["MONTO_28"]) : 0;

                    aux.Ppi = datos.Lector["PPI"] != DBNull.Value ?
                        Convert.ToInt32(datos.Lector["PPI"]) : 0;
                    aux.Plurianual = datos.Lector["PLURIANUAL"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["PLURIANUAL"]) : 0;
                    aux.Techos2026 = datos.Lector["TECHOS"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["TECHOS"]) : 0;
                    aux.MesBase = datos.Lector["MES_BASE"] != DBNull.Value ?
                        Convert.ToDateTime(datos.Lector["MES_BASE"]) : (DateTime?)null;
                    aux.Observacion = datos.Lector["OBSERVACIONES"] != DBNull.Value ?
                        (string)datos.Lector["OBSERVACIONES"] : string.Empty;
                    aux.Prioridad = new Prioridad
                    {
                        Id = datos.Lector["ID_PRIORIDAD"] != DBNull.Value ? (int)datos.Lector["ID_PRIORIDAD"] : 0,
                        Nombre = datos.Lector["PRIORIDAD"] != DBNull.Value ? (string)datos.Lector["PRIORIDAD"] : string.Empty
                    };

                    if (datos.Lector["ID_UNIDAD_MEDIDA"] != DBNull.Value)
                    {
                        aux.UnidadMedida = new UnidadMedida
                        {
                            Id = (int)datos.Lector["ID_UNIDAD_MEDIDA"],
                            Nombre = datos.Lector["NOMBRE_UNIDAD"] != DBNull.Value ?
                                (string)datos.Lector["NOMBRE_UNIDAD"] : string.Empty
                        };
                    }

                    aux.ValorMedida = datos.Lector["VALOR_MEDIDA"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["VALOR_MEDIDA"]) : 0;

                    lista.Add(aux);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public List<Formulacion> listar()
        {
            List<Formulacion> lista = new List<Formulacion>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = @"SELECT 
                                F.ID, F.ID_BASE, F.MONTO_26, F.MONTO_27, F.MONTO_28, F.PPI, 
                                F.ID_UNIDAD_MEDIDA, F.VALOR_MEDIDA, 
                                UM.NOMBRE AS NOMBRE_UNIDAD,
                                O.ID AS OBRA_ID, 
                                A.NOMBRE AS AREA,
                                A.ID AS AREA_ID,
                                E.NOMBRE AS EMPRESA,
                                E.ID AS EMPRESA_ID,
                                O.NUMERO,
                                CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA,
                                C.ID AS CONTRATA_ID, 
                                O.AÑO,
                                O.ETAPA,
                                O.OBRA AS OBRA_NUMERO,
                                B.NOMBRE AS BARRIO,
                                B.ID AS BARRIO_ID,
                                O.DESCRIPCION,
                                LG.NOMBRE AS LINEA_GESTION,
                                LG.ID AS LINEA_GESTION_ID,
                                BD.PROYECTO AS PROYECTO,
                                BD.ID AS PROYECTO_ID,
 (
                    SELECT ISNULL(SUM(CERT.MONTO_TOTAL), 0) 
                    FROM CERTIFICADOS CERT 
                    INNER JOIN AUTORIZANTES AUT ON CERT.CODIGO_AUTORIZANTE = AUT.CODIGO_AUTORIZANTE
                    WHERE AUT.OBRA = O.ID 
                    AND YEAR(CERT.MES_APROBACION) IN (2026, 2027, 2028)
                ) + 
                (
                    SELECT ISNULL(SUM(LA.CERTIFICADO), 0) 
                    FROM LEGITIMOS_ABONOS LA 
                    WHERE LA.Obra = O.ID 
                    AND YEAR(LA.MES_APROBACION) IN (2026, 2027, 2028)
                ) AS PLURIANUAL,
                F.TECHOS,
                F.MES_BASE AS MES_BASE,
                F.OBSERVACIONES AS OBSERVACIONES,
                P.NOMBRE AS PRIORIDAD,
                P.ID AS ID_PRIORIDAD
                                FROM FORMULACION F
                                LEFT JOIN UNIDADES_MEDIDA UM ON F.ID_UNIDAD_MEDIDA = UM.ID
                                INNER JOIN OBRAS O ON F.ID_BASE = O.ID
                                INNER JOIN EMPRESAS E ON O.EMPRESA = E.ID
                                INNER JOIN AREAS A ON O.AREA = A.ID
                                INNER JOIN CONTRATA C ON O.CONTRATA = C.ID
                                INNER JOIN BARRIOS B ON O.BARRIO = B.ID
                                LEFT JOIN BD_PROYECTOS BD ON O.ID = BD.ID_BASE
                                LEFT JOIN LINEA_DE_GESTION LG ON BD.LINEA_DE_GESTION = LG.ID
                                LEFT JOIN PRIORIDADES P ON F.PRIORIDAD = P.ID
                                WHERE 1=1";



                query += " ORDER BY F.ID";
                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Formulacion aux = new Formulacion();
                    aux.Id = (int)datos.Lector["ID"];

                    // Cargar datos de Obra
                    aux.Obra = new Obra
                    {
                        Id = (int)datos.Lector["OBRA_ID"],
                        Area = new Area
                        {
                            Id = datos.Lector["AREA_ID"] != DBNull.Value ? (int)datos.Lector["AREA_ID"] : 0,
                            Nombre = datos.Lector["AREA"] != DBNull.Value ? (string)datos.Lector["AREA"] : string.Empty
                        },
                        Empresa = new Empresa
                        {
                            Id = datos.Lector["EMPRESA_ID"] != DBNull.Value ? (int)datos.Lector["EMPRESA_ID"] : 0,
                            Nombre = datos.Lector["EMPRESA"] != DBNull.Value ? (string)datos.Lector["EMPRESA"] : string.Empty
                        },
                        Numero = datos.Lector["NUMERO"] != DBNull.Value ? (int?)datos.Lector["NUMERO"] : null,
                        Contrata = new Contrata
                        {
                            Id = datos.Lector["CONTRATA_ID"] != DBNull.Value ? (int)datos.Lector["CONTRATA_ID"] : 0,
                            Nombre = datos.Lector["CONTRATA"] != DBNull.Value ? (string)datos.Lector["CONTRATA"] : string.Empty
                        },
                        Año = datos.Lector["AÑO"] != DBNull.Value ? (int?)datos.Lector["AÑO"] : null,
                        Etapa = datos.Lector["ETAPA"] != DBNull.Value ? (int?)datos.Lector["ETAPA"] : null,
                        ObraNumero = datos.Lector["OBRA_NUMERO"] != DBNull.Value ? (int?)datos.Lector["OBRA_NUMERO"] : null,
                        Barrio = new Barrio
                        {
                            Id = datos.Lector["BARRIO_ID"] != DBNull.Value ? (int)datos.Lector["BARRIO_ID"] : 0,
                            Nombre = datos.Lector["BARRIO"] != DBNull.Value ? (string)datos.Lector["BARRIO"] : string.Empty
                        },
                        Descripcion = datos.Lector["DESCRIPCION"] != DBNull.Value ? (string)datos.Lector["DESCRIPCION"] : string.Empty,
                        LineaGestion = new LineaGestion
                        {
                            Id = datos.Lector["LINEA_GESTION_ID"] != DBNull.Value ? (int)datos.Lector["LINEA_GESTION_ID"] : 0,
                            Nombre = datos.Lector["LINEA_GESTION"] != DBNull.Value ? (string)datos.Lector["LINEA_GESTION"] : string.Empty
                        },
                        Proyecto = new BdProyecto
                        {
                            Id = datos.Lector["PROYECTO_ID"] != DBNull.Value ? (int)datos.Lector["PROYECTO_ID"] : 0,
                            Proyecto = datos.Lector["PROYECTO"] != DBNull.Value ? (string)datos.Lector["PROYECTO"] : string.Empty
                        }
                    };

                    // Cargar datos de Formulacion
                    aux.Monto_26 = datos.Lector["MONTO_26"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["MONTO_26"]) : 0;

                    aux.Monto_27 = datos.Lector["MONTO_27"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["MONTO_27"]) : 0;

                    aux.Monto_28 = datos.Lector["MONTO_28"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["MONTO_28"]) : 0;

                    aux.Ppi = datos.Lector["PPI"] != DBNull.Value ?
                        Convert.ToInt32(datos.Lector["PPI"]) : 0;
                    aux.Plurianual = datos.Lector["PLURIANUAL"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["PLURIANUAL"]) : 0;
                    aux.Techos2026 = datos.Lector["TECHOS"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["TECHOS"]) : 0;
                    aux.MesBase = datos.Lector["MES_BASE"] != DBNull.Value ?
                        Convert.ToDateTime(datos.Lector["MES_BASE"]) : (DateTime?)null;
                    aux.Observacion = datos.Lector["OBSERVACIONES"] != DBNull.Value ?
                        (string)datos.Lector["OBSERVACIONES"] : string.Empty;
                    aux.Prioridad = new Prioridad
                    {
                        Id = datos.Lector["ID_PRIORIDAD"] != DBNull.Value ? (int)datos.Lector["ID_PRIORIDAD"] : 0,
                        Nombre = datos.Lector["PRIORIDAD"] != DBNull.Value ? (string)datos.Lector["PRIORIDAD"] : string.Empty
                    };

                    if (datos.Lector["ID_UNIDAD_MEDIDA"] != DBNull.Value)
                    {
                        aux.UnidadMedida = new UnidadMedida
                        {
                            Id = (int)datos.Lector["ID_UNIDAD_MEDIDA"],
                            Nombre = datos.Lector["NOMBRE_UNIDAD"] != DBNull.Value ?
                                (string)datos.Lector["NOMBRE_UNIDAD"] : string.Empty
                        };
                    }

                    aux.ValorMedida = datos.Lector["VALOR_MEDIDA"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["VALOR_MEDIDA"]) : 0;

                    lista.Add(aux);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
