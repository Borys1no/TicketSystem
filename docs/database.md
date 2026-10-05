# Base de Datos — SupportFlow

## 1. Propósito

SupportFlow utiliza PostgreSQL como sistema de gestión de base de datos y Entity Framework Core como ORM.

La persistencia se encuentra dentro del proyecto `SupportFlow.Infrastructure`, manteniendo separadas las responsabilidades de infraestructura respecto al dominio y la aplicación.

La base de datos almacena actualmente dos entidades principales:

* `Users`
* `Tickets`

---

## 2. Tecnologías

* PostgreSQL
* Entity Framework Core 8
* Npgsql
* .NET 8

Entity Framework Core se utiliza para:

* Mapear las entidades del dominio a tablas.
* Consultar y modificar información.
* Configurar claves y relaciones.
* Administrar migraciones.
* Mantener sincronizado el modelo de aplicación con la estructura de la base de datos.

---

## 3. DbContext

El acceso principal a la base de datos se centraliza mediante:

`SupportFlow.Infrastructure.Data.ApplicationDbContext`

El contexto expone actualmente dos conjuntos:

```csharp
public DbSet<Ticket> Tickets { get; set; }
public DbSet<User> Users { get; set; }
```

Esto representa las dos tablas principales utilizadas por la aplicación.

La configuración adicional de las entidades se carga mediante:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(ApplicationDbContext).Assembly
);
```

Esto permite mantener la configuración de cada entidad separada del `DbContext`.

Por ejemplo, la configuración de `Ticket` se encuentra en:

`SupportFlow.Infrastructure/Configurations/TicketConfiguration.cs`

---

# 4. Tabla Users

La entidad `User` representa a los usuarios registrados en SupportFlow.

Actualmente contiene los siguientes campos:

| Campo          | Tipo de dominio | Nullable | Descripción                     |
| -------------- | --------------- | -------: | ------------------------------- |
| `Id`           | `Guid`          |       No | Identificador único del usuario |
| `Name`         | `string`        |       No | Nombre                          |
| `LastName`     | `string`        |       No | Apellido                        |
| `Email`        | `string`        |       No | Correo electrónico              |
| `PasswordHash` | `string`        |       No | Contraseña almacenada como hash |
| `Department`   | `Department`    |       No | Departamento del usuario        |
| `PhoneNumber`  | `string`        |       Sí | Número telefónico               |
| `Role`         | `Role`          |       No | Rol de autorización             |

El identificador se genera dentro de la entidad mediante:

```csharp
Id = Guid.NewGuid();
```

---

## 5. Seguridad de usuarios

La entidad `User` no almacena la contraseña original.

El campo:

```text
PasswordHash
```

contiene el resultado del proceso de hashing realizado por la infraestructura.

Actualmente SupportFlow utiliza BCrypt para generar y verificar las contraseñas.

El flujo es:

```text
Contraseña proporcionada
        │
        ▼
PasswordHasher
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

Durante el inicio de sesión, la contraseña proporcionada por el usuario se verifica contra el hash almacenado.

---

# 6. Roles y departamentos

Los campos `Role` y `Department` corresponden a enumeraciones del dominio.

### Roles

Actualmente están definidos:

* `Employee`
* `Technician`
* `Adminitrador`

> `Adminitrador` mantiene intencionalmente la escritura actual del código porque forma parte de los valores utilizados por la autorización JWT.

### Departamentos

Actualmente están definidos:

* `Contabilidad`
* `Gerencia`
* `Sistemas`
* `Produccion`

La validación de los valores pertenece al dominio.

---

# 7. Tabla Tickets

La entidad `Ticket` representa una solicitud de soporte.

Actualmente contiene:

| Campo              | Tipo de dominio | Nullable | Descripción                    |
| ------------------ | --------------- | -------: | ------------------------------ |
| `Id`               | `Guid`          |       No | Identificador único del ticket |
| `TicketNumber`     | `int`           |       No | Número secuencial del ticket   |
| `Title`            | `string`        |       No | Título del ticket              |
| `Description`      | `string`        |       Sí | Descripción del problema       |
| `Priority`         | `Priority`      |       No | Prioridad del ticket           |
| `Status`           | `TicketStatus`  |       No | Estado actual                  |
| `CreateAt`         | `DateTime`      |       No | Fecha y hora de creación       |
| `CreatedByUserId`  | `Guid`          |       No | Usuario que creó el ticket     |
| `AssignedToUserId` | `Guid`          |       Sí | Técnico asignado               |

---

# 8. Identificación de tickets

Los tickets utilizan dos identificadores diferentes.

