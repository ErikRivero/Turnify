const API_URL = "https://localhost:7226";

export function cerrarSesion() {
    sessionStorage.removeItem("turnifySesion");
    window.location.replace("login.html");
}

export function obtenerSesion() {
    try {
        const sesion = JSON.parse(
            sessionStorage.getItem("turnifySesion")
        );

        const vencimiento = Date.parse(sesion?.expiraUtc);

        if (!sesion?.token ||
            !sesion?.usuario ||
            !Number.isFinite(vencimiento) ||
            vencimiento <= Date.now()) {
            return null;
        }

        return sesion;
    } catch {
        return null;
    }
}

export async function solicitarApi(ruta, opciones = {}) {
    const sesion = obtenerSesion();

    if (!sesion) {
        cerrarSesion();
        throw new Error("La sesión venció.");
    }

    const headers = new Headers(opciones.headers);
    headers.set("Authorization", `Bearer ${sesion.token}`);

    let respuesta;

    try {
        respuesta = await fetch(`${API_URL}${ruta}`, {
            ...opciones,
            headers,
            cache: "no-store"
        });
    } catch {
        throw new Error(
            "No se pudo conectar con la API. Verificá que esté ejecutándose."
        );
    }

    if (respuesta.status === 401) {
        cerrarSesion();
        throw new Error("La sesión venció.");
    }

    if (!respuesta.ok) {
        const error = await respuesta.json().catch(() => null);

        throw new Error(
            error?.mensaje ??
            `La operación falló. Código: ${respuesta.status}.`
        );
    }

    if (respuesta.status === 204) {
        return null;
    }

    return respuesta.json();
}