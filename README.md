# Maquinaria-Web-juan

Cliente web en **Blazor Web (.NET 10)** para la [API-Maquinaria-juan](https://github.com/JMGProgrammer) — CRUD de inventario de maquinaria conectado con `HttpClient`, nivel básico, tal como se vio en clase.

Proyecto de estudio: estructura mínima, sin diseño propio (Bootstrap del template), con comentarios breves en cada sección para entender el flujo completo.

---

## Arquitectura general

```
Navegador ──Blazor Server──▶ Maquinaria-Web-juan ──HttpClient──▶ API-Maquinaria-juan ──EF Core──▶ MariaDB
                             (puerto 5020)                      (puerto 5244)                     (puerto 3306)
```

El flujo de una petición de lectura:

```
Equipments.razor → EquipmentApiClient (GetFromJsonAsync) → EquipmentController → EquipmentService → EquipmentRepository → MySQL
```

### Estructura del proyecto Blazor

```
Maquinaria-Web-juan/
├── Program.cs                     → registro de servicios + AddHttpClient<EquipmentApiClient>
├── appsettings.json               → ApiBaseUrl (URL de la API)
├── Models/
│   └── Equipment.cs               → espejo del EquipmentReadDTO de la API
├── DTOs/
│   ├── EquipmentInputDTO.cs       → base con validaciones [Required]/[StringLength]
│   ├── EquipmentCreateDTO.cs      → body del POST
│   └── EquipmentUpdateDTO.cs      → body del PUT
├── Services/
│   └── EquipmentApiClient.cs      → cliente tipado: un método por endpoint
└── Components/Pages/
    ├── Equipments.razor           → /equipments            — lista + eliminar
    ├── EquipmentDetail.razor      → /equipments/{id}       — detalle (GET por id)
    ├── EquipmentCreate.razor      → /equipments/nuevo      — alta (EditForm + POST)
    └── EquipmentEdit.razor        → /equipments/{id}/edit  — edición (carga + PUT)
```

---

## Requisitos

| Requisito        | Versión / detalle                              |
|------------------|------------------------------------------------|
| .NET SDK         | 10.0                                           |
| MariaDB / MySQL  | corriendo en `localhost:3306`                  |
| IDE              | Rider, Visual Studio 2022+ o `dotnet` CLI      |

---

## Inicialización paso a paso

### 1. Levantar MariaDB

En esta configuración el servicio **no arranca solo** al encender la máquina:

```bash
sudo service mariadb start
sudo service mariadb status   # debe decir "running"
```

La base de datos que usa la API es `api_maquinaria` (creada por las migraciones de la API).

### 2. Configurar y correr la API

El código de la API vive en su propio repo: `API-Maquinaria-juan` (carpeta hermana).

```bash
cd API-Maquinaria-juan/API-Maquinaria-juan
```

**a) Connection string** — está en *user-secrets* (no en appsettings, para no subir claves):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=api_maquinaria;User=miUsuario;Password=miClave;"
```

*(reemplazá `miUsuario`/`miClave` por tus credenciales de MariaDB)*

**b) Migraciones** — ya están aplicadas en el repo; si empezás una base desde cero:

```bash
dotnet ef database update
```

**c) Correr la API** — con el perfil `http` (no `https`):

```bash
dotnet run --launch-profile http
```

La API queda en `http://localhost:5244`. Verificación rápida:

```bash
curl http://localhost:5244/api/equipment
```

> ¿Por qué perfil `http`? Con el perfil `https` la API redirige todo a HTTPS con un
> certificado de desarrollo que el `HttpClient` del servidor no confía → fallas de
> conexión difíciles de diagnosticar.

### 3. Correr la web

```bash
cd Maquinaria-Web-juan
dotnet run
```

Abrir `http://localhost:5020/equipments` (el puerto exacto lo muestra la consola al arrancar).

La URL de la API se configura en `appsettings.json`:

```json
"ApiBaseUrl": "http://localhost:5244/api/"
```

Si la API corre en otro host/puerto, cambiar solo ese valor — no hace falta tocar código.

---

## Uso de la aplicación

| Página             | Ruta                      | Qué hace                                                            |
|--------------------|---------------------------|---------------------------------------------------------------------|
| Equipos            | `/equipments`             | Lista todos los equipos + botón Eliminar por fila                   |
| Nuevo equipo       | `/equipments/nuevo`       | Form de alta (POST); valida los campos antes de llamar a la API      |
| Detalle            | `/equipments/{id}`        | Muestra un equipo (GET por id); "no existe" si la API responde 404   |
| Editar             | `/equipments/{id}/edit`   | Carga los datos y los reenvía (PUT)                                  |

Endpoints de la API que consume (`EquipmentController`):

| Verbo   | Ruta                  | Respuesta                                  |
|---------|-----------------------|--------------------------------------------|
| GET     | `/api/equipment`      | 200 + lista de `EquipmentReadDTO`          |
| GET     | `/api/equipment/{id}` | 200 + equipo · 404 si no existe            |
| POST    | `/api/equipment`      | 201 + equipo creado · 400 si regla inválida|
| PUT     | `/api/equipment/{id}` | 200 + equipo actualizado · 404 · 400       |
| DELETE  | `/api/equipment/{id}` | 200 · 404                                  |

---

## Manejo de errores (patrón usado)

```
API caída       → HttpRequestException → el servicio la deja subir → la página la atrapa y muestra alert
Regla inválida  → la API responde 400  → el servicio lanza Exception con el mensaje → la página lo muestra
Id inexistente  → la API responde 404  → el servicio devuelve null      → la página muestra "no encontrado"
```

- Todo `await` con su `try/catch` correspondiente en las páginas (una excepción no manejada corta el circuito de Blazor Server).
- `finally { _cargando = false; }` en cada carga: el estado de "cargando" se limpia pase lo que pase.

---

## Problemas frecuentes

| Síntoma                                                    | Causa                          | Solución                              |
|------------------------------------------------------------|--------------------------------|---------------------------------------|
| "No se pudo conectar con la API (¿está corriendo...?)"     | API apagada                    | Levantar la API (paso 2c)             |
| "Connection refused" en la API al listar                   | MariaDB apagado                | `sudo service mariadb start`          |
| `dotnet ef` no reconocido                                  | Falta la tool                  | `dotnet tool install --global dotnet-ef` |
| Error 401/1045 al conectar MariaDB                         | Credenciales del secret mal    | Re-setear el user-secret              |
| La web abre pero la lista queda en "Cargando..."           | API en perfil https (redirect) | Correr API con perfil `http`          |

---

## Decisiones de diseño (breve)

- **Cliente tipado** (`AddHttpClient<T>`) en vez de `HttpClient` suelto: la BaseAddress se configura una vez y las páginas solo conocen métodos con nombres claros.
- **DTOs espejo**: mismas propiedades y validaciones que la API → el `EditForm` valida en el navegador antes de gastar un pedido HTTP.
- **404 → null** en `GetByIdAsync`: la página decide qué mostrar; el servicio no conoce UI.
