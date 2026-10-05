# API Reference

## 1. Introducción

SupportFlow expone una API REST desarrollada con ASP.NET Core.

La API utiliza:

* HTTP/HTTPS.
* JSON.
* JWT Bearer Authentication.
* Autorización basada en roles.
* Swagger/OpenAPI durante el desarrollo.

La ruta base de los Controllers es:

```text
/api
```

Los recursos principales son:

```text
/api/auth
/api/users
/api/tickets
```

---

# 2. Autenticación

Los endpoints protegidos requieren un token JWT.

El token se obtiene mediante:

```http
POST /api/auth/login
```

Una vez obtenido, debe enviarse en el header:

```http
Authorization: Bearer <token>
```

No se debe enviar el token dentro del body de las solicitudes.

---

# 3. Roles

Los roles disponibles actualmente son:

```text
Employee
Technician
Adminitrador
```

> `Adminitrador` está escrito de esta manera deliberadamente porque corresponde al valor actualmente utilizado por el sistema y por los atributos `[Authorize]`.

---

# 4. Auth

## POST `/api/auth/login`

Autentica un usuario y genera un JWT.

### Autenticación requerida

No.

### Request

El endpoint recibe las credenciales del usuario.

Ejemplo conceptual:

```json
{
  "email": "usuario@supportflow.com",
  "password": "123456"
}
```

### Flujo

```text
Credenciales
     │
     ▼
LoginCommand
     │
     ▼
Buscar usuario
     │
     ▼
Verificar BCrypt
     │
     ▼
Generar JWT
     │
     ▼
Respuesta
```

### Respuesta exitosa

```http
200 OK
```

La respuesta contiene el token generado por el sistema.

### Credenciales inválidas

Cuando las credenciales no son válidas, la operación es rechazada.

La API no debe revelar información innecesaria que permita determinar si falló específicamente el correo o la contraseña.

---

# 5. Users

## POST `/api/users`

Crea un nuevo usuario.

### Autenticación

Depende de la configuración actual del Controller.

La creación de usuarios se procesa mediante:

```text
CreateUserCommand
CreateUserCommandHandler
```

### Request

Ejemplo conceptual:

```json
{
  "name": "Juan",
  "lastName": "Pérez",
  "email": "juan@supportflow.com",
  "password": "123456",
  "role": 1,
  "department": 2
}
```

Los valores concretos de `role` y `department` corresponden a los enums definidos en el dominio.

### Procesamiento

Antes de almacenar el usuario:

1. Se normaliza el correo.
2. Se comprueba que no exista otro usuario con el mismo correo.
3. Se genera un hash BCrypt de la contraseña.
4. Se almacena el usuario.

El correo se normaliza mediante:

```text
Trim()
ToLowerInvariant()
```

### Respuesta exitosa

```http
201 Created
```

La respuesta utiliza `UserResponse`.

La contraseña no forma parte de la respuesta.

### Correo duplicado

Si el correo ya existe:

```http
400 Bad Request
```

Ejemplo:

```json
{
  "message": "Email is already registered."
}
```

---

## GET `/api/users`

Obtiene la lista de usuarios.

### Respuesta exitosa

```http
200 OK
```

La respuesta utiliza DTOs de usuario.

Las contraseñas no deben exponerse.

---

## GET `/api/users/{id}`

Obtiene un usuario mediante su identificador.

### Parámetro

```text
id
```

Tipo:

```text
Guid
```

Ejemplo:

```http
GET /api/users/2cbb0daf-073d-436e-9821-2b524b6f237c
```

### Respuesta exitosa

```http
200 OK
```

### Usuario inexistente

```http
404 Not Found
```

---

# 6. Tickets

Los tickets constituyen el recurso principal de SupportFlow.

La ruta base es:

```text
/api/tickets
```

---

## POST `/api/tickets`

Crea un nuevo ticket.

### Roles permitidos

```text
Employee
Adminitrador
```

### Autenticación

Sí.

### Request

Ejemplo:

```json
{
  "title": "No funciona mi computadora",
  "description": "El equipo no inicia correctamente",
  "priority": 2
}
```

La prioridad corresponde al enum `Priority`.

```text
Low
Medium
High
Critical
```

### Identidad del creador

El `CreatedByUserId` no debe ser enviado por el cliente.

La API obtiene el identificador desde:

```text
ClaimTypes.NameIdentifier
```

Esto garantiza que el ticket quede asociado al usuario autenticado.

### Estado inicial

Todos los tickets nuevos comienzan en:

```text
Pending
```

### Respuesta

```http
201 Created
```

La respuesta utiliza:

```text
TicketResponse
```

---

# 7. GET `/api/tickets`

Obtiene todos los tickets.

### Roles permitidos

```text
Technician
Adminitrador
```

### Autenticación

Sí.

### Respuesta

```http
200 OK
```

Devuelve una colección de `TicketResponse`.

---

# 8. GET `/api/tickets/{id}`

Obtiene un ticket específico.

### Roles permitidos

```text
Technician
Adminitrador
```

### Parámetro

```text
id: Guid
```

Ejemplo:

```http
GET /api/tickets/05054e73-89d2-494a-be42-efb11c781a0f
```

### Respuesta exitosa

```http
200 OK
```

### Ticket inexistente

```http
404 Not Found
```

Ejemplo:

```json
{
  "message": "Ticket not found."
}
```

---

# 9. GET `/api/tickets/my`

Obtiene los tickets creados por el usuario autenticado.

### Roles permitidos

```text
Employee
```

### Autenticación

Sí.

### Identidad

El usuario se obtiene desde:

```text
ClaimTypes.NameIdentifier
```

El cliente no puede indicar qué usuario consultar.

### Respuesta

```http
200 OK
```

Devuelve únicamente los tickets cuyo:

```text
CreatedByUserId
```

corresponde al usuario autenticado.

Ejemplo conceptual:

```json
[
  {
    "id": "129de2bd-4e6c-45ba-8251-c4f38cd924f0",
    "ticketNumber": 4,
    "title": "Ticket creado por Pedro",
    "description": "Prueba de permisos de Employee",
    "priority": "Medium",
    "status": "Pending",
    "createdAt": "2026-08-25T02:16:08.215098Z",
    "createdByUserId": "9b83fa3f-f66a-4cd9-b573-ac5f25e3d57b",
    "assignedToUserId": null
  }
]
```

---

# 10. GET `/api/tickets/assigned`

Obtiene los tickets asignados al técnico autenticado.

### Roles permitidos

```text
Technician
```

### Autenticación

Sí.

### Identidad

El identificador se obtiene desde:

```text
ClaimTypes.NameIdentifier
```

La consulta utiliza ese identificador para buscar:

```text
AssignedToUserId
```

### Respuesta

```http
200 OK
```

Devuelve únicamente los tickets asignados al técnico autenticado.

Si el técnico no tiene tickets asignados, la respuesta es una colección vacía.

Ejemplo:

```json
[]
```

---

# 11. Asignación de tickets

La asignación utiliza:

```text
AssignTicketCommand
AssignTicketCommandHandler
```

### Rol permitido

```text
Adminitrador
```

La asignación requiere que el usuario seleccionado sea un `Technician`.

Una asignación válida produce:

```text
Pending → InProgress
```

o:

```text
Reopened → InProgress
```

El técnico asignado se almacena en:

```text
AssignedToUserId
```

---

# 12. Resolución de tickets

La resolución utiliza:

```text
ResolveTicketCommand
ResolveTicketCommandHandler
```

### Rol permitido

```text
Technician
```

Pero tener el rol `Technician` no es suficiente.

El técnico autenticado debe ser el mismo usuario almacenado en:

```text
AssignedToUserId
```

### Estado requerido

El ticket debe encontrarse en:

```text
InProgress
```

### Transición

```text
InProgress → Resolved
```

Si el técnico no es el asignado:

```http
403 Forbidden
```

---

# 13. Cierre de tickets

La operación utiliza:

```text
CloseTicketCommand
CloseTicketCommandHandler
```

### Rol permitido

```text
Adminitrador
```

### Estado requerido

El ticket debe estar:

```text
Resolved
```

### Transición

```text
Resolved → Closed
```

Si el ticket no se encuentra en el estado correcto, la operación es rechazada.

---

# 14. Reapertura de tickets

La operación utiliza:

```text
ReopenTicketCommand
ReopenTicketCommandHandler
```

### Rol permitido

```text
Adminitrador
```

### Estado requerido

El ticket debe estar:

```text
Closed
```

### Transición

```text
Closed → Reopened
```

Después puede volver a ser asignado a un técnico.

---

# 15. Resumen de autorización

| Endpoint                    | Employee | Technician | Adminitrador |
| --------------------------- | :------: | :--------: | :----------: |
| `POST /api/tickets`         |    Sí    |     No     |      Sí      |
| `GET /api/tickets`          |    No    |     Sí     |      Sí      |
| `GET /api/tickets/{id}`     |    No    |     Sí     |      Sí      |
| `GET /api/tickets/my`       |    Sí    |     No     |      No      |
| `GET /api/tickets/assigned` |    No    |     Sí     |      No      |
| Asignar ticket              |    No    |     No     |      Sí      |
| Resolver ticket             |    No    |     Sí     |      No      |
| Cerrar ticket               |    No    |     No     |      Sí      |
| Reabrir ticket              |    No    |     No     |      Sí      |

---

# 16. Códigos HTTP

La API utiliza códigos HTTP para representar el resultado de las operaciones.

| Código | Significado                                                          |
| -----: | -------------------------------------------------------------------- |
|  `200` | Operación exitosa                                                    |
|  `201` | Recurso creado                                                       |
|  `400` | Solicitud inválida o regla de aplicación no válida                   |
|  `401` | Usuario no autenticado o token inválido                              |
|  `403` | Usuario autenticado pero sin autorización para realizar la operación |
|  `404` | Recurso no encontrado                                                |
|  `500` | Error interno no controlado                                          |

---

# 17. Formato de errores

Las excepciones gestionadas por la aplicación se convierten en respuestas JSON.

Formato actual:

```json
{
  "message": "Mensaje del error"
}
```

Ejemplo:

```http
404 Not Found
```

```json
{
  "message": "Ticket not found."
}
```

---

# 18. JWT y autorización

Los endpoints protegidos utilizan:

```http
Authorization: Bearer <JWT>
```

El token contiene la información necesaria para identificar al usuario y determinar su rol.

La API utiliza las claims del token para:

* Identificar al usuario.
* Autorizar operaciones.
* Asociar tickets al usuario autenticado.
* Validar la pertenencia de un ticket asignado.

---

# 19. Swagger / OpenAPI

Durante el desarrollo, SupportFlow utiliza Swagger para explorar y probar la API.

Swagger permite:

* Consultar los endpoints.
* Revisar los modelos.
* Enviar solicitudes HTTP.
* Probar endpoints protegidos proporcionando un JWT.

La interfaz está disponible cuando la API se ejecuta en el entorno configurado para Swagger.

---

# 20. Ejemplo de flujo completo

Un flujo típico de uso es:

```text
1. Login
   │
   ▼
POST /api/auth/login
   │
   ▼
JWT
   │
   ▼
2. Crear ticket
   │
   ▼
POST /api/tickets
   │
   ▼
Pending
   │
   ▼
3. Administrador asigna técnico
   │
   ▼
InProgress
   │
   ▼
4. Técnico consulta sus tickets
   │
   ▼
GET /api/tickets/assigned
   │
   ▼
5. Técnico resuelve
   │
   ▼
Resolved
   │
   ▼
6. Administrador cierra
   │
   ▼
Closed
```

Si posteriormente el ticket necesita atención nuevamente:

```text
Closed
   │
   ▼
Reopened
   │
   ▼
Nueva asignación
   │
   ▼
InProgress
```

---

# 21. Consideraciones

Esta documentación describe la API implementada actualmente.

Las funcionalidades futuras no deben considerarse parte del contrato actual hasta que hayan sido implementadas y probadas.

Cuando se agreguen o modifiquen endpoints, esta documentación debe actualizarse junto con el código correspondiente.
