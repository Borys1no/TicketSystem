# Guía de Desarrollo — SupportFlow

## 1. Propósito

Este documento explica cómo preparar el entorno de desarrollo, ejecutar SupportFlow, trabajar con la base de datos y seguir el flujo básico de desarrollo del proyecto.

Está dirigido principalmente a desarrolladores que necesiten ejecutar o modificar el proyecto localmente.

---

# 2. Requisitos

Para trabajar con SupportFlow se necesita:

* .NET 8 SDK.
* PostgreSQL.
* Git.
* Un editor o IDE compatible con .NET.
* `dotnet-ef` para trabajar con migraciones de Entity Framework Core.

Opcionalmente:

* Cliente gráfico para PostgreSQL.
* Herramientas para realizar solicitudes HTTP.
* Navegador para utilizar Swagger.

---

# 3. Estructura del proyecto

El repositorio utiliza una solución dividida en cuatro proyectos principales:

```text
SupportFlow/
├── src/
│   ├── SupportFlow.Domain/
│   ├── SupportFlow.Application/
│   ├── SupportFlow.Infrastructure/
│   └── SupportFlow.API/
│
├── docs/
│
└── SupportFlow.sln
```

Cada proyecto tiene una responsabilidad específica.

### Domain

Contiene:

* Entidades.
* Enumeraciones.
* Reglas de negocio.

### Application

Contiene:

* Commands.
* Queries.
* DTOs.
* Interfaces.
* Mappers.
* Casos de uso.

### Infrastructure

Contiene:

* Entity Framework Core.
* PostgreSQL.
* Repositorios.
* Seguridad.
* JWT.
* BCrypt.
* Migraciones.
* Configuraciones de persistencia.

### API

Contiene:

* Controllers.
* Middleware.
* Configuración de la aplicación.
* Punto de entrada HTTP.

---

# 4. Obtener el proyecto

Clonar el repositorio:

```bash
git clone git@github.com:Borys1no/TicketSystem.git
```

Entrar al proyecto:

```bash
cd TicketSystem
```

La carpeta local del proyecto utilizada durante el desarrollo es:

```text
~/Documentos/Projects/SupportFlow
```

---

# 5. Restaurar dependencias

Desde la raíz del proyecto:

```bash
dotnet restore
```

Esto restaura los paquetes NuGet utilizados por los proyectos de la solución.

---

# 6. Compilar el proyecto

Para comprobar que la solución compila:

```bash
dotnet build
```

Una compilación exitosa indica que los proyectos y sus dependencias pueden compilarse correctamente.

Durante el desarrollo es recomendable ejecutar `dotnet build` después de realizar cambios importantes.

---

# 7. PostgreSQL

SupportFlow utiliza PostgreSQL como base de datos.

La aplicación necesita una base de datos configurada antes de poder ejecutar correctamente las operaciones que requieren persistencia.

La base de datos utilizada durante el desarrollo es:

```text
supportflow
```

La configuración de conexión se encuentra en los archivos de configuración de la API y debe mantenerse fuera del código fuente cuando contenga credenciales reales.

---

# 8. Configuración

La API utiliza el sistema de configuración de ASP.NET Core.

Entre los valores necesarios se encuentran los relacionados con:

* Base de datos.
* JWT.
* Entorno de ejecución.

La configuración puede variar según el entorno:

```text
appsettings.json
appsettings.Development.json
```

Los valores sensibles no deben publicarse en el repositorio.

---

# 9. Entity Framework Core

Entity Framework Core se utiliza para administrar la persistencia.

El contexto principal es:

```text
SupportFlow.Infrastructure.Data.ApplicationDbContext
```

Las entidades actualmente persistidas son:

```text
User
Ticket
```

---

# 10. Migraciones

Las migraciones permiten versionar la estructura de la base de datos.

Las migraciones existentes se encuentran en:

```text
src/SupportFlow.Infrastructure/Migrations
```

La migración inicial actualmente registrada es:

```text
20260817201222_InitialCreate
```

---

# 11. Crear una migración

Después de modificar el modelo de persistencia, se puede crear una nueva migración con:

```bash
dotnet ef migrations add NombreDeLaMigracion \
  --project src/SupportFlow.Infrastructure \
  --startup-project src/SupportFlow.API
```