### Id

```csharp
Guid Id
```

Es el identificador interno único de la entidad.

Se genera mediante:

```csharp
Id = Guid.NewGuid();
```

### TicketNumber

```csharp
int TicketNumber
```

Es el número utilizado para identificar el ticket de una forma más sencilla para los usuarios.

Actualmente está configurado como generado por la base de datos:

```csharp
builder.Property(x => x.TicketNumber)
    .ValueGeneratedOnAdd();
```

Por lo tanto, `Id` y `TicketNumber` cumplen funciones diferentes.

---

# 9. Configuración de Ticket

La configuración de persistencia se encuentra en:

`SupportFlow.Infrastructure/Configurations/TicketConfiguration.cs`

La clave primaria es:

```csharp
builder.HasKey(x => x.Id);
```

Por lo tanto:

```text
Tickets.Id
```

es la clave primaria de la tabla.

---

# 10. Restricciones de campos

### Title

El título es obligatorio y tiene un máximo de 200 caracteres:

```csharp
builder.Property(x => x.Title)
    .IsRequired()
    .HasMaxLength(200);
```

### Description

La descripción es opcional y tiene un máximo de 2000 caracteres:

```csharp
builder.Property(x => x.Description)
    .HasMaxLength(2000);
```

### Priority

La prioridad es obligatoria y se almacena como texto:

```csharp
builder.Property(x => x.Priority)
    .HasConversion<string>()
    .IsRequired();
```

Por lo tanto, la base de datos no almacena directamente el valor numérico del enum.

### Status

El estado también es obligatorio y se almacena como texto:

```csharp
builder.Property(x => x.Status)
    .HasConversion<string>()
    .IsRequired();
```

### CreateAt

La fecha de creación es obligatoria:

```csharp
builder.Property(x => x.CreateAt)
    .IsRequired();
```

La entidad genera la fecha utilizando UTC:

```csharp
CreateAt = DateTime.UtcNow;
```

---

# 11. Relación entre Tickets y Users

Un ticket tiene dos referencias hacia la tabla `Users`.

### Usuario creador

```text
Tickets.CreatedByUserId
        │
        ▼
Users.Id
```

Representa al usuario que creó el ticket.

La relación se configura mediante:

```csharp
builder.HasOne<User>()
    .WithMany()
    .HasForeignKey(x => x.CreatedByUserId)
    .OnDelete(DeleteBehavior.Restrict);
```

### Técnico asignado

```text
Tickets.AssignedToUserId
        │
        ▼
Users.Id
```

Representa al técnico responsable actualmente del ticket.

Esta relación es opcional porque `AssignedToUserId` es nullable.

Se configura mediante:

```csharp
builder.HasOne<User>()
    .WithMany()
    .HasForeignKey(x => x.AssignedToUserId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

# 12. Diagrama simplificado

```text
┌─────────────────────────┐
│          Users          │
├─────────────────────────┤
│ Id (PK)                 │
│ Name                    │
│ LastName                │
│ Email                   │
│ PasswordHash            │
│ Department              │
│ PhoneNumber             │
│ Role                    │
└────────────┬────────────┘
             │
       ┌─────┴─────┐
       │           │
       │           │
CreatedBy     AssignedTo
       │           │
       ▼           ▼
┌─────────────────────────┐
│         Tickets         │
├─────────────────────────┤
│ Id (PK)                 │
│ TicketNumber            │
│ Title                   │
│ Description             │
│ Priority                │
│ Status                  │
│ CreateAt                │
│ CreatedByUserId (FK)    │
│ AssignedToUserId (FK)   │
└─────────────────────────┘
```

Un mismo usuario puede aparecer como creador de tickets y también como técnico asignado a otros tickets, siempre que su rol permita realizar dicha asignación.

---

# 13. Eliminación de usuarios relacionados

Las relaciones entre `Tickets` y `Users` utilizan:

```csharp
DeleteBehavior.Restrict
```

Esto evita que eliminar un usuario provoque automáticamente la eliminación de los tickets relacionados.

La decisión protege el historial de soporte.

Por ejemplo, un ticket creado anteriormente debe conservar información sobre su creador aunque ese usuario deje de utilizar el sistema.

Actualmente no existe un `Cascade Delete` entre usuarios y tickets.

---

# 14. Navegación entre entidades

Actualmente `Ticket` almacena los identificadores:

```csharp
public Guid CreatedByUserId { get; private set; }
public Guid? AssignedToUserId { get; private set; }
```

pero no contiene propiedades de navegación como:

```csharp
public User CreatedByUser { get; private set; }
public User? AssignedToUser { get; private set; }
```

La configuración actual utiliza directamente las claves foráneas mediante:

```csharp
HasOne<User>()
.WithMany()
.HasForeignKey(...)
```

Esto mantiene las entidades del dominio sencillas y evita introducir propiedades de navegación que actualmente no son necesarias para las operaciones implementadas.

Si en el futuro las consultas necesitan recuperar frecuentemente información completa del creador o técnico asignado, esta decisión podrá revisarse.

---

# 15. Migraciones

Entity Framework Core utiliza migraciones para versionar los cambios realizados en el modelo de persistencia.

Las migraciones se encuentran actualmente en:

`SupportFlow.Infrastructure/Migrations`

La migración inicial existente es:

```text
20260817201222_InitialCreate
```

También se mantiene:

```text
ApplicationDbContextModelSnapshot.cs
```

El snapshot representa el estado conocido del modelo de Entity Framework Core y permite detectar cambios posteriores.

---

# 16. Flujo de una migración

El flujo conceptual es:

```text
Cambio en entidades/configuración
          │
          ▼
