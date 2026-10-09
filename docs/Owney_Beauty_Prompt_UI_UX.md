# Owney Beauty — Prompt de implementación UI/UX (Angular)

> **Cómo usar este documento:** copia la sección **«PROMPT»** completa y pégala en una sesión nueva de tu asistente de código, abierta en la carpeta `Owney Beauty Web UI`. El resto del documento es el anexo de referencia que el prompt cita (contrato de la API, sistema de diseño y temas). Mantén el archivo en el repositorio para que el asistente pueda leerlo.

| Campo | Valor |
|---|---|
| Producto | Owney Beauty — red social + marketplace de belleza (República Dominicana) |
| Frontend | `Owney Beauty Web UI` — Angular 22 standalone + signals, ABP Angular 10.6, Tailwind CSS 4 |
| Backend | `Owney Beauty Web API` — ABP 10.6 / .NET 10, rama `feature/sprint-0-1-2-fundacion` |
| Diseño de referencia | Prototipo: https://owney-beauty-staging.b12sites.com/ (selección de rol → Cliente / Profesional / Administrador) |
| Fecha | 2026-10-08 |

---

## PROMPT

```text
Eres un ingeniero frontend senior especializado en Angular 22 (standalone, signals, control flow @if/@for),
ABP Framework Angular 10.6, Tailwind CSS 4, accesibilidad (WCAG 2.2 AA) y diseño de producto móvil-primero.

Vas a reconstruir la interfaz de OWNEY BEAUTY, una RED SOCIAL de belleza con marketplace de servicios
(República Dominicana). El repositorio actual es `Owney Beauty Web UI`. Lee primero, en este orden:
  1. docs/Owney_Beauty_Prompt_UI_UX.md (este documento: contrato de API, diseño y temas) — si no está
     en este repo, está en `../Owney Beauty Web API/docs/`.
  2. .abpstudio/ai-rules/app.mdc y .cursor/rules/** (reglas del proyecto ABP).
  3. src/app/** (estado actual: login propio + una maqueta en AppComponent con datos fijos).

OBJETIVO
Construir una aplicación web responsive (PWA) que:
  A) Consuma DE VERDAD los endpoints ya disponibles en la API (Anexo A): registro con aceptación de
     documentos legales, inicio de sesión, verificación de correo/teléfono, cuenta híbrida (cambio de
     modo Cliente ↔ Profesional), consentimientos, recuperación de contraseña, y los mantenimientos
     administrativos (geografía, monedas, impuestos, documentos legales y auditoría de negocio).
  B) Implemente TODAS las pantallas del prototipo (Anexo C) con el mismo estilo, usando datos de ejemplo
     detrás de servicios con interfaz definida (para conectarlos a la API real cuando existan los
     módulos de Perfiles, Contenido, Feed, Reservas, etc.).
  C) Sea multitema: 8 temas de marca, cada uno con variante clara y oscura (Anexo B), cambiables en
     tiempo de ejecución y persistidos.

REGLAS NO NEGOCIABLES
  - Angular standalone + signals + ChangeDetectionStrategy.OnPush en todos los componentes. Nada de NgModules nuevos.
  - Tailwind 4 con tokens CSS (variables) del Anexo B. Ningún color "hardcodeado" en componentes:
    siempre clases que apunten a tokens (bg-background, text-foreground, bg-primary, border-border...).
  - Proxies de la API: genéralos con la herramienta/CLI de ABP (`abp generate-proxy -t ng -m accounts`
    y `-m platform`, con la API corriendo). NUNCA escribas proxies a mano. Si no puedes generarlos,
    crea servicios temporales en `src/app/core/api-temp/` con EXACTAMENTE los DTOs del Anexo A y
    márcalos con `// TODO: reemplazar por proxy generado`.
  - Textos en español dominicano como idioma por defecto, preparado para inglés (ABP localization:
    `::Clave` o los recursos `Accounts::` / `Platform::`). No mezcles idiomas en una misma vista.
  - Nunca mostrar que un pago está "en escrow": usar "Pago protegido" (decisión legal del producto).
  - La multi-tenencia está DESACTIVADA en la API: elimina el campo "Código de empresa" del login y el
    TenantContextService.
  - Seguridad: nunca guardes contraseñas; tokens solo vía el OAuthService de ABP; no registres datos
    personales en consola; todos los formularios con validación cliente + mostrar errores del servidor.
  - Accesibilidad: foco visible, navegación por teclado, labels/aria, contraste AA en TODOS los temas,
    `prefers-reduced-motion` respetado, objetivos táctiles ≥ 44 px.
  - Rendimiento: rutas lazy por feature, imágenes con `NgOptimizedImage`, skeletons en cargas, sin
    librerías de UI pesadas adicionales (componentes propios con Tailwind; Angular CDK permitido).

ARQUITECTURA DE CARPETAS (espejo de los módulos del backend)
  src/app/
    core/          auth (OAuth, guards por permiso y por modo), http (interceptores), theme, i18n, layout
    shared/ui/     componentes base (Anexo B §B.5)
    features/
      auth/        login, registro (multipaso), verificación, recuperar contraseña, reset-password
      onboarding/  selección de rol, aceptación de documentos legales pendientes
      account/     mi cuenta, modos, privacidad y consentimientos, seguridad
      social/      feed, explorar, comunidad, publicación (MOCK hasta módulo CNT/FED)
      discover/    búsqueda, filtros, perfil de profesional (MOCK hasta módulos PRF/FED)
      booking/     flujo de reserva, mis citas, reseñas (MOCK hasta módulos BKG/PAY/REP)
      pro/         panel profesional: agenda, servicios, promociones, perfil (MOCK)
      admin/       dashboard (MOCK), geografía, monedas, impuestos, documentos legales, auditoría (REAL)
      legal/       visor público de términos/privacidad

INTERCEPTORES / SERVICIOS TRANSVERSALES
  - Accept-Language: idioma activo (es/en).
  - Idempotency-Key: genera un UUID v4 por cada operación de escritura "crítica" (registro, publicar
    documento, crear/editar mantenimientos, y en el futuro reservas y pagos). Reutiliza la MISMA clave
    si el usuario reintenta la misma operación (p. ej. tras un error de red). Ver Anexo A §A.3.
  - Errores: mapear el formato de error de ABP (Anexo A §A.3) a toasts y a errores de campo. 401 →
    renovar token o ir a login; 403 → página "Sin permiso"; 429 → mensaje con el Retry-After; 409/422
    de idempotencia → mensajes específicos.
  - Permisos: usar `ConfigStateService` / `PermissionService` de ABP (`/api/abp/application-configuration`)
    para mostrar/ocultar menús y proteger rutas. Nunca decidir permisos por el modo activo.

