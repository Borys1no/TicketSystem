# Architecture

## 1. Objetivo

SupportFlow utiliza una arquitectura basada en los principios de **Clean Architecture**, complementada con conceptos de **Domain-Driven Design (DDD)**.

El objetivo principal es separar las responsabilidades del sistema para evitar que la lógica de negocio, la persistencia y la exposición HTTP queden mezcladas.

La aplicación está dividida en cuatro proyectos:

```text
SupportFlow
│
├── SupportFlow.Domain
├── SupportFlow.Application
├── SupportFlow.Infrastructure
└── SupportFlow.API
```

La separación permite que cada capa tenga una responsabilidad clara.

---

# 2. Vista general

La arquitectura puede representarse de la siguiente manera:

```text
                         ┌─────────────────────┐
                         │     Cliente HTTP     │
                         └──────────┬──────────┘
                                    │
                                    ▼
                         ┌─────────────────────┐
                         │   SupportFlow.API   │
                         │                     │
                         │ Controllers         │
                         │ Middleware          │
                         │ Authentication      │
                         └──────────┬──────────┘
                                    │
                                    ▼
                    ┌──────────────────────────────┐
                    │   SupportFlow.Application    │
                    │                              │
                    │ Commands                     │
                    │ Queries                      │
                    │ DTOs                         │
                    │ Interfaces                   │
                    │ Mappers                      │
                    └──────────────┬───────────────┘
                                   │
                         ┌─────────┴─────────┐
                         │                   │
                         ▼                   ▼
              ┌──────────────────┐   ┌──────────────────┐
              │ SupportFlow      │   │ SupportFlow      │
              │ Domain           │   │ Infrastructure   │
              │                  │   │                  │
              │ Entities         │   │ EF Core          │
              │ Enums            │   │ PostgreSQL       │
              │ Business Rules   │   │ Repositories     │
              │                  │   │ JWT / BCrypt     │
              └──────────────────┘   └──────────────────┘
```

La capa Domain representa el núcleo del negocio.

Application contiene los casos de uso.

Infrastructure implementa las dependencias externas.

API expone el sistema mediante HTTP.

---

# 3. Regla de dependencias

Una de las ideas principales de Clean Architecture es controlar la dirección de las dependencias.

La regla general utilizada en SupportFlow es:

```text
API
 │
 ▼
Application
 │
 ▼
Domain
```

Infrastructure implementa las interfaces definidas por Application:

```text
Application
     │
     │ define
     ▼
Interfaces
     ▲
     │ implementa
     │
Infrastructure
```

De esta forma, Application no necesita conocer detalles concretos de PostgreSQL, Entity Framework Core, BCrypt o JWT.

---

# 4. SupportFlow.Domain

Domain es el núcleo del sistema.

Su responsabilidad es representar conceptos del negocio y mantener sus reglas fundamentales.

Actualmente contiene:

```text
SupportFlow.Domain
│
├── Entities
│   ├── Ticket.cs
│   └── User.cs
│
└── Enums
    ├── Department.cs
    ├── Priority.cs
    ├── Role.cs
    └── TicketStatus.cs
```

## 4.1 Entidades

Las entidades principales son:

### User

Representa a un usuario del sistema.

Contiene información necesaria para identificar al usuario y determinar su rol dentro del sistema.

### Ticket

Representa una solicitud de soporte.

Además de almacenar información, `Ticket` contiene reglas relacionadas con su ciclo de vida.

Entre las operaciones del dominio se encuentran:

```text
AssignTo()
Resolve()
Close()
Reopen()
```

Esto evita que la aplicación tenga que modificar directamente el estado interno del ticket sin pasar por sus reglas.

---

# 5. Enums del dominio

Los enums permiten representar valores controlados por las reglas del sistema.

## Role

```text
Employee
Technician
Adminitrador
```

## Department

```text
Contabilidad
Gerencia
Sistemas
Produccion
```

## Priority

```text
Low
Medium
High
Critical
```

## TicketStatus

```text
Pending
InProgress
Resolved
Closed
Reopened
```

Los valores forman parte del modelo de dominio y se utilizan en las diferentes capas de la aplicación.

---

# 6. SupportFlow.Application

