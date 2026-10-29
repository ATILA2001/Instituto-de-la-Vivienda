/* =====================================================================================
   VISTAS PARA POWER BI: REDETERMINACIONES Y RELIQUIDACIONES
   Base: IVC (SQL Server 2019). Requiere SQL Server 2016 SP1+ (CREATE OR ALTER, EOMONTH).

   Capas (cada una se apoya en la anterior):
     1. dbo.vw_PBI_Redeterminaciones  -> una fila por redeterminación, con MontoRedet calculado
     2. dbo.vw_PBI_Reliquidaciones    -> una fila por (redeterminación x certificado del mes del salto o posterior, hasta el tope)
     3. dbo.vw_PBI_Pagos              -> certificados reales + reliquidaciones, con SIGAF prorrateado,
                                         estado derivado y último pase SADE

   REGLAS DE NEGOCIO (fijadas el 22/09/2026; R6 el 01/10/2026; R1, R2 y R7 el 28/10/2026 por dictamen del área):
     R1. El cálculo se hace al 1° del MES DEL SALTO: cuentan como ejecutado los certificados de meses
         ESTRICTAMENTE ANTERIORES y reliquidan los del mes del salto y POSTERIORES. La comparación es a nivel
         mes (EOMONTH), no a nivel día. MES_APROBACION es el mes de ejecución de la obra.
         (Hasta el 28/10/2026 el mes del salto contaba como ejecutado y no reliquidaba.)
     R2. (28/10/2026, dictamen del área) El anticipo financiero (TIPO_PAGO = 1) se trata IGUAL que un certificado:
         cuenta como ejecutado si es de un mes anterior al salto y reliquida si es del mes del salto o posterior.
         Hasta esa fecha no reliquidaba.
     R3. "Paso 3b": el PRIMER certificado posterior al salto absorbe el porcentaje acumulado
         pre-salto (su porcentaje aplicado = porcentaje propio + porcentaje ejecutado pre-salto).
     R4. Los legítimos abonos NO participan del reparto del SIGAF por expediente.
         (Pendiente futuro: si algún día comparten expediente, agregar LEGITIMOS_ABONOS a vw_PBI_Pagos.)
     R5. Cada autorizante se calcula con SUS PROPIOS certificados (corrige el bug de la app, que
         reutilizaba la lista filtrada del primer autorizante para todos los demás).
     R6. (01/10/2026) Una redeterminación RECHAZADA (etapa 35) o FUERA DE PLAZO (etapa 36) tiene MontoRedet = 0,
         no acumula en la base de las siguientes y no genera reliquidaciones. Las que están EN TRÁMITE se
         calculan normalmente. Los Ids 35 y 36 son los mismos que usa la app (ReglasRedeterminaciones.EtapasDesestimadas).
     R7. (28/10/2026) TOPE: la suma de las reliquidaciones de una redeterminación no supera su MontoRedet. La que
         lo alcanza se recorta al resto disponible y las siguientes no se generan (MontoReliquidacionSinTope
         conserva el valor sin recortar).

   FÓRMULAS:
     Ejecutado        = SUM(MONTO_TOTAL) de certificados del autorizante con mes < mes del salto (todos los tipos de pago, incluido el anticipo)
     PorcentajeUtil   = Ejecutado / MONTO_AUTORIZADO * 100          (0 si MONTO_AUTORIZADO = 0)
     Faltante         = MAX(0, 100 - PorcentajeUtil)
     MontoRedet[1]    = MONTO_AUTORIZADO * Ponderacion/100 * Faltante/100
     MontoRedet[n>1]  = (MONTO_AUTORIZADO + SUM(MontoRedet anteriores del mismo autorizante)) * Ponderacion/100 * Faltante/100
     PorcentajeCert   = MONTO_TOTAL / MONTO_AUTORIZADO * 100
     MontoReliq       = MontoRedet * (PorcentajeCert + [PorcentajeUtil si es el primer cert post-salto]) / 100
     Sigaf            = SUM(IMPORTE_PP del expediente) * Monto / SUM(Monto de todos los pagos del mismo expediente)

   NOTAS:
     - LÍMITE DE RECURSIÓN: la acumulación de redeterminaciones usa un CTE recursivo. SQL Server lo corta a
       100 niveles por autorizante y una vista no puede cambiar ese límite. Hoy el máximo es ~28. Si algún
       autorizante llegara a 100, la consulta que lee la vista debe agregar el hint al final:
           SELECT * FROM dbo.vw_PBI_Pagos OPTION (MAXRECURSION 0);
       En Power BI eso se logra importando con una "consulta SQL nativa" en lugar de elegir la vista.
     - Las etapas que anulan el monto (R6) se definen UNA sola vez, en el CTE RedetFlags de la primera vista.
     - Los joins entre tablas por número de expediente usan COLLATE DATABASE_DEFAULT porque
       PASES_SADE y GEDO tienen collations distintas al resto de la base.
     - Las redeterminaciones sin Salto o sin PORCENTAJE_PONDERACION se listan con MontoRedet NULL
       (igual que la app). Las redeterminaciones sin Nro se ordenan al final dentro del autorizante.
     - Si dos redeterminaciones del mismo autorizante comparten Nro, se desempata por Id.
   ===================================================================================== */

