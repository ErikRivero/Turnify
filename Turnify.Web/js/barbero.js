import {
    solicitarApi,
    obtenerSesion,
    cerrarSesion
} from "./api.js";

const tabla = document.getElementById("tablaBarberos");
const mensaje = document.getElementById("mensaje");
const botonActualizar = document.getElementById("btnActualizar");
const botonSalir = document.getElementById("btnSalir");
const nombreUsuario = document.getElementById("nombreUsuario");

const formulario = document.getElementById("BarberoForm");
const tituloFormulario = document.getElementById("tituloFormulario");
const campoNombre = document.getElementById("nombre");
const campoApellido = document.getElementById("apellido");
const botonGuardar = document.getElementById("btnGuardar");
const botonCancelar = document.getElementById("btnCancelar");

let idEnEdicion = null;
let ocupado = false;

function mostrarMensaje(texto, tipo = "danger") {
    mensaje.textContent = texto;
    mensaje.className = `alert alert-${tipo}`;
}

function ocultarMensaje() {
    mensaje.textContent = "";
    mensaje.className = "alert d-none";
}

function establecerOcupado(valor) {
    ocupado = valor;
    botonActualizar.disabled = valor;

    formulario.querySelectorAll("input, button").forEach(control => {
        control.disabled = valor;
    });

    tabla.querySelectorAll("button").forEach(boton => {
        boton.disabled = valor;
    });

    tabla.setAttribute("aria-busy", String(valor));
}

function limpiarFormulario() {
    idEnEdicion = null;
    formulario.reset();

    tituloFormulario.textContent = "Nuevo Barbero";
    botonGuardar.textContent = "Crear Barbero";
    botonCancelar.classList.add("d-none");
}

function comenzarEdicion(Barbero) {
    if (ocupado) return;

    ocultarMensaje();
    idEnEdicion = Barbero.id;

    campoNombre.value = Barbero.nombre;
    campoApellido.value = Barbero.apellido;

    tituloFormulario.textContent = "Editar Barbero";
    botonGuardar.textContent = "Guardar cambios";
    botonCancelar.classList.remove("d-none");

    formulario.scrollIntoView({
        behavior: "smooth",
        block: "center"
    });

    campoNombre.focus({ preventScroll: true });
}

function agregarCelda(fila, texto) {
    const celda = document.createElement("td");

    celda.textContent = texto;
    fila.append(celda);

    return celda;
}

function mostrarBarberos(Barberos) {
    tabla.replaceChildren();

    for (const Barbero of Barberos) {
        const fila = document.createElement("tr");

        agregarCelda(fila, Barbero.apellido);
        agregarCelda(fila, Barbero.nombre);

        const celdaEstado = agregarCelda(fila, "");
        const estado = document.createElement("span");

        estado.className = Barbero.activo
            ? "badge text-bg-success"
            : "badge text-bg-secondary";

        estado.textContent = Barbero.activo
            ? "Activo"
            : "Inactivo";

        celdaEstado.append(estado);

        const celdaAcciones = agregarCelda(fila, "");
        const acciones = document.createElement("div");
        acciones.className = "d-flex flex-wrap gap-2";

        const editar = document.createElement("button");
        editar.type = "button";
        editar.className = "btn btn-outline-light btn-sm";
        editar.textContent = "Editar";
        editar.disabled = ocupado;

        editar.addEventListener("click", () => comenzarEdicion(Barbero));
        acciones.append(editar);

        if (Barbero.activo) {
            const baja = document.createElement("button");
            baja.type = "button";
            baja.className = "btn btn-outline-danger btn-sm";
            baja.textContent = "Dar de baja";
            baja.disabled = ocupado;

            baja.addEventListener("click", () => darDeBaja(Barbero));
            acciones.append(baja);
        }

        celdaAcciones.append(acciones);
        tabla.append(fila);
    }
}

async function refrescarListado() {
    const Barberos = await solicitarApi("/api/Barberos");

    if (!Array.isArray(Barberos)) {
        throw new Error("El servidor devolvió un listado inválido.");
    }

    mostrarBarberos(Barberos);

    return Barberos;
}

async function cargarBarberos() {
    if (ocupado) return;

    ocultarMensaje();
    establecerOcupado(true);
    tabla.replaceChildren();

    try {
        const Barberos = await refrescarListado();

        if (Barberos.length === 0) {
            mostrarMensaje(
                "Todavía no hay Barberos registrados.",
                "info"
            );
        }
    } catch (error) {
        mostrarMensaje(error.message);
    } finally {
        establecerOcupado(false);
    }
}

async function guardarBarbero(event) {
    event.preventDefault();

    if (ocupado) return;

    ocultarMensaje();

    const nombre = campoNombre.value.trim();
    const apellido = campoApellido.value.trim();

    if (!nombre || !apellido) {
        mostrarMensaje("Completá nombre y apellido.");
        return;
    }

    const esEdicion = idEnEdicion !== null;

    const ruta = esEdicion
        ? `/api/barberos/${idEnEdicion}`
        : "/api/barberos";

    establecerOcupado(true);
    let guardado = false;

    try {
        await solicitarApi(ruta, {
            method: esEdicion ? "PUT" : "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nombre,
                apellido
            })
        });

        guardado = true;
        limpiarFormulario();

        await refrescarListado();

        mostrarMensaje(
            esEdicion
                ? "Barbero actualizado correctamente."
                : "Barbero creado correctamente.",
            "success"
        );
    } catch (error) {
        if (guardado) {
            tabla.replaceChildren();

            mostrarMensaje(
                "El barbero se guardó, pero no se pudo actualizar el listado. " +
                "Presioná Actualizar listado.",
                "warning"
            );
        } else {
            mostrarMensaje(error.message);
        }
    } finally {
        establecerOcupado(false);
    }
}

async function darDeBaja(Barbero) {
    if (ocupado) return;

    const confirmado = window.confirm(
        `¿Dar de baja a "${Barbero.nombre} ${Barbero.apellido}"? ` +
        "Se conservarán sus turnos anteriores."
    );

    if (!confirmado) return;

    ocultarMensaje();
    establecerOcupado(true);

    let guardado = false;

    try {
        await solicitarApi(`/api/Barberos/${Barbero.id}`, {
            method: "DELETE"
        });

        guardado = true;

        if (idEnEdicion === Barbero.id) {
            limpiarFormulario();
        }

        await refrescarListado();

        mostrarMensaje(
            "Barbero dado de baja correctamente.",
            "success"
        );
    } catch (error) {
        if (guardado) {
            tabla.replaceChildren();

            mostrarMensaje(
                "La baja se guardó, pero no se pudo actualizar el listado. " +
                "Presioná Actualizar listado.",
                "warning"
            );
        } else {
            mostrarMensaje(error.message);
        }
    } finally {
        establecerOcupado(false);
    }
}

function iniciarPagina() {
    const sesion = obtenerSesion();

    if (!sesion) {
        cerrarSesion();
        return;
    }

    nombreUsuario.textContent = sesion.usuario.nombreCompleto;

    botonSalir.addEventListener("click", cerrarSesion);
    botonActualizar.addEventListener("click", cargarBarberos);
    formulario.addEventListener("submit", guardarBarbero);

    botonCancelar.addEventListener("click", () => {
        limpiarFormulario();
        ocultarMensaje();
    });

    cargarBarberos();
}

iniciarPagina();