Application representa la capa de casos de uso.

Su responsabilidad es coordinar las operaciones que el sistema puede realizar sin encargarse directamente de detalles como HTTP o SQL.

Estructura:

```text
SupportFlow.Application
│
├── Commands
│   ├── Auth
│   ├── Tickets
│   └── Users
│
├── DTOs
│   ├── Tickets
│   └── Users
│
├── Interfaces
│
├── Mappers
│
└── Queries
    ├── Tickets
    └── Users
```

---

# 7. Commands

Los Commands representan operaciones que modifican el estado del sistema.

Actualmente existen Commands para:

### Autenticación

```text
LoginCommand
LoginCommandHandler
```

### Usuarios

```text
CreateUserCommand
CreateUserCommandHandler
```

### Tickets

```text
CreateTicketCommand
AssignTicketCommand
ResolveTicketCommand
CloseTicketCommand
ReopenTicketCommand
```

Cada Command tiene un Handler responsable de ejecutar el caso de uso.

Ejemplo conceptual:

```text
HTTP Request
     │
     ▼
Controller
     │
     ▼
Command
     │
     ▼
CommandHandler
     │
     ▼
Domain
     │
     ▼
Repository
```

---

# 8. Queries

Las Queries representan operaciones de lectura.

Actualmente existen consultas para usuarios y tickets.

## Tickets

```text
GetTicketsQueryHandler
GetTicketByIdQueryHandler
GetMyTicketsQueryHandler
GetMyAssignedTicketsQueryHandler
```

## Users

```text
GetUsersQuery
GetUsersQueryHandler

GetUserByIdQuery
GetUserByIdQueryHandler
```

El objetivo es mantener separadas las operaciones de lectura de las operaciones que modifican el sistema.

---

# 9. DTOs

Los DTOs permiten controlar qué información entra y sale de la API.

Actualmente existen DTOs para tickets y usuarios:

```text
DTOs
├── Tickets
│   ├── AssignTicketRequest
│   ├── CreateTicketRequest
│   └── TicketResponse
│
└── Users
    ├── CreateUserRequest
    └── UserResponse
```

Esta separación evita exponer directamente las entidades del dominio como contrato de la API.

Por ejemplo:

```text
Cliente
   │
   ▼
CreateTicketRequest
   │
   ▼
CreateTicketCommand
   │
   ▼
Ticket
```

Para las respuestas:

```text
Ticket
   │
   ▼
TicketMapper
   │
   ▼
TicketResponse
   │
   ▼
Cliente
```

---

# 10. Interfaces

Application define abstracciones para servicios que necesita utilizar.

Actualmente existen:

```text
IJwtTokenGenerator
IPasswordHasher
ITicketRepository
IUserRepository
```

Estas interfaces permiten que Application dependa de contratos y no de implementaciones concretas.

Por ejemplo:

```text
Application
     │
     ▼
ITicketRepository
     ▲
     │
     │ implementación
     │
Infrastructure
     │
     ▼
TicketRepository
```

Esto facilita cambiar la implementación sin modificar los casos de uso.

---

# 11. Mappers

Los mappers transforman entidades en DTOs.

Actualmente existen:

```text
TicketMapper
UserMapper
```

Su responsabilidad es evitar que los Controllers tengan que construir manualmente las respuestas.

El flujo es:

```text
Entity
   │
   ▼
Mapper
   │
   ▼
Response DTO
```

---

# 12. SupportFlow.Infrastructure

Infrastructure contiene las implementaciones concretas de servicios externos.

Estructura:

```text
SupportFlow.Infrastructure
│
├── Configurations
│   └── TicketConfiguration.cs
│
├── Data
│   └── ApplicationDbContext.cs
│
├── Migrations
│
├── Repositories
│   ├── TicketRepository.cs
│   └── UserRepository.cs
│
├── Security
│   ├── JwTokenGenerator.cs
│   └── PasswordHasher.cs
│
└── DependencyInjection.cs
```

---

# 13. Persistencia

La persistencia utiliza:

```text
Entity Framework Core
        │
        ▼
Npgsql
        │
        ▼
PostgreSQL
```

`ApplicationDbContext` representa la sesión de Entity Framework Core con la base de datos.

