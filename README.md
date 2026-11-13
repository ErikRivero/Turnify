# Turnify

Trabajo Práctico Integrador de Programación II: gestión interna de una barbería.

## Integrantes

- Rivero Erik
- Mercado Lautaro

## Funcionalidades

- Inicio público y login con JWT.
- Alta, consulta, modificación y baja lógica de clientes, barberos y servicios.
- Reserva, reprogramación, cancelación y finalización de turnos.
- Control de superposición de horarios para un mismo barbero.
- Dashboard por fechas: turnos por estado e importe de servicios completados.
- Menú Soporte, Transacción, Dashboard y Acerca de.

El importe de los completados no representa ganancia ni pagos confirmados: no se registran gastos ni cobros. Una atención completada tampoco equivale necesariamente a un corte, ya que puede corresponder a otro servicio.

## Tecnologías

C#, ASP.NET Core y Entity Framework Core 8, SQL Server LocalDB, HTML, CSS, JavaScript con fetch, Bootstrap 5.3.8 y Swagger (Swashbuckle 6.6.2).

```text
Turnify/
├── Turnify.API.slnx
├── Turnify.API/       # Controllers, DTOs, JWT y configuración
├── Turnify.Core/      # Modelo, contexto, migraciones y reglas de negocio
├── Turnify.Web/       # Frontend estático
├── docs/
│   ├── modelo-er.md
│   └── pruebas.md
├── README.md
└── .gitignore
```

API referencia Core; Web consume API por HTTP. La solución contiene dos proyectos .NET y el frontend se sirve por separado. La clase de Servicios se llama actualmente `Service`, ubicada en `Turnify.Core/Services/Service.cs`.

## Requisitos

- Windows y SQL Server Express LocalDB, instancia `(localdb)\MSSQLLocalDB`.
- Visual Studio 2026 con desarrollo ASP.NET y soporte .NET 8 / soluciones SLNX.
- Runtime ASP.NET Core 8 y SDK compatible con SLNX. Se verificó compilación con SDK 10.0.401, manteniendo destino net8.0.
- Visual Studio Code con Live Server.
- Internet para restaurar NuGet y cargar Bootstrap desde CDN.
- SQL Server Management Studio, opcional para inspeccionar tablas.

## Instalación y ejecución

### 1. Abrir la solución

Abrir `Turnify.API.slnx` en Visual Studio y establecer `Turnify.API` como proyecto de inicio. Restaurar NuGet y compilar. Desde una terminal en la raíz:

```powershell
dotnet restore .\Turnify.API.slnx
dotnet build .\Turnify.API.slnx --no-restore
```

### 2. Configurar secretos locales

Clic derecho sobre el proyecto Turnify.API → Administrar secretos de usuario. Configurar el siguiente esquema, completando los campos vacíos antes de ejecutar:

```json
{
  "ConnectionStrings": {
    "TurnifyDB": "Server=(localdb)\\MSSQLLocalDB;Database=TurnifyDB;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": ""
  },
  "UsuarioInicial": {
    "NombreUsuario": "admin",
    "NombreCompleto": "Administrador",
    "Password": "",
    "RestablecerPassword": false
  }
}
```

La contraseña inicial debe tener al menos 12 caracteres. No hay una contraseña universal del proyecto. Jwt:Key es una clave de firma diferente de la contraseña: al menos 32 bytes aleatorios en Base64.

Generar esa clave en PowerShell y copiar el resultado a Jwt:Key:

```powershell
$turnifyBytes = New-Object byte[] 32
$turnifyGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$turnifyGenerator.GetBytes($turnifyBytes)
[Convert]::ToBase64String($turnifyBytes)
$turnifyGenerator.Dispose()
```

Los secretos son locales a cada computadora/usuario y no están cifrados en disco. No incluirlos en GitHub. Conservar las otras secciones cuando se edite este archivo.

### 3. Crear la base

En la Consola del Administrador de paquetes NuGet de Visual Studio:

```powershell
Update-Database -Project Turnify.Core -StartupProject Turnify.API
```