FLUJOS QUE DEBEN FUNCIONAR CONTRA LA API REAL (criterios de aceptación)
  1. Registro: paso 1 datos (nombre, correo o teléfono E.164, contraseña con la política del Anexo A),
     paso 2 "¿Qué buscas?" (Cliente: "Reserva servicios de belleza" / Profesional: "Ofrece tus
     servicios"), paso 3 aceptar documentos legales vigentes (cargados de
     GET /api/accounts/legal-document/current, cada uno abrible en un visor) + consentimiento de
     marketing opcional. Errores genéricos del servidor se muestran tal cual (no revelar cuentas).
  2. Tras registrarse: iniciar sesión automáticamente y llevar a "Verifica tu correo" con un input de
     6 dígitos (pegar el código completo, autoavance, reenvío con cuenta regresiva de 60 s que respeta
     el error VerificationResendTooSoon y su dato `seconds`).
  3. Login: usuario/correo + contraseña (password grant de transición). Dejar preparado el flujo
     Authorization Code + PKCE (ver Anexo A §A.1) detrás de un flag de environment.
  4. Recuperar contraseña: POST /api/account/send-password-reset-code y ruta
     /account/reset-password?userId=&resetToken= que llama a verify-password-reset-token y reset-password.
  5. Mi cuenta: GET /api/accounts/my-account. Selector de modo (solo visible si availableModes > 1).
     "Quiero ofrecer servicios" → enable-mode (2) → switch-mode (2). Si pendingLegalDocuments no está
     vacío, bloquear la app con un modal de aceptación (accept-legal-documents).
  6. Privacidad: listar y cambiar consentimientos (GET consents / PUT consent).
  7. Verificar teléfono desde Mi cuenta (send-phone-code con número opcional + confirm-phone).
  8. Administración (solo con permisos): CRUD de provincias y municipios (sin borrar: activar/desactivar),
     monedas (marcar predeterminada), impuestos con vigencia, documentos legales (borrador → publicar;
     publicado = solo lectura), y tabla de auditoría de negocio con filtros y paginación.

NAVEGACIÓN (Anexo C §C.1)
  - Móvil: barra inferior fija (Inicio, Buscar, Crear/Comunidad, Citas, Perfil) + header compacto.
  - Escritorio: header con navegación (Inicio, Buscar, Social, Citas, Mi panel) para cliente;
    sidebar para Profesional y Administración.
  - El modo activo (Cliente/Profesional) cambia el menú y la home, nunca los permisos.

TEMAS
  Implementa el ThemeService del Anexo B §B.3: 8 temas × {claro, oscuro, sistema}. Selector accesible
  en el header (icono paleta) y en Mi cuenta → Apariencia. Persistir en localStorage
  (`owney.theme`, `owney.mode`). Sin parpadeo al cargar (script inline en index.html que aplica la
  clase antes de que Angular arranque). Respetar prefers-color-scheme cuando el modo sea "sistema".

ENTREGA POR FASES (haz commits pequeños por fase y verifica `npm run build` + `ng lint` en cada una)
  F1 Fundaciones: tokens y temas, tipografías, ThemeService, layout, componentes base, interceptores.
  F2 Auth real: login, registro multipaso, verificación, recuperar/restablecer contraseña, onboarding legal.
  F3 Cuenta real: mi cuenta, modos, privacidad/consentimientos, apariencia.
  F4 Administración real: mantenimientos de Platform y Accounts + auditoría.
  F5 Prototipo completo con mocks: home, búsqueda, perfil profesional, reserva, mis citas, favoritos,
     notificaciones, comunidad, panel profesional y dashboard admin.
  F6 Calidad: accesibilidad (axe), estados vacíos/errores/carga en todas las vistas, pruebas unitarias
     (vitest) de servicios e interceptores, revisión de contraste de los 16 esquemas de color.

Al terminar cada fase, resume qué endpoints se consumen, qué queda con mock y cualquier desajuste
encontrado con el contrato del Anexo A (no inventes campos: si algo falta en la API, repórtalo).
```

---

## Anexo A — Contrato de la API disponible (Sprints 0–2)

### A.1 Autenticación (OpenIddict)

| Elemento | Valor |
|---|---|
| Issuer / API | Desarrollo: `https://localhost:44389` (o la URL pública de Railway) |
| Cliente | `OwneyBeauty_App` (público, sin secreto) |
| Scopes | `openid profile email phone roles offline_access OwneyBeauty` |
| Access token | 15 minutos · Refresh token: 14 días, **rotativo** (cada refresh devuelve uno nuevo) |
| Flujo recomendado | Authorization Code + **PKCE obligatorio** (`responseType: 'code'`) |
| Flujo de transición | Password grant (`responseType: 'password'`), habilitado mientras exista el login propio |
| Política de contraseña | Mínimo 8 caracteres, al menos 1 dígito y 1 minúscula. Bloqueo de 15 min tras 5 intentos fallidos |
| Login fallido | `400` con mensaje genérico (no revela si la cuenta existe) |
| Límite | 10 intentos/minuto por IP en login, registro y recuperación → `429` + `Retry-After` |

`environment.ts` (ejemplo para PKCE):

```ts
oAuthConfig: {
  issuer: 'https://localhost:44389/',
  redirectUri: baseUrl,
  clientId: 'OwneyBeauty_App',
  responseType: 'code',           // PKCE; usar 'password' solo para el login propio de transición
  scope: 'openid profile email phone roles offline_access OwneyBeauty',
  requireHttps: true,
},
apis: {
  default:  { url: 'https://localhost:44389', rootNamespace: 'Ieh.OwneyBeauty' },
  Accounts: { url: 'https://localhost:44389', rootNamespace: 'Ieh.OwneyBeauty.Accounts' },
  Platform: { url: 'https://localhost:44389', rootNamespace: 'Ieh.OwneyBeauty.Platform' },
},
```

> La multi-tenencia está desactivada. No enviar `__tenant`.

### A.2 Convenciones

- JSON en **camelCase**. Los **enums se envían y reciben como números**.
- Fechas en **UTC ISO-8601** (`2026-10-09T01:50:49Z`). La UI convierte a `America/Santo_Domingo` para mostrar.
- Listas paginadas: `?SkipCount=0&MaxResultCount=10&Sorting=name` → `{ totalCount, items: [] }`. `MaxResultCount` ≤ 100.
- Enviar siempre `Accept-Language: es` o `en`: los mensajes de error vuelven traducidos.

### A.3 Errores, idempotencia y límites

Formato de error de ABP (todas las respuestas 4xx/5xx de negocio):

