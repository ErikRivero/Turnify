# Pruebas y revisión

## Evidencia disponible

El 09/11/2026 se compiló la copia de Documentos/Turnify mediante `dotnet build .\Turnify.API.slnx --no-restore --nologo`: 0 errores y 0 advertencias, SDK 10.0.401. Esto no constituye una prueba integral de ejecución ni una restauración limpia en otro equipo.

El 13/11/2026 se retiró de acerca.html el enlace redundante a css/styles.css (inexistente), conservando css/style.css. Se verificaron las referencias locales de los HTML. Los archivos de esta copia usan los nombres style.css y barbero.js. Se inicializó Git local y se comprobó que las exclusiones cubren bin, obj, .vs, archivos .user y secretos. La revisión de valores sensibles en appsettings no produjo alertas; no sustituye una auditoría integral.

## Lista de pruebas manuales pendientes de registro

- [ ] Inicio público y navegación completos.
- [ ] Login correcto y rechazo de contraseña incorrecta.
- [ ] Endpoints sin token o con token vencido devuelven 401.
- [ ] ABMC de servicios, clientes y barberos persiste tras recargar.
- [ ] No se puede reservar con registros inactivos ni en una fecha pasada.
- [ ] Superposición del mismo barbero rechazada; horarios consecutivos permitidos.
- [ ] Reprogramar conserva precio y duración y rechaza conflictos.
- [ ] Cancelar conserva historial y libera el horario.
- [ ] Completar solo permite pendientes cuya finalización ya pasó.
- [ ] Dashboard coincide con el rango y suma solo precios de completados.
- [ ] Acerca de muestra los integrantes reales.
- [ ] Vista móvil y tablas utilizables.
- [ ] Ejecución desde una base nueva siguiendo el README.

## Concurrencia

En una base de prueba, enviar en paralelo dos POST para el mismo barbero y horario futuro libre. Debe confirmarse una sola reserva; la otra debe recibir conflicto. Verificar el resultado en la base. Dos clics sucesivos en Swagger no prueban simultaneidad.

## Publicación

- [ ] Revisar los archivos a publicar y no incluir datos personales reales ni secretos.
- [ ] Conservar migraciones y archivos de configuración sin credenciales.
- [ ] Excluir bin, obj, .vs, archivos .user, bases locales y ZIP.
- [ ] Recordar que .gitignore no elimina secretos que ya estén en el historial.
- [ ] Confirmar dominio y estructura con el docente.
- [ ] Crear repositorio público y verificar acceso desde otra cuenta.
- [ ] Preparar video si se desea el adicional.
