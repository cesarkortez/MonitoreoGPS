# Documentación Técnica

## 1. Consistencia Eventual entre Redis y PostgreSQL

- **TTL en Redis**: Cada coordenada y ruta cacheada tiene TTL de 5 min.  
- **Transacción distribuida**: Al eliminar un vehículo, usamos `TransactionScope` para borrar en PostgreSQL y luego en Redis. Si Redis falla, lanzamos excepción para rollback en PostgreSQL.

## 2. Patrón Specification

La validación de coordenadas se refactorizó desde un método espagueti a `CoordinateSpecification` implementando `ISpecification<VehicleCoordinate>`.

## 3. Circuit Breaker

Usamos Polly con política `CircuitBreakerAsync(2, 30s)`. Tras 2 fallos de Redis consecutivos en RoutingService, el breaker abre durante 30 s y devuelve **503 Service Unavailable**.

## 4. Mock de GPS

- Se simulan 5 vehículos enviando coordenadas cada 2 s.  
- Hay un 15 % de “fallo” aleatorio (coordenada omitida).  
- El breaker desactiva Ruteo tras 2 fallos consecutivos (en tu código ajustaste a 2).

## 5. Diagramas

- **docs/architecture.png**: flujo global y recuperación de fallos.  
- **docs/sequence.puml**: secuencia de envío → geolocalización → ruteo → auditoría.