dotnet ef migrations add <Nombre>
          │
          ▼
Nueva migración
          │
          ▼
dotnet ef database update
          │
          ▼
PostgreSQL
```

Las migraciones deben formar parte del control de versiones del proyecto.

Esto permite que la estructura de la base de datos pueda reproducirse y evolucionar junto con el código.

---

# 17. Separación de responsabilidades

La persistencia está organizada de acuerdo con Clean Architecture.

```text
Domain
  │
  │ define entidades
  ▼
Application
  │
  │ define interfaces
  ▼
Infrastructure
  │
  ├── DbContext
  ├── Configurations
  ├── Repositories
  ├── Migrations
  └── Security
        │
        ▼
   PostgreSQL
```

El dominio no conoce PostgreSQL ni Entity Framework Core.

La aplicación tampoco depende directamente de una implementación concreta de PostgreSQL.

Infrastructure es responsable de implementar la persistencia.

---

# 18. Repositorios

La aplicación define interfaces de repositorio, por ejemplo:

```text
ITicketRepository
IUserRepository
```

Infrastructure contiene las implementaciones concretas.

Esto permite que Application trabaje con abstracciones en lugar de depender directamente de `ApplicationDbContext`.

El flujo es:

```text
Application
     │
     ▼
ITicketRepository
     │
     ▼
TicketRepository
     │
     ▼
ApplicationDbContext
     │
     ▼
PostgreSQL
```

---

# 19. Estado actual de la base de datos

La persistencia actual cubre las necesidades principales del sistema:

* Usuarios.
* Autenticación mediante credenciales almacenadas como hash.
* Roles.
* Departamentos.
* Tickets.
* Prioridades.
* Estados.
* Usuario creador.
* Técnico asignado.
* Fecha de creación.
* Número secuencial de ticket.
* Migraciones de Entity Framework Core.

La estructura es deliberadamente sencilla porque SupportFlow se encuentra todavía en una etapa inicial de desarrollo.

---

# 20. Posibles evoluciones

A medida que aumente la funcionalidad del sistema podrían aparecer nuevas necesidades de persistencia, por ejemplo:

* Historial de cambios de estado.
* Historial de asignaciones.
* Comentarios de los tickets.
* Archivos adjuntos.
* Fechas de resolución y cierre.
* Auditoría.
* Notificaciones.
* Categorías de tickets.
* SLA.
* Registro de actividad.
* Soft delete de usuarios.
* Índices adicionales para consultas frecuentes.

Estas funcionalidades no forman parte del modelo actual y no deben considerarse implementadas hasta que sean incorporadas explícitamente al dominio, aplicación e infraestructura.

---

## 21. Principio de diseño

La base de datos debe evolucionar junto con el dominio y no convertirse en el lugar donde se concentra la lógica de negocio.

Las reglas importantes del sistema pertenecen al dominio.

Entity Framework Core y PostgreSQL tienen como responsabilidad principal persistir ese modelo.

La infraestructura debe facilitar la persistencia sin convertirse en la fuente de las reglas de negocio.

---

## 22. Resumen

La persistencia actual de SupportFlow utiliza una estructura simple:

```text
Users
  │
  ├── CreatedByUserId ──► Tickets
  │
  └── AssignedToUserId ─► Tickets
```

con PostgreSQL como base de datos, Entity Framework Core como ORM y migraciones para controlar la evolución del esquema.

Esta estructura es suficiente para la funcionalidad implementada actualmente y puede evolucionar posteriormente sin comprometer la separación de responsabilidades de la arquitectura.
