Slotsi
Slotsi es una aplicación móvil para la gestión y agenda de citas en barberías y salones de belleza. 
Conecta a los clientes con los negocios de su zona y le da a los dueños herramientas para administrar sus servicios, trabajadores, citas y reputación.

Proyecto académico de *Integrador VI* — Ingeniería en Sistemas de la Información, UNAN-CUR-Matagalpa.

Características

1-Para clientes

Registro e inicio de sesión

Búsqueda de negocios y consulta de sus servicios

Agendado de citas seleccionando servicio, trabajador, día y hora

Calificación y reseñas de los negocios

Ranking de Mejores Calificados con filtro por categoría (Salones, Barbería)

Recuperación de contraseña por código enviado al correo

2-Para dueños de negocio

Registro del negocio con foto de perfil y documentos de verificación

Gestión de servicios (nombre, duración, precio, categoría)

Gestión de trabajadores y sus horarios de trabajo

Agenda de citas y resumen del día

Reporte de cancelaciones

Centro de reseñas

Suscripción de uso de la aplicación (pago mensual)


Tecnologías
Capa	Tecnología
App móvil	.NET MAUI (C#, patrón MVVM)
API	ASP.NET Core Web API con autenticación JWT
Base de datos	MongoDB (`MongoDB.Driver`)
Almacenamiento de archivos	Supabase Storage
Envío de correos	EmailJS
Control de versiones	Git / GitHub

Estructura del repositorio
```
Slotsi/
├── Slotsi.Api/           # API REST
│   ├── Controllers/      # Auth, Calificacion, Citas, Negocios,
│   │                     # Servicios, Suscripcion, Trabajadores, Usuarios
│   ├── Entities/
│   ├── Mapping/
│   ├── Repositories/
│   └── Infrastructure/
├── Slotsi.Shared/        # DTOs compartidos entre la API y la app
│   └── Dtos/
└── slotsi-citas/         # Aplicación .NET MAUI
    ├── Services/         # Servicios y clientes de la API
    ├── ViewModel/        # ViewModels (MVVM)
    ├── Views/            # Páginas XAML
    ├── App.xaml
    ├── AppShell.xaml
    └── MauiProgram.cs
```
---
Base de datos
La base de datos se llama `SlotsiCitasDB` y contiene las siguientes colecciones:
Colección	Descripción
`Usuarios`	Clientes y dueños de negocio (`TipoUsuario`: `true` = dueño, `false` = cliente)
`Negocios`	Datos de cada negocio
`Trabajadores`	Trabajadores asociados a un negocio (`negocio_id`)
`servicios`	Servicios que ofrece cada negocio
`Citas`	Citas agendadas
`Calificaciones`	Reseñas entre clientes y negocios
`PagoSub`	Estado de la suscripción de cada dueño
`HistorialPagos`	Historial de pagos de suscripción
`RecuperarContraseña`	Códigos temporales para recuperar la contraseña
---
Requisitos previos
Visual Studio 2022 (o superior) con las cargas de trabajo Desarrollo de .NET Multiplataforma (MAUI) y ASP.NET y desarrollo web
.NET SDK compatible con el proyecto
MongoDB: local (por ejemplo con Docker) o en MongoDB Atlas
Emulador Android o dispositivo físico para probar la app
Git

Instalación y ejecución
1. Clonar el repositorio
```bash
git clone <URL-DEL-REPOSITORIO>
cd Slotsi
```
2. Configurar la API
En `Slotsi.Api`, configura los valores sensibles en User Secrets. No se suben las claves reales al repositorio.
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "Base de datos"
  },
  "Jwt": {
    "Key": "<clave-secreta-larga>",
    "Issuer": "<emisor>",
    "Audience": "<audiencia>"
  }
}
```
La API valida la configuración JWT al arrancar. Si falta `Jwt:Key`, se lanzará un error de validación de opciones.
Para ejecutarla:
```bash
cd Slotsi.Api
dotnet run
```
3. Configurar la app MAUI
URL de la API — archivo

Services/Api/ApiConfiguracion.cs:
public const int Puerto = 5286;               // puerto donde corre Slotsi.Api
public const string IpLocalPc = "<IP-DE-TU-PC>"; // IP local de tu PC (ipconfig en la terminal de windows para ver la ip)

En un dispositivo físico la app usa IpLocalPc; PC y celular deben estar en la misma red Wi-Fi.
En emulador/simulador usa localhost.
Si la API corre con HTTP, permite el puerto en el firewall de Windows.

5. Ejecutar la app
En Visual Studio, selecciona el proyecto MAUI como proyecto de inicio.
Elige el emulador de Android o tu dispositivo.
Presiona F5.Para ejecutar API y app a la vez, configura **varios proyectos de inicio** en las propiedades de la solución.
Arquitectura
```
App MAUI (Views + ViewModels)
        │  HTTP + JWT
        ▼
   Slotsi.Api  (Controllers → Repositories)
        │
        ▼
    MongoDB (SlotsiCitasDB)
```
Los DTOs de `Slotsi.Shared` son el contrato común entre la app y la API.

Equipo
Integrante	y Roles
Mell Gabriel Traña Balmaceda -->	Gerente de proyecto, DBA, calidad, DevOps
Christian Josué Escorcia Blandón -->	Líder técnico, administración del repositorio
Jorge Samir Palacios Vilchez -->	Diseño UX/UI, Scrum Master

Directora del proyecto y Product Owner: Esther Vilchez
Docente: Maricela

Estado del proyecto
En desarrollo. Solucionando detalles finales para presentacion el dia martes 03 de noviembre
Licencia
Proyecto académico. Todos los derechos reservados por sus autores.