```json
{
  "error": {
    "code": "Owney.Accounts:VerificationResendTooSoon",
    "message": "Espera 42 segundos antes de pedir otro código.",
    "details": null,
    "data": { "seconds": 42 },
    "validationErrors": [ { "message": "...", "members": ["email"] } ]
  }
}
```

| HTTP | Significado | Qué hace la UI |
|---|---|---|
| 400 | Validación de datos (`validationErrors`) | Marcar campos |
| 401 | Sin sesión / token vencido | Refrescar token o ir a login |
| 403 | Regla de negocio (`code` presente) o sin permiso | Toast con `message`; sin `code` → página "Sin permiso" |
| 404 | Recurso inexistente | Estado vacío |
| 409 | `Owney.Platform:IdempotentRequestInProgress` | "Procesando…", reintentar en segundos con la misma clave |
| 422 | `Owney.Platform:IdempotencyKeyReused` | Generar una clave nueva (cambió el contenido) |
| 429 | Demasiadas solicitudes | Mostrar mensaje y esperar `Retry-After` segundos |

**Idempotencia:** en escrituras críticas envía `Idempotency-Key: <uuid-v4>`. Si repites la misma petición con la misma clave, la API devuelve la respuesta original con el encabezado `Idempotent-Replayed: true` y **no** la ejecuta de nuevo.

Códigos de error de negocio disponibles:

| Código | Cuándo |
|---|---|
| `Owney.Accounts:RegistrationFailed` | Registro no completado (genérico, p. ej. correo/teléfono ya usado) |
| `Owney.Accounts:EmailOrPhoneRequired` | Falta correo y teléfono |
| `Owney.Accounts:UserNameTaken` | Nombre de usuario ocupado |
| `Owney.Accounts:LegalDocumentsNotAccepted` | No se aceptaron los documentos vigentes |
| `Owney.Accounts:ModeNotAvailable` / `ModeNotSupportedYet` | Modo no habilitado / Negocio aún no disponible |
| `Owney.Accounts:NoEmailToVerify` / `AlreadyVerified` | Cuenta sin correo / ya verificado |
| `Owney.Accounts:VerificationCodeInvalid` / `VerificationCodeExpired` / `VerificationTooManyAttempts` | Código incorrecto / vencido / bloqueado |
| `Owney.Accounts:VerificationResendTooSoon` (`data.seconds`) / `VerificationDailyLimitReached` | Reenvío antes de 60 s / límite diario (10) |
| `Owney.Accounts:PhoneNumberInvalid` | Teléfono sin formato E.164 |
| `Owney.Accounts:LegalDocumentAlreadyPublished` / `LegalDocumentNotPublished` / `LegalDocumentVersionExists` | Reglas de documentos legales |
| `Owney.Platform:DuplicateCode` (`data.code`) | Código repetido en mantenimientos |
| `Owney.Platform:CannotDeactivateDefaultCurrency` / `InvalidTaxRate` | Reglas de monedas / impuestos |

### A.4 Enums

| Enum | Valores |
|---|---|
| `AccountMode` | `1` Cliente · `2` Profesional · `3` Negocio (aún no habilitado) |
| `OnboardingStep` | `0` RoleSelection · `1` Profile · `2` Verification · `3` Completed |
| `VerificationChannel` | `1` Email · `2` Phone |
| `LegalDocumentType` | `1` Términos · `2` Privacidad · `3` Cancelación · `4` Acuerdo con profesionales · `5` Política de contenido |
| `ConsentType` | `1` Tratamiento de datos · `2` Marketing · `3` Ubicación |

### A.5 Módulo Accounts — `/api/accounts/...`

| Método y ruta | Auth | Cuerpo / query | Respuesta |
|---|---|---|---|
| `POST /api/accounts/registration/register` | Pública | `RegisterDto` | `RegistrationResultDto` |
| `GET /api/accounts/my-account` | Usuario | — | `MyAccountDto` |
| `POST /api/accounts/my-account/switch-mode` | Usuario | `{ mode }` | `MyAccountDto` |
| `POST /api/accounts/my-account/enable-mode` | Usuario | `{ mode }` | `MyAccountDto` |
| `POST /api/accounts/my-account/advance-onboarding` | Usuario | `{ step }` | `MyAccountDto` |
| `POST /api/accounts/my-account/accept-legal-documents` | Usuario | `{ legalDocumentIds: [guid] }` | `MyAccountDto` |
| `GET /api/accounts/my-account/consents` | Usuario | — | `{ items: ConsentStatusDto[] }` |
| `PUT /api/accounts/my-account/consent` | Usuario | `{ type, granted }` | `ConsentStatusDto` |
| `POST /api/accounts/verification/send-email-code` | Usuario | — | `VerificationCodeSentDto` |
| `POST /api/accounts/verification/confirm-email` | Usuario | `{ code: "123456" }` | `204` |
| `POST /api/accounts/verification/send-phone-code` | Usuario | `{ phoneNumber?: "+18095551234" }` | `VerificationCodeSentDto` |
| `POST /api/accounts/verification/confirm-phone` | Usuario | `{ code }` | `204` |
| `GET /api/accounts/legal-document/current?language=es` | Pública | — | `{ items: PublicLegalDocumentDto[] }` |
| `GET /api/accounts/legal-document` | `Accounts.LegalDocuments.Manage` | `Type, Language, IsCurrent, Sorting, SkipCount, MaxResultCount` | `PagedResult<LegalDocumentDto>` |
| `GET /api/accounts/legal-document/{id}` | ídem | — | `LegalDocumentDto` |
| `POST /api/accounts/legal-document` | ídem | `CreateLegalDocumentDto` | `LegalDocumentDto` (borrador) |
| `PUT /api/accounts/legal-document/{id}` | ídem | `UpdateLegalDocumentDto` (solo borradores) | `LegalDocumentDto` |
| `POST /api/accounts/legal-document/{id}/publish` | ídem | — | `LegalDocumentDto` (vigente) |

