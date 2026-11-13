import {
    solicitarApi,
    obtenerSesion,
    cerrarSesion
} from "./api.js";

const tabla = document.getElementById("tablaServicios");
const mensaje = document.getElementById("mensaje");
const botonActualizar = document.getElementById("btnActualizar");
const botonSalir = document.getElementById("btnSalir");
const nombreUsuario = document.getElementById("nombreUsuario");

const formulario = document.getElementById("servicioForm");
const tituloFormulario = document.getElementById("tituloFormulario");
const campoNombre = document.getElementById("nombre");
const campoPrecio = document.getElementById("precio");
const campoDuracion = document.getElementById("duracion");
const botonGuardar = document.getElementById("btnGuardar");
const botonCancelar = document.getElementById("btnCancelar");

let idEnEdicion = null;
let ocupado = false;

const formatoMoneda = new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency: "ARS"
});

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

    tituloFormulario.textContent = "Nuevo servicio";
    botonGuardar.textContent = "Crear servicio";
    botonCancelar.classList.add("d-none");
}

function comenzarEdicion(servicio) {
    if (ocupado) return;

    ocultarMensaje();
    idEnEdicion = servicio.id;

    campoNombre.value = servicio.nombre;
    campoPrecio.value = servicio.precio;
    campoDuracion.value = servicio.duracionMinutos;

    tituloFormulario.textContent = "Editar servicio";
    botonGuardar.textContent = "Guardar cambios";
    botonCancelar.classList.remove("d-none");

    formulario.scrollIntoView({
        behavior: "smooth",
        block: "center"
    });

    campoNombre.focus({ preventScroll: true });
}

function agregarCelda(fila, texto, clase = "") {
    const celda = document.createElement("td");

    celda.textContent = texto;
    celda.className = clase;

    fila.append(celda);
    return celda;
}

function mostrarServicios(servicios) {
    tabla.replaceChildren();

    for (const servicio of servicios) {
        const fila = document.createElement("tr");

        agregarCelda(fila, servicio.nombre);

        agregarCelda(
            fila,
            formatoMoneda.format(servicio.precio),
            "text-end"
        );

        agregarCelda(fila, `${servicio.duracionMinutos} min`);

        const celdaEstado = agregarCelda(fila, "");
        const estado = document.createElement("span");

        estado.className = servicio.activo
            ? "badge text-bg-success"
            : "badge text-bg-secondary";

        estado.textContent = servicio.activo
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
        editar.addEventListener("click", () => comenzarEdicion(servicio));

        acciones.append(editar);

        if (servicio.activo) {
            const baja = document.createElement("button");
            baja.type = "button";
            baja.className = "btn btn-outline-danger btn-sm";
            baja.textContent = "Dar de baja";
            baja.disabled = ocupado;

            baja.addEventListener("click", () => darDeBaja(servicio));

            acciones.append(baja);
        }

        celdaAcciones.append(acciones);
        tabla.append(fila);
    }
}

async function refrescarListado() {
    const servicios = await solicitarApi("/api/servicios");

    if (!Array.isArray(servicios)) {
        throw new Error("El servidor devolvió un listado inválido.");
    }

    mostrarServicios(servicios);

    return servicios;
}

async function cargarServicios() {
    if (ocupado) return;

    ocultarMensaje();
    establecerOcupado(true);
    tabla.replaceChildren();

    try {
        const servicios = await refrescarListado();

        if (servicios.length === 0) {
            mostrarMensaje(
                "Todavía no hay servicios registrados.",
                "info"
            );
        }
    } catch (error) {
        mostrarMensaje(error.message);
    } finally {
        establecerOcupado(false);
    }
}

async function guardarServicio(event) {
    event.preventDefault();

    if (ocupado) return;

    ocultarMensaje();

    const nombre = campoNombre.value.trim();
    const precio = Number(campoPrecio.value);
    const duracionMinutos = Number(campoDuracion.value);

    if (!nombre ||
        !Number.isFinite(precio) ||
        precio <= 0 ||
        !Number.isInteger(duracionMinutos) ||
        duracionMinutos <= 0) {
        mostrarMensaje(
            "Ingresá un nombre, un precio positivo y una duración entera positiva."
        );
        return;
    }

    const esEdicion = idEnEdicion !== null;

    const ruta = esEdicion
        ? `/api/servicios/${idEnEdicion}`
        : "/api/servicios";

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
                precio,
                duracionMinutos
            })
        });

        guardado = true;
        limpiarFormulario();

        await refrescarListado();

        mostrarMensaje(
            esEdicion
                ? "Servicio actualizado correctamente."
                : "Servicio creado correctamente.",
            "success"
        );
    } catch (error) {
        if (guardado) {
            tabla.replaceChildren();

            mostrarMensaje(
                "El servicio se guardó, pero no se pudo actualizar el listado. " +
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

async function darDeBaja(servicio) {
    if (ocupado) return;

    const confirmado = window.confirm(
        `¿Dar de baja "${servicio.nombre}"? Se conservarán sus turnos anteriores.`
    );

    if (!confirmado) return;

    ocultarMensaje();
    establecerOcupado(true);

    let guardado = false;

    try {
        await solicitarApi(`/api/servicios/${servicio.id}`, {
            method: "DELETE"
        });

        guardado = true;

        if (idEnEdicion === servicio.id) {
            limpiarFormulario();
        }

        await refrescarListado();

        mostrarMensaje(
            "Servicio dado de baja correctamente.",
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
    botonActualizar.addEventListener("click", cargarServicios);
    formulario.addEventListener("submit", guardarServicio);

    botonCancelar.addEventListener("click", () => {
        limpiarFormulario();
        ocultarMensaje();
    });

    cargarServicios();
}

iniciarPagina();