Los repositorios encapsulan las operaciones de persistencia.

Actualmente existen:

```text
TicketRepository
UserRepository
```

Estos implementan las interfaces definidas en Application.

---

# 14. Configuración de entidades

Las configuraciones específicas de Entity Framework Core se mantienen separadas del `DbContext` cuando corresponde.

Actualmente existe:

```text
Configurations/TicketConfiguration.cs
```

Su responsabilidad es definir cómo la entidad `Ticket` se representa en la base de datos.

Esto evita sobrecargar las entidades del dominio con detalles propios de Entity Framework Core.

---

# 15. Migraciones

Las migraciones de Entity Framework Core se almacenan en:

```text
SupportFlow.Infrastructure/Migrations/
```

Actualmente existe una migración inicial:

```text
20260817201222_InitialCreate
```

Además se mantiene:

```text
ApplicationDbContextModelSnapshot
```

El historial de migraciones permite versionar la estructura de la base de datos junto con el código.

---

# 16. Seguridad e infraestructura

Infrastructure también contiene implementaciones relacionadas con seguridad.

## PasswordHasher

```text
IPasswordHasher
       ▲
       │
PasswordHasher
```

La implementación utiliza BCrypt para generar y verificar hashes de contraseñas.

## JWT

```text
IJwtTokenGenerator
       ▲
       │
JwTokenGenerator
```

La implementación concreta genera los tokens utilizados por la API.

Application solamente conoce las interfaces.

---

# 17. Dependency Injection

Las implementaciones de Infrastructure se registran mediante:

```text
DependencyInjection.cs
```

La API utiliza esta configuración al iniciar la aplicación.

El objetivo es centralizar el registro de:

* `DbContext`
* Repositorios.
* Hashing de contraseñas.
* Generación de JWT.
* Servicios necesarios por Application.

Conceptualmente:

```text
Program.cs
    │
    ▼
Infrastructure Dependency Injection
    │
    ├── DbContext
    ├── Repositories
    ├── PasswordHasher
    └── JWT Generator
```

---

# 18. SupportFlow.API

API es la capa de entrada HTTP.

Actualmente contiene:

```text
SupportFlow.API
│
├── Controllers
│   ├── AuthController.cs
│   ├── TicketsController.cs
│   └── UsersController.cs
│
├── Middleware
│   └── ExceptionHandlingMiddleware.cs
│
├── Program.cs
└── Properties
    └── launchSettings.json
```

---

# 19. Controllers

Los Controllers tienen como responsabilidad:

1. Recibir solicitudes HTTP.
2. Validar información básica necesaria para la solicitud.
3. Obtener la identidad del usuario autenticado cuando corresponde.
4. Construir Commands o ejecutar Queries.
5. Devolver respuestas HTTP.

No deberían contener las reglas principales del dominio.

---

# 20. AuthController

`AuthController` expone las operaciones relacionadas con autenticación.

El flujo principal es:

```text
Login Request
      │
      ▼
AuthController
      │
      ▼
LoginCommand
      │
      ▼
LoginCommandHandler
      │
      ├── UserRepository
      │
      ├── PasswordHasher
      │
      └── JwtTokenGenerator
      │
      ▼
JWT
```

---

# 21. UsersController

`UsersController` expone operaciones relacionadas con usuarios.

Entre ellas:

```text
Crear usuario
Consultar usuarios
Consultar usuario por ID
```

La creación utiliza:

```text
CreateUserCommand
CreateUserCommandHandler
```

El Handler realiza las validaciones correspondientes y utiliza `IUserRepository` para persistir el usuario.

---

# 22. TicketsController

`TicketsController` expone las operaciones principales del sistema.

Entre ellas:

```text
Crear ticket
Consultar tickets
Consultar ticket por ID
Consultar mis tickets
Consultar tickets asignados
Asignar ticket
Resolver ticket
Cerrar ticket
Reabrir ticket
```

El Controller delega la lógica a Commands y Queries.

Ejemplo:

```text
POST /api/tickets
        │
        ▼
TicketsController
        │
        ▼
CreateTicketCommand
        │
        ▼
CreateTicketCommandHandler
        │
        ▼
Ticket
        │
        ▼
TicketRepository
```