```ts
interface RegisterDto {
  email?: string;               // email o phoneNumber (al menos uno)
  phoneNumber?: string;         // E.164: /^\+[1-9]\d{7,14}$/
  userName?: string;            // opcional, 3–64, [a-zA-Z0-9._]; si falta se genera
  name: string;                 // ≤ 64
  surname?: string;
  password: string;             // 8–128
  initialMode: 1 | 2;           // Cliente | Profesional
  acceptedLegalDocumentIds: string[];
  marketingConsent: boolean;
  language: 'es' | 'en';
}
interface RegistrationResultDto { userId: string; userName: string; activeMode: number;
  requiresEmailVerification: boolean; requiresPhoneVerification: boolean; }
interface MyAccountDto {
  id: string; userName: string; name?: string; surname?: string;
  email?: string;               // null si la cuenta se registró solo con teléfono
  emailConfirmed: boolean; phoneNumber?: string; phoneNumberConfirmed: boolean;
  roles: string[];              // client, professional, admin, kyc-analyst, support, moderator, finance, marketing...
  availableModes: number[]; activeMode: number; onboardingStep: number;
  pendingLegalDocuments: PublicLegalDocumentDto[];
}
interface VerificationCodeSentDto { channel: 1 | 2; maskedTarget: string; expiresAt: string; }
interface ConsentStatusDto { type: 1 | 2 | 3; granted: boolean; updatedAt?: string; }
interface PublicLegalDocumentDto { id: string; type: number; language: string; version: string;
  title: string; content?: string /* Markdown */; externalUrl?: string; requiresAcceptance: boolean; publishedTime?: string; }
interface LegalDocumentDto extends PublicLegalDocumentDto { isCurrent: boolean;
  creationTime: string; creatorId?: string; lastModificationTime?: string; }
interface CreateLegalDocumentDto { type: number; language: string; version: string; title: string;
  content?: string; externalUrl?: string; requiresAcceptance: boolean; }
interface UpdateLegalDocumentDto { title: string; content?: string; externalUrl?: string; requiresAcceptance: boolean; }
```

> Después de registrarse, el usuario recibe automáticamente un código por correo (o SMS si solo dio teléfono). El código vence en 10 minutos, admite 5 intentos y se puede reenviar cada 60 segundos.

### A.6 Módulo Platform — `/api/platform/...`

| Método y ruta | Auth | Notas |
|---|---|---|
| `GET /api/platform/geography/provinces` | Pública | 32 provincias activas, ordenadas por nombre |
| `GET /api/platform/geography/municipalities/{provinceId}` | Pública | Municipios activos de la provincia |
| `GET /api/platform/currency/active` | Pública | Monedas activas; la predeterminada primero (DOP) |
| `GET/POST /api/platform/province`, `GET/PUT /api/platform/province/{id}` | `Platform.Geography.Manage` | Filtros: `Filter`, `IsActive`. No hay borrado: usar `isActive` |
| `GET/POST /api/platform/municipality`, `GET/PUT /api/platform/municipality/{id}` | `Platform.Geography.Manage` | Filtros: `ProvinceId`, `Filter`, `IsActive` |
| `GET/POST /api/platform/currency`, `GET/PUT /api/platform/currency/{id}`, `POST /api/platform/currency/{id}/set-default` | `Platform.Currencies.Manage` | Solo una predeterminada; no se puede desactivar la predeterminada |
| `GET/POST /api/platform/tax-rate`, `GET/PUT /api/platform/tax-rate/{id}` | `Platform.TaxRates.Manage` | `rate` es fracción: `0.18` = 18 % |
| `GET /api/platform/tax-rate/effective?countryCode=DO&code=ITBIS&at=` | Usuario | Tasa vigente o `null` |
| `GET /api/platform/business-audit-event` | `Platform.BusinessAudit.View` | Filtros: `Module, AuditAction, EntityType, EntityId, UserId, Succeeded, From, To` |

```ts
interface ProvinceDto { id: string; countryCode: string; code: string; name: string; isActive: boolean; creationTime: string; }
interface CreateUpdateProvinceDto { code: string; name: string; isActive: boolean; }
interface MunicipalityDto { id: string; provinceId: string; code: string; name: string; isActive: boolean; }
interface CreateUpdateMunicipalityDto { provinceId: string; code: string; name: string; isActive: boolean; }
interface CurrencyDto { id: string; code: string; name: string; symbol: string; decimalPlaces: number; isDefault: boolean; isActive: boolean; }
interface CreateCurrencyDto { code: string; name: string; symbol: string; decimalPlaces: number; }
interface UpdateCurrencyDto { name: string; symbol: string; decimalPlaces: number; isActive: boolean; }
interface TaxRateDto { id: string; countryCode: string; code: string; name: string; rate: number;
  effectiveFrom: string; effectiveTo?: string; isActive: boolean; }
interface CreateTaxRateDto { countryCode: string; code: string; name: string; rate: number; effectiveFrom: string; effectiveTo?: string; }
interface UpdateTaxRateDto { name: string; rate: number; effectiveFrom: string; effectiveTo?: string; isActive: boolean; }
interface BusinessAuditEventDto { id: string; module: string; action: string; entityType: string; entityId?: string;
  succeeded: boolean; reason?: string; dataJson?: string; userId?: string; userName?: string;
  clientIpAddress?: string; correlationId?: string; creationTime: string; }
```

### A.7 Endpoints estándar de ABP que la UI también usa

| Ruta | Uso |
|---|---|
| `POST /connect/token` | Login (password grant), refresh token |
| `GET /api/abp/application-configuration` | Usuario actual, permisos concedidos (`auth.grantedPolicies`), settings visibles (política de contraseña), idioma |
| `POST /api/account/send-password-reset-code` | `{ email, appName: "Angular", returnUrl?, returnUrlHash? }` |
| `POST /api/account/verify-password-reset-token` | `{ userId, resetToken }` → `boolean` |
| `POST /api/account/reset-password` | `{ userId, resetToken, password }` |
| `GET/PUT /api/account/my-profile`, `POST /api/account/my-profile/change-password` | Perfil básico y cambio de contraseña |
| `/api/identity/users`, `/api/identity/roles`, `/api/permission-management/permissions` | Administración de usuarios, roles y permisos (pantallas de ABP Identity) |

> El auto-registro nativo de ABP (`/api/account/register`) está **desactivado**: registrar siempre por `/api/accounts/registration/register`.

---

## Anexo B — Sistema de diseño y temas

### B.1 ADN visual (extraído del prototipo)

El prototipo usa un estilo **minimalista monocromo de lujo**, alineado con el logo (rostro de línea plateada, mano y tulipán sobre negro):

- **Base neutra:** blancos, negros y escala de grises (`gray-50 … gray-900`); acentos mínimos. Botones primarios negros sobre blanco (y su inverso en oscuro).
- **Formas:** tarjetas `rounded-xl`, botones e inputs `rounded-lg`, chips/avatares `rounded-full`. Bordes finos `border-border` en lugar de sombras pesadas; sombras suaves solo al hover.
- **Tipografía:** sans-serif limpia, jerarquía por peso (`font-bold` títulos, `font-semibold` subtítulos, `font-medium` UI). Etiquetas de sección en mayúsculas pequeñas con tracking amplio (eyebrow: "BELLEZA CERCA DE TI").
- **Imagen:** fotografía grande de trabajos reales (hero con degradado `from-black/70 via-black/30 to-transparent`), tarjetas de profesional con foto 4:5, posts con imagen 1:1.
- **Microinteracciones:** transiciones de 150–200 ms, `active:scale-[.98]` en botones, skeletons grises, corazón de favorito animado.