USE IVC;
GO

/* -------------------------------------------------------------------------------------
   Índices sugeridos (opcionales, mejoran el tiempo de refresco del tablero).
   Descomentar y ejecutar una sola vez si las vistas resultan lentas.
------------------------------------------------------------------------------------- */
-- CREATE NONCLUSTERED INDEX IX_Certificados_CodAut_Mes
--     ON dbo.Certificados (CODIGO_AUTORIZANTE, MES_APROBACION) INCLUDE (MONTO_TOTAL, TIPO_PAGO, EXPEDIENTE_PAGO);
-- CREATE NONCLUSTERED INDEX IX_Redeterminaciones_CodAut_Nro
--     ON dbo.Redeterminaciones (CODIGO_AUTORIZANTE, Nro) INCLUDE (Salto, PORCENTAJE_PONDERACION, Codigo_Redet, ETAPA, Expediente);
-- CREATE NONCLUSTERED INDEX IX_ExpedientesReliq_CodRedet_Mes
--     ON dbo.EXPEDIENTES_RELIQ (CODIGO_REDET, MES_APROBACION) INCLUDE (EXPEDIENTE);
-- CREATE NONCLUSTERED INDEX IX_Devengados_Exp
--     ON dbo.DEVENGADOS (EE_FINANCIERA) INCLUDE (IMPORTE_PP);
-- GO


/* =====================================================================================
   1. REDETERMINACIONES CALCULADAS
   ===================================================================================== */
