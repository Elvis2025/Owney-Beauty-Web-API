# Owney Beauty — Backend: ejecución local y despliegue en Railway

Estado: Sprints 0–2 (Fundación + Identidad y Cuentas). Rama: `develop`. Estructura de módulos: `docs/Owney_Beauty_Estructura_Modulos.md`.

## 1. Qué hay en el backend

| Pieza | Proyecto | Función |
|---|---|---|
| Común | `src/*/Common` | Money, GeoPoint, DateTimeRange, códigos de error, `IBusinessAuditLogger` |
| Módulo Platform | `src/*/Modules/Platform` | Geografía RD, monedas, impuestos (ITBIS), idempotencia, auditoría de negocio, feature flags, settings |
| Módulo Accounts | `src/*/Modules/Accounts` | Registro, verificación de correo/teléfono, roles, cuenta híbrida y modos, documentos legales, consentimientos, alertas de seguridad |
| API | `src/Ieh.OwneyBeauty.HttpApi.Host` | REST + OAuth (OpenIddict), rate limiting, idempotencia, health checks, Swagger |
| Worker | `src/Ieh.OwneyBeauty.Worker` | Outbox/inbox de eventos, background jobs (correos), workers periódicos |
| Migrador | `src/Ieh.OwneyBeauty.DbMigrator` | Migraciones EF Core + datos semilla (roles, geografía, monedas, ITBIS, clientes OAuth) |
| Infraestructura | `src/Ieh.OwneyBeauty.Infrastructure` | Redis (cache/locks), OpenTelemetry, interruptor de procesamiento en segundo plano |

Endpoints: Swagger en `/swagger` · salud: `/health/live` (proceso) y `/health-status` (base de datos y Redis).

## 2. Ejecutar en local

Compilado en **Debug** (Visual Studio o `dotnet run`), el Host, el DbMigrator y el Worker usan el ambiente **Development**:

- Leen `appsettings.Development.json`, que apunta a la base local `Host=localhost;Port=55432;Database=OwneyBeauty;User ID=postgres;Password=postgres;`.
- **No** cargan `appsettings.secrets.json` (credenciales de Railway); ese archivo solo se usa fuera de Development.
- Si defines `DOTNET_ENVIRONMENT`/`ASPNETCORE_ENVIRONMENT`, ese valor manda. La imagen de Docker (Release) usa `Production`.

> El puerto 5432 suele estar ocupado por un PostgreSQL instalado en Windows con otra contraseña; por eso la base de desarrollo usa el 55432.

Base de datos local con Docker:

```bash
docker run -d --name owney-pg-local -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=OwneyBeauty -p 55432:5432 postgres:16-alpine
```

Luego, en Debug:

```bash
dotnet run --project src/Ieh.OwneyBeauty.DbMigrator      # migra y siembra (o F5 en Visual Studio)
dotnet run --project src/Ieh.OwneyBeauty.HttpApi.Host    # API en https://localhost:44389
dotnet test Ieh.OwneyBeauty.slnx                          # dominio + integración + arquitectura
```

Usuario administrador inicial: `admin` / `1q2w3E*` (**cámbialo** en cualquier entorno compartido).

## 3. Despliegue en Railway

### 3.1 Servicios

| Servicio | Archivo de configuración | Comando | Notas |
|---|---|---|---|
| `owney-api` | `railway/api.railway.json` | `/app/start-api.sh` | Pre-deploy: `/app/start-migrator.sh` (migra antes de cada despliegue). Health check `/health/live` |
| `owney-worker` | `railway/worker.railway.json` | `/app/start-worker.sh` | Sin puerto público |
| PostgreSQL | Plugin de Railway | — | Activar backups. Extensiones futuras: `postgis`, `pg_trgm`, `btree_gist` |
| Redis | Plugin de Railway | — | Necesario cuando la API tenga más de una réplica |

