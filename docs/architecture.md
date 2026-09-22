# Arquitectura — Módulo de Gestión de Pedidos

## Diagrama de contexto (C4 — Nivel 1)

```mermaid
C4Context
    Person(usuario, "Usuario de negocio", "Crea clientes, productos y pedidos")
    System(pedidos, "Módulo de Gestión de Pedidos", "API + Frontend + Base de datos")

    Rel(usuario, pedidos, "Usa vía navegador")
```

## Diagrama de contenedores (C4 — Nivel 2)

```mermaid
C4Container
    Person(usuario, "Usuario de negocio")

    System_Boundary(pedidos, "Módulo de Gestión de Pedidos") {
        Container(frontend, "Frontend", "HTML + Bootstrap + JavaScript", "Interfaz para gestionar clientes, productos y pedidos")
        Container(api, "API de Pedidos", "ASP.NET Core 8", "Expone endpoints REST y orquesta los casos de uso")
        ContainerDb(db, "Base de datos", "PostgreSQL 16", "Almacena clientes, productos, pedidos y expone el procedimiento de reporte")
    }

    Rel(usuario, frontend, "Interactúa vía HTTP", "HTTPS")
    Rel(frontend, api, "Consume", "JSON/HTTPS")
    Rel(api, db, "Lee/Escribe", "SQL / Npgsql")
```

## Arquitectura hexagonal (puertos y adaptadores)

```mermaid
graph TB
    subgraph Adaptadores_entrantes["Adaptadores entrantes (driving)"]
        API[Pedidos.Api<br/>Controllers REST]
    end

    subgraph Nucleo["Núcleo de la aplicación"]
        APP[Pedidos.Application<br/>Casos de uso + Puertos]
        DOM[Pedidos.Domain<br/>Entidades + Reglas de negocio]
        APP --> DOM
    end

    subgraph Adaptadores_salientes["Adaptadores salientes (driven)"]
        EF[EF Core Repositories<br/>Customer / Product / Order]
        SP[Npgsql directo<br/>Stored Procedure - Reporte]
    end

    DB[(PostgreSQL)]

    API -->|invoca| APP
    APP -.->|puerto: ICustomerRepository<br/>IProductRepository<br/>IOrderRepository<br/>IUnitOfWork| EF
    APP -.->|puerto: IOrderReportRepository| SP
    EF --> DB
    SP --> DB
```

**Regla de dependencia**: las flechas de compilación siempre apuntan hacia adentro. `Pedidos.Domain` no conoce a nadie. `Pedidos.Application` sólo conoce a `Pedidos.Domain` y define los puertos (interfaces) que necesita, sin saber quién los implementa. `Pedidos.Infrastructure` implementa esos puertos usando EF Core y Npgsql. `Pedidos.Api` es el único proyecto que conoce ambos lados y realiza el cableado de inyección de dependencias en `Program.cs`.

**Entidades desacopladas del ORM**: las clases en `Pedidos.Domain` son POCOs puros, sin atributos ni referencias a Entity Framework. El mapeo objeto-relacional vive exclusivamente en `Pedidos.Infrastructure/Persistence/Configurations` mediante Fluent API.

## Caso de uso principal: crear pedido

1. `OrdersController` recibe la solicitud y la delega a `IOrderService`.
2. `OrderService` (Application) valida que el cliente exista, que los productos existan y tengan stock suficiente.
3. Las reglas de negocio (reservar stock, calcular subtotales, invariantes de estado) viven en las entidades `Order` y `Product` (Domain).
4. `OrderService` pide a los repositorios (puertos) persistir los cambios; `Pedidos.Infrastructure` los traduce a SQL vía EF Core dentro de una única unidad de trabajo (`IUnitOfWork`).

## Reporte con procedimiento almacenado

El resumen de pedidos por cliente (`GET /api/orders/reports/customer/{id}`) no se resuelve con EF Core: `Pedidos.Application` define el puerto `IOrderReportRepository`, y `Pedidos.Infrastructure.Reporting.OrderReportRepository` lo implementa invocando directamente, vía Npgsql, la función almacenada `get_customer_order_summary` (`database/init/02_stored_procedure.sql`). Esto demuestra el uso real de un procedimiento almacenado dentro de la arquitectura hexagonal, sin acoplar el núcleo de la aplicación a PostgreSQL.

## Modelo de datos

| Tabla | Descripción | Índices |
|---|---|---|
| `customers` | Clientes | único en `email` |
| `products` | Catálogo de productos | PK |
| `orders` | Pedidos | `customer_id`, `order_date` |
| `order_items` | Detalle de cada pedido | `order_id`, `product_id` |

Ver definición completa en `database/init/01_schema.sql`.