CREATE OR ALTER VIEW dbo.vw_PBI_Redeterminaciones
AS
WITH RedetFlags AS
(
    -- R6: ÚNICO lugar donde se enumeran las etapas que anulan el monto (35 Rechazada, 36 Fuera de Plazo).
    -- Mismos Ids que EtapasDesestimadas en Negocio/Redeterminaciones/ReglasRedeterminaciones.cs. Sin etapa => 0 (se calcula).
    SELECT
        r.Id,
        CAST(CASE WHEN r.ETAPA IN (35, 36) THEN 1 ELSE 0 END AS bit) AS EsDesestimada
    FROM dbo.Redeterminaciones r
),
Base AS
(
    SELECT
        r.Id                                            AS RedeterminacionId,
        r.Codigo_Redet                                  AS CodigoRedet,
        r.CODIGO_AUTORIZANTE                            AS CodigoAutorizante,
        a.Id                                            AS AutorizanteId,
        a.OBRA                                          AS ObraId,
        r.Nro                                           AS Nro,
        r.Salto                                         AS Salto,
        EOMONTH(r.Salto)                                AS SaltoMes,
        r.ETAPA                                         AS EtapaId,
        fl.EsDesestimada                                AS EsDesestimada,
        CAST(r.PORCENTAJE_PONDERACION AS decimal(19,6)) AS PorcentajePonderacion,
        CAST(a.MONTO_AUTORIZADO AS decimal(19,6))       AS MontoAutorizado,
        ROW_NUMBER() OVER (PARTITION BY r.CODIGO_AUTORIZANTE
                           ORDER BY CASE WHEN r.Nro IS NULL THEN 1 ELSE 0 END, r.Nro, r.Id) AS Orden,
        -- R1 + R2: ejecutado en meses estrictamente anteriores al del salto, incluido el anticipo. R5: solo certificados de ESTE autorizante.
        CAST(ISNULL((
            SELECT SUM(c.MONTO_TOTAL)
            FROM dbo.Certificados c
            WHERE c.CODIGO_AUTORIZANTE = r.CODIGO_AUTORIZANTE
              AND c.MES_APROBACION IS NOT NULL
              AND EOMONTH(c.MES_APROBACION) < EOMONTH(r.Salto)
        ), 0) AS decimal(19,6))                         AS EjecutadoPreSalto
    FROM dbo.Redeterminaciones r
    INNER JOIN dbo.Autorizantes a ON a.CODIGO_AUTORIZANTE = r.CODIGO_AUTORIZANTE
    INNER JOIN RedetFlags fl       ON fl.Id = r.Id
    WHERE r.Salto IS NOT NULL
      AND r.PORCENTAJE_PONDERACION IS NOT NULL
),
Porc AS
(
    SELECT
        b.*,
        -- decimal(28,12): con 6 decimales el redondeo del porcentaje desviaba hasta ~1 peso en montos de cientos de millones
        CAST(CASE WHEN b.MontoAutorizado > 0
                  THEN b.EjecutadoPreSalto / b.MontoAutorizado * 100
                  ELSE 0 END AS decimal(28,12))        AS PorcentajeUtilizado
    FROM Base b
),
Falt AS
(
    SELECT
        p.*,
        CAST(CASE WHEN 100 - p.PorcentajeUtilizado > 0
                  THEN 100 - p.PorcentajeUtilizado
                  ELSE 0 END AS decimal(28,12))        AS Faltante
    FROM Porc p
),
Rec AS
(
    -- Ancla: primera redeterminación de cada autorizante.
    -- R6: EsDesestimada = 1 => monto 0, y por lo tanto no suma en el acumulado.
    -- Se acumula con decimal(28,8) para no arrastrar redondeos nivel a nivel; se redondea a 4 decimales recién al exponer.
    SELECT
        f.RedeterminacionId, f.CodigoAutorizante, f.Orden,
        CAST(CASE WHEN f.EsDesestimada = 1 THEN 0
                  ELSE f.MontoAutorizado * f.PorcentajePonderacion / 100 * f.Faltante / 100 END AS decimal(28,8)) AS MontoRedet,
        CAST(CASE WHEN f.EsDesestimada = 1 THEN 0
                  ELSE f.MontoAutorizado * f.PorcentajePonderacion / 100 * f.Faltante / 100 END AS decimal(28,8)) AS AcumuladoRedet
    FROM Falt f
    WHERE f.Orden = 1

    UNION ALL

    -- Recursión: base = MontoAutorizado + suma de las redeterminaciones anteriores del mismo autorizante
    SELECT
        f.RedeterminacionId, f.CodigoAutorizante, f.Orden,
        CAST(CASE WHEN f.EsDesestimada = 1 THEN 0
                  ELSE (f.MontoAutorizado + p.AcumuladoRedet) * f.PorcentajePonderacion / 100 * f.Faltante / 100 END AS decimal(28,8)),
        CAST(p.AcumuladoRedet + CASE WHEN f.EsDesestimada = 1 THEN 0
                  ELSE (f.MontoAutorizado + p.AcumuladoRedet) * f.PorcentajePonderacion / 100 * f.Faltante / 100 END AS decimal(28,8))
    FROM Falt f
    INNER JOIN Rec p ON p.CodigoAutorizante = f.CodigoAutorizante
                    AND f.Orden = p.Orden + 1
),
UltimoPase AS
(
    SELECT
        ps.EXPEDIENTE COLLATE DATABASE_DEFAULT          AS Expediente,
        ps.[BUZON DESTINO]                              AS BuzonSade,
        ps.[FECHA ULTIMO PASE]                          AS FechaSade,
        ROW_NUMBER() OVER (PARTITION BY ps.EXPEDIENTE ORDER BY ps.[FECHA ULTIMO PASE] DESC) AS rn
    FROM dbo.PASES_SADE ps
)
SELECT
    r.Id                                    AS RedeterminacionId,
    r.Codigo_Redet                          AS CodigoRedet,
    r.CODIGO_AUTORIZANTE                    AS CodigoAutorizante,
    a.Id                                    AS AutorizanteId,
    a.OBRA                                  AS ObraId,
    r.Nro,
    r.Tipo,
    r.Salto,
    EOMONTH(r.Salto)                        AS SaltoMes,
    r.PORCENTAJE_PONDERACION                AS PorcentajePonderacion,
    r.Expediente,
    r.ETAPA                                 AS EtapaId,
    er.Nombre                               AS EtapaNombre,
    fl.EsDesestimada,
    CASE WHEN fl.EsDesestimada = 1         THEN 3
         WHEN r.ETAPA IN (37, 38)          THEN 4
         WHEN r.ETAPA IN (12, 22, 33, 39)  THEN 1
         ELSE 2 END                         AS EstadoGeneralId,
    CASE WHEN fl.EsDesestimada = 1         THEN 'DESESTIMADO'
         WHEN r.ETAPA IN (37, 38)          THEN 'NO INICIADO'
         WHEN r.ETAPA IN (12, 22, 33, 39)  THEN 'APROBADO'
         ELSE 'EN TRAMITE' END              AS EstadoGeneral,
    r.ID_USUARIO                            AS UsuarioId,
    r.Observaciones,
    a.MONTO_AUTORIZADO                      AS MontoAutorizado,
    f.EjecutadoPreSalto,
    f.PorcentajeUtilizado,
    f.Faltante,
    CAST(rec.MontoRedet AS decimal(19,4))   AS MontoRedet,
    up.BuzonSade,
    up.FechaSade
