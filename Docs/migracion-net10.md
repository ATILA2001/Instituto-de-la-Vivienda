# Migración de IVC a .NET 10 (Blazor Server) — análisis y plan

Fecha del análisis: 03/11/2026. Base analizada: rama `refactor/fase4-integridad-edicion` (= `dev` 5e76bca + 9971aee). Proyectos de referencia: `SAF` (main 5671c66, 09/10/2026), `SAI` (dev 55dd033, 22/05/2026), `Auth.Web` (dev a120203, 13/08/2026).

---

## 1. Estado verificado del legado

| Verificación | Resultado |
|---|---|
| `msbuild IVC.sln` (Debug) | OK, sólo warnings MSB3277 de versiones de referencias en `Negocio` |
| `dotnet test Tests/IVC.Tests` | 77 tests OK |
| Árbol de trabajo | Limpio |
| `refactor/fase4-integridad-edicion` | 1 commit sobre `dev` (9971aee), pusheada, **PR contra `dev` sin abrir** (no pude verificarlo: `gh` no está instalado; confirmar en GitHub) |
| `dev` vs `main` | `dev` tiene **33 commits** que producción no tiene: fase 2 (calculador puro, `IContextoWeb`), reglas del área R1/R2/R7 (PR #11), fase 3 (`GridPageBase` y grillas, PR #12 y #13), hotfix 880c42f (PR #14), seguridad 929c927 (PR #15) |
| `origin/Presu-V2` | 3 commits del 06/11/2025 (`AutorizantePresupuestoEF`, `AutorizantePresupuestoNegocio` de 287 líneas, cambios grandes en `AutorizantesEF.aspx`), nunca mergeados; hoy divergen fuerte de `dev` |

---

## 2. Pendientes previos a la migración

### 2.1 Imprescindibles antes de arrancar

1. **Cerrar el ciclo del legado y llevarlo a producción.** Abrir el PR de `refactor/fase4-integridad-edicion` → `dev`; luego `dev` → `main` y desplegar. Motivo: el legado va a seguir siendo producción durante toda la migración, así que tiene que quedar congelado en un estado estable y conocido, con las reglas R1/R2/R7 que ya replican las vistas Power BI desplegadas el 01/11. Después del merge, el legado queda en modo "sólo hotfix".
2. **Decisiones de negocio abiertas que definen qué lógica se porta.** No conviene portar reglas indefinidas y corregirlas dos veces (legado + app nueva + vistas SQL):
   - provisoria + definitiva del mismo salto (hoy doble conteo; el área dijo que la definitiva reemplaza; falta definir def < prov);
   - tipos de pago que cuentan como ejecutado (RENGLON 10/20/30, SUBSIDIADO, ADELANTO, EXCEDENTE, RELIQUIDACION);
   - R7 con `MontoRedet` negativo (3 casos en prod);
   - redeterminaciones sin salto y ETAPA NULL (la app dice NO INICIADO y la vista EN TRÁMITE).
   No bloquean el inicio, pero sí la fase de Redeterminaciones (fase 6). Las reglas puras ya cerradas (`ReglasRedeterminaciones`, 205 líneas, 25 tests) se portan tal cual.
3. **Destino de `Presu-V2`.** Decidir si se descarta o si "presupuesto por autorizante" se especifica como feature nueva del proyecto nuevo. No mergearlo al legado: toca `AutorizantesEF.aspx` que fue reescrito en fase 3.
4. **Limpieza de datos de producción** detectada el 30/09: 3 saltos con año inválido, 6 redeterminaciones duplicadas, 14 pares DEFINITIVA/acdir en 404-2, porcentajes fuera de rango, 22 filas huérfanas en `EXPEDIENTES_RELIQ`, campo `Tipo` inconsistente. Son datos, no código, pero EF Core con `Nullable` habilitado y tipos estrictos los va a hacer visibles como errores. Mejor limpiarlos antes de la fase 2.
5. **Convención de URLs y registro en Auth.Web.** Los permisos (`perms_json`) se otorgan por URL exacta. Hoy son `/ObrasEF.aspx`; en Blazor serán `/obras`, `/autorizantes`, etc. Hay que decidir las URLs antes de la fase 1 porque definen: el `ApplicationClient` nuevo (ClientId + AllowedReturnUrls), las `Pages` nuevas con sus permisos por rol/área y las `AreaRoute`. Lo hace el administrador de Auth.Web en `/admin/clients`, `/admin/routes` y permisos.

### 2.2 Pendientes del legado que NO conviene resolver en WebForms

Se resuelven por diseño al migrar; gastar esfuerzo en el legado es doble trabajo:

| Pendiente del legado | Cómo lo resuelve la arquitectura destino |
|---|---|
| ≈1.900 líneas muertas en `CalculoRedeterminacionNegocioEF` (regiones de paginación), ≈400 en ADO, 9 métodos muertos en `UsuarioNegocioEF`, referencias sin uso (IdentityModel, Jwt, DirectoryServices, PresentationCore) | No se portan |
| Fase 4 pendiente: migrar las 8 páginas ADO (Barrios, Empresas, Líneas, BdProyectos, Movimientos) a EF6 | Se hacen directamente en EF Core (fase 4 del plan) |
| Paginación en BD de Redeterminaciones/Formulación con `PaginationControl` propio | `RadzenDataGrid` con `LoadData` o carga progresiva de `GridPageBase` (SAF) |
| `TreeViewSearch` (873 líneas C# + 963 JS), filtros guardados en `HttpContext.Items`, estado en `Session`/`ViewState` | Filtros nativos de Radzen + `EstadoGrillas` por circuito (SAF) |
| CSRF (`ViewStateUserKey`), rechazo por Origin, guards por área en `RedeterminacionesEF` | Antiforgery de ASP.NET Core, `PermissionPageBase` con `CanAccess/CanEdit` por página y revalidación en cada handler |
| Elección de base por rama (`IVC_TEST` en `dev`, `IVC_PROD` en `main`) | `appsettings` + variables de entorno del `web.config` del servidor, como SAF |
| `debug="true"`, `Web.config`, OWIN 4.2 + DataProtection 2.2 | Pipeline ASP.NET Core 10, cookie compartida con `SharedCookieTicketDataFormat` |
| Doble suscripción `AcceptChanges`, modal "Modificar reliquidación" inalcanzable, dos eventos inline en un postback | Desaparecen con el modelo de componentes |
| `Session["Usuario"]` y caché de claims en sesión | Servicio scoped `IUsuarioActual` por circuito |

### 2.3 Pendientes de infraestructura (fuera del repo)

- Sitio IIS nuevo con ASP.NET Core Hosting Bundle (SAF usa `OutOfProcess`), binding HTTPS y URL propia; o bien el mismo host con una ruta nueva. La URL define los AllowedReturnUrls en Auth.Web.
- `web.config` del servidor con las variables de entorno (`ConnectionStrings__IvcConnection`, `ConnectionStrings__AuthWebConnection`, `SharedCookie__*`, `AuthWeb__BaseUrl`, `AuthWeb__ClientId`), igual que SAF. Perfil `.pubxml` fuera de git con `IsTransformWebConfigDisabled=true` y `SkipExtraFilesOnServer=true` (lección del 31/07/2026 en SAF).
- Usuario SQL con **escritura** sobre `IVC` / `IVC_TEST` (SAF sólo lee IVC) y lectura sobre la base de Auth.Web (key ring). La contraseña expuesta en el historial ya fue rotada el 01/11/2026.
- El repo nuevo debe vivir al lado de `Auth.Web` en la misma carpeta (SAF linkea `..\Auth.Web\Security\SharedCookieTicketDto.cs`; SAI usa `..\..\Auth.Web`, revisar).

---

## 3. Inventario de lo que hay que portar

### 3.1 Páginas (WebForms)

| Página | Markup / code-behind (líneas) | Acceso a datos | Destino en la app nueva | Tamaño |
|---|---|---|---|---|
| `ObrasEF.aspx` | 433 / 436 | EF6 (`ObraNegocioEF`, `CalculoObraNegocioEF`) | `/obras` | M |
| `AutorizantesEF.aspx` | 537 / 674 | EF6 (`AutorizanteNegocioEF`, `CalculoRedeterminacionNegocioEF`, SADE/SIGAF/ACDIR) | `/autorizantes` | L |
| `CertificadosEF.aspx` | 398 / 746 | EF6 (`CalculoRedeterminacionNegocioEF`) | `/certificados` | L |
| `RedeterminacionesEF.aspx` | 396 / 1.054 | EF6 (`RedeterminacionNegocioEF`, paginación en BD, carga SADE/SIGAF) | `/redeterminaciones` | L |
| `LegitimosEF.aspx` | 370 / 574 | EF6 (`LegitimoNegocioEF`) | `/legitimos` | M |
| `FormulacionEF.aspx` | 488 / 731 | EF6 (`FormulacionNegocioEF`, pivot por años, paginación en BD) | `/formulacion` | L |
| `BdProyectos.aspx` | 273 / 453 | ADO (`BdProyectoNegocio`, `ObraNegocio`) | `/proyectos` | M |
| `MovimientosGestion.aspx` | 224 / 463 | ADO (`MovimientoNegocio`) | `/movimientos` | M |
| `AbmlBarrio` + `ModificarBarrio` | 70+39 / 113+67 | ADO | `/barrios` (una sola grilla con edición inline) | S |
| `AbmlEmpresa` + `ModificarEmpresa` | 69+40 / 106+68 | ADO | `/empresas` | S |
| `LineasGestion.aspx` | 62 / 109 | ADO | `/lineas-gestion` | S |
| `LineasGestionFF.aspx` | 74 / 125 | ADO | `/lineas-gestion-ff` | S |
| `PanelConfiguracion.aspx` | 76 / 92 | ADO (`ABMPlaniNegocio`, `UsuariosVinculadosNegocio`) | `/admin/configuracion` | S |
| `Inicio.aspx` | 107 / 148 | claims | `/home` (menú por `perms_json`) | S |
| `Startup.aspx`, `AccessDenied`, `Error`, masters, `Global.cs`, `Startup.Auth.cs`, `Sso/` | ≈1.300 | OWIN | Ya existe en SAF: `Extensions/`, `Security/`, `Components/Account/` | copiar |
| `TreeViewSearch`, `PaginationControl`, `ToastNotification` | ≈1.400 C# + 963 JS | — | Radzen (filtros, paginado, `NotificationService`) | no se porta |

Total de code-behind vivo: ≈ 7.000 líneas de C# + ≈ 4.800 de markup. SAF completo (sin tests ni migraciones) tiene ≈ 12.600 líneas de C# y Razor; IVC nuevo debería quedar en un orden similar o algo mayor.

### 3.2 Negocio y dominio

- `Negocio`: 8.260 líneas, de las que ≈ 2.300 son muertas (regiones de paginación de `Calculo…`, ADO sin uso). Núcleo a portar como **código puro con tests**: `Redeterminaciones/ReglasRedeterminaciones` (205), `MapeoEstadoRedet` (53), `EstadoGeneral`, `UltimoPaseSade`, `BuzonEtapaMatcher` (161, archivo en ISO-8859-1 sin BOM: convertir a UTF-8), `MontoHelper` (55), `FormulacionCiclo` (41), `Infraestructura/SeguridadHttp` (21), reglas de permisos de `UserHelper` (`PuedeModificar*`).
- Servicios con EF a reescribir: `CalculoRedeterminacionNegocioEF` (métodos vivos: `ListarAutorizantesYRedeterminaciones`, `ListarCertificadosYReliquidaciones`, `CalcularCertificadosPorExpedientes`, `CalcularPropiedadesAdicionales`), `DatosExternosNegocioEF` (SADE, SIGAF, ACDIR con `SqlQuery` crudo y caché), `RedeterminacionNegocioEF`, `AutorizanteNegocioEF`, `ObraNegocioEF`, `CalculoObraNegocioEF`, `LegitimoNegocioEF`, `FormulacionNegocioEF`, catálogos (`Concepto`, `EstadoRedet`, `EstadoAutorizante`, `TipoPago`, `Prioridad`, `UnidadMedida`), `ExpedienteReliqNegocioEF`.
- `Dominio`: 27 entidades EF6 mapeadas con `[Table]` y 3 DTOs (`ObraDTO`, `CertificadoDTO`, `AutorizanteDTO`, 92 propiedades). Tablas usadas por ADO sin entidad fiel: `MOVIMIENTOS_GESTION` (la entidad `MovimientoEF` apunta a `Movimientos`), `LINEA_DE_GESTION_FF`, `BD_PROYECTOS` (`ProyectoEF` no coincide con las columnas que usa ADO), `ABM_PLANI`, `OBRAS` para los combos. `PASES_SADE` tiene `[Key] Expediente` no único (en EF Core: `HasNoKey`, sólo lectura, como ya hace SAF).
- Tests existentes (77) a portar: `ReglasRedeterminacionesTests` (R1–R7), `MontoHelperTests`, `MapeoEstadoRedetTests`, `PropiedadesPagoTests`, `UserHelperTests`, `UserHelperSesionTests` (con `ContextoWebFalso`), `SeguridadHttpTests`.
- `Database/vw_PowerBI_Redeterminaciones.sql` y `Docs/calculo-redeterminaciones.html` se mantienen: son la especificación funcional y el oráculo de paridad.

### 3.3 Reglas transversales que hay que preservar

- **Identidad IVC**: claims `area` (ids de Auth.Web) → `Areas.AuthAreaId` → `Areas.Id` (lista `IvcAreaIds`); `UsuariosVinculados` por `AuthUserId` (alta automática, nombre sincronizado, `IsPlanningOpenOverride`); admin = rol Admin/Administrador/Administradores. Tres áreas SECRETARIA (18, 19, 20) mapean al AuthAreaId 36.
- **Permisos funcionales** (`UserHelper`): `PuedeModificarPlanificacion` (admin, o planificación abierta en `ABM_PLANI`, o override; nunca Redeterminaciones ni Secretaría), `PuedeEditarSeguimientoAutorizante` (planificación o área Redeterminaciones), `PuedeModificarFormulacion` (admin o formulación abierta, no Redet/Secretaría), `PuedeModificarRedeterminaciones` (todos salvo Secretaría). Se verifican en cada handler que escribe, además de ocultar controles.
- **Filtro por área** en Obras/Autorizantes/Certificados/Legítimos/Formulación; `RedeterminacionesEF` es cross-area por diseño (sin guard, decisión del 01/11).
- **Columnas financieras ocultas** al área Redeterminaciones en Obras.
- `perms_version` revalidado contra Auth.Web cada 5 minutos con fail-open (idéntico en SAF: `PermissionVersionService`).
- Cultura `es-AR` para montos (`MontoHelper` acepta signo negativo: existen porcentajes negativos en producción).
- `FormulacionAnioBase` (hoy 2027) como configuración.

---

## 4. Arquitectura destino

### 4.1 Plantilla: SAF

SAF es la referencia, no SAI, por cuatro razones: es el proyecto más reciente (octubre 2026 contra mayo 2026 de SAI); ya lee la base IVC con EF Core (`IvcDbContext` con nombres de columna verificados y la lección de collation `IsUnicode(false)` en `EXPEDIENTE`); ya tiene `GridPageBase`/`PermissionPageBase` con carga progresiva en segundo plano (pensada justamente para columnas SADE/SIGAF), auditoría, export y persistencia de filtros; y su `Program.cs` modular (`Extensions/`) es el modelo que Auth.Web también documenta. De SAI se toma la metodología documentada en su README (MVVM: code-behind `.razor.cs`, estado en ViewModels, convenciones de carpetas, lifetimes) que SAF ya aplica.

Stack: `net10.0`, `Microsoft.NET.Sdk.Web`, `Nullable` + `ImplicitUsings`, EF Core 10.0.8 (SqlServer, Design, Tools, DataProtection.EntityFrameworkCore), Radzen.Blazor 10.3.1, ClosedXML 0.105.0 (reemplaza EPPlus 4.5.3), MSTest 3.6.4 + bUnit 1.40 + Moq 4.20 + EF InMemory en `Tests/`. SDK 10.0.401 ya instalado en la máquina.

### 4.2 Estructura propuesta (proyecto único `IVC`, como SAF y SAI)

```
IVC/
├─ Application/                  # Reglas puras y DTOs por feature (sin EF, sin UI) — 100 % testeable
│  ├─ Common/                    # MontoHelper, Validaciones, atributos Export/Audit
│  ├─ Identidad/                 # UsuarioIvc (DTO), ReglasPermisosIvc (ex UserHelper.PuedeModificar*)
│  ├─ Obras/Dtos/
│  ├─ Redeterminaciones/         # ReglasRedeterminaciones, MapeoEstadoRedet, BuzonEtapaMatcher, UltimoPaseSade, Dtos/
│  ├─ Formulacion/               # FormulacionCiclo, pivot, Dtos/
│  ├─ Legitimos/Dtos/
│  └─ Catalogos/Dtos/            # Barrios, Empresas, Líneas, Proyectos, Movimientos
├─ Components/
│  ├─ Account/                   # Login, AccessDenied, RedirectToLogin (copiados de SAF)
│  ├─ Layout/                    # MainLayout con menú armado desde perms_json (hoy Inicio.aspx + masters)
│  ├─ Pages/<Feature>/           # Pagina.razor + Pagina.razor.cs sobre GridPageBase
│  ├─ Shared/                    # GridToolbar, GridFooterTotales, EstadoCarga, PanelIdentidad (de SAF)
│  ├─ GridPageBase.cs            # copiado de SAF, adaptado a permisos IVC por área
│  └─ PermissionPageBase.cs
├─ Data/
│  ├─ Entities/                  # 27 entidades + las tablas que hoy usa ADO (database-first)
│  ├─ IvcDbContext.cs            # lectura/escritura sobre IVC; SIN migraciones (esquema existente)
│  └─ DataProtectionDbContext.cs # key ring de Auth.Web (copiado de SAF)
├─ Extensions/                   # IvcBuilderExtensions / IvcPipelineExtensions
├─ Repositories/{Abstractions,Implementations}/   # Transient, un DbContext por operación (factory)
├─ Services/{Abstractions,Implementations}/       # Scoped; devuelven DTOs de Application
├─ Security/                     # AdminClaims, SharedCookieTicketDataFormat (+ link a Auth.Web DTO)
├─ Shared/                       # NotificationHelper, EstadoGrillas, GridTextos
├─ Database/                     # vistas Power BI (se mueven del repo viejo) + scripts versionados de tablas nuevas
├─ Docs/                         # calculo-redeterminaciones.html (se mueve) + este documento
├─ Tests/                        # Application/, Services/, Repositories/ (InMemory), Components/ (bUnit)
├─ appsettings.json              # claves vacías; valores por user secrets / variables de entorno
└─ README.md
```

### 4.3 Decisiones de diseño que difieren de SAF

| Tema | SAF | Propuesta para IVC | Motivo |
|---|---|---|---|
| Base propia con migraciones (`AppDbContext`) | Sí | **No**: un solo `IvcDbContext` database-first sin migraciones; las tablas nuevas que hagan falta (auditoría de cambios, listas) se crean con scripts versionados en `Database/`, como ya se hace con las vistas | El esquema IVC es preexistente y lo comparten SAF, Power BI y procesos de carga; EF Migrations sobre él es riesgo sin beneficio |
| IVC es sólo lectura | Sí | Lectura y escritura | Es la base propia de la app |
| Concurrencia optimista (`RowVersion`) | Sí en tablas propias | No disponible sin alterar el esquema. Queda "último escribe gana", como hoy; se registra como mejora futura con script de `ROWVERSION` por tabla | No tocar el esquema en la migración |
| Auditoría `CambiosAuditoria` | Sí | Recomendado incluirla (tabla nueva en IVC por script). Decisión del usuario | Hoy IVC no audita quién cambió qué |
| Identidad | Sólo claims | Servicio scoped `IUsuarioActual` que resuelve áreas IVC y `UsuariosVinculados` una vez por circuito | Reemplaza `Session["Usuario"]` y `UserHelper` |
| Permisos | `CanAccess/CanCreate/CanEdit/CanDelete` por URL | Lo mismo **más** las reglas por área/planificación de `ReglasPermisosIvc`, combinadas en `GridPageBase` | IVC tiene reglas funcionales que `perms_json` no expresa |
| Grillas grandes | Carga progresiva + completado SADE en segundo plano | Igual; además `LoadData` server-side para Redeterminaciones y Formulación que hoy paginan en BD | En prod hay 8.750 reliquidaciones y 5.003 pagos |
| Export | ClosedXML con atributos `ExportIgnore` | Igual, exportando lo filtrado | Hoy exporta `DatosFiltrados()` |

---

## 5. Plan paso a paso

Cada fase termina con build + tests verdes, revisión del diff y prueba en pantalla contra `IVC_TEST`. La cookie compartida permite que el legado y la app nueva convivan con el mismo login (SAF ya convive con IVC); se usa para validar cada módulo en test. El corte a producción es big-bang (ver fase 9).

### Fase 0 — Cierre y decisiones (sin código nuevo)
1. PR `refactor/fase4-integridad-edicion` → `dev`; PR `dev` → `main`; despliegue del legado. Legado pasa a "sólo hotfix".
2. Decidir: nombre del repo y del proyecto (`IVC`), URLs de las páginas (tabla 3.1), si va auditoría de cambios, destino de `Presu-V2`.
3. Pedir al administrador de Auth.Web: `ApplicationClient` nuevo (ClientId, AllowedReturnUrls de test), `Pages` con las URLs nuevas y permisos iniciales para un usuario de prueba por área (DGDO planificación, Redeterminaciones, Secretaría, admin).
4. Pedir infraestructura: sitio IIS de test, usuario SQL con escritura sobre `IVC_TEST`.
5. Consultar al área las decisiones de 2.1.2 (pueden llegar durante las fases 1–5).

### Fase 1 — Esqueleto que loguea
Repo nuevo al lado de `Auth.Web`. Copiar de SAF: `Program.cs` + `Extensions/`, `Security/`, `Components/Account`, `Layout`, `Shared`, `GridPageBase`, `PermissionPageBase`, `PermissionService`, `PermissionVersionService`, `DataProtectionDbContext`, `DbContextHealthCheck`, `Tests/` (harness bUnit), `.gitignore`, README adaptado. Menú construido desde `perms_json` (equivalente a `Inicio.aspx`). `/healthz`. Entregable: app vacía que inicia sesión contra Auth.Web de test con el ClientId nuevo, muestra el menú según permisos y cierra sesión. Agregar la carpeta al `proyectos.code-workspace`.

### Fase 2 — Modelo de datos database-first
1. Scaffold de comparación contra `IVC_TEST` (`dotnet ef dbcontext scaffold` en un proyecto descartable) para verificar nombres de columnas, tipos, nulabilidad y claves reales de las 27 tablas + `MOVIMIENTOS_GESTION`, `LINEA_DE_GESTION_FF`, `BD_PROYECTOS`, `ABM_PLANI`.
2. Escribir las entidades a mano en `Data/Entities` con Fluent API en `IvcDbContext` (no se usa el scaffold tal cual). `HasNoKey` para `PASES_SADE`, `GEDO`, `DEVENGADOS`. `IsUnicode(false)` donde la columna sea `varchar`.
3. Repositorios por agregado (Obras, Autorizantes, Redeterminaciones, Certificados, Legítimos, Formulación, Catálogos, DatosExternos) con `IDbContextFactory`.
4. Warmup como SAF. Tests de repositorio con InMemory para lo que no dependa de SQL crudo.
Entregable: contexto que lee y escribe cada tabla en test, cubierto por un smoke test de lectura por entidad.

### Fase 3 — Identidad y permisos IVC
`IUsuarioActual` (claims → áreas IVC, upsert `UsuariosVinculados`, override de planificación), `ReglasPermisosIvc` pura con los tests portados de `UserHelperTests`/`UserHelperSesionTests`, `ABM_PLANI` como servicio. Primera página real: `/admin/configuracion` (planificación y formulación abiertas, overrides por usuario), sólo admin. Es chica y prueba todo el circuito: permisos, escritura, notificaciones.

### Fase 4 — Catálogos (las 8 páginas ADO)
Barrios, Empresas, Líneas de Gestión, Líneas FF, BdProyectos, Movimientos de Gestión sobre `GridPageBase` con edición inline, confirmación de borrado y errores visibles (hoy borran físico sin confirmar y tragan el error). Reemplaza la "Fase 4 ADO" pendiente del legado. Primer módulo publicable.

### Fase 5 — Obras
`/obras`: filtro por área, columnas financieras ocultas al área Redeterminaciones, acciones según `PuedeModificarPlanificacion`, filtros persistentes, export. Portar `ObraNegocioEF` + `CalculoObraNegocioEF` (los indicadores de montos por obra que hoy también calcula el SQL crudo de `ObraNegocio`).

### Fase 6 — Redeterminaciones (el núcleo)
1. Portar a `Application/Redeterminaciones` sin cambios de semántica: `ReglasRedeterminaciones`, `MapeoEstadoRedet`, `EstadoGeneral`, `UltimoPaseSade`, `BuzonEtapaMatcher` (a UTF-8), con sus tests. Incorporar aquí las decisiones del área que hayan llegado.
2. `DatosExternosService` (SADE, SIGAF, ACDIR) con caché por circuito o `IMemoryCache` acotada.
3. `AutorizantesService` y `CertificadosService` (hoy `ListarAutorizantesYRedeterminaciones` y `ListarCertificadosYReliquidaciones` duplican reglas: unificar sobre las reglas puras).
4. Páginas `/autorizantes` (edición inline de expediente y estado según `PuedeEditarSeguimientoAutorizante`), `/certificados`, `/redeterminaciones` (cross-area, `LoadData` server-side, carga SADE/SIGAF secuencial con estado ERROR visible), `/legitimos`.
5. **Prueba de paridad**: script que compara, sobre `IVC_TEST`, la salida de la app nueva contra `vw_PBI_Redeterminaciones`/`vw_PBI_Reliquidaciones` y contra la exportación Excel del legado (montos redeterminados, reliquidaciones, tope, SADE). Es la condición de salida de la fase: aquí hay dinero.

### Fase 7 — Formulación
Pivot por años desde `Formulacion:AnioBase`, paginado server-side, edición según `PuedeModificarFormulacion`, export. Mover el ciclo (`FormulacionCiclo`) a `Application/Formulacion` con tests.

### Fase 8 — Endurecimiento y documentación
Rendimiento con volúmenes de producción (restaurar `IVC` en test), revisión de seguridad (cabeceras ya vienen de SAF), README definitivo, `Docs/calculo-redeterminaciones.html` actualizado con las ubicaciones nuevas, mover `Database/` del repo viejo, CI opcional (`dotnet build` + `dotnet test`, decisión del usuario, hoy no hay CI en ningún proyecto hermano salvo lo que exista en Auth.Web).

### Fase 9 — Corte a producción (big-bang, decisión del 03/11/2026)
Las fases 4 a 7 se validan con usuarios en `IVC_TEST`, donde la app nueva y el legado conviven con el mismo login. Producción cambia de una vez cuando estén todas las páginas: alta del cliente, `Pages`, permisos y `AreaRoute` en Auth.Web de producción; sitio IIS de producción con su `web.config` de variables; legado en sólo lectura una o dos semanas (quitando permisos de escritura vía Auth.Web), luego apagado; repo `Instituto-de-la-Vivienda` archivado con un README que apunte al nuevo.

---

## 6. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Diferencias de cálculo en redeterminaciones (dinero) | Reglas puras portadas sin cambios + tests existentes + prueba de paridad contra vistas SQL y legado (fase 6.5) |
| Esquema IVC con claves no únicas, collations `varchar`, columnas con espacios (`FECHA ULTIMO PASE`) | Scaffold de verificación en fase 2; `HasNoKey`/`IsUnicode(false)`; lecciones ya documentadas en `SAF/Data/IvcDbContext.cs` |
| Rendimiento Blazor Server con grillas de miles de filas y llamadas a SADE/SIGAF por fila | Carga progresiva y completado en segundo plano de `GridPageBase` (SAF); `LoadData` server-side; probar con volumen de producción en fase 8 |
| Permisos: URLs nuevas obligan a reconfigurar Auth.Web; usuarios con `perms_json` viejo | Convención de URLs cerrada en fase 0; alta de páginas en Auth.Web antes de cada corte; `perms_version` fuerza re-login |
| Doble mantenimiento legado/nuevo durante meses | Legado congelado a hotfix tras fase 0; cada hotfix se anota en este documento para replicarlo en la fase correspondiente |
| Big-bang: el valor llega al final y el riesgo se concentra en el corte | Validación por módulo con usuarios en `IVC_TEST` durante las fases 4–7; ensayo del corte completo en test antes de producción |
| Cultura y formatos (`es-AR`, decimales con coma, negativos) | Configurar `RequestLocalization` fijo en `es-AR`; portar `MontoHelper` con sus tests |
| Logout centralizado (`/connect/logout` de Auth.Web vs `SignOut` + redirect de SAF) | Copiar el comportamiento de SAF, que ya está validado en producción |

---

## 7. Decisiones tomadas el 03/11/2026

| Tema | Decisión |
|---|---|
| Repo / proyecto | `IVC` (repo `ATILA2001/IVC`, `IVC.csproj`, namespace `IVC`), junto a Auth.Web, SAF y SAI |
| URLs | Minúsculas con guiones: `/obras`, `/autorizantes`, `/certificados`, `/redeterminaciones`, `/legitimos`, `/formulacion`, `/proyectos`, `/movimientos`, `/barrios`, `/empresas`, `/lineas-gestion`, `/lineas-gestion-ff`, `/admin/configuracion`, `/home` |
| Auditoría de cambios | Sí, desde la fase 1: tabla `CambiosAuditoria` en IVC creada por script en `Database/`, `GridPageBase` de SAF tal cual |
| `Presu-V2` | Se descarta; rama remota borrada el 03/11/2026 (último commit `1b82c2b`, por si hiciera falta recuperarla) |
| Corte a producción | **Big-bang**: la app nueva sale cuando estén todas las páginas. La fase 9 se ajusta: convivencia sólo en test para validar con usuarios |
| CI | Sí, desde la fase 1: workflow de build + test en PRs a `dev` y `main`, sin despliegue |
| Orden de módulos | Catálogos → Obras → Redeterminaciones → Formulación |
| Base de desarrollo | `IVC_TEST` con `dotnet user-secrets`; `appsettings.json` con claves vacías |

Pendientes de fase 0 que siguen abiertos: PR fase 4 → `dev` → `main` y despliegue (en curso el 03/11), dictámenes del área (2.1.2), limpieza de datos (2.1.4), alta del cliente y páginas en Auth.Web de test, sitio IIS de test y usuario SQL con escritura.
