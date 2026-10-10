# Owney Beauty — Estructura de módulos del backend

El backend usa **una sola solución ABP 10 por capas** (la plantilla estándar de ABP). Los módulos de negocio **no son proyectos aparte**: cada módulo es una carpeta `Modules/<Módulo>` dentro de cada capa, igual que en Voucher Warehouse.

## 1. Dónde va cada cosa

```
src/
  Ieh.OwneyBeauty.Domain.Shared/
    Common/                         OwneyErrorCodes (convención Owney.<Módulo>:<Código>)
    Localization/OwneyBeauty/       es.json / en.json  ← textos de TODOS los módulos
    Modules/<Módulo>/               <Módulo>Consts, <Módulo>ErrorCodes, <Módulo>SettingNames, <Módulo>DbProperties
      Enums/                        enums públicos
      Events/                       ETOs (eventos distribuidos)

  Ieh.OwneyBeauty.Domain/
    Common/                         Money, GeoPoint, DateTimeRange, Auditing/IBusinessAuditLogger
    Modules/<Módulo>/
      Models/                       entidades y agregados
      Managers/                     domain services (reglas que involucran varias entidades)
      Services/ Handlers/           servicios de dominio técnicos y manejadores de eventos
      Data/                         data seed contributors
      Settings/                     SettingDefinitionProvider

  Ieh.OwneyBeauty.Application.Contracts/
    Modules/<Módulo>/               <Módulo>Permissions + <Módulo>PermissionDefinitionProvider (y features)
      <Funcionalidad>/Dto/          DTOs de entrada y salida (uno por archivo)
      <Funcionalidad>/Service/      I<Algo>AppService
      Integration/                  (cuando haga falta) I<Algo>IntegrationService para lecturas entre módulos

  Ieh.OwneyBeauty.Application/
    Modules/<Módulo>/
      <Funcionalidad>/Service/      <Algo>AppService : OwneyBeautyAppService
      <Funcionalidad>/Mappers/      mappers de Mapperly (Entidad → DTO)
      Helpers/                      utilidades propias del módulo

  Ieh.OwneyBeauty.EntityFrameworkCore/EntityFrameworkCore/
    OwneyBeautyDbContext.cs         un único DbContext: DbSet de todos los módulos
    Modules/<Módulo>/<Módulo>ModelBuilderExtensions.cs   builder.Configure<Módulo>() (tablas en el esquema del módulo)
```

Namespaces = ruta de carpetas: `Ieh.OwneyBeauty.Modules.Platform.Geography.Dto`, `Ieh.OwneyBeauty.Modules.Accounts.Models`, etc.

Módulos actuales:

| Módulo | Esquema BD | Ruta API | Funcionalidades |
|---|---|---|---|
| `Platform` | `platform` | `/api/platform/...` | Geography, Currencies, TaxRates, BusinessAudit, idempotencia, feature flags |
| `Accounts` | `accounts` | `/api/accounts/...` | Registration, Verification, LegalDocuments, MyAccount, roles, alertas de seguridad |

## 2. Reglas (verificadas por `test/Ieh.OwneyBeauty.ArchitectureTests`)

1. Un módulo **solo** puede usar de otro módulo: sus constantes (raíz), `Enums`, `Events` e `Integration`. Nunca sus `Models`, `Managers`, `Service`, `Mappers`, etc.
2. Las reacciones entre módulos van por **eventos distribuidos** (`IDistributedEventBus` + outbox/inbox). Entre módulos se guardan **IDs**, no claves foráneas.
3. `Common` no depende de ningún módulo.
4. Domain y Application no dependen de EF Core.
5. App services e interfaces en `Service/`, DTOs en `Dto/`, mappers en `Mappers/`, entidades en `Models/`.
6. Las reglas de negocio viven en las entidades y managers; los app services solo orquestan. Las entidades no se crean mapeando DTOs.
7. Cada acción sensible escribe en la auditoría de negocio (`IBusinessAuditLogger`).
8. Errores con `BusinessException` y código `Owney.<Módulo>:<Código>`, traducido en `Localization/OwneyBeauty/es.json` y `en.json`.

## 3. Pasos para crear un módulo nuevo (ej. `Content`)

1. **Domain.Shared** → `Modules/Content/`: `ContentConsts`, `ContentErrorCodes` (namespace `Owney.Content`), `ContentDbProperties.DbSchema = "content"`; enums y ETOs en sus carpetas. Registrar el namespace de errores en `OwneyBeautyDomainSharedModule` (`MapCodeNamespace`).
2. **Domain** → `Modules/Content/Models`, `Managers`, `Data`, `Settings`.
3. **Application.Contracts** → `Modules/Content/ContentPermissions.cs` + provider; `Modules/Content/<Funcionalidad>/Dto` y `/Service`.
4. **Application** → `Modules/Content/<Funcionalidad>/Service` (heredan de `OwneyBeautyAppService`) y `/Mappers`.
5. **EntityFrameworkCore** → `Modules/Content/ContentModelBuilderExtensions.cs`; en `OwneyBeautyDbContext` añadir los `DbSet` y `builder.ConfigureContent()`.
6. **Host** → añadir `"Content"` a `OwneyModules` en `OwneyBeautyHttpApiHostModule` (expone `/api/content/...`).
7. **Textos** → claves en `Localization/OwneyBeauty/es.json` y `en.json`.
8. **Migración** → `cd src/Ieh.OwneyBeauty.EntityFrameworkCore && dotnet ef migrations add <Nombre>`.
9. **Pruebas** → dominio en `test/Ieh.OwneyBeauty.Domain.Tests/Owney`, integración en `test/Ieh.OwneyBeauty.Application.Tests/Owney` (+ clase concreta en `EntityFrameworkCore.Tests`). Las de arquitectura descubren el módulo solas.
10. **Proxies del frontend** → `abp generate-proxy -t ng -m content -u <url-api>`.
