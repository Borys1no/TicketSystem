# Seguridad — SupportFlow

## 1. Propósito

SupportFlow utiliza varios mecanismos para proteger el acceso a la API y a los datos del sistema.

Actualmente la seguridad se basa principalmente en:

* Autenticación mediante JWT.
* Hashing de contraseñas con BCrypt.
* Autorización basada en roles.
* Identificación del usuario mediante los claims del token.
* Validaciones de propiedad sobre determinadas operaciones.
* Restricciones de las reglas de negocio en el dominio.
* Manejo centralizado de excepciones.

El objetivo es evitar que un usuario pueda acceder o modificar información para la que no tiene autorización.

---

# 2. Autenticación

La autenticación determina quién está realizando una solicitud.

SupportFlow utiliza **JSON Web Tokens (JWT)**.

El flujo general es:

```text
Usuario
   │
   │ email + contraseña
   ▼
POST /api/auth/login
   │
   ▼
LoginCommandHandler
   │
   ├── Busca usuario
   ├── Verifica contraseña
   └── Genera JWT
           │
           ▼
        Cliente
           │
           │ Authorization: Bearer <token>
           ▼
        API
```

Una vez autenticado, el cliente debe enviar el token en las solicitudes que requieren autorización.

---

# 3. JWT

El token JWT es generado por la infraestructura mediante:

`SupportFlow.Infrastructure.Security.JwTokenGenerator`

La aplicación solicita la generación del token mediante la abstracción:

```text
IJwtTokenGenerator
```

De esta manera, la capa Application no depende directamente de la implementación concreta de JWT.

---

# 4. Identidad del usuario

Una decisión importante de seguridad es que la API **no confía en el identificador de usuario enviado por el cliente para determinar quién realiza una operación protegida**.

En su lugar, obtiene el identificador desde el JWT.

Por ejemplo:

```csharp id="t6q5x4"
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
```

Posteriormente se valida que el claim pueda convertirse correctamente a `Guid`.

Esto permite establecer una identidad confiable a partir del usuario autenticado.

---

# 5. Creación de tickets

Al crear un ticket, el cliente proporciona información como:

* Título.
* Descripción.
* Prioridad.

Pero no determina libremente quién es el creador.

La API obtiene el usuario autenticado desde:

```text
ClaimTypes.NameIdentifier
```

y utiliza ese identificador como `CreatedByUserId`.

El flujo es:

```text
JWT
 │
 ▼
NameIdentifier
 │
 ▼
UserId autenticado
 │
 ▼
CreateTicketCommand
 │
 ▼
CreatedByUserId
```

Esto evita una vulnerabilidad de suplantación en la creación de tickets.

---

# 6. Autorización por roles

La autorización determina qué operaciones puede realizar un usuario autenticado.

SupportFlow utiliza autorización basada en roles mediante:

```csharp
[Authorize(Roles = "...")]
```

Los roles actuales son:

* `Employee`
* `Technician`
* `Adminitrador`

> `Adminitrador` mantiene intencionalmente la escritura utilizada actualmente en el código y en los tokens.

---

# 7. Matriz de permisos

La autorización actual de los endpoints de tickets es:

| Operación             | Employee | Technician | Adminitrador |
| --------------------- | :------: | :--------: | :----------: |
| Crear ticket          |    Sí    |     No     |      Sí      |
| Ver todos los tickets |    No    |     Sí     |      Sí      |
| Ver ticket por ID     |    No    |     Sí     |      Sí      |
| Ver mis tickets       |    Sí    |     No     |      No      |
| Ver tickets asignados |    No    |     Sí     |      No      |
| Resolver ticket       |    No    |     Sí     |      No      |
| Asignar ticket        |    No    |     No     |      Sí      |
| Cerrar ticket         |    No    |     No     |      Sí      |
| Reabrir ticket        |    No    |     No     |      Sí      |

La autorización se realiza antes de ejecutar la operación correspondiente.

---

# 8. Diferencia entre autenticación y autorización

En SupportFlow ambos conceptos están separados.

### Autenticación

Responde:

> ¿Quién eres?

Se realiza mediante JWT.

### Autorización

Responde:

> ¿Qué puedes hacer?

Se realiza principalmente mediante los roles contenidos en el contexto autenticado.

Por ejemplo:

```text
Usuario autenticado
       │
       ▼
JWT válido
       │
       ▼
Role = Technician
       │
       ▼
Puede acceder a operaciones permitidas
       │
       ├── GET tickets
       ├── GET assigned
       └── Resolve
```

---

# 9. Verificación de propiedad

El rol por sí solo no siempre es suficiente.

Algunas operaciones también deben verificar que el usuario tenga relación con el recurso.

