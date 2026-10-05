# Domain Decisions

Este documento registra las principales decisiones de negocio y reglas de dominio adoptadas durante el desarrollo de SupportFlow.

El objetivo es mantener documentado **por qué** el sistema se comporta de determinada manera y evitar que futuras modificaciones contradigan las reglas existentes.

---

## 1. Ticket

Un ticket representa una solicitud de soporte creada por un usuario del sistema.

Cada ticket contiene, entre otros datos:

* Identificador único.
* Número de ticket.
* Título.
* Descripción.
* Prioridad.
* Estado.
* Fecha de creación.
* Usuario creador.
* Técnico asignado.

La creación del ticket se realiza a través de la capa Application, mientras que las reglas que controlan su comportamiento pertenecen al dominio.

---

## 2. Estado inicial

Todo ticket nuevo comienza en:

```text
Pending
```

El estado inicial es asignado por el dominio y no depende del valor enviado por el cliente.

Esto garantiza que un usuario no pueda crear directamente un ticket como `Resolved`, `Closed` o `InProgress`.

---

## 3. Estados disponibles

Los estados definidos para los tickets son:

```text
Pending
InProgress
Resolved
Closed
Reopened
```

### Significado

| Estado       | Descripción                                                      |
| ------------ | ---------------------------------------------------------------- |
| `Pending`    | El ticket fue creado pero todavía no está siendo atendido.       |
| `InProgress` | El ticket fue asignado a un técnico y está siendo atendido.      |
| `Resolved`   | El técnico terminó la atención y considera resuelto el problema. |
| `Closed`     | El ticket fue cerrado administrativamente.                       |
| `Reopened`   | Un ticket cerrado volvió a estar disponible para atención.       |

---

## 4. Transiciones de estado

Las transiciones no son arbitrarias.

El flujo principal es:

```text
Pending
   │
   │ Assign
   ▼
InProgress
   │
   │ Resolve
   ▼
Resolved
   │
   │ Close
   ▼
Closed
   │
   │ Reopen
   ▼
Reopened
   │
   │ Assign
   ▼
InProgress
```

### Reglas

#### Pending → InProgress

Un ticket puede pasar a `InProgress` cuando un administrador lo asigna a un técnico válido.

La asignación requiere que el usuario asignado tenga el rol `Technician`.

#### InProgress → Resolved

Un ticket puede ser resuelto únicamente cuando se encuentra en `InProgress`.

Además, solamente el técnico actualmente asignado puede realizar esta operación.

#### Resolved → Closed

Un ticket puede cerrarse únicamente cuando se encuentra en `Resolved`.

Actualmente esta operación corresponde al rol `Adminitrador`.

#### Closed → Reopened

Un ticket puede reabrirse únicamente cuando se encuentra en `Closed`.

Actualmente esta operación corresponde al rol `Adminitrador`.

#### Reopened → InProgress

Un ticket reabierto puede volver a `InProgress` cuando es asignado nuevamente a un técnico.

---

## 5. Reglas de asignación

La asignación de tickets tiene las siguientes reglas:

1. El usuario asignado debe existir.
2. El usuario asignado debe tener el rol `Technician`.
3. El ticket debe encontrarse en `Pending` o `Reopened`.
4. Al realizarse la asignación, el identificador del técnico queda almacenado en `AssignedToUserId`.
5. La asignación cambia el estado del ticket a `InProgress`.

Estas reglas se mantienen en el dominio para evitar que diferentes partes de la aplicación implementen comportamientos distintos.

---

## 6. Regla de resolución

Un técnico no puede resolver cualquier ticket.

Para resolver un ticket:

1. El ticket debe existir.
2. El ticket debe estar en estado `InProgress`.
3. El usuario autenticado debe ser el técnico asignado al ticket.

La comprobación de identidad se realiza utilizando el usuario autenticado y no un identificador proporcionado libremente por el cliente.

Esto evita que un técnico pueda resolver tickets asignados a otro técnico.

---

## 7. Regla de cierre

Un ticket solamente puede cerrarse cuando se encuentra en:

```text
Resolved
```

La operación está restringida al rol `Adminitrador`.

Si el ticket se encuentra en otro estado, la operación no es válida.

---

## 8. Regla de reapertura

Un ticket solamente puede reabrirse cuando se encuentra en:

```text
Closed
```

La operación está restringida al rol `Adminitrador`.

Después de ser reabierto, el ticket queda en:

```text
Reopened
```

Desde ese estado puede volver a asignarse a un técnico.

---

## 9. Prioridades

Los tickets disponen de cuatro niveles de prioridad:

```text
Low
Medium
High
Critical
```

Todo ticket debe tener una prioridad.

