# Paradox - Service C# (Core Backend)

Este es el servicio principal del proyecto **Paradox**, construido con **ASP.NET Core 8**. Se encarga de la lógica de negocio, gestión de proyectos, integración con IA y persistencia de datos.

## 🚀 Tecnologías

*   **Framework:** .NET 8 (ASP.NET Core Web API)
*   **Base de Datos:** PostgreSQL (con Entity Framework Core)
*   **IA:** OpenAI API (GPT-4 Turbo)
*   **Integración GitHub:** Octokit
*   **Documentación:** Swagger / OpenAPI
*   **Autenticación:** JWT (JSON Web Tokens)

## ✨ Funcionalidades Principales

1.  **Gestión de Tableros Ágiles**:
    *   CRUD de Proyectos, Issues y Usuarios.
    *   Plantillas de proyectos predefinidas.
2.  **Asistente de IA (Paradox AI)**:
    *   Chat contextual sobre el estado del proyecto.
    *   **Herramientas (Function Calling):** La IA puede crear issues, mover tarjetas y comentar en GitHub por ti.
3.  **Sincronización con GitHub**:
    *   Recibe eventos (Webhooks) desde el `service-java` para mantener el estado sincronizado.
    *   Permite realizar acciones en GitHub (ej. comentar issues) mediante `GithubService`.

## 🛠️ Configuración

El servicio utiliza variables de entorno para su configuración. Asegúrate de tener un archivo `.env` en la raíz del proyecto (o configurarlas en Docker).

### Variables Clave (`.env`)

```env
# Base de Datos
DB_HOST=localhost
DB_PORT=5432
DB_NAME=paradox
DB_USER=adminparadox
DB_PASSWORD=tu_password

# OpenAI
OPENAI_API_KEY=sk-...
OPENAI_MODEL=gpt-4-turbo-preview

# GitHub (Para acciones de la IA)
GITHUB_TOKEN=ghp_...

# JWT
JWT_SECRET=tu_secreto_super_seguro
JWT_ISSUER=Paradox
JWT_AUDIENCE=ParadoxAPI
```

## 🏃‍♂️ Ejecución

### Opción 1: Docker (Recomendado)

Este servicio está orquestado mediante `docker-compose` en la raíz del proyecto.

```bash
docker compose up --build -d service-csharp
```

### Opción 2: Local (.NET CLI)

1.  Asegúrate de tener la base de datos PostgreSQL corriendo.
2.  Restaura las dependencias y corre el proyecto:

```bash
cd service-csharp
dotnet restore
dotnet run
```

El servicio estará disponible en `http://localhost:5233` (o el puerto configurado en `launchSettings.json`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:

*   **URL:** `http://localhost:4001/swagger` (Si usas Docker y el puerto 4001)
*   **Local:** `http://localhost:5233/swagger`

## 📂 Estructura del Proyecto

*   `Controllers/`: Endpoints de la API (Sync, Auth, Projects, etc.).
*   `Services/`: Lógica de negocio (`OpenAiService`, `GithubService`, `BoardTools`).
*   `Models/`: Entidades de la base de datos.
*   `Data/`: Contexto de Entity Framework (`ParadoxContext`).

## 📚 Documentación API (Swagger)

Una vez iniciado el servicio, puedes explorar y probar los endpoints en:
