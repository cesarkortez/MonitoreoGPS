cat > README.md << 'EOF'
# MonitoreoGPS

## Descripción
Sistema de monitoreo de flotas basado en microservicios: Geolocation, Routing y Audit.

## Ejecución local
Clona el repositorio y levanta los servicios con Docker Compose:

## Puntos de Acceso 
Geolocation: /api/geolocation

Routing: /api/routing

Audit: /api/audit/coordinate


```bash
git clone https://github.com/cesarkortez/MonitoreoGPS.git
cd MonitoreoGPS
docker-compose up --build