FROM dbo.Redeterminaciones r
INNER JOIN RedetFlags       fl  ON fl.Id = r.Id
LEFT JOIN dbo.Autorizantes  a   ON a.CODIGO_AUTORIZANTE = r.CODIGO_AUTORIZANTE
LEFT JOIN dbo.Estados_Redet er  ON er.Id = r.ETAPA
LEFT JOIN Falt              f   ON f.RedeterminacionId = r.Id
LEFT JOIN Rec               rec ON rec.RedeterminacionId = r.Id
LEFT JOIN UltimoPase        up  ON up.Expediente = r.Expediente COLLATE DATABASE_DEFAULT AND up.rn = 1;
GO


/* =====================================================================================
   2. RELIQUIDACIONES (virtuales): redeterminación x certificado posterior al salto
   ===================================================================================== */
CREATE OR ALTER VIEW dbo.vw_PBI_Reliquidaciones
AS
WITH Post AS
(
    SELECT
        rd.RedeterminacionId,
        rd.CodigoRedet,
        rd.CodigoAutorizante,
        rd.AutorizanteId,
        rd.ObraId,
        rd.Nro                                          AS NroRedet,
        rd.Salto,
        rd.EtapaId,
        rd.EstadoGeneralId,
        rd.EstadoGeneral,
        rd.MontoAutorizado,
        rd.MontoRedet,
        rd.PorcentajeUtilizado,
        c.Id                                            AS CertificadoId,
        c.MES_APROBACION                                AS MesAprobacion,
        c.MONTO_TOTAL                                   AS MontoCertificado,
        c.TIPO_PAGO                                     AS TipoPagoCertificado,
        -- R3: orden de los certificados del mes del salto y posteriores; el primero absorbe lo acumulado pre-salto
        ROW_NUMBER() OVER (PARTITION BY rd.RedeterminacionId ORDER BY c.MES_APROBACION, c.Id) AS OrdenPostSalto
    FROM dbo.vw_PBI_Redeterminaciones rd
    INNER JOIN dbo.Certificados c ON c.CODIGO_AUTORIZANTE = rd.CodigoAutorizante
    WHERE rd.MontoRedet IS NOT NULL
      AND rd.EsDesestimada = 0                              -- R6: rechazada / fuera de plazo no reliquida
      AND c.MES_APROBACION IS NOT NULL
      AND EOMONTH(c.MES_APROBACION) >= rd.SaltoMes         -- R1: mes del salto y posteriores. R2: todos los tipos de pago, incluido el anticipo
),
Calc AS
(
    SELECT
        p.*,
        CAST(CASE WHEN p.MontoAutorizado > 0
                  THEN p.MontoCertificado / p.MontoAutorizado * 100
                  ELSE 0 END AS decimal(28,12))        AS PorcentajeCertificado
    FROM Post p
),
Tope AS
(
    -- R7: acumulado de las reliquidaciones anteriores de la misma redeterminación (en el orden de R3)
    SELECT
        c.*,
        CAST(c.MontoRedet
             * (c.PorcentajeCertificado + CASE WHEN c.OrdenPostSalto = 1 THEN c.PorcentajeUtilizado ELSE 0 END)
             / 100 AS decimal(19,4))                                        AS MontoReliquidacionSinTope,
        ISNULL(SUM(CAST(c.MontoRedet
             * (c.PorcentajeCertificado + CASE WHEN c.OrdenPostSalto = 1 THEN c.PorcentajeUtilizado ELSE 0 END)
             / 100 AS decimal(19,4)))
             OVER (PARTITION BY c.RedeterminacionId ORDER BY c.OrdenPostSalto
                   ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0)   AS AcumuladoPrevio
    FROM Calc c
)
SELECT
    CONCAT(c.RedeterminacionId, '-', c.CertificadoId)   AS ReliquidacionKey,
    c.RedeterminacionId,
    c.CodigoRedet,
    c.CodigoAutorizante,
    c.AutorizanteId,
    c.ObraId,
    c.NroRedet,
    c.Salto,
    c.EtapaId,
    c.EstadoGeneralId,
    c.EstadoGeneral,
    c.CertificadoId,
    c.MesAprobacion,
    c.MontoCertificado,
    c.TipoPagoCertificado,
    c.MontoAutorizado,
    c.MontoRedet,
    c.OrdenPostSalto,
    CAST(CASE WHEN c.OrdenPostSalto = 1 THEN 1 ELSE 0 END AS bit)         AS EsPrimerPostSalto,
    c.PorcentajeCertificado,
    c.PorcentajeUtilizado                                                   AS PorcentajeAcumuladoPreSalto,
    CAST(c.PorcentajeCertificado
         + CASE WHEN c.OrdenPostSalto = 1 THEN c.PorcentajeUtilizado ELSE 0 END
         AS decimal(19,6))                                                  AS PorcentajeAplicado,
    c.MontoReliquidacionSinTope,
    -- R7: recortada al resto disponible del MontoRedet
    CAST(CASE WHEN c.AcumuladoPrevio + c.MontoReliquidacionSinTope > c.MontoRedet
              THEN c.MontoRedet - c.AcumuladoPrevio
              ELSE c.MontoReliquidacionSinTope END AS decimal(19,4))          AS MontoReliquidacion,
    er.EXPEDIENTE                                                           AS ExpedienteReliq
FROM Tope c
LEFT JOIN dbo.EXPEDIENTES_RELIQ er
       ON er.CODIGO_REDET = c.CodigoRedet
      AND YEAR(er.MES_APROBACION)  = YEAR(c.MesAprobacion)
      AND MONTH(er.MES_APROBACION) = MONTH(c.MesAprobacion)
WHERE c.AcumuladoPrevio < c.MontoRedet;                -- R7: con el tope cubierto no se generan más reliquidaciones
GO


/* =====================================================================================
   3. PAGOS UNIFICADOS: certificados reales + reliquidaciones, con SIGAF prorrateado
      (R4: los legítimos abonos no participan del reparto)
   ===================================================================================== */
CREATE OR ALTER VIEW dbo.vw_PBI_Pagos
AS
WITH Pagos AS
(
    SELECT
        'CERTIFICADO'                                   AS Origen,
        CAST(c.Id AS varchar(30))                       AS PagoKey,
        c.Id                                            AS CertificadoId,
        CAST(NULL AS int)                               AS RedeterminacionId,
        c.CODIGO_AUTORIZANTE                            AS CodigoAutorizante,
        a.Id                                            AS AutorizanteId,
        a.OBRA                                          AS ObraId,
        NULLIF(LTRIM(RTRIM(c.EXPEDIENTE_PAGO)), '') COLLATE DATABASE_DEFAULT AS Expediente,
        c.MES_APROBACION                                AS MesAprobacion,
        CAST(c.MONTO_TOTAL AS decimal(19,4))            AS Monto,
        c.TIPO_PAGO                                     AS TipoPagoId,
        tp.Nombre                                       AS TipoPagoNombre,
        CAST(CASE WHEN a.MONTO_AUTORIZADO > 0
                  THEN c.MONTO_TOTAL / a.MONTO_AUTORIZADO * 100
                  ELSE NULL END AS decimal(19,6))      AS PorcentajeSobreAutorizado,
        c.PORC_EJEC_FISICA                              AS PorcEjecFisica
    FROM dbo.Certificados c
    LEFT JOIN dbo.Autorizantes a ON a.CODIGO_AUTORIZANTE = c.CODIGO_AUTORIZANTE
    LEFT JOIN dbo.Tipo_Pago tp   ON tp.Id = c.TIPO_PAGO

    UNION ALL

    SELECT
        'RELIQUIDACION',
        rq.ReliquidacionKey,
        rq.CertificadoId,
        rq.RedeterminacionId,
        rq.CodigoRedet,                                 -- la app expone el código de la redeterminación como "código autorizante" de la reliquidación
        rq.AutorizanteId,
        rq.ObraId,
        NULLIF(LTRIM(RTRIM(rq.ExpedienteReliq)), '') COLLATE DATABASE_DEFAULT,
        rq.MesAprobacion,
        rq.MontoReliquidacion,
        3,
        'RELIQUIDACION',
        rq.PorcentajeAplicado,
        NULL
    FROM dbo.vw_PBI_Reliquidaciones rq
),
Devengado AS
(
    SELECT
        d.EE_FINANCIERA COLLATE DATABASE_DEFAULT        AS Expediente,
        SUM(d.IMPORTE_PP)                               AS TotalDevengado
    FROM dbo.DEVENGADOS d
    GROUP BY d.EE_FINANCIERA
),
UltimoPase AS
(
    SELECT
        ps.EXPEDIENTE COLLATE DATABASE_DEFAULT          AS Expediente,
        ps.[BUZON DESTINO]                              AS BuzonSade,
        ps.[FECHA ULTIMO PASE]                          AS FechaSade,
        ROW_NUMBER() OVER (PARTITION BY ps.EXPEDIENTE ORDER BY ps.[FECHA ULTIMO PASE] DESC) AS rn
    FROM dbo.PASES_SADE ps
),
Reparto AS
(
    SELECT
        p.*,
        SUM(p.Monto)  OVER (PARTITION BY p.Expediente)  AS SumaMontoExpediente,
        COUNT(*)      OVER (PARTITION BY p.Expediente)  AS CantidadPagosExpediente
    FROM Pagos p
),
ConSigaf AS
(
    SELECT
        r.*,
        d.TotalDevengado,
        CAST(CASE
                WHEN r.Expediente IS NULL OR d.TotalDevengado IS NULL OR d.TotalDevengado <= 0 THEN NULL
                WHEN r.CantidadPagosExpediente = 1 THEN d.TotalDevengado
                WHEN r.SumaMontoExpediente > 0 THEN d.TotalDevengado * r.Monto / r.SumaMontoExpediente
                ELSE NULL
             END AS decimal(19,4))                      AS Sigaf
    FROM Reparto r
    LEFT JOIN Devengado d ON d.Expediente = r.Expediente
)
SELECT
    s.Origen,
    s.PagoKey,
    s.CertificadoId,
    s.RedeterminacionId,
    s.CodigoAutorizante,
    s.AutorizanteId,
    s.ObraId,
    s.Expediente,
    s.MesAprobacion,
    s.Monto,
    s.TipoPagoId,
    s.TipoPagoNombre,
    s.PorcentajeSobreAutorizado,
    s.PorcEjecFisica,
    s.TotalDevengado,
    s.SumaMontoExpediente,
    s.CantidadPagosExpediente,
    s.Sigaf,
    CASE WHEN s.Expediente IS NULL THEN 'NO INICIADO'
         WHEN s.Sigaf > 0           THEN 'DEVENGADO'
         ELSE 'EN TRAMITE' END                          AS Estado,
    up.BuzonSade,
    up.FechaSade
FROM ConSigaf s
LEFT JOIN UltimoPase up ON up.Expediente = s.Expediente AND up.rn = 1;
GO


/* =====================================================================================
   CONSULTAS DE VERIFICACIÓN (no forman parte de las vistas)
   ===================================================================================== */
-- Un autorizante de muestra, paso a paso:
-- SELECT * FROM dbo.vw_PBI_Redeterminaciones WHERE CodigoAutorizante = 'XXX' ORDER BY Nro;
-- SELECT * FROM dbo.vw_PBI_Reliquidaciones    WHERE CodigoAutorizante = 'XXX' ORDER BY NroRedet, MesAprobacion;
-- SELECT * FROM dbo.vw_PBI_Pagos              WHERE AutorizanteId = (SELECT Id FROM dbo.Autorizantes WHERE CODIGO_AUTORIZANTE = 'XXX') ORDER BY MesAprobacion;

-- Control: el total reliquidado por redeterminación vs. su MontoRedet
-- SELECT rd.CodigoRedet, rd.MontoRedet, SUM(rq.MontoReliquidacion) AS TotalReliquidado
-- FROM dbo.vw_PBI_Redeterminaciones rd
-- LEFT JOIN dbo.vw_PBI_Reliquidaciones rq ON rq.RedeterminacionId = rd.RedeterminacionId
-- GROUP BY rd.CodigoRedet, rd.MontoRedet;