Se aplica la migración Inicial existente. No generar otra migración Inicial al clonar. Este comando escribe en SQL Server y crea las tablas cuando corresponde.

### 4. Ejecutar la API

Seleccionar el perfil **https** y ejecutar desde Visual Studio. Alternativa desde la raíz:

```powershell
dotnet run --project .\Turnify.API\Turnify.API.csproj --launch-profile https
```

- API: https://localhost:7226
- Swagger: https://localhost:7226/swagger/index.html

El perfil activa Development, necesario para Swagger y el inicializador. Si falta confianza en el certificado HTTPS local, ejecutar `dotnet dev-certs https --trust` y aceptar el aviso del sistema.

En el primer arranque se crea el usuario configurado si no existe. Después de comprobar el login, retirar la sección UsuarioInicial de los secretos, conservando Jwt y la conexión.

Para restablecer una contraseña olvidada en desarrollo, configurar el mismo usuario, la nueva contraseña y RestablecerPassword=true. Reiniciar, comprobar el acceso y retirar inmediatamente esa sección. No se requiere borrar el usuario ni la base.

### 5. Ejecutar la web

Abrir **Turnify.Web como carpeta en VS Code**, hacer clic derecho en index.html → Open with Live Server.

Abrir http://127.0.0.1:5500/index.html y mantener la API ejecutándose.

No abrir el HTML con doble clic como archivo local (file://). El CORS acepta exactamente http://127.0.0.1:5500; otro puerto o el host localhost requiere adaptar la política. Si Live Server toma otro puerto, detener su instancia anterior.

Si cambia el puerto de la API, revisar API_URL en js/api.js, js/login.js y js/dashboard.js.

### 6. Probar

Iniciar sesión con las credenciales propias. Crear al menos un cliente, un barbero y un servicio activos; luego reservar un turno futuro.

En Swagger, ejecutar POST /api/auth/login y pegar solo el token en Authorize, sin escribir Bearer.

## Reglas y límites

- La API calcula finalización y precio pactado usando el servicio elegido.
- Los turnos no cancelados del mismo barbero no se superponen; se permite empezar exactamente al finalizar otro.
- Reprogramar conserva precio y duración y requiere estado pendiente.
- Completar requiere que haya pasado la hora de finalización y que el turno esté pendiente; no es automático.
- Las bajas lógicas conservan historial y no cancelan reservas existentes.
- Se usan transacciones Serializable para las operaciones de turnos. La concurrencia debe probarse con solicitudes paralelas.
- El dashboard filtra por fecha de inicio e incluye ambos extremos del rango de días.
- Navegador y servidor trabajan con hora local de la barbería; el vencimiento del JWT usa UTC.
- JWT vence en 30 minutos. Cerrar sesión elimina el token de la pestaña, pero no revoca otras copias. No hay refresh tokens ni roles diferenciados.
- La validación de la API protege los datos; la redirección del frontend no reemplaza esa validación.

## API principal

| Método | Ruta | Función |
| --- | --- | --- |
| POST | /api/auth/login | Login público |
| GET, POST | /api/clientes, /api/barberos, /api/servicios | Listado y alta |
| GET, PUT, DELETE | /api/clientes/{id}, /api/barberos/{id}, /api/servicios/{id} | Detalle, modificación y baja lógica |
| GET, POST | /api/turnos | Listado y reserva |
| GET | /api/turnos/{id} | Detalle |
| PUT | /api/turnos/{id}/reprogramar | Cambiar horario |
| PATCH | /api/turnos/{id}/cancelar | Cancelar |
| PATCH | /api/turnos/{id}/completar | Completar |
| GET | /api/dashboard?desde=AAAA-MM-DD&hasta=AAAA-MM-DD | Resumen; sin fechas consulta hoy |

Excepto el login, los endpoints requieren Authorization: Bearer seguido del token.

## Documentación

- [Modelo E-R](docs/modelo-er.md).
- [Pruebas y revisión de entrega](docs/pruebas.md).

La publicación en GitHub y la grabación del video adicional se realizan por separado. Confirmar con el docente el dominio y la organización del frontend estático para la entrega.