En cada servicio: **Settings → Config-as-code → Railway config file** = la ruta del archivo de la tabla. Ambos usan el `Dockerfile` de la raíz (una sola imagen con API, Worker y Migrador).

### 3.2 Variables de entorno

| Variable | API | Worker | Valor |
|---|---|---|---|
| `ConnectionStrings__Default` | ✅ | ✅ | `Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};User ID=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}};SSL Mode=Require;Trust Server Certificate=true` |
| `StringEncryption__DefaultPassPhrase` | ✅ | ✅ | Frase aleatoria **idéntica** en ambos servicios (cifra settings sensibles) |
| `App__SelfUrl` | ✅ | | URL pública de la API (`https://api.owneybeauty.com`) |
| `App__AngularUrl` | ✅ | | URL pública de la web |
| `App__CorsOrigins` | ✅ | | Orígenes permitidos separados por coma |
| `App__RedirectAllowedUrls` | ✅ | | URL de la web |
| `AuthServer__Authority` | ✅ | | Igual que `App__SelfUrl` |
| `AuthServer__RequireHttpsMetadata` | ✅ | | `false` (Railway termina TLS en su proxy) |
| `AuthServer__CertificateBase64` | ✅ | | Certificado `.pfx` de OpenIddict en base64 (ver §3.3) |
| `AuthServer__CertificatePassPhrase` | ✅ | | Contraseña del `.pfx` |
| `BackgroundProcessing__IsEnabled` | `false` | `true` | La API no procesa jobs; el Worker sí |
| `Redis__IsEnabled` / `Redis__Configuration` | ✅ | ✅ | `true` / `${{Redis.REDIS_URL}}` sin el prefijo `redis://` (formato `host:port,password=...`) |
| `OpenIddict__Applications__OwneyBeauty_App__RootUrl` | ✅ (migrador) | | URL de la web (redirect de OAuth) |
| `OpenIddict__Applications__OwneyBeauty_App__AllowPasswordFlow` | ✅ (migrador) | | `true` mientras la web use el login propio; `false` al pasar a PKCE |
| `OpenIddict__Applications__OwneyBeauty_Swagger__RootUrl` | ✅ (migrador) | | URL pública de la API |
| `Settings__Abp.Mailing.Smtp.*` | ✅ | ✅ | Proveedor de correo (host, puerto, usuario, contraseña, `Abp.Mailing.DefaultFromAddress`) |
| `OpenTelemetry__Otlp__Endpoint` | opcional | opcional | Colector OTLP (Grafana Cloud, Honeycomb…) |

> El migrador corre como pre-deploy del servicio API, así que usa las variables de ese servicio.

### 3.3 Certificado de OpenIddict (producción)

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p "<contraseña-fuerte>"
base64 -w0 openiddict.pfx > openiddict.pfx.b64      # PowerShell: [Convert]::ToBase64String([IO.File]::ReadAllBytes("openiddict.pfx"))
```

Copia el contenido a `AuthServer__CertificateBase64` y la contraseña a `AuthServer__CertificatePassPhrase`. **No subas el .pfx al repositorio** (ya está en `.gitignore`).

### 3.4 Checklist antes del primer despliegue (⬜ Pendiente — se hará antes de publicar la app)

- [ ] **Rotar la contraseña de PostgreSQL en Railway.** La anterior quedó en el historial de git.
- [ ] Respaldar la base actual antes de aplicar la migración `Owney_Foundation_S0_S2`, que elimina las tablas de ejemplo y convierte las fechas a `timestamptz`.
- [ ] Generar el certificado de OpenIddict y la frase de `StringEncryption`.
- [ ] Configurar SMTP (sin SMTP en producción, los códigos de verificación no llegan).
- [ ] Cambiar la contraseña del usuario `admin`.
- [ ] Publicar en `/api/accounts/legal-document` los Términos y la Política de privacidad aprobados por el abogado (el registro exige aceptarlos).
