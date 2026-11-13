import {
    solicitarApi,
    obtenerSesion,
    cerrarSesion
} from "./api.js";

const tabla = document.getElementById("tablaClientes");
const mensaje = document.getElementById("mensaje");
const botonActualizar = document.getElementById("btnActualizar");
const botonSalir = document.getElementById("btnSalir");
const nombreUsuario = document.getElementById("nombreUsuario");

const formulario = document.getElementById("clienteForm");
const tituloFormulario = document.getElementById("tituloFormulario");
const campoNombre = document.getElementById("nombre");
const campoApellido = document.getElementById("apellido");
const campoTelefono = document.getElementById("telefono");
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

    tituloFormulario.textContent = "Nuevo cliente";
    botonGuardar.textContent = "Crear cliente";
    botonCancelar.classList.add("d-none");
}

function comenzarEdicion(cliente) {
    if (ocupado) return;

    ocultarMensaje();
    idEnEdicion = cliente.id;

    campoNombre.value = cliente.nombre;
    campoApellido.value = cliente.apellido;
    campoTelefono.value = cliente.telefono;

    tituloFormulario.textContent = "Editar cliente";
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

function mostrarClientes(clientes) {
    tabla.replaceChildren();

    for (const cliente of clientes) {
        const fila = document.createElement("tr");

        agregarCelda(fila, cliente.apellido);
        agregarCelda(fila, cliente.nombre);
        agregarCelda(fila, cliente.telefono);

        const celdaEstado = agregarCelda(fila, "");
        const estado = document.createElement("span");

        estado.className = cliente.activo
            ? "badge text-bg-success"
            : "badge text-bg-secondary";

        estado.textContent = cliente.activo
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

        editar.addEventListener("click", () => comenzarEdicion(cliente));
        acciones.append(editar);

        if (cliente.activo) {
            const baja = document.createElement("button");
            baja.type = "button";
            baja.className = "btn btn-outline-danger btn-sm";
            baja.textContent = "Dar de baja";
            baja.disabled = ocupado;

            baja.addEventListener("click", () => darDeBaja(cliente));
            acciones.append(baja);
        }

        celdaAcciones.append(acciones);
        tabla.append(fila);
    }
}

async function refrescarListado() {
    const clientes = await solicitarApi("/api/clientes");

    if (!Array.isArray(clientes)) {
        throw new Error("El servidor devolvió un listado inválido.");
    }

    mostrarClientes(clientes);

    return clientes;
}

async function cargarClientes() {
    if (ocupado) return;

    ocultarMensaje();
    establecerOcupado(true);
    tabla.replaceChildren();

    try {
        const clientes = await refrescarListado();

        if (clientes.length === 0) {
            mostrarMensaje(
                "Todavía no hay clientes registrados.",
                "info"
            );
        }
    } catch (error) {
        mostrarMensaje(error.message);
    } finally {
        establecerOcupado(false);
    }
}

async function guardarCliente(event) {
    event.preventDefault();

    if (ocupado) return;

    ocultarMensaje();

    const nombre = campoNombre.value.trim();
    const apellido = campoApellido.value.trim();
    const telefono = campoTelefono.value.trim();

    if (!nombre || !apellido || !telefono) {
        mostrarMensaje("Completá nombre, apellido y teléfono.");
        return;
    }

    const esEdicion = idEnEdicion !== null;

    const ruta = esEdicion
        ? `/api/clientes/${idEnEdicion}`
        : "/api/clientes";

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
                apellido,
                telefono
            })
        });

        guardado = true;
        limpiarFormulario();

        await refrescarListado();

        mostrarMensaje(
            esEdicion
                ? "Cliente actualizado correctamente."
                : "Cliente creado correctamente.",
            "success"
        );
    } catch (error) {
        if (guardado) {
            tabla.replaceChildren();

            mostrarMensaje(
                "El cliente se guardó, pero no se pudo actualizar el listado. " +
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

async function darDeBaja(cliente) {
    if (ocupado) return;

    const confirmado = window.confirm(
        `¿Dar de baja a "${cliente.nombre} ${cliente.apellido}"? ` +
        "Se conservarán sus turnos anteriores."
    );

    if (!confirmado) return;

    ocultarMensaje();
    establecerOcupado(true);

    let guardado = false;

    try {
        await solicitarApi(`/api/clientes/${cliente.id}`, {
            method: "DELETE"
        });

        guardado = true;

        if (idEnEdicion === cliente.id) {
            limpiarFormulario();
        }

        await refrescarListado();

        mostrarMensaje(
            "Cliente dado de baja correctamente.",
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
    botonActualizar.addEventListener("click", cargarClientes);
    formulario.addEventListener("submit", guardarCliente);

    botonCancelar.addEventListener("click", () => {
        limpiarFormulario();
        ocultarMensaje();
    });

    cargarClientes();
}

iniciarPagina();