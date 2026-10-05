# SupportFlow

SupportFlow es una API para la gestión de tickets de soporte desarrollada con ASP.NET Core y C#.

El proyecto fue construido aplicando principios de Clean Architecture y Domain-Driven Design (DDD), buscando separar las reglas de negocio, los casos de uso, la infraestructura y la exposición HTTP de la aplicación.

El sistema permite gestionar usuarios, autenticación, autorización por roles y el ciclo de vida de los tickets de soporte.

## Objetivos

Los principales objetivos del proyecto son:

* Implementar una API REST para la gestión de tickets.
* Aplicar una separación clara de responsabilidades mediante Clean Architecture.
* Centralizar las reglas de negocio dentro del dominio.
* Implementar autenticación mediante JWT.
* Proteger las contraseñas mediante hashing con BCrypt.
* Implementar autorización basada en roles.
* Persistir información utilizando PostgreSQL y Entity Framework Core.
* Implementar el ciclo de vida de un ticket mediante reglas de dominio.
* Manejar errores de forma centralizada.
* Mantener una estructura preparada para futuras extensiones.

## Tecnologías

* C#
* .NET 8
* ASP.NET Core
* Entity Framework Core
* PostgreSQL
* Npgsql
* JWT Bearer Authentication
* BCrypt.Net-Next
* Swagger / OpenAPI

## Arquitectura

SupportFlow utiliza una arquitectura basada en cuatro proyectos principales:

```text
SupportFlow
│
├── SupportFlow.Domain
├── SupportFlow.Application
├── SupportFlow.Infrastructure
└── SupportFlow.API
```

### Domain

Contiene el núcleo del sistema y las reglas de negocio.

```text
SupportFlow.Domain
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

Esta capa no depende de infraestructura ni de ASP.NET Core.

Entre las principales reglas del dominio se encuentra el ciclo de vida de los tickets, incluyendo las operaciones de asignación, resolución, cierre y reapertura.

### Application

Contiene los casos de uso de la aplicación y las abstracciones necesarias para ejecutarlos.

```text
SupportFlow.Application
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
├── Mappers
│
└── Queries
    ├── Tickets
    └── Users
```

Los Commands representan operaciones que modifican el estado del sistema.

Los Queries representan operaciones de consulta.

Esta capa define interfaces para repositorios y servicios como generación de tokens y hashing de contraseñas, sin depender directamente de sus implementaciones.

### Infrastructure

Contiene las implementaciones concretas relacionadas con persistencia, seguridad e infraestructura externa.

```text
SupportFlow.Infrastructure
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

Aquí se encuentran Entity Framework Core, PostgreSQL, los repositorios, la generación de JWT y el hashing de contraseñas.

### API

Es la capa de entrada HTTP de la aplicación.

```text
SupportFlow.API
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

Los Controllers reciben las peticiones HTTP y delegan la ejecución de los casos de uso a la capa Application.

El middleware de excepciones proporciona un punto centralizado para convertir excepciones de la aplicación en respuestas HTTP.

## Funcionalidades implementadas

### Usuarios

La API permite:

* Crear usuarios.
* Consultar usuarios.
* Consultar usuarios por identificador.
* Validar que el correo electrónico no esté duplicado.
* Normalizar los correos electrónicos.
* Almacenar contraseñas mediante BCrypt.

### Autenticación

El sistema utiliza JWT para autenticar usuarios.

El flujo general es:

```text
Usuario
   │
   ▼
POST /api/auth/login
   │
   ▼
Validación de credenciales
   │
   ▼
Verificación BCrypt
   │
   ▼
Generación de JWT
   │
   ▼
Token
```

El identificador y el rol del usuario se incluyen en las claims utilizadas posteriormente para autorización.

### Autorización

El acceso a las operaciones se controla mediante roles.

Los roles actualmente definidos son:

```text
Employee
Technician
Adminitrador
```

`Adminitrador` mantiene deliberadamente esta escritura porque forma parte del valor utilizado actualmente en las claims y atributos de autorización.

### Tickets

La API permite:

* Crear tickets.
* Consultar tickets.
* Consultar un ticket por ID.
* Consultar los tickets creados por el usuario autenticado.
* Consultar los tickets asignados al técnico autenticado.
* Asignar tickets.
* Resolver tickets.
* Cerrar tickets.
* Reabrir tickets.

## Ciclo de vida de un ticket

Los estados disponibles son:

```text
Pending
   │
   ▼