---

# 23. Middleware global

La API utiliza:

```text
ExceptionHandlingMiddleware
```

Este middleware intercepta excepciones que no fueron gestionadas por capas inferiores y las transforma en respuestas HTTP.

La intención es evitar repetir la misma lógica de manejo de errores en todos los Controllers.

El flujo es:

```text
Request
   │
   ▼
ExceptionHandlingMiddleware
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
   ├── éxito ───────────────► Response
   │
   └── excepción
            │
            ▼
ExceptionHandlingMiddleware
            │
            ▼
HTTP Error Response
```

---

# 24. Autenticación y autorización

La autenticación utiliza JWT Bearer.

La autorización se realiza mediante roles.

Ejemplo:

```text
[Authorize(Roles = "Technician")]
```

Esto significa que la API comprueba primero si existe una identidad autenticada y posteriormente si esa identidad posee el rol requerido.

La autorización HTTP no sustituye las reglas del dominio.

Ambas trabajan juntas:

```text
JWT
 │
 ▼
Authentication
 │
 ▼
Authorization
 │
 ▼
Application
 │
 ▼
Domain Rules
```

---

# 25. Identidad del usuario

Cuando una operación depende del usuario autenticado, la API obtiene su identificador desde:

```text
ClaimTypes.NameIdentifier
```

Por ejemplo, al crear un ticket:

```text
JWT
 │
 ▼
NameIdentifier
 │
 ▼
CreatedByUserId
 │
 ▼
Ticket
```

Esto evita confiar en identificadores sensibles enviados por el cliente.

---

# 26. Flujo completo de una operación

Un ejemplo completo es la creación de un ticket.

```text
┌──────────────────┐
│ Cliente HTTP     │
└────────┬─────────┘
         │
         │ POST /api/tickets
         ▼
┌──────────────────┐
│ TicketsController│
└────────┬─────────┘
         │
         │ CreateTicketCommand
         ▼
┌──────────────────────────┐
│ CreateTicketCommandHandler│
└────────┬─────────────────┘
         │
         ▼
┌──────────────────┐
│ Ticket Domain    │
│                  │
│ Pending          │
│ CreatedAt        │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ ITicketRepository│
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ TicketRepository │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Entity Framework │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ PostgreSQL       │
└──────────────────┘
```

La respuesta vuelve siguiendo el camino inverso mediante `TicketMapper` y `TicketResponse`.

---

# 27. Por qué no se accede directamente a la base de datos desde los Controllers

Una decisión importante es evitar este tipo de estructura:

```text
Controller
    │
    ▼
DbContext
    │
    ▼
PostgreSQL
```

Aunque puede funcionar en proyectos pequeños, mezcla responsabilidades.

SupportFlow utiliza:

```text
Controller
    │
    ▼
Application
    │
    ▼
Repository Interface
    │
    ▼
Repository Implementation
    │
    ▼
DbContext
    │
    ▼
PostgreSQL
```

Esto proporciona una separación más clara entre HTTP, casos de uso y persistencia.

---

# 28. Por qué Application define interfaces

Application necesita persistir información, generar tokens y trabajar con contraseñas, pero no necesita conocer cómo se realizan técnicamente esas operaciones.

Por eso define contratos como:

```text
ITicketRepository
IUserRepository
IPasswordHasher
IJwtTokenGenerator
```

Infrastructure proporciona las implementaciones.

Esto reduce el acoplamiento y facilita reemplazar componentes en el futuro.

---

# 29. Por qué las reglas de tickets están en Domain

Una regla como:

```text
Un ticket InProgress puede ser resuelto.
```

es una regla del negocio, no una regla HTTP ni una regla de PostgreSQL.

Por eso pertenece al dominio.

De esta forma, incluso si en el futuro SupportFlow tuviera:

* una aplicación web;
* una aplicación móvil;
* un proceso automático;
* una integración externa;

todos deberían respetar las mismas reglas del ticket.

---

# 30. Estado actual de la arquitectura

La arquitectura actual proporciona:

* Separación entre dominio, aplicación, infraestructura y API.
* Commands y Queries para los casos de uso.
* Interfaces para depe