Tipografías recomendadas (Google Fonts, con respaldo del sistema):
- **UI:** `Inter` (400/500/600/700) → `ui-sans-serif, system-ui, -apple-system, "Segoe UI", sans-serif`.
- **Marca / titulares hero:** `Marcellus` (mayúsculas elegantes tipo Optima, como el wordmark "OWNEY BEAUTY") → `"Optima", Georgia, serif`. Solo para el logotipo textual, el hero y los títulos de sección grandes.

### B.2 Tokens semánticos

Todos los componentes usan estos tokens. Cada tema define sus valores en claro y oscuro.

| Token | Uso |
|---|---|
| `--background` / `--foreground` | Fondo de la app y texto principal |
| `--card` / `--card-foreground` | Tarjetas, hojas, modales |
| `--muted` / `--muted-foreground` | Superficies secundarias y texto atenuado |
| `--primary` / `--primary-foreground` | Acción principal (Reservar, Publicar, Continuar) |
| `--secondary` / `--secondary-foreground` | Acciones secundarias, chips |
| `--accent` / `--accent-foreground` | Hover, selección, destacados suaves |
| `--metal` | Detalle de marca (líneas del logo, insignia verificada, divisores premium) |
| `--border` / `--input` / `--ring` | Bordes, campos y foco |
| `--success` / `--warning` / `--destructive` / `--info` | Estados (cita confirmada, pendiente, cancelada, informativo) |
| `--chart-1 … --chart-5` | Gráficos de paneles |
| `--hero-overlay` | Degradado sobre fotos del hero |

Estados de cita (de los textos del prototipo): **Confirmada** → success · **Pendiente** → warning · **En curso** → info · **Completada** → muted · **Cancelada / Reembolsada** → destructive (atenuado).

### B.3 Implementación (Tailwind 4 + ThemeService)

`src/styles.css` (estructura; los valores van en `src/styles/themes.css` del §B.4):

```css
@import "tailwindcss";
@import "./styles/themes.css";

@custom-variant dark (&:where(.dark, .dark *));

@theme inline {
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  --color-card: var(--card);
  --color-card-foreground: var(--card-foreground);
  --color-muted: var(--muted);
  --color-muted-foreground: var(--muted-foreground);
  --color-primary: var(--primary);
  --color-primary-foreground: var(--primary-foreground);
  --color-secondary: var(--secondary);
  --color-secondary-foreground: var(--secondary-foreground);
  --color-accent: var(--accent);
  --color-accent-foreground: var(--accent-foreground);
  --color-metal: var(--metal);
  --color-border: var(--border);
  --color-input: var(--input);
  --color-ring: var(--ring);
  --color-success: var(--success);
  --color-warning: var(--warning);
  --color-destructive: var(--destructive);
  --color-info: var(--info);
  --color-chart-1: var(--chart-1);
  --color-chart-2: var(--chart-2);
  --color-chart-3: var(--chart-3);
  --color-chart-4: var(--chart-4);
  --color-chart-5: var(--chart-5);
  --font-sans: "Inter", ui-sans-serif, system-ui, -apple-system, "Segoe UI", sans-serif;
  --font-display: "Marcellus", "Optima", Georgia, serif;
  --radius-lg: 0.75rem;
  --radius-xl: 1rem;
}

html { color-scheme: light; }
html.dark { color-scheme: dark; }
body { @apply bg-background text-foreground antialiased; }
```

ThemeService (contrato):

```ts
export type ThemeName = 'noir' | 'champagne' | 'rose' | 'orquidea' | 'esmeralda' | 'caribe' | 'terracota' | 'medianoche';
export type ThemeMode = 'light' | 'dark' | 'system';
// Aplica en <html>: data-theme="<ThemeName>" y la clase .dark cuando corresponda.
// Persiste en localStorage: owney.theme / owney.mode. Escucha matchMedia('(prefers-color-scheme: dark)').
```

Script anti-parpadeo en `index.html` (antes de cargar Angular):

```html
<script>
  (function () {
    try {
      var t = localStorage.getItem('owney.theme') || 'noir';
      var m = localStorage.getItem('owney.mode') || 'system';
      var dark = m === 'dark' || (m === 'system' && matchMedia('(prefers-color-scheme: dark)').matches);
      document.documentElement.setAttribute('data-theme', t);
      if (dark) document.documentElement.classList.add('dark');
    } catch (e) {}
  })();
</script>
```

### B.4 Paletas (8 temas × claro/oscuro) — `src/styles/themes.css`

Todos los pares `primary`/`primary-foreground` y `foreground`/`background` están elegidos para cumplir contraste AA. Si cambias un valor, vuelve a verificarlo.

