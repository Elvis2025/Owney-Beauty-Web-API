# Módulos de Owney Beauty

Cada carpeta de `modules/` es un **bounded context** (módulo ABP) con sus propias capas, su `DbContext` y su esquema de PostgreSQL. El host (`src/`) solo los compone. `platform` y `accounts` son la **plantilla de referencia**: copia su estructura para cada módulo nuevo.

```
modules/<modulo>/src/
  Ieh.OwneyBeauty.<Modulo>.Domain.Shared        enums, constantes, códigos de error, ETOs (eventos), localización es/en
  Ieh.OwneyBeauty.<Modulo>.Domain               agregados, domain services, seeders, settings
  Ieh.OwneyBeauty.<Modulo>.Application.Contracts DTOs, interfaces de app services, permisos
  Ieh.OwneyBeauty.<Modulo>.Application          app services (casos de uso) y mappers Mapperly
  Ieh.OwneyBeauty.<Modulo>.EntityFrameworkCore  I<Modulo>DbContext, <Modulo>DbContext, Configure<Modulo>()
```

## Reglas (verificadas por `test/Ieh.OwneyBeauty.ArchitectureTests`)

1. Un módulo **nunca** referencia el host ni las capas `Domain`, `Application` o `EntityFrameworkCore` de otro módulo.
2. De otro módulo solo se puede usar `Domain.Shared` (eventos, enums) y, desde la capa `Application`, su `Application.Contracts` (integration services).
3. Entre módulos se guardan **IDs**, no claves foráneas. Las reacciones entre módulos van por **eventos distribuidos** (`IDistributedEventBus` + outbox/inbox).
4. Las reglas de negocio viven en las entidades/domain services; los app services solo orquestan. Las entidades no se crean mapeando DTOs.
5. Cada acción sensible escribe en la auditoría de negocio (`IBusinessAuditLogger` del Shared Kernel).
6. Errores con `BusinessException` y código `Owney.<Modulo>:<Codigo>`, traducido en `Localization/<Modulo>/es.json` y `en.json`.

## Pasos para crear un módulo nuevo

1. Copiar `modules/platform` → `modules/<modulo>` y renombrar namespaces/proyectos.
2. Definir `<Modulo>DbProperties.DbSchema` (por ejemplo, `content`).
3. Registrar en el host:
   - Referencias de proyecto en `src/Ieh.OwneyBeauty.{Domain.Shared,Domain,Application.Contracts,Application,EntityFrameworkCore}`.
   - `[DependsOn]` en cada módulo del host.
   - `OwneyBeautyDbContext`: implementar `I<Modulo>DbContext`, añadir `[ReplaceDbContext(typeof(I<Modulo>DbContext))]`, los `DbSet` y `builder.Configure<Modulo>()`.
   - `OwneyBeautyHttpApiHostModule.ConfigureConventionalControllers`: `RootPath = "<modulo>"`, `RemoteServiceName = "<Modulo>"`.
   - `Application` del módulo: `context.Services.AddMapperlyObjectMapper<<Modulo>ApplicationModule>()`.
4. Añadir los 5 proyectos a `Ieh.OwneyBeauty.slnx` y como referencia de `test/Ieh.OwneyBeauty.ArchitectureTests`.
5. Crear la migración: `cd src/Ieh.OwneyBeauty.EntityFrameworkCore && dotnet ef migrations add <Nombre>`.
6. Pruebas: unitarias de dominio en `test/Ieh.OwneyBeauty.Domain.Tests/Owney`, de integración abstractas en `test/Ieh.OwneyBeauty.Application.Tests/Owney` y su clase concreta en `test/Ieh.OwneyBeauty.EntityFrameworkCore.Tests`.
7. Regenerar los proxies del frontend: `abp generate-proxy -t ng -m <modulo> -u <url-api>`.
