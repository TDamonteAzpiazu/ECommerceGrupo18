# Grupo 18 — Arquitectura de Microservicios

**Materia:** Construcción de Aplicaciones Informáticas — UBA  
**Integrantes:** Damonte Azpiazu Tobías(909981), Diana Valentino Luca(910167)  
**Tecnología:** .NET 9 · ASP.NET Core · SQLite + Dapper · Serilog · Swagger

---

## Descripción

Sistema de eCommerce implementado como arquitectura de microservicios. Cada módulo expone una REST API independiente con documentación Swagger, manejo estructurado de errores, logging con Serilog, health checks y propagación de Correlation ID.

---

## Microservicios

| Servicio | Puerto | Responsabilidad |
|---|---|---|
| Products.API | 7001 | Gestión de productos, stock y categorías |
| Users.API | 7142 | Registro, login y bloqueo de usuarios |
| Orders.API | 7163 | Creación y gestión de órdenes |
| Cart.API | 7150 | Carrito de compras por usuario |
| Notifications.API | 7032 | Envío y registro de notificaciones |

---

## Requisitos previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 o superior
- No requiere instalación de base de datos (SQLite se crea automáticamente)

---

## Cómo ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone https://github.com/ECommerceGrupo18/ECommerceGrupo18.git
cd ECommerceGrupo18
```

### 2. Crear la carpeta de base de datos

En la raíz del repositorio, crear manualmente la carpeta:

```
ECommerceGrupo18/
└── database/        ← crear esta carpeta
```

La base de datos `app.db` se genera automáticamente al correr `Products.API` por primera vez.

### 3. Configurar los puertos en appsettings.json

Cada proyecto tiene un `appsettings.json` con las URLs de los otros servicios. Verificar que los puertos coincidan con los del `launchSettings.json` de cada proyecto antes de correr.

### 4. Establecer múltiples proyectos de inicio

En Visual Studio:

1. Clic derecho sobre la solución → **Establecer proyectos de inicio**
2. Seleccionar **Múltiples proyectos de inicio**
3. Poner todos los proyectos en **Start**
4. Aceptar

### 5. Correr la solución

Presionar `Ctrl+F5` (sin debugger) o `F5` (con debugger).

Cada servicio imprime su URL de Swagger en la consola al iniciar:

```
Swagger Products: https://localhost:7001/swagger    ← Products.API
Swagger Users: https://localhost:7142/swagger    ← Users.API
Swagger Orders: https://localhost:7163/swagger    ← Orders.API
...
```

---

## Endpoints por servicio

### Products.API — `/api/products`
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/products` | Listar productos (filtros: `?categoria=` `?nombre=`) |
| GET | `/api/products/{id}` | Obtener producto por ID |
| POST | `/api/products` | Crear producto |
| PUT | `/api/products/{id}` | Actualizar producto |
| DELETE | `/api/products/{id}` | Eliminar producto |

### Users.API — `/api/users`
| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/users/register` | Registrar usuario |
| POST | `/api/users/login` | Autenticar usuario |

### Orders.API — `/api/orders`
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/orders` | Listar órdenes (filtro: `?usuarioId=`) |
| GET | `/api/orders/{id}` | Obtener orden por ID |
| POST | `/api/orders` | Crear orden |
| PUT | `/api/orders/{id}/status` | Actualizar estado de orden |

### Cart.API — `/api/cart`
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/cart/{userId}` | Obtener carrito del usuario |
| POST | `/api/cart/{userId}/items` | Agregar producto al carrito |
| PUT | `/api/cart/{userId}/items/{productId}` | Actualizar cantidad |
| DELETE | `/api/cart/{userId}/items/{productId}` | Quitar producto del carrito |
| DELETE | `/api/cart/{userId}` | Vaciar carrito |

### Notifications.API — `/api/notifications`
| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/notifications/send` | Enviar notificación |
| GET | `/api/notifications/{userId}` | Listar notificaciones del usuario |

---

## Health Checks

Cada servicio expone tres endpoints de salud:

| Endpoint | Descripción |
|---|---|
| `/health` | Estado general (BD + API) |
| `/health/ready` | Estado de la base de datos SQLite |
| `/health/live` | Estado de la API |
| `/health-ui` | Dashboard visual |

Ejemplo de respuesta:
```json
{
  "status": "Healthy",
  "entries": {
    "sqlite-db": { "status": "Healthy", "description": "SELECT 1 ejecutado OK" },
    "api-status": { "status": "Healthy", "description": "API operativa" }
  }
}
```

---

## Logging

Cada servicio genera logs en dos destinos:

- **Consola:** solo errores (`Error` o superior)
- **Archivo:** `logs/audit.log` con rotación diaria, registra todas las requests HTTP

Formato del archivo de log:
```
2026-06-11 18:53:22 | INF | Products.API | GET | /api/products | 200 | 12ms | {correlationId}
```

---

## Estructura del proyecto

```
ECommerceGrupo18/
├── ECommerceGrupo18.slnx
├── database/
│   └── app.db
├── Products.API/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Models/
│   ├── Repository/
│   ├── Services/
│   ├── Exceptions/
│   ├── ExceptionHandlers/
│   ├── HealthChecks/
│   ├── Middleware/
│   ├── logs/
│   └── Program.cs
├── Users.API/
├── Orders.API/
├── Cart.API/
└── Notifications.API/
```

---

## Diagrama de arquitectura

[*Link al documento con el diagrama de arquitectura*](https://docs.google.com/document/d/1fq3ANRE8Tl-OOBfAIuDK6OFwO_eURwBIjV8L33Un3II/edit?usp=sharing)

---

## Capturas de Swagger

[*Link al documento con las capturas*](https://docs.google.com/document/d/1hv2PvzoHAcpcook_TQ_BNe39jk3pijkkStR0oKTqTSI/edit?usp=sharing)