```css
/* Estados compartidos (se pueden sobrescribir por tema) */
:root {
  --success: #15803D; --warning: #B45309; --destructive: #DC2626; --info: #1D4ED8;
  --hero-overlay: linear-gradient(to top, rgb(0 0 0 / .72), rgb(0 0 0 / .32) 45%, rgb(0 0 0 / .05));
}
.dark {
  --success: #4ADE80; --warning: #FBBF24; --destructive: #F87171; --info: #93C5FD;
}

/* 1) OWNEY NOIR — tema por defecto: el monocromo del prototipo y del logo (negro + plata) */
:root, [data-theme="noir"] {
  --background:#FFFFFF; --foreground:#0A0A0A; --card:#FFFFFF; --card-foreground:#0A0A0A;
  --muted:#F4F4F5; --muted-foreground:#6B6B72; --primary:#0A0A0A; --primary-foreground:#FAFAFA;
  --secondary:#F4F4F5; --secondary-foreground:#18181B; --accent:#EDEDEE; --accent-foreground:#0A0A0A;
  --metal:#9CA3AF; --border:#E4E4E7; --input:#E4E4E7; --ring:#A1A1AA;
  --chart-1:#0A0A0A; --chart-2:#52525B; --chart-3:#A1A1AA; --chart-4:#C0C0C0; --chart-5:#D4D4D8;
}
.dark:where([data-theme="noir"]), .dark:not([data-theme]) {
  --background:#0A0A0A; --foreground:#F5F5F5; --card:#141414; --card-foreground:#F5F5F5;
  --muted:#1F1F1F; --muted-foreground:#A1A1AA; --primary:#E7E7E7; --primary-foreground:#0A0A0A;
  --secondary:#1F1F1F; --secondary-foreground:#F5F5F5; --accent:#262626; --accent-foreground:#F5F5F5;
  --metal:#C0C0C0; --border:#262626; --input:#2A2A2A; --ring:#737373;
  --chart-1:#F5F5F5; --chart-2:#C0C0C0; --chart-3:#8A8A8A; --chart-4:#5C5C5C; --chart-5:#3A3A3A;
}

/* 2) CHAMPAGNE — marfil cálido y oro champán (bodas, eventos) */
[data-theme="champagne"] {
  --background:#FBF8F3; --foreground:#1F1A14; --card:#FFFFFF; --card-foreground:#1F1A14;
  --muted:#F3EDE3; --muted-foreground:#76695A; --primary:#8C6B3A; --primary-foreground:#FFFFFF;
  --secondary:#F3EDE3; --secondary-foreground:#3B2E1E; --accent:#EADBC4; --accent-foreground:#3B2E1E;
  --metal:#B08D57; --border:#E7DDCD; --input:#E7DDCD; --ring:#B08D57;
  --chart-1:#8C6B3A; --chart-2:#B08D57; --chart-3:#D4B483; --chart-4:#7A6E5D; --chart-5:#E7DDCD;
}
.dark:where([data-theme="champagne"]) {
  --background:#14110D; --foreground:#F5EFE6; --card:#1C1813; --card-foreground:#F5EFE6;
  --muted:#26201A; --muted-foreground:#B5A893; --primary:#D4B483; --primary-foreground:#1A140D;
  --secondary:#26201A; --secondary-foreground:#F5EFE6; --accent:#2E261D; --accent-foreground:#F5EFE6;
  --metal:#D4B483; --border:#332A20; --input:#3A3025; --ring:#D4B483;
  --chart-1:#D4B483; --chart-2:#B08D57; --chart-3:#8C6B3A; --chart-4:#B5A893; --chart-5:#4A3D2E;
}

/* 3) ROSÉ — rosa empolvado con baya profunda (uñas, maquillaje) */
[data-theme="rose"] {
  --background:#FFF7F8; --foreground:#2A1218; --card:#FFFFFF; --card-foreground:#2A1218;
  --muted:#FBE9EC; --muted-foreground:#84545E; --primary:#A3264A; --primary-foreground:#FFFFFF;
  --secondary:#F8DDE3; --secondary-foreground:#5A1328; --accent:#F4C6D0; --accent-foreground:#5A1328;
  --metal:#C98A9A; --border:#F2D3DA; --input:#F2D3DA; --ring:#D9668A;
  --chart-1:#A3264A; --chart-2:#D9668A; --chart-3:#F28AA8; --chart-4:#84545E; --chart-5:#F4C6D0;
}
.dark:where([data-theme="rose"]) {
  --background:#170A0E; --foreground:#FBEFF2; --card:#211015; --card-foreground:#FBEFF2;
  --muted:#2D161D; --muted-foreground:#C99AA6; --primary:#F28AA8; --primary-foreground:#2A0B15;
  --secondary:#2D161D; --secondary-foreground:#FBEFF2; --accent:#3A1A24; --accent-foreground:#FBEFF2;
  --metal:#E7A9B9; --border:#3D1F29; --input:#47252F; --ring:#F28AA8;
  --chart-1:#F28AA8; --chart-2:#D9668A; --chart-3:#A3264A; --chart-4:#C99AA6; --chart-5:#5A1328;
}

/* 4) ORQUÍDEA — violeta (los acentos purple del prototipo) */
[data-theme="orquidea"] {
  --background:#FCFAFF; --foreground:#1E1530; --card:#FFFFFF; --card-foreground:#1E1530;
  --muted:#F1ECFA; --muted-foreground:#6A5E84; --primary:#6D28D9; --primary-foreground:#FFFFFF;
  --secondary:#EDE5FB; --secondary-foreground:#3B1A73; --accent:#E9DDFD; --accent-foreground:#4C1D95;
  --metal:#A78BFA; --border:#E6DEF5; --input:#E6DEF5; --ring:#A78BFA;
  --chart-1:#6D28D9; --chart-2:#8B5CF6; --chart-3:#C4A5FF; --chart-4:#6A5E84; --chart-5:#E9DDFD;
}
.dark:where([data-theme="orquidea"]) {
  --background:#110C1B; --foreground:#F3EEFD; --card:#1A1328; --card-foreground:#F3EEFD;
  --muted:#241B36; --muted-foreground:#B3A5D0; --primary:#C4A5FF; --primary-foreground:#1E1033;
  --secondary:#241B36; --secondary-foreground:#F3EEFD; --accent:#2E2245; --accent-foreground:#F3EEFD;
  --metal:#C4A5FF; --border:#31264A; --input:#3A2D57; --ring:#C4A5FF;
  --chart-1:#C4A5FF; --chart-2:#A78BFA; --chart-3:#8B5CF6; --chart-4:#B3A5D0; --chart-5:#3B1A73;
}

/* 5) ESMERALDA — verde spa (facial, masajes, bienestar) */
[data-theme="esmeralda"] {
  --background:#F7FBF9; --foreground:#0F1F19; --card:#FFFFFF; --card-foreground:#0F1F19;
  --muted:#E8F3EE; --muted-foreground:#557067; --primary:#047857; --primary-foreground:#FFFFFF;
  --secondary:#E2F1EA; --secondary-foreground:#064E3B; --accent:#CDEBDD; --accent-foreground:#064E3B;
  --metal:#6FB89A; --border:#D6E9E0; --input:#D6E9E0; --ring:#34D399;
  --chart-1:#047857; --chart-2:#10B981; --chart-3:#6EE7B7; --chart-4:#557067; --chart-5:#CDEBDD;
}
.dark:where([data-theme="esmeralda"]) {
  --background:#08130F; --foreground:#ECF7F2; --card:#0F1D18; --card-foreground:#ECF7F2;
  --muted:#152821; --muted-foreground:#9CC2B2; --primary:#6EE7B7; --primary-foreground:#052E22;
  --secondary:#152821; --secondary-foreground:#ECF7F2; --accent:#1A3329; --accent-foreground:#ECF7F2;
  --metal:#6EE7B7; --border:#1E3A2F; --input:#244539; --ring:#6EE7B7;
  --chart-1:#6EE7B7; --chart-2:#34D399; --chart-3:#10B981; --chart-4:#9CC2B2; --chart-5:#064E3B;
}

/* 6) CARIBE — turquesa del mar dominicano con coral para promociones */
[data-theme="caribe"] {
  --background:#F5FBFC; --foreground:#0B2027; --card:#FFFFFF; --card-foreground:#0B2027;
  --muted:#E3F3F6; --muted-foreground:#4B6F7B; --primary:#0E7490; --primary-foreground:#FFFFFF;
  --secondary:#DDF1F5; --secondary-foreground:#164E63; --accent:#C7EEF5; --accent-foreground:#164E63;
  --metal:#F97362; --border:#D3EAF0; --input:#D3EAF0; --ring:#22D3EE;
  --chart-1:#0E7490; --chart-2:#22D3EE; --chart-3:#F97362; --chart-4:#4B6F7B; --chart-5:#C7EEF5;
}
.dark:where([data-theme="caribe"]) {
  --background:#071418; --foreground:#E9F7FA; --card:#0D1F25; --card-foreground:#E9F7FA;
  --muted:#132A31; --muted-foreground:#92BBC6; --primary:#5EE3F5; --primary-foreground:#052A33;
  --secondary:#132A31; --secondary-foreground:#E9F7FA; --accent:#173840; --accent-foreground:#E9F7FA;
  --metal:#FF8F7F; --border:#1B3B44; --input:#21464F; --ring:#5EE3F5;
  --chart-1:#5EE3F5; --chart-2:#22D3EE; --chart-3:#FF8F7F; --chart-4:#92BBC6; --chart-5:#164E63;
}

/* 7) TERRACOTA — tierra cálida (barberías, salones urbanos) */
[data-theme="terracota"] {
  --background:#FCF8F5; --foreground:#2B1A12; --card:#FFFFFF; --card-foreground:#2B1A12;
  --muted:#F5EAE2; --muted-foreground:#80634F; --primary:#A84B23; --primary-foreground:#FFFFFF;
  --secondary:#F3E2D7; --secondary-foreground:#6B2D14; --accent:#F1D5C4; --accent-foreground:#6B2D14;
  --metal:#D08B67; --border:#EEDCD0; --input:#EEDCD0; --ring:#E07A4F;
  --chart-1:#A84B23; --chart-2:#E07A4F; --chart-3:#F0B08F; --chart-4:#80634F; --chart-5:#F1D5C4;
}
.dark:where([data-theme="terracota"]) {
  --background:#160E0A; --foreground:#F8ECE5; --card:#211510; --card-foreground:#F8ECE5;
  --muted:#2C1D16; --muted-foreground:#C9A493; --primary:#F0936A; --primary-foreground:#2B1206;
  --secondary:#2C1D16; --secondary-foreground:#F8ECE5; --accent:#3A251B; --accent-foreground:#F8ECE5;
  --metal:#F0B08F; --border:#3E2A20; --input:#4A3226; --ring:#F0936A;
  --chart-1:#F0936A; --chart-2:#E07A4F; --chart-3:#A84B23; --chart-4:#C9A493; --chart-5:#6B2D14;
}

/* 8) MEDIANOCHE — azul noche con oro (premium, Owney Verified Pro) */
[data-theme="medianoche"] {
  --background:#F7F8FB; --foreground:#0E1526; --card:#FFFFFF; --card-foreground:#0E1526;
  --muted:#E9ECF3; --muted-foreground:#5A647C; --primary:#1E2A4A; --primary-foreground:#FFFFFF;
  --secondary:#E4E8F1; --secondary-foreground:#1E2A4A; --accent:#F2E3BF; --accent-foreground:#5A4410;
  --metal:#C9A24A; --border:#DDE2EC; --input:#DDE2EC; --ring:#C9A24A;
  --chart-1:#1E2A4A; --chart-2:#3B4F80; --chart-3:#C9A24A; --chart-4:#5A647C; --chart-5:#F2E3BF;
}
.dark:where([data-theme="medianoche"]) {
  --background:#0A0F1C; --foreground:#EEF1F8; --card:#111829; --card-foreground:#EEF1F8;
  --muted:#182136; --muted-foreground:#9AA6C2; --primary:#E2C275; --primary-foreground:#1A1406;
  --secondary:#182136; --secondary-foreground:#EEF1F8; --accent:#1F2A44; --accent-foreground:#EEF1F8;
  --metal:#E2C275; --border:#232F4B; --input:#2A3858; --ring:#E2C275;
  --chart-1:#E2C275; --chart-2:#C9A24A; --chart-3:#6B7FB3; --chart-4:#9AA6C2; --chart-5:#2A3858;
}
```

