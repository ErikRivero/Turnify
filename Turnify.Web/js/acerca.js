import {
    obtenerSesion,
    cerrarSesion
} from "./api.js";

const sesion = obtenerSesion();

if (!sesion) {
    cerrarSesion();
} else {
    document.getElementById("nombreUsuario").textContent =
        sesion.usuario.nombreCompleto;

    document.getElementById("btnSalir")
        .addEventListener("click", cerrarSesion);
}