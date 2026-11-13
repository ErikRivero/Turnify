import {
    solicitarApi,
    obtenerSesion,
    cerrarSesion
} from "./api.js";

const tabla = document.getElementById("tablaTurnos");
const mensaje = document.getElementById("mensaje");
const botonActualizar = document.getElementById("btnActualizar");
const botonSalir = document.getElementById("btnSalir");
const nombreUsuario = document.getElementById("nombreUsuario");
const formulario = document.getElementById("turnoForm");
const camposReserva = document.getElementById("camposReserva");
const campoCliente = document.getElementById("clienteId");
const campoBarbero = document.getElementById("barberoId");
const campoServicio = document.getElementById("servicioId");
const campoFecha = document.getElementById("fechaHoraInicio");
const detalleServicio = document.getElementById("detalleServicio");
const botonReservar = document.getElementById("btnReservar");
const panelReprogramar = document.getElementById("panelReprogramar");
const detalleReprogramar = document.getElementById("detalleReprogramar");
const formularioReprogramar = document.getElementById("reprogramarForm");
const camposReprogramar = document.getElementById("camposReprogramar");
const campoNuevaFecha = document.getElementById("nuevaFecha");
const botonCancelarReprogramacion = document.getElementById("btnCancelarReprogramacion");

let idReprogramando = null;

let guardando = false;
let opcionesDisponibles = false;

const serviciosActivos = new Map();
let cargando = false;

const formatoMoneda = new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency: "ARS"
});

const formatoFecha = new Intl.DateTimeFormat("es-AR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    hourCycle: "h23"
});

const estados = {
    1: {
        texto: "Pendiente",
        clase: "text-bg-warning"
    },
    2: {
        texto: "Completado",
        clase: "text-bg-success"
    },
    3: {
        texto: "Cancelado",
        clase: "text-bg-secondary"
    }
};

function mostrarMensaje(texto, tipo = "danger") {
    mensaje.textContent = texto;
    mensaje.className = `alert alert-${tipo}`;
}

function agregarCelda(fila, texto, clase = "") {
    const celda = document.createElement("td");

    celda.textContent = texto;
    celda.className = clase;

    fila.append(celda);
    return celda;
}

function nombreCompleto(persona) {
    if (!persona) {
        return "Sin datos";
    }

    return `${persona.apellido}, ${persona.nombre}`;
}

function mostrarFecha(valor) {
    if (!valor) {
        return "Sin fecha";
    }

    const fecha = new Date(valor);

    return Number.isNaN(fecha.getTime())
        ? "Fecha inválida"
        : formatoFecha.format(fecha);
}

function mostrarTurnos(turnos) {
    tabla.replaceChildren();

    for (const turno of turnos) {
        const fila = document.createElement("tr");

        agregarCelda(fila, nombreCompleto(turno.cliente));
        agregarCelda(fila, nombreCompleto(turno.barbero));
        agregarCelda(fila, turno.servicio?.nombre ?? "Sin datos");

        agregarCelda(
            fila,
            mostrarFecha(turno.fechaHoraInicio),
            "text-nowrap"
        );

        agregarCelda(
            fila,
            mostrarFecha(turno.fechaHoraFin),
            "text-nowrap"
        );

        agregarCelda(
            fila,
            formatoMoneda.format(turno.precioPactado),
            "text-end text-nowrap"
        );

        const estado = estados[turno.estado] ?? {
            texto: "Desconocido",
            clase: "text-bg-secondary"
        };

        const celdaEstado = agregarCelda(fila, "");
        const etiqueta = document.createElement("span");

        etiqueta.className = `badge ${estado.clase}`;
        etiqueta.textContent = estado.texto;

       celdaEstado.append(etiqueta);

const celdaAcciones = agregarCelda(fila, "");

if (turno.estado === 1) {
    const acciones = document.createElement("div");
    acciones.className = "d-flex gap-2";

    const completar = document.createElement("button");
    completar.type = "button";
    completar.className = "btn btn-outline-success btn-sm";
    completar.textContent = "Completar";

    completar.addEventListener("click", () => {
        cambiarEstado(turno, "completar");
    });

    const cancelar = document.createElement("button");
    cancelar.type = "button";
    cancelar.className = "btn btn-outline-danger btn-sm";
    cancelar.textContent = "Cancelar";

    cancelar.addEventListener("click", () => {
        cambiarEstado(turno, "cancelar");
    });
    const reprogramar = document.createElement("button");
reprogramar.type = "button";
reprogramar.className = "btn btn-outline-light btn-sm";
reprogramar.textContent = "Reprogramar";

reprogramar.addEventListener("click", () => {
    comenzarReprogramacion(turno);
});

acciones.className = "d-flex flex-wrap gap-2";
acciones.append(reprogramar, completar, cancelar);
    celdaAcciones.append(acciones);
} else {
    celdaAcciones.textContent = "—";
}

tabla.append(fila);
    }
}