> Nota de especificidad: `.dark:where([data-theme="x"])` hereda la especificidad de `.dark`; como `data-theme` y `.dark` están ambos en `<html>`, declara los bloques oscuros **después** de los claros (como arriba).

### B.5 Componentes base (`shared/ui`)

| Componente | Variantes / notas |
|---|---|
| `ob-button` | `primary`, `secondary`, `outline`, `ghost`, `destructive`, `link`; tamaños `sm/md/lg/icon`; estado `loading` |
| `ob-input`, `ob-textarea`, `ob-select`, `ob-otp-input` (6 dígitos) | Label, ayuda, error, prefijo/sufijo (`RD$`, `+1`) |
| `ob-card` | `header/content/footer`; variante `interactive` (hover) |
| `ob-badge` / `ob-status-chip` | Estados de cita y verificación ("Verificada", "Verificación pendiente", "No verificada") |
| `ob-avatar` | Imagen o iniciales; anillo `metal` si verificado |
| `ob-rating` | Estrellas lectura/edición, accesible |
| `ob-tabs`, `ob-segmented` | "Próximas / Pasadas / Todas" |
| `ob-dialog`, `ob-sheet` (móvil), `ob-confirm` | "¿Eliminar este servicio?" |
| `ob-toast` | Éxito/error/info; lectura por lectores de pantalla (`aria-live`) |
| `ob-empty-state` | Icono + título + texto + acción ("Sin citas", "Sin favoritos", "Sin resultados") |
| `ob-skeleton` | Tarjetas, listas, perfil |
| `ob-price` | Formato `RD$1,500` (es-DO), moneda desde la API |
| `ob-theme-picker` | 8 muestras de color + claro/oscuro/sistema |
| `ob-bottom-nav`, `ob-sidebar`, `ob-header` | Layouts por rol/modo |

---

## Anexo C — Inventario de pantallas del prototipo

Textos tal como aparecen en el prototipo (usarlos como copia base).

