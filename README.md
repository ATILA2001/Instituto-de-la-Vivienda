# IVC — Instituto de la Vivienda

Aplicación web interna para la gestión de obras, autorizantes, certificados, redeterminaciones de precios, legítimos abonos y formulación presupuestaria. Se autentica con la cookie compartida que emite Auth.Web (mismo esquema que SAI y SAF).

## Estructura

| Proyecto | Tecnología | Contenido |
|---|---|---|
| `Dominio` | .NET Framework 4.8, Entity Framework 6 | Entidades (`*EF.cs`), `IVCdbContext`, DTOs, anillo de claves de DataProtection |
| `Negocio` | .NET Framework 4.8 | Reglas de negocio y acceso a datos. Las clases `*NegocioEF` usan EF6; las clases `*Negocio` sin sufijo usan ADO.NET (`AccesoDatos`) y quedan sólo para Barrios, Empresas, Proyectos, Movimientos y Líneas de Gestión |
| `WebForms` | ASP.NET WebForms + OWIN | Páginas, master pages, controles (`CustomControls/`) y el pipeline de autenticación/autorización (`Global.cs`, `Startup.Auth.cs`, `Sso/`) |
| `Tests/IVC.Tests` | MSTest, net48 | Pruebas unitarias de `Negocio` |
| `Database/` | SQL | Vistas para Power BI que replican las reglas de redeterminaciones |
| `Docs/` | HTML | Documentación funcional del cálculo de redeterminaciones |

## Requisitos

- Visual Studio 2022 o superior con la carga de trabajo "Desarrollo de ASP.NET y web" (.NET Framework 4.8).
- SDK de .NET 8 o superior (sólo para `dotnet test` del proyecto de pruebas).
- SQL Server con la base `IVC` y acceso de lectura a la base de Auth.Web (anillo de claves de DataProtection).

## Configuración local

Hay dos archivos de configuración que **no se versionan** (están en `.gitignore` y excluidos del paquete de publicación). Se crean a partir de sus plantillas:

```
WebForms/connectionStrings.example.config  ->  WebForms/connectionStrings.config
WebForms/SharedCookie.example.config       ->  WebForms/SharedCookie.config
```

- `connectionStrings.config`: cadenas `IVC_TEST`, `IVC_PROD`, `ConnectionStringTest`, `ConnectionStringProd` y `AuthWebConnection`. Hoy el código elige test o prod por rama (`dev` usa `IVC_TEST`/`ConnectionStringTest`, `main` usa `IVC_PROD`/`ConnectionStringProd`), por lo que ambas entradas deben existir.
- `SharedCookie.config`: nombre de la cookie compartida, `ApplicationName` de DataProtection, URL base y `ClientId` de Auth.Web. Deben coincidir con la configuración de Auth.Web. Cada clave puede reemplazarse por la variable de entorno indicada en la plantilla.

Sin estos archivos la aplicación no arranca: `Startup.Auth.cs` lanza una excepción si falta el nombre de la cookie o el `ApplicationName`.

## Compilar y probar

```
msbuild IVC.sln -t:Build -p:Configuration=Debug
dotnet test Tests/IVC.Tests/IVC.Tests.csproj --no-build
```

La solución debe compilar sin errores. Los warnings `CS0168` que quedan corresponden a bloques `catch (Exception ex) { throw; }` de la capa ADO.NET.

## Ramas y flujo de trabajo

- `dev`: integración. Todo el desarrollo se mergea acá primero.
- `main`: producción. Sólo recibe merges desde `dev` o desde ramas `hotfix/*`.
- Ramas de trabajo: `feature/<tema>`, `fix/<tema>`, `hotfix/<tema>`.
- Un `hotfix/*` se mergea **a `main` y a `dev` desde la misma rama**. No aplicar el mismo cambio dos veces con commits distintos: deja a `dev` y `main` con historias divergentes.
- Mensajes de commit en formato Conventional Commits: `feat(...)`, `fix(...)`, `refactor(...)`, `docs(...)`, `chore(...)`.
- Al cerrar una rama mergeada, borrarla del remoto.

## Autenticación y permisos

- La cookie compartida la valida OWIN (`Startup.Auth.cs`) con el anillo de claves de Auth.Web.
- `Global.cs` autoriza cada request contra el claim `perms_json` (lista de páginas permitidas) y revalida `perms_version` contra Auth.Web cada 5 minutos, con tolerancia a fallos (fail-open) igual que SAI y SAF.
- El usuario de negocio (`UsuarioEF`) se construye desde los claims y se cachea en Session; se descarta si cambia el `NameIdentifier` autenticado.
- Las reglas de edición por rol y estado de planificación están centralizadas en `UserHelper.PuedeModificar*` y se verifican en cada handler que escribe datos, además de ocultar los controles en la interfaz.
