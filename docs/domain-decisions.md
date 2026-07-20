#Domain Decisions - Sprint 1
### Ticket Status 
Todo ticket nace en estado `Pending`.

Estados Definidos: 
-Pending 
-InProgress
-Resolved
-Closed
-Reopened

##Priority
Las prioridades disponibles son: 

-Low
-Medium
-High
-Critical

Reglas:

-Si dos tickets tienen la misma prioridad, se atienden por orden de llegada (FIFO).
-Todo ticket debe tener una prioridad.
-En el futuro, la prioridad podria calcularse automaticamente mediante reglas de negocio

## Metadata

Los siguientes campos son generados por el sistema:

-CreateAt
-CreateBy
-Satus inicial