### C.1 Entrada y navegación
- **Bienvenida / selección de rol:** marca "OWNEY BEAUTY", "Marketplace de servicios de belleza en República Dominicana", "Selecciona un rol para explorar la plataforma". Opciones: **Cliente** ("Reserva servicios de belleza"), **Profesional** ("Ofrece tus servicios"), **Administrador** ("Gestiona la plataforma"). En la app real esto se convierte en: login/registro + paso "¿Qué buscas?" del registro; el administrador entra por login normal.
- **Header cliente:** Inicio · Buscar · Social · Citas · Mi panel · campana de notificaciones con contador · menú de usuario · "Cerrar sesión".

### C.2 Cliente
- **Inicio:** hero con foto ("Tu belleza, tu estilo" / "Descubre los mejores profesionales de belleza en República Dominicana" / botón "Explorar profesionales"). Secciones: **Categorías** (Cabello, Manicure, Pedicure, Maquillaje, Facial, Masajes, Cejas & Pestañas, Depilación), **Promociones activas** (p. ej. "Balayage + Corte 15% Off", "RD$500 Off Masaje"), **Profesionales destacadas** (foto, nombre, especialidad, zona, ★ rating, "Desde RD$…", insignia "Verificada", "Agregar a favoritos"), **Comunidad** (posts con imagen, texto, ♡ y comentarios), "Ver todas".
- **Buscar:** "Buscar profesionales", input "Buscar por nombre, especialidad...", "Filtros": Categoría, Rating mínimo, Tipo de servicio (**En salón** / **A domicilio**), Zona (Piantini, Naco, Zona Colonial, Santiago…). Estado vacío "Sin resultados — Intenta ajustar tus filtros o búsqueda".
- **Perfil de profesional:** portada, avatar, nombre, insignia, bio ("Maquilladora profesional con experiencia en bodas…"), rating y "Reseñas (n)". Pestañas **Portafolio · Servicios · Reseñas**. Cada servicio: nombre, descripción, duración, precio, modalidad, botón "Reservar". Estado "Aún no hay reseñas".
- **Reserva (flujo):** "Detalles del servicio" → "Selecciona fecha" (calendario Dom–Sáb) → "Horario disponible" (o "No hay horarios disponibles para esta fecha") → "Tipo de servicio" (Salón / A domicilio) → "Tu dirección" (si domicilio) → "Notas (opcional)" ("Alguna indicación especial...") → **Resumen**: servicio, "Traslado a domicilio / Tarifa de traslado", descuento, **Total** → "Continuar al pago" → "Confirmar y pagar" → "Procesando pago..." → "¡Reservación confirmada!" con "Ver mis citas" / "Ir al inicio". Sello: **"Pago protegido"** (no usar la palabra escrow).
- **Mis citas:** pestañas **Próximas · Pasadas · Todas**; tarjeta con fecha (día/mes), servicio, profesional, hora, estado (Confirmada, Pendiente, En curso, Completada, Cancelada, Reembolsada). Acciones: "Cancelar/Reembolso" → modal "Motivo de cancelación" ("Indica el motivo...") → "Confirmar cancelación y reembolso"; "Dejar reseña" → estrellas + "Cuéntanos tu experiencia..." → "Publicar reseña". Vacío: "No tienes citas próximas".
- **Favoritos:** "Mis favoritos"; vacío "Sin favoritos — Agrega profesionales a tus favoritos para encontrarlos fácilmente".
- **Notificaciones:** lista con tipos (Nueva reservación, Reservación confirmada, Servicio completado "¡Deja tu reseña!", Reembolso aprobado/rechazado, Nueva reseña, ¡Verificación aprobada!) y "Marcar todas como leídas"; vacío "Sin notificaciones — Aquí aparecerán tus notificaciones".
- **Comunidad:** feed de publicaciones, "Comparte tu trabajo con la comunidad..." (crear post para profesionales), "Escribe un comentario...", likes.

### C.3 Profesional (modo profesional)
- **Mi panel:** "Agenda de hoy" ("No tienes citas para hoy"), "Citas pendientes" (Aprobar/Completar), tarjetas **Resumen**: Citas completadas, Ingresos, Calificación, "Reseñas recientes". "Acciones rápidas".
- **Mi agenda:** vista por día/semana con estados.
- **Mis servicios:** lista + "Nuevo servicio" / "Editar servicio": Nombre, Descripción, Categoría, Duración (min), Precio (RD$), Tipo (Salón / A domicilio / ambos), Activo/Inactivo; "¿Eliminar este servicio?". Vacío: "Sin servicios — Crea tu primer servicio para empezar a recibir reservaciones".
- **Mis promociones:** "Nueva promoción / Editar promoción": Título, Descripción, Tipo de descuento (**Porcentaje (%)** / **Monto fijo (RD$)**), Valor, Servicios aplicables, Fecha inicio, Fecha fin; "¿Eliminar esta promoción?". Vacío: "Sin promociones — Crea promociones para atraer más clientes".
- **Mi perfil:** datos públicos, portafolio, estado de verificación (Verificada / Verificación pendiente / Verificación rechazada "Revisa los requisitos").

### C.4 Administración
- **Panel de administración:** métricas **Usuarios totales, Profesionales registrados, Reservaciones, Ingresos totales**; "Verificaciones pendientes" (Aprobar / Rechazar; vacío "No hay verificaciones pendientes"); "Reembolsos pendientes" (vacío "No hay reembolsos pendientes"); lista de profesionales con estado.
- **Nuevas pantallas reales (API disponible):** Geografía (provincias/municipios), Monedas, Impuestos, Documentos legales (borrador → publicar), Auditoría de negocio, y las pantallas de ABP Identity (usuarios, roles, permisos).

---

## Anexo D — Qué es real y qué es mock

| Área | Estado en la API | En la UI |
|---|---|---|
| Registro, login, verificación, recuperar contraseña | ✅ Disponible | Real |
| Mi cuenta, modos, consentimientos, documentos legales pendientes | ✅ Disponible | Real |
| Geografía y monedas (públicos) | ✅ Disponible | Real (selector de provincia/municipio en direcciones y filtros) |
| Admin: geografía, monedas, impuestos, documentos legales, auditoría | ✅ Disponible | Real |
| Perfiles, seguir, publicaciones, feed, explorar | ⬜ Sprints 3–5 | Mock con interfaces `ProfilesApi`, `SocialApi`, `FeedApi` |
| Profesionales/KYC, catálogo, agenda, reservas, domicilio | ⬜ Sprints 6–9 | Mock con interfaces `CatalogApi`, `BookingApi` |
| Pagos, reseñas, notificaciones, chat | ⬜ Sprints 10–12 | Mock con interfaces `PaymentsApi`, `ReviewsApi`, `NotificationsApi` |

Los mocks deben vivir en `src/app/features/**/data/*.mock.ts`, detrás de un `InjectionToken` por API, para reemplazarlos por los proxies generados sin tocar los componentes.