Por ejemplo, un técnico no debería poder resolver cualquier ticket solamente por tener el rol `Technician`.

Al resolver un ticket se verifica que el ticket esté asignado al técnico que intenta realizar la operación.

La regla es equivalente a:

```text
Ticket.AssignedToUserId
        │
        │ debe coincidir
        ▼
TechnicianId autenticado
```

Si no coinciden, la operación es rechazada.

Esto proporciona una segunda capa de seguridad además de la autorización por rol.

---

# 10. Reglas de seguridad dentro del dominio

Las reglas de negocio importantes no dependen exclusivamente del controlador.

Por ejemplo, `Ticket.AssignTo()` valida que:

1. El usuario asignado tenga rol `Technician`.
2. El ticket se encuentre en estado `Pending` o `Reopened`.

De forma similar:

`Resolve()` solamente permite resolver tickets en estado `InProgress`.

`Close()` solamente permite cerrar tickets en estado `Resolved`.

`Reopen()` solamente permite reabrir tickets en estado `Closed`.

Esto significa que incluso si una operación llega desde otro punto de la aplicación, las reglas fundamentales del dominio continúan aplicándose.

---

# 11. Contraseñas

Las contraseñas no se almacenan en texto plano.

SupportFlow utiliza BCrypt mediante:

`SupportFlow.Infrastructure.Security.PasswordHasher`

La interfaz utilizada por Application es:

```text
IPasswordHasher
```

La implementación concreta utiliza:

```text
BCrypt.Net-Next
```

---

# 12. Registro de contraseñas

Durante el registro de un usuario:

```text
Contraseña
    │
    ▼
IPasswordHasher
    │
    ▼
PasswordHasher
    │
    ▼
BCrypt.HashPassword()
    │
    ▼
PasswordHash
    │
    ▼
Base de datos
```

La contraseña original no se almacena.

---

# 13. Inicio de sesión

Durante el login:

```text
Contraseña proporcionada
        │
        ▼
PasswordHasher.Verify()
        │
        ▼
BCrypt
        │
        ▼
PasswordHash almacenado
```

Si la contraseña es válida, se genera el JWT.

Si no es válida, la autenticación es rechazada.

---

# 14. Normalización del correo electrónico

El correo electrónico se normaliza antes de realizar determinadas operaciones.

Actualmente se utiliza:

```csharp id="0w7b2f"
command.Email.Trim().ToLowerInvariant();
```

Esto permite tratar de forma consistente valores como:

```text
usuario@example.com
 Usuario@example.com
USUARIO@EXAMPLE.COM
```

La normalización se utiliza tanto durante el registro como durante el inicio de sesión.

Además, antes de registrar un usuario se comprueba si el correo ya existe.

---

# 15. Protección contra duplicados

El registro de usuarios comprueba que el correo electrónico no esté previamente registrado.

Si ya existe, la aplicación genera una excepción de operación inválida.

El middleware global transforma esta situación en una respuesta HTTP apropiada.

Por ejemplo:

```json id="iyx50b"
{
  "message": "Email is already registered."
}
```

---

# 16. Manejo de errores

La API cuenta con un middleware global:

`SupportFlow.API/Middleware/ExceptionHandlingMiddleware.cs`

Su objetivo es evitar que cada controlador tenga que implementar manualmente el mismo mecanismo de manejo de excepciones.

El flujo es:

```text
Request
   │
   ▼
Middleware de excepciones
   │
   ▼
Controller
   │
   ▼
Application
   │
   ▼
Domain
   │
   │ excepción
   ▼
Middleware
   │
   ▼
HTTP Response
```

---

# 17. Mapeo de excepciones HTTP

Actualmente se manejan las siguientes excepciones:

| Excepción                     | HTTP |
| ----------------------------- | ---: |
| `KeyNotFoundException`        |  404 |
| `UnauthorizedAccessException` |  403 |
| `InvalidOperationException`   |  400 |
| Otras excepciones             |  500 |

La respuesta utiliza actualmente una estructura sencilla:

```json id="bqj0ca"
{
  "message": "Mensaje de error"
}
```

---

# 18. Diferencia entre 401 y 403

SupportFlow distingue entre problemas de autenticación y autorización.

### HTTP 401 — Unauthorized

Se utiliza cuando la API no puede identificar correctamente al usuario autenticado.

Por ejemplo, cuando falta o es inválido el identificador esperado dentro del token.

### HTTP 403 — Forbidden

Se utiliza cuando el usuario está autenticado pero no tiene permiso para realizar una determinada operación.

Por ejemplo:

```text
Employee
   │
   ▼
GET /api/tickets
   │
   ▼
403 Forbidden
```

porque consultar todos los tickets requiere un rol autorizado.

---

# 19. Protección de información sensible