Por ejemplo:

```bash
dotnet ef migrations add AddTicketComments \
  --project src/SupportFlow.Infrastructure \
  --startup-project src/SupportFlow.API
```

El nombre debe describir claramente el cambio realizado.

---

# 12. Aplicar migraciones

Para actualizar la base de datos:

```bash
dotnet ef database update \
  --project src/SupportFlow.Infrastructure \
  --startup-project src/SupportFlow.API
```

El comando utiliza la configuración de la aplicación para determinar la conexión a PostgreSQL.

---

# 13. Flujo recomendado para cambios de base de datos

Cuando se modifica una entidad o configuración de persistencia:

```text
Modificar modelo
      │
      ▼
Compilar
      │
      ▼
Crear migración
      │
      ▼
Revisar migración
      │
      ▼
Aplicar migración localmente
      │
      ▼
Probar aplicación
```

Las migraciones forman parte del código del proyecto y deben mantenerse bajo control de versiones.

---

# 14. Ejecutar la API

Desde la raíz del repositorio se puede ejecutar:

```bash
dotnet run --project src/SupportFlow.API
```

La URL exacta puede depender de la configuración del proyecto.

Durante el desarrollo actual la API se ejecuta en:

```text
http://localhost:5098
```

---

# 15. Swagger

SupportFlow utiliza Swagger para facilitar las pruebas de la API durante el desarrollo.

Una vez iniciada la API, Swagger permite:

* Consultar los endpoints.
* Revisar modelos.
* Probar solicitudes.
* Revisar respuestas HTTP.
* Trabajar con autenticación JWT cuando está configurada.

La disponibilidad exacta de Swagger puede depender del entorno configurado.

---

# 16. Autenticación durante las pruebas

Para probar endpoints protegidos primero es necesario autenticarse.

El flujo general es:

```text
POST /api/auth/login
        │
        ▼
    JWT token
        │
        ▼
Authorization: Bearer <token>
        │
        ▼
Endpoint protegido
```

El token debe mantenerse privado.

No debe compartirse en repositorios, capturas públicas, issues ni documentación.

---

# 17. Flujo funcional básico

Una prueba funcional básica del sistema puede seguir este orden:

```text
1. Crear usuario
       │
       ▼
2. Iniciar sesión
       │
       ▼
3. Obtener JWT
       │
       ▼
4. Crear ticket
       │
       ▼
5. Consultar tickets
       │
       ▼
6. Asignar técnico
       │
       ▼
7. Resolver ticket
       │
       ▼
8. Cerrar ticket
       │
       ▼
9. Reabrir ticket
```

No todos los pasos pueden ser realizados por el mismo rol.

Las operaciones están restringidas mediante autorización.

---

# 18. Roles durante el desarrollo

Los roles actualmente utilizados son:

| Rol            | Responsabilidad principal              |
| -------------- | -------------------------------------- |
| `Employee`     | Crear y consultar sus propios tickets  |
| `Technician`   | Consultar y resolver tickets asignados |
| `Adminitrador` | Administrar operaciones de tickets     |

La escritura `Adminitrador` es intencional y debe mantenerse mientras forme parte del código actual.

---

# 19. Pruebas manuales

Durante el desarrollo se pueden utilizar:

* Swagger.
* `.http` de Visual Studio/Rider.
* `curl`.
* Postman.
* Insomnia.

Las pruebas deben comprobar tanto:

* Casos exitosos.
* Casos no autorizados.
* Datos inválidos.
* Transiciones de estado inválidas.

Por ejemplo:

```text
Employee
   │
   └── GET /api/tickets
             │
             ▼
          403
```

Esto es tan importante como comprobar que un usuario autorizado obtiene `200 OK`.

---

# 20. Verificación después de cambios

Después de realizar cambios importantes se recomienda ejecutar:

```bash
dotnet build
```

y posteriormente probar los endpoints afectados.

Cuando exista una migración:

```bash
dotnet ef database update \
  --project src/SupportFlow.Infrastructure \
  --startup-project src/SupportFlow.API
```

Después se debe comprobar que la API inicia correctamente.

---

# 21. Git

El proyecto utiliza Git para controlar la evolución del código.

El flujo básico es:

```text
Modificar
   │
   ▼
Probar
   │
   ▼
Revisar
   │
   ▼
Commit
   │
   ▼
Push
```

Los commits deben representar cambios lógicos y entendibles.

---

# 22. Commits

Se recomienda utilizar mensajes descriptivos.

Ejemplos utilizados en el proyecto:

```text
feat: add JWT authentication and password hashing
feat: implement ticket lifecycle operations
feat: add ticket query endpoints
refactor: centralize exception handling
feat: add employee ticket access
feat: add technician assigned tickets
```

La intención es que el historial de Git permita entender qué funcionalidad o cambio fue incorporado en cada etapa.

---

# 23. No mezclar cambios no relacionados

Un commit debería representar una unidad lógica.

Por ejemplo, no es recomendable combinar en un mismo commit:

```text
Nuevo endpoint
+
Cambio de arquitectura
+
Corrección de documentación
+
Cambio de base de datos
```

si esos cambios no forman parte de la misma modificación.

Es preferible mantener cambios relacionados agrupados y cambios independientes separados.

---

# 24. Documentación

La documentación principal del proyecto se encuentra en:

```text
docs/
```

Documentos actuales:

```text
docs/
├── architecture.md
├── api.md
├── database.md
├── domain-decisions.md
├── security.md
└── development.md
```

El `README.md` actúa como punto de entrada general del proyecto.

---

# 25. Flujo recomendado de desarrollo

Para implementar una nueva funcionalidad:

```text
1. Entender el requisito
        │
        ▼
2. Revisar las reglas del dominio
        │
        ▼
3. Determinar el caso de uso
        │
        ▼
4. Implementar en Application
        │
        ▼
5. Implementar persistencia si es necesaria
        │
        ▼
6. Exponer mediante API
        │
        ▼
7. Compilar
        │
        ▼
8. Probar
        │
        ▼
9. Documentar
        │
        ▼
10. Commit
```

La implementación debe respetar la dirección de dependencias definida por Clean Architecture.

---

# 26. Regla para nuevas funcionalidades

Antes de agregar código debe determinarse dónde pertenece la responsabilidad.

### Si es una regla de negocio

Debe evaluarse para `Domain`.

### Si es un caso de uso

Debe evaluarse para `Application`.

### Si es acceso a datos o tecnología externa

Debe evaluarse para `Infrastructure`.

### Si es HTTP

Debe evaluarse para `API`.

Esto evita convertir los controladores en lugares donde se concentre toda la lógica del sistema.

---

# 27. Errores y excepciones

Las reglas de negocio pueden generar excepciones cuando una operación no es válida.

El middleware global de excepciones transforma esas excepciones en respuestas HTTP.

Por lo tanto, al agregar nuevas operaciones se debe evitar duplicar innecesariamente el mismo manejo de excepciones en cada controlador.

Antes de agregar un nuevo `try/catch`, debe evaluarse si la excepción ya puede ser manejada correctamente por:

```text
ExceptionHandlingMiddleware
```

---

# 28. Estado actual del proyecto

SupportFlow se encuentra actualmente en una etapa funcional inicial.

Ya están implementados:

* Autenticación.
* JWT.
* Hashing de contraseñas.
* Usuarios.
* Roles.
* Tickets.
* Prioridades.
* Estados.
* Asignación.
* Resolución.
* Cierre.
* Reapertura.
* Consultas de tickets.
* Tickets propios del empleado.
* Tickets asignados al técnico.
* Validaciones de autorización.
* Manejo global de excepciones.
* Persistencia con PostgreSQL.
* Migraciones.

La siguiente evolución del proyecto deberá mantener la separación de responsabilidades existente.

---

# 29. Principio general de desarrollo

SupportFlow debe evolucionar de manera incremental.

Cada nueva funcionalidad debe:

1. Tener un objetivo claro.
2. Respetar las reglas del dominio.
3. Mantener la separación de capas.
4. Evitar introducir complejidad innecesaria.
5. Ser probada antes de considerarse terminada.
6. Ser documentada cuando cambie el comportamiento del sistema.
7. Quedar registrada mediante un commit lógico.

El objetivo no es construir la mayor cantidad de código posible, sino mantener un sistema comprensible, mantenible y preparado para evolucionar.