InProgress
   │
   ▼
Resolved
   │
   ▼
Closed
   │
   ▼
Reopened
   │
   ▼
InProgress
```

No todas las transiciones son libres. Las operaciones disponibles están protegidas por reglas de dominio.

Por ejemplo:

* Un ticket nuevo comienza en `Pending`.
* La asignación de un técnico mueve el ticket a `InProgress`.
* Un técnico asignado puede resolver el ticket.
* Un ticket resuelto puede ser cerrado.
* Un ticket cerrado puede ser reabierto.
* La resolución está restringida al técnico al que fue asignado el ticket.

## Prioridades

Los tickets pueden tener las siguientes prioridades:

```text
Low
Medium
High
Critical
```

Todo ticket debe tener una prioridad.

Como decisión inicial del dominio, cuando dos tickets tienen la misma prioridad se considera el orden de llegada (FIFO).

La prioridad automática mediante reglas de negocio queda como una posible evolución futura.

## Roles y permisos

La autorización actual se basa en los siguientes roles:

| Operación                   |    Employee    |   Technician   |  Adminitrador  |
| --------------------------- | :------------: | :------------: | :------------: |
| Crear ticket                |       Sí       |       No       |       Sí       |
| Consultar todos los tickets |       No       |       Sí       |       Sí       |
| Consultar ticket por ID     |       No       |       Sí       |       Sí       |
| Consultar mis tickets       |       Sí       |       No       |       No       |
| Consultar tickets asignados |       No       |       Sí       |       No       |
| Resolver ticket             |       No       |       Sí       |       No       |
| Asignar ticket              |       No       |       No       |       Sí       |
| Cerrar ticket               |       No       |       No       |       Sí       |
| Reabrir ticket              |       No       |       No       |       Sí       |
| Crear usuario               | Según endpoint | Según endpoint | Según endpoint |

La autorización efectiva se encuentra definida mediante los atributos `[Authorize]` de los Controllers.

## Seguridad

### Contraseñas

Las contraseñas no se almacenan en texto plano.

El sistema utiliza BCrypt para generar y verificar hashes de contraseñas.

```text
Contraseña
    │
    ▼
BCrypt
    │
    ▼
PasswordHash
    │
    ▼
Base de datos
```

### JWT

Después de una autenticación correcta, la API genera un token JWT.

Los endpoints protegidos utilizan el token para identificar al usuario y determinar sus permisos.

### Identidad del creador

Al crear un ticket, el usuario creador no se toma desde el cuerpo enviado por el cliente.

El identificador se obtiene de la claim `NameIdentifier` del JWT.

Esto evita que un usuario autenticado pueda intentar crear un ticket atribuyéndolo a otro usuario mediante el request.

### Tickets asignados

La resolución de tickets también valida que el técnico autenticado sea realmente el técnico asignado al ticket.

Esto evita que un técnico pueda resolver tickets que pertenecen a otro técnico.

## Manejo de errores

SupportFlow cuenta con un middleware global de manejo de excepciones:

```text
ExceptionHandlingMiddleware
```

El middleware transforma excepciones conocidas en respuestas HTTP apropiadas.

Entre los casos contemplados:

| Excepción                     | HTTP |
| ----------------------------- | ---: |
| `KeyNotFoundException`        |  404 |
| `UnauthorizedAccessException` |  403 |
| `InvalidOperationException`   |  400 |
| Otras excepciones             |  500 |

Las respuestas utilizan actualmente un formato sencillo:

```json
{
  "message": "Mensaje del error"
}
```

## Persistencia

La aplicación utiliza:

* PostgreSQL como base de datos.
* Entity Framework Core como ORM.
* Npgsql como proveedor de PostgreSQL.
* Migraciones de Entity Framework Core para versionar el esquema.

El contexto principal es:

```text
ApplicationDbContext
```

La configuración específica de los tickets se encuentra en:

```text
SupportFlow.Infrastructure/Configurations/TicketConfiguration.cs
```

## Migraciones

Las migraciones se encuentran dentro de:

```text
src/SupportFlow.Infrastructure/Migrations/
```

Para aplicar las migraciones a la base de datos se utiliza Entity Framework Core.

Ejemplo:

```bash
dotnet ef database update \
  --project src/SupportFlow.Infrastructure \
  --startup-project src/SupportFlow.API