async function cargarTurnos() {
   if (cargando || guardando) return;

    cargando = true;
    mensaje.className = "alert d-none";
    tabla.replaceChildren();

    botonActualizar.disabled = true;
    botonActualizar.textContent = "Cargando...";
    tabla.setAttribute("aria-busy", "true");

    try {
        const turnos = await solicitarApi("/api/turnos");

        if (!Array.isArray(turnos)) {
            throw new Error("El servidor devolvió un listado inválido.");
        }

        mostrarTurnos(turnos);

        if (turnos.length === 0) {
            mostrarMensaje(
                "Todavía no hay turnos registrados.",
                "info"
            );
        }
    } catch (error) {
        mostrarMensaje(error.message);
    } finally {
        cargando = false;
        botonActualizar.disabled = false;
        botonActualizar.textContent = "Actualizar listado";
        tabla.setAttribute("aria-busy", "false");
    }
}
function completarSelect(select, elementos, textoInicial, obtenerTexto) {
    select.replaceChildren(new Option(textoInicial, ""));

    for (const elemento of elementos) {
        select.add(
            new Option(obtenerTexto(elemento), String(elemento.id))
        );
    }
}

function actualizarDetalleServicio() {
    const servicio = serviciosActivos.get(
        Number(campoServicio.value)
    );

    detalleServicio.textContent = servicio
        ? `${formatoMoneda.format(servicio.precio)} · ` +
          `${servicio.duracionMinutos} minutos`
        : "Seleccioná un servicio para ver su precio y duración.";
}

function actualizarFechaMinima() {
    // Redondeamos al próximo minuto.
    const fecha = new Date(
        Math.ceil((Date.now() + 1) / 60000) * 60000
    );

    const anio = fecha.getFullYear();
    const mes = String(fecha.getMonth() + 1).padStart(2, "0");
    const dia = String(fecha.getDate()).padStart(2, "0");
    const hora = String(fecha.getHours()).padStart(2, "0");
    const minutos = String(fecha.getMinutes()).padStart(2, "0");

    campoFecha.min = `${anio}-${mes}-${dia}T${hora}:${minutos}`;
}

async function cargarOpciones() {
    opcionesDisponibles = false;
    camposReserva.disabled = true;

    const [clientes, barberos, servicios] = await Promise.all([
        solicitarApi("/api/clientes"),
        solicitarApi("/api/barberos"),
        solicitarApi("/api/servicios")
    ]);

    if (![clientes, barberos, servicios].every(Array.isArray)) {
        throw new Error("No se pudieron interpretar las opciones de reserva.");
    }

    const clientesActivos = clientes.filter(cliente => cliente.activo);
    const barberosActivos = barberos.filter(barbero => barbero.activo);
    const serviciosHabilitados = servicios.filter(servicio => servicio.activo);

    completarSelect(
        campoCliente,
        clientesActivos,
        "Seleccioná un cliente",
        cliente => `${cliente.apellido}, ${cliente.nombre}`
    );

    completarSelect(
        campoBarbero,
        barberosActivos,
        "Seleccioná un barbero",
        barbero => `${barbero.apellido}, ${barbero.nombre}`
    );

    completarSelect(
        campoServicio,
        serviciosHabilitados,
        "Seleccioná un servicio",
        servicio => servicio.nombre
    );

    serviciosActivos.clear();

    for (const servicio of serviciosHabilitados) {
        serviciosActivos.set(servicio.id, servicio);
    }

    actualizarDetalleServicio();

    const faltantes = [];

    if (clientesActivos.length === 0) faltantes.push("clientes");
    if (barberosActivos.length === 0) faltantes.push("barberos");
    if (serviciosHabilitados.length === 0) faltantes.push("servicios");

    if (faltantes.length > 0) {
        throw new Error(
            `Para reservar necesitás registros activos de: ${faltantes.join(", ")}.`
        );
    }

    opcionesDisponibles = true;
    camposReserva.disabled = false;
}