El proyecto debe evitar almacenar o exponer información sensible innecesariamente.

Actualmente:

* Las contraseñas se almacenan como hashes.
* El usuario autenticado se identifica mediante JWT.
* Las operaciones protegidas utilizan autorización por roles.
* El identificador de creación del ticket proviene del token y no de una entrada confiable del cliente.

No se deben registrar contraseñas, tokens completos ni otros secretos en logs.

---

# 20. Configuración de secretos

Los secretos de infraestructura, como:

* Connection strings.
* Claves JWT.
* Credenciales.
* Otros secretos de entorno.

no deben almacenarse directamente en el código fuente.

En desarrollo pueden utilizarse mecanismos de configuración de .NET apropiados.

En producción se debe utilizar un mecanismo seguro de gestión de secretos proporcionado por la infraestructura utilizada.

---

# 21. HTTPS

La API debe ejecutarse mediante HTTPS en entornos donde se transmitan credenciales o tokens.

Esto es especialmente importante porque las solicitudes de autenticación contienen credenciales y las solicitudes posteriores contienen el JWT.

El transporte seguro evita que estas credenciales puedan ser interceptadas durante la comunicación.

---

# 22. Capas de seguridad

La seguridad de SupportFlow no depende de un único mecanismo.

Actualmente existe una combinación de controles:

```text
┌───────────────────────────────┐
│          HTTPS                │
├───────────────────────────────┤
│       JWT Authentication      │
├───────────────────────────────┤
│       Role Authorization      │
├───────────────────────────────┤
│   Resource/Ownership Checks   │
├───────────────────────────────┤
│       Domain Rules            │
├───────────────────────────────┤
│   Database FK Restrictions    │
└───────────────────────────────┘
```

Cada capa tiene una responsabilidad diferente.

---

# 23. Seguridad de la base de datos

Las relaciones entre `Tickets` y `Users` utilizan:

```csharp id="3apx9q"
DeleteBehavior.Restrict
```

Esto evita que eliminar un usuario provoque automáticamente la eliminación de tickets relacionados.

De esta forma se protege la integridad del historial de soporte.

---

# 24. Limitaciones actuales

La seguridad implementada cubre los requisitos principales de la etapa actual, pero todavía existen aspectos que podrían mejorarse.

Entre ellos:

* Rate limiting para endpoints sensibles.
* Políticas de contraseñas más completas.
* Expiración y renovación de tokens.
* Revocación de sesiones.
* Auditoría de acciones sensibles.
* Logs estructurados y seguros.
* Gestión centralizada de secretos en producción.
* Protección adicional contra ataques de fuerza bruta.
* Validación y políticas CORS según el cliente final.
* Headers de seguridad.
* Hardening de producción.
* Pruebas automatizadas de autorización y seguridad.
* Revisión periódica de dependencias.

Estas funcionalidades no deben considerarse implementadas actualmente.

---

# 25. Principios de seguridad

El proyecto sigue algunos principios fundamentales:

### No confiar en datos sensibles enviados por el cliente

La identidad del usuario se obtiene del contexto autenticado.

### Aplicar mínimo privilegio

Cada rol recibe únicamente los permisos necesarios para sus operaciones actuales.

### Validar propiedad cuando corresponde

Tener un rol válido no significa automáticamente tener acceso a cualquier recurso.

### Mantener reglas importantes en el dominio

Las reglas de negocio críticas no deben depender exclusivamente de los controladores.

### No almacenar contraseñas en texto plano

Las contraseñas se almacenan mediante hashing con BCrypt.

### Proteger la integridad de los datos

Las relaciones de base de datos utilizan restricciones apropiadas para evitar eliminaciones accidentales.

---

# 26. Estado actual

La seguridad de SupportFlow se encuentra en una etapa funcional inicial.

Actualmente el sistema cuenta con:

* Login mediante JWT.
* Hashing de contraseñas con BCrypt.
* Normalización de correos.
* Validación de correos duplicados.
* Autorización por roles.
* Identidad obtenida desde `ClaimTypes.NameIdentifier`.
* Protección contra suplantación al crear tickets.
* Verificación de técnico asignado al resolver tickets.
* Reglas de seguridad dentro del dominio.
* Manejo global de excepciones.
* Restricciones de integridad en base de datos.

Las mejoras avanzadas de seguridad deberán incorporarse conforme el proyecto evolucione hacia producción.

---

# 27. Principio general

La seguridad de SupportFlow debe considerarse una responsabilidad transversal.

La autenticación identifica al usuario, la autorización controla el acceso, el dominio protege las reglas de negocio y la infraestructura protege la persistencia.

Ninguna de estas capas debe considerarse suficiente por sí sola.

El objetivo es mantener varias capas de protección para reducir el impacto de errores o accesos no autorizados.
