# Módulo de Gestión de Pedidos

Prueba de concepto de un módulo de Gestión de Pedidos construido con **.NET 8**, **arquitectura hexagonal (puertos y adaptadores)**, **PostgreSQL** y un frontend simple en **Bootstrap**.

Arquitectura documentada en [docs/architecture.md](docs/architecture.md) (diagramas C4 en Mermaid).

## Stack

- **API**: ASP.NET Core 8 Web API
- **Base de datos**: PostgreSQL 16 (tablas, índices y un procedimiento almacenado)
- **Frontend**: HTML + Bootstrap 5 + JavaScript plano (sin build tooling)
- **Tests**: xUnit + Moq, con cobertura ≥80% sobre `Pedidos.Domain` y `Pedidos.Application`
- **Orquestación**: Docker Compose
- **CI**: GitHub Actions (build + test + gate de cobertura)

## Estructura del repositorio

```
src/
  Pedidos.Domain/          Entidades y reglas de negocio (sin dependencias externas)
  Pedidos.Application/     Casos de uso y puertos (interfaces)
  Pedidos.Infrastructure/  Adaptadores: EF Core, repositorios, procedimiento almacenado
  Pedidos.Api/              API REST (ASP.NET Core)
tests/
  Pedidos.Domain.Tests/
  Pedidos.Application.Tests/
frontend/                   Frontend estático (Bootstrap + JS)
database/init/              Scripts SQL: esquema, índices, procedimiento almacenado, datos de ejemplo
docs/architecture.md         Diagramas de arquitectura
```

## Cómo correr el proyecto en local

### Opción A: todo con Docker Compose (recomendada)

Requiere Docker y Docker Compose.

```bash
git clone https://github.com/brahyanescdev/gestion-pedidos.git
cd gestion-pedidos
docker compose up --build
```

Servicios expuestos:

- Frontend: http://localhost:8081
- API: http://localhost:8080 (Swagger en http://localhost:8080/swagger)
- PostgreSQL: localhost:5433 (usuario/clave `pedidos`/`pedidos`, base `pedidos`)

Al iniciar, PostgreSQL ejecuta automáticamente los scripts de `database/init/` (esquema, índices, procedimiento almacenado y datos de ejemplo). Para reiniciar con la base de datos limpia:

```bash
docker compose down -v && docker compose up --build
```

### Opción B: API en el SDK de .NET + PostgreSQL en Docker

Útil para desarrollar/depurar la API directamente, sin reconstruir la imagen en cada cambio.

1. Levanta sólo la base de datos:
   ```bash
   docker compose up -d postgres
   ```
2. Corre la API con el SDK de .NET 8:
   ```bash
   dotnet run --project src/Pedidos.Api/Pedidos.Api.csproj
   ```
   La API toma la cadena de conexión de `src/Pedidos.Api/appsettings.json`, que ya apunta a `localhost:5433` (el puerto expuesto por `docker compose up -d postgres`).
3. Sirve el frontend estático (por ejemplo con `npx serve frontend` o abriendo `frontend/index.html` directamente) y ajusta `frontend/config.js` si la API no corre en `http://localhost:8080`.

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| GET/POST | `/api/customers` | Listar / crear clientes |
| GET/POST | `/api/products` | Listar / crear productos |
| GET/POST | `/api/orders` | Listar / crear pedidos |
| POST | `/api/orders/{id}/confirm` | Confirmar pedido |
| POST | `/api/orders/{id}/complete` | Completar pedido confirmado |
| POST | `/api/orders/{id}/cancel` | Cancelar pedido (libera el stock reservado) |
| GET | `/api/orders/reports/customer/{customerId}` | Resumen de pedidos por cliente (procedimiento almacenado) |

## Cómo correr los tests localmente

Requiere el SDK de .NET 8.

```bash
dotnet test Pedidos.sln --collect:"XPlat Code Coverage" --results-directory ./coverage
```

Para ver el porcentaje de cobertura consolidado sobre Domain + Application:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator \
  "-reports:./coverage/**/coverage.cobertura.xml" \
  "-targetdir:./coverage/report" \
  "-reporttypes:TextSummary" \
  "-classfilters:+Pedidos.Domain.*;+Pedidos.Application.*"
cat ./coverage/report/Summary.txt
```

El mismo flujo corre automáticamente en cada Pull Request vía GitHub Actions ([.github/workflows/ci.yml](.github/workflows/ci.yml)), con un gate que falla el build si la cobertura de Domain + Application baja del 80%.

## Flujo de trabajo

Todo cambio se integra a `main` mediante Pull Request; no se realizan pushes directos a `main`.