async function reservarTurno(event) {
    event.preventDefault();

    if (guardando || cargando || !opcionesDisponibles) return;

    mensaje.className = "alert d-none";

    const clienteId = Number(campoCliente.value);
    const barberoId = Number(campoBarbero.value);
    const servicioId = Number(campoServicio.value);
    const fechaHoraInicio = campoFecha.value;

    const fecha = new Date(fechaHoraInicio);

    if (![clienteId, barberoId, servicioId].every(
        id => Number.isInteger(id) && id > 0
    )) {
        mostrarMensaje("Seleccioná cliente, barbero y servicio.");
        return;
    }

    if (!Number.isFinite(fecha.getTime()) ||
        fecha.getTime() <= Date.now()) {
        mostrarMensaje("Seleccioná una fecha y hora futura.");
        return;
    }

    guardando = true;
    camposReserva.disabled = true;
    botonActualizar.disabled = true;
    botonReservar.textContent = "Reservando...";

    let reservado = false;

    try {
        await solicitarApi("/api/turnos", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                clienteId,
                barberoId,
                servicioId,
                fechaHoraInicio
            })
        });

        reservado = true;

        formulario.reset();
        actualizarDetalleServicio();
        actualizarFechaMinima();

        const turnos = await solicitarApi("/api/turnos");

        if (!Array.isArray(turnos)) {
            throw new Error("El servidor devolvió un listado inválido.");
        }

        mostrarTurnos(turnos);

        mostrarMensaje("Turno reservado correctamente.", "success");
    } catch (error) {
        if (reservado) {
            tabla.replaceChildren();

            mostrarMensaje(
                "El turno se reservó, pero no se pudo actualizar el listado. " +
                "Presioná Actualizar listado; no vuelvas a reservarlo.",
                "warning"
            );
        } else {
            mostrarMensaje(error.message);
        }
    } finally {
        guardando = false;
        camposReserva.disabled = !opcionesDisponibles;
        botonActualizar.disabled = false;
        botonReservar.textContent = "Reservar turno";
    }
}
async function cambiarEstado(turno, accion) {
    if (guardando || cargando) return;

    if (accion !== "cancelar" && accion !== "completar") {
        return;
    }

    const pregunta = accion === "cancelar"
        ? `¿Cancelar el turno de ${nombreCompleto(turno.cliente)} ` +
          `del ${mostrarFecha(turno.fechaHoraInicio)}?`
        : `¿Confirmás que se realizó el servicio de ` +
          `${nombreCompleto(turno.cliente)}?`;

    if (!window.confirm(pregunta)) return;

    mensaje.className = "alert d-none";

    guardando = true;
    camposReserva.disabled = true;
    botonActualizar.disabled = true;

    tabla.querySelectorAll("button").forEach(boton => {
        boton.disabled = true;
    });

    let actualizado = false;

    try {
        await solicitarApi(`/api/turnos/${turno.id}/${accion}`, {
            method: "PATCH"
        });

        actualizado = true;
        if (idReprogramando === turno.id) {
    cerrarReprogramacion();
}
        const turnos = await solicitarApi("/api/turnos");

        if (!Array.isArray(turnos)) {
            throw new Error("El servidor devolvió un listado inválido.");
        }

        mostrarTurnos(turnos);

        mostrarMensaje(
            accion === "cancelar"
                ? "Turno cancelado correctamente."
                : "Turno completado correctamente.",
            "success"
        );
    } catch (error) {
        if (actualizado) {
            tabla.replaceChildren();

            mostrarMensaje(
                "El estado se actualizó, pero no se pudo cargar el listado. " +
                "Presioná Actualizar listado.",
                "warning"
            );
        } else {
            mostrarMensaje(error.message);
        }
    } finally {
        guardando = false;
        camposReserva.disabled = !opcionesDisponibles;
        botonActualizar.disabled = false;

        tabla.querySelectorAll("button").forEach(boton => {
            boton.disabled = false;
        });
    }
}
async function iniciarPagina() {
    const sesion = obtenerSesion();

    if (!sesion) {
        cerrarSesion();
        return;
    }

    nombreUsuario.textContent = sesion.usuario.nombreCompleto;

    botonSalir.addEventListener("click", cerrarSesion);
    botonActualizar.addEventListener("click", cargarTurnos);

    formulario.addEventListener("submit", reservarTurno);
    campoServicio.addEventListener("change", actualizarDetalleServicio);
    campoFecha.addEventListener("focus", actualizarFechaMinima);

    actualizarFechaMinima();
    formularioReprogramar.addEventListener(
    "submit",
    guardarReprogramacion
);

botonCancelarReprogramacion.addEventListener("click", () => {
    if (!guardando) {
        cerrarReprogramacion();
    }
});

campoNuevaFecha.addEventListener("focus", () => {
    actualizarFechaMinima();
    campoNuevaFecha.min = campoFecha.min;
});
    await cargarTurnos();

    try {
        await cargarOpciones();
    } catch (error) {
        mostrarMensaje(error.message);
    }
}
function cerrarReprogramacion() {
    idReprogramando = null;
    formularioReprogramar.reset();
    panelReprogramar.classList.add("d-none");
}

