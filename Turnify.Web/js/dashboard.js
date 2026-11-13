const API_URL = "https://localhost:7226";

const nombreUsuario = document.getElementById("nombreUsuario");
const botonSalir = document.getElementById("btnSalir");
const formulario = document.getElementById("filtroForm");
const campoDesde = document.getElementById("desde");
const campoHasta = document.getElementById("hasta");
const botonConsultar = document.getElementById("btnConsultar");
const indicadores = document.getElementById("indicadores");
const mensaje = document.getElementById("mensaje");

const formatoMoneda = new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency: "ARS"
});

const definiciones = [
    ["totalTurnos", "Total de turnos"],
    ["turnosPendientes", "Pendientes"],
    ["turnosCompletados", "Completados"],
    ["turnosCancelados", "Cancelados"],
    ["importeServiciosCompletados", "Importe de servicios completados"]
];

const valores = new Map();

function obtenerSesion() {
    try {
        const sesion = JSON.parse(
            sessionStorage.getItem("turnifySesion")
        );

        if (!sesion?.token || !sesion?.usuario) {
            return null;
        }

        const vencimiento = Date.parse(sesion.expiraUtc);

        if (!Number.isFinite(vencimiento) ||
            vencimiento <= Date.now()) {
            return null;
        }

        return sesion;
    } catch {
        return null;
    }
}

function cerrarSesion() {
    sessionStorage.removeItem("turnifySesion");
    window.location.replace("login.html");
}

function fechaLocalActual() {
    const hoy = new Date();

    const anio = hoy.getFullYear();
    const mes = String(hoy.getMonth() + 1).padStart(2, "0");
    const dia = String(hoy.getDate()).padStart(2, "0");

    return `${anio}-${mes}-${dia}`;
}

function mostrarMensaje(texto, tipo = "danger") {
    mensaje.textContent = texto;
    mensaje.className = `alert alert-${tipo}`;
}

function crearTarjetas() {
    for (const [clave, titulo] of definiciones) {
        const columna = document.createElement("div");
        columna.className = "col-12 col-sm-6 col-lg-4";

        const tarjeta = document.createElement("article");
        tarjeta.className = "panel-resumen p-4 h-100";

        const encabezado = document.createElement("h2");
        encabezado.className = "h6 texto-secundario";
        encabezado.textContent = titulo;

        const valor = document.createElement("p");
        valor.className = "h2 fw-bold mb-0";
        valor.textContent = "—";

        tarjeta.append(encabezado, valor);
        columna.append(tarjeta);
        indicadores.append(columna);

        valores.set(clave, valor);
    }
}

async function cargarResumen() {
    const sesion = obtenerSesion();

    if (!sesion) {
        cerrarSesion();
        return;
    }

    const desde = campoDesde.value;
    const hasta = campoHasta.value;

    if (!desde || !hasta || desde > hasta) {
        mostrarMensaje("Ingresá un rango de fechas válido.");
        return;
    }

    mensaje.className = "alert d-none";
    botonConsultar.disabled = true;
    botonConsultar.textContent = "Consultando...";
    indicadores.setAttribute("aria-busy", "true");

    // Evitamos mostrar cifras de una consulta anterior mientras cargamos.
    for (const valor of valores.values()) {
        valor.textContent = "—";
    }

    try {
        const parametros = new URLSearchParams({ desde, hasta });

        const respuesta = await fetch(
            `${API_URL}/api/dashboard?${parametros}`,
            {
                headers: {
                    Authorization: `Bearer ${sesion.token}`
                },
                cache: "no-store"
            }
        );

        if (respuesta.status === 401) {
            cerrarSesion();
            return;
        }

        if (!respuesta.ok) {
            mostrarMensaje(
                `No se pudo obtener el resumen. Código: ${respuesta.status}.`
            );
            return;
        }

        const datos = await respuesta.json();
        const resumen = datos.resumen;

        const resumenValido = resumen &&
            definiciones.every(([clave]) =>
                typeof resumen[clave] === "number" &&
                Number.isFinite(resumen[clave])
            );

        if (!resumenValido) {
            mostrarMensaje("El servidor devolvió un resumen inválido.");
            return;
        }

        for (const [clave] of definiciones) {
            const valor = valores.get(clave);

            valor.textContent =
                clave === "importeServiciosCompletados"
                    ? formatoMoneda.format(resumen[clave])
                    : String(resumen[clave]);
        }

        if (resumen.totalTurnos === 0) {
            mostrarMensaje(
                "No hay turnos registrados para este período.",
                "info"
            );
        }
    } catch {
        mostrarMensaje(
            "No se pudo cargar el dashboard. " +
            "Verificá la conexión y que la API esté ejecutándose."
        );
    } finally {
        botonConsultar.disabled = false;
        botonConsultar.textContent = "Consultar";
        indicadores.setAttribute("aria-busy", "false");
    }
}

function iniciarDashboard() {
    const sesion = obtenerSesion();

    if (!sesion) {
        cerrarSesion();
        return;
    }

    nombreUsuario.textContent = sesion.usuario.nombreCompleto;

    campoDesde.value = fechaLocalActual();
    campoHasta.value = campoDesde.value;

    crearTarjetas();

    botonSalir.addEventListener("click", cerrarSesion);

    formulario.addEventListener("submit", (event) => {
        event.preventDefault();
        cargarResumen();
    });

    cargarResumen();
}

iniciarDashboard();