```

## Ejecución local

### Requisitos

Para ejecutar el proyecto localmente se necesita:

* .NET 8 SDK
* PostgreSQL
* EF Core CLI

Comprobar la versión de .NET:

```bash
dotnet --version
```

### Restaurar dependencias

Desde la raíz del proyecto:

```bash
dotnet restore
```

### Compilar

```bash
dotnet build
```

### Configurar PostgreSQL

La cadena de conexión se configura mediante los archivos de configuración de la API:

```text
src/SupportFlow.API/appsettings.json
src/SupportFlow.API/appsettings.Development.json
```

Las credenciales reales de una instalación local no deben subirse al repositorio.

### Ejecutar migraciones

```bash
dotnet ef database update \
  --project src/SupportFlow.Infrastructure \
  --startup-project src/SupportFlow.API
```

### Ejecutar la API

```bash
dotnet run --project src/SupportFlow.API
```

La URL utilizada actualmente durante el desarrollo es:

```text
http://localhost:5098
```

## Swagger

Durante el desarrollo la API expone documentación interactiva mediante Swagger/OpenAPI.

Esto permite consultar los endpoints disponibles y realizar pruebas HTTP directamente desde el navegador.

## Endpoints principales

### Autenticación

```text
POST /api/auth/login
```

Permite autenticar un usuario y obtener un JWT.

### Usuarios

```text
GET  /api/users
GET  /api/users/{id}
POST /api/users
```

### Tickets

```text
POST /api/tickets
GET  /api/tickets
GET  /api/tickets/{id}
GET  /api/tickets/my
GET  /api/tickets/assigned
```

Operaciones del ciclo de vida:

```text
POST /api/tickets/{id}/assign
POST /api/tickets/{id}/resolve
POST /api/tickets/{id}/close
POST /api/tickets/{id}/reopen
```

Los requisitos de autenticación y los roles permitidos dependen de cada endpoint.

La documentación detallada de la API se encuentra en:

```text
docs/api.md
```

## Estructura del repositorio

```text
SupportFlow/
│
├── docs/
│   └── domain-decisions.md
│
├── src/
│   ├── SupportFlow.API/
│   ├── SupportFlow.Application/
│   ├── SupportFlow.Domain/
│   └── SupportFlow.Infrastructure/
│
├── .gitignore
├── README.md
├── SupportFlow.sln
└── resume.md
```

Los directorios `bin/` y `obj/` son artefactos generados por .NET y no forman parte de la arquitectura funcional del proyecto.

## Estado actual

La versión actual del proyecto cuenta con:

* Arquitectura Clean Architecture.
* Entidades de dominio para usuarios y tickets.
* Reglas de negocio para el ciclo de vida de tickets.
* PostgreSQL.
* Entity Framework Core.
* Migraciones.
* Repositorios.
* Creación y consulta de usuarios.
* Autenticación JWT.
* Hashing de contraseñas con BCrypt.
* Autorización basada en roles.
* Creación segura de tickets.
* Consulta de tickets.
* Consulta de tickets propios para empleados.
* Consulta de tickets asignados para técnicos.
* Asignación de tickets.
* Resolución de tickets.
* Cierre de tickets.
* Reapertura de tickets.
* Validaciones de propiedad/asignación.
* Manejo global de excepciones.

El proyecto representa una versión funcional de una API de gestión de tickets y puede utilizarse como base para futuras funcionalidades.

## Documentación

La documentación complementaria se encuentra en:

```text
docs/
├── architecture.md
├── api.md
├── database.md
├── development.md
├── domain-decisions.md
└── security.md
```

## Próximas posibles mejoras

Estas funcionalidades no forman parte de la versión actual y podrían incorporarse posteriormente:

* Pruebas automatizadas unitarias.
* Pruebas de integración.
* Paginación de consultas.
* Filtros y búsqueda de tickets.
* Historial de cambios de tickets.
* Sistema de comentarios.
* Notificaciones.
* Auditoría.
* Gestión más avanzada de permisos.
* Observabilidad y logging estructurado.
* Despliegue en un entorno cloud.
* Documentación OpenAPI más detallada.
* Mejoras de validación y manejo de errores.
* Frontend para consumidores de la API.

## Autor

Proyecto desarrollado como ejercicio práctico de arquitectura backend, desarrollo de APIs y aplicación de principios de diseño de software.