function comenzarReprogramacion(turno) {
    if (guardando || cargando) return;

    mensaje.className = "alert d-none";
    idReprogramando = turno.id;

    formularioReprogramar.reset();

    actualizarFechaMinima();
    campoNuevaFecha.min = campoFecha.min;

    detalleReprogramar.textContent =
        `${nombreCompleto(turno.cliente)} · ` +
        `${turno.servicio?.nombre ?? "Servicio"} · ` +
        `Horario actual: ${mostrarFecha(turno.fechaHoraInicio)}.`;

    panelReprogramar.classList.remove("d-none");

    panelReprogramar.scrollIntoView({
        behavior: "smooth",
        block: "center"
    });

    campoNuevaFecha.focus({ preventScroll: true });
}

async function guardarReprogramacion(event) {
    event.preventDefault();

    if (guardando || cargando || idReprogramando === null) return;

    mensaje.className = "alert d-none";

    const fechaHoraInicio = campoNuevaFecha.value;
    const fecha = new Date(fechaHoraInicio);

    if (!Number.isFinite(fecha.getTime()) ||
        fecha.getTime() <= Date.now()) {
        mostrarMensaje("Seleccioná una fecha y hora futura.");
        return;
    }

    guardando = true;
    camposReserva.disabled = true;
    camposReprogramar.disabled = true;
    botonActualizar.disabled = true;

    tabla.querySelectorAll("button").forEach(boton => {
        boton.disabled = true;
    });

    let actualizado = false;

    try {
        await solicitarApi(
            `/api/turnos/${idReprogramando}/reprogramar`,
            {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    fechaHoraInicio
                })
            }
        );

        actualizado = true;
        cerrarReprogramacion();

        const turnos = await solicitarApi("/api/turnos");

        if (!Array.isArray(turnos)) {
            throw new Error("El servidor devolvió un listado inválido.");
        }

        mostrarTurnos(turnos);

        mostrarMensaje(
            "Turno reprogramado correctamente.",
            "success"
        );
    } catch (error) {
        if (actualizado) {
            tabla.replaceChildren();

            mostrarMensaje(
                "El horario se actualizó, pero no se pudo cargar el listado. " +
                "Presioná Actualizar listado.",
                "warning"
            );
        } else {
            mostrarMensaje(error.message);
        }
    } finally {
        guardando = false;
        camposReserva.disabled = !opcionesDisponibles;
        camposReprogramar.disabled = false;
        botonActualizar.disabled = false;

        tabla.querySelectorAll("button").forEach(boton => {
            boton.disabled = false;
        });
    }
}
iniciarPagina();