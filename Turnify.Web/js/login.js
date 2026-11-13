const API_URL = "https://localhost:7226";

const formulario = document.getElementById("loginForm");
const campoUsuario = document.getElementById("nombreUsuario");
const campoPassword = document.getElementById("password");
const botonIngresar = document.getElementById("btnIngresar");
const mensaje = document.getElementById("mensaje");

function mostrarMensaje(texto, tipo = "danger") {
    mensaje.textContent = texto;
    mensaje.className = `alert alert-${tipo}`;
}

formulario.addEventListener("submit", async (event) => {
    event.preventDefault();

    mensaje.className = "alert d-none";

    const nombreUsuario = campoUsuario.value.trim();
    const password = campoPassword.value;

    if (!nombreUsuario || !password) {
        mostrarMensaje("Ingresá tu usuario y contraseña.");
        return;
    }

    botonIngresar.disabled = true;
    botonIngresar.textContent = "Ingresando...";

    // Eliminamos cualquier sesión anterior antes de intentar otra.
    sessionStorage.removeItem("turnifySesion");

    try {
        const respuesta = await fetch(`${API_URL}/api/auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nombreUsuario,
                password
            })
        });

        if (respuesta.status === 401) {
            mostrarMensaje("Usuario o contraseña incorrectos.");
            return;
        }

        if (!respuesta.ok) {
            mostrarMensaje(
                `No se pudo iniciar sesión. Código: ${respuesta.status}.`
            );
            return;
        }

        const datos = await respuesta.json();

        if (!datos.token || !datos.usuario || !datos.expiraUtc) {
            mostrarMensaje("La respuesta del servidor está incompleta.");
            return;
        }

        sessionStorage.setItem("turnifySesion", JSON.stringify({
            token: datos.token,
            expiraUtc: datos.expiraUtc,
            usuario: datos.usuario
        }));

        campoPassword.value = "";

    window.location.replace("dashboard.html");
    } catch {
        mostrarMensaje(
            "No se pudo completar el inicio de sesión. " +
            "Verificá que la API esté ejecutándose y revisá " +
            "la consola del navegador."
        );
    } finally {
        botonIngresar.disabled = false;
        botonIngresar.textContent = "Ingresar";
    }
});
