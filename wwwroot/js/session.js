window.aresSession = {
    iniciarSesion: async function (usuario, password) {
        try {
            const respuesta = await fetch("/session/login", {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    Nombre_usuario: usuario,
                    Password: password
                })
            });

            const texto = await respuesta.text();

            let datos = {};

            try {
                datos = texto ? JSON.parse(texto) : {};
            } catch {
                datos = { mensaje: texto };
            }

            return {
                correcto: respuesta.ok,
                estado: respuesta.status,
                mensaje: datos.mensaje || datos.message || ""
            };
        } catch (error) {
            return {
                correcto: false,
                estado: 0,
                mensaje: "No se pudo conectar con WebEmpresa: " +
                         error.message
            };
        }
    }
};