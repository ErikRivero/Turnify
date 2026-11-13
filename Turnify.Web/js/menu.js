const contenedorMenu = document.getElementById("menuPrincipal");

if (contenedorMenu) {
    contenedorMenu.innerHTML = `
        <nav class="container py-3" aria-label="Navegación principal">
            <ul class="nav gap-2 align-items-center">
                <li class="nav-item">
                    <a class="nav-link" href="dashboard.html">
                        Dashboard
                    </a>
                </li>

                <li class="nav-item dropdown">
                    <button
                        id="menuSoporte"
                        class="nav-link dropdown-toggle"
                        type="button"
                        data-bs-toggle="dropdown"
                        aria-expanded="false">
                        Soporte
                    </button>

                    <ul class="dropdown-menu dropdown-menu-dark">
                        <li>
                            <a class="dropdown-item" href="servicios.html">
                                Servicios
                            </a>
                        </li>
                        <li>
                            <a class="dropdown-item" href="clientes.html">
                                Clientes
                            </a>
                        </li>
                        <li>
                            <a class="dropdown-item" href="barberos.html">
                                Barberos
                            </a>
                        </li>
                    </ul>
                </li>

                <li class="nav-item">
                    <a class="nav-link" href="turnos.html">
                        Transacción: Turnos
                    </a>
                </li>

                <li class="nav-item">
                    <a class="nav-link" href="acerca.html">
                        Acerca de
                    </a>
                </li>
            </ul>
        </nav>
    `;

    const paginaActual = window.location.pathname.split("/").pop();

    contenedorMenu.querySelectorAll("a").forEach(enlace => {
        if (enlace.getAttribute("href") === paginaActual) {
            enlace.classList.add("active");
            enlace.setAttribute("aria-current", "page");
        }
    });

    const paginasSoporte = [
        "servicios.html",
        "clientes.html",
        "barberos.html"
    ];

    if (paginasSoporte.includes(paginaActual)) {
        document.getElementById("menuSoporte").classList.add("active");
    }
}