La prioridad forma parte de la información del ticket desde su creación.

### FIFO

Como decisión inicial del dominio, cuando varios tickets tienen la misma prioridad se considera el orden de llegada para determinar cuál debería atenderse primero.

Esto corresponde a una regla conceptual de atención:

```text
Mayor prioridad
       +
Orden de llegada
       =
Orden de atención
```

La implementación de una cola automática de atención no forma parte de la versión actual.

---

## 10. Identidad del creador

El usuario que crea un ticket se determina a partir de la identidad autenticada.

El `CreatedByUserId` utilizado para crear el ticket se obtiene de la claim `NameIdentifier` del JWT.

El cliente no puede elegir arbitrariamente otro usuario como creador mediante el request.

Esta decisión separa:

```text
Identidad autenticada
        ↓
Usuario creador
        ↓
Ticket
```

de los datos que el cliente puede modificar.

---

## 11. Fecha de creación

La fecha de creación del ticket es generada por el sistema.

El cliente no controla el valor de la fecha de creación.

El dominio utiliza UTC para registrar este dato.

Esto permite mantener una referencia temporal consistente independientemente de la zona horaria del cliente.

---

## 12. Número de ticket

Además del identificador único `Guid`, los tickets disponen de un número de ticket para identificación funcional.

Ejemplo:

```text
Ticket #1
Ticket #2
Ticket #3
```

El identificador interno y el número visible cumplen funciones diferentes:

* `Id`: identificación técnica única.
* `TicketNumber`: identificación funcional para usuarios.

---

## 13. Roles relacionados con el dominio

Los roles actualmente definidos son:

```text
Employee
Technician
Adminitrador
```

La autorización HTTP se implementa en la capa API, mientras que determinadas reglas relacionadas con los usuarios son protegidas adicionalmente dentro del dominio.

Por ejemplo, `Ticket.AssignTo()` valida que el usuario recibido tenga el rol `Technician`.

Esto evita depender exclusivamente de `[Authorize]` para proteger una regla de negocio.

---

## 14. Separación entre autorización y reglas de dominio

SupportFlow diferencia dos conceptos:

### Autorización

Determina si un usuario puede acceder a una operación HTTP.

Ejemplo:

```text
[Authorize(Roles = "Technician")]
```

### Regla de dominio

Determina si la operación es válida para el estado actual del objeto.

Ejemplo:

```text
Un ticket InProgress puede ser resuelto.
Un ticket Pending no puede ser resuelto.
```

Ambas capas son necesarias.

La autorización protege el acceso al caso de uso, mientras que el dominio protege la consistencia de las entidades.

---

## 15. Errores de dominio

Cuando una operación viola una regla de negocio, el dominio puede impedir la operación mediante una excepción.

Ejemplos:

* Intentar resolver un ticket que no está `InProgress`.
* Intentar cerrar un ticket que no está `Resolved`.
* Intentar reabrir un ticket que no está `Closed`.
* Intentar asignar un usuario que no es `Technician`.

La capa superior transforma estas situaciones en respuestas HTTP apropiadas.

---

## 16. Decisiones de seguridad relacionadas con el dominio

Las siguientes decisiones fueron adoptadas para evitar manipulación de datos sensibles:

### Crear ticket

El creador se obtiene del JWT.

No se confía en un `CreatedByUserId` enviado por el cliente.

### Resolver ticket

El técnico que solicita la resolución debe coincidir con `AssignedToUserId`.

No basta con tener el rol `Technician`.

### Contraseñas

Las contraseñas no forman parte de la información expuesta mediante los DTOs de respuesta.

Las contraseñas se almacenan mediante hashes BCrypt.

---

## 17. Decisiones pendientes

Las siguientes funcionalidades no forman parte de las reglas actuales y deberán definirse antes de incorporarlas:

* Sistema de comentarios.
* Historial detallado de cambios.
* Auditoría.
* Notificaciones.
* SLA.
* Escalamiento automático.
* Asignación automática.
* Priorización automática.
* Reglas avanzadas para `Critical`.
* Reglas de permisos más granulares.
* Reglas para reasignación de tickets.
* Tiempo máximo de atención.
* Métricas y tiempos de resolución.

Estas funcionalidades no deben incorporarse simplemente modificando los estados actuales sin revisar primero las reglas de dominio.

---

## 18. Principio general

Las reglas que determinan si una operación es válida deben permanecer cerca del modelo de dominio siempre que representen una regla real del negocio.

La API puede controlar quién tiene acceso a una operación, pero el dominio debe seguir siendo capaz de proteger la consistencia de sus propias entidades.

La intención de esta separación es que SupportFlow pueda evolucionar sin convertir los Controllers o los repositorios en responsables de las reglas de negocio.
