$(document).ready(function () {
    // Verificar si el usuario tiene credenciales guardadas
    if (localStorage.getItem('rememberMe') === 'true') {
        // Si el checkbox estaba seleccionado la última vez
        $("#username").val(localStorage.getItem('username'));
        $("#password").val(localStorage.getItem('password'));
        $("#rememberMe").prop('checked', true);
        $("#checkIcon").show(); // Mostrar el ícono verde de check
    }

    // ===============================
    // LOGIN SUBMIT
    // ===============================
    $("#loginForm").on("submit", function (event) {

        event.preventDefault();

        const username = $("#username").val();
        const password = $("#password").val();
        const token = $('input[name="__RequestVerificationToken"]').val();
        const rememberMe = $("#rememberMe").prop("checked");

        const data = {
            Usuario: username,
            Contrasena: password
        };

        fetch(loginUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify(data)
        })
            // ===============================
            // LEER RESPUESTA SIEMPRE (200 o 401)
            // ===============================
            .then(async response => {

                let result;

                try {
                    result = await response.json();
                } catch {
                    throw { message: "Respuesta inválida del servidor." };
                }

                // Si HTTP != 200
                if (!response.ok) {
                    return Promise.reject(result);
                }

                return result;
            })

            // ===============================
            // LOGIN OK
            // ===============================
            .then(data => {

                if (!data.success) {
                    throw data;
                }

                // Guardar JWT
                localStorage.setItem("JwtToken", data.token);

                // Guardar usuario sesión
                localStorage.setItem("userSession", JSON.stringify(data.user));

                // ===============================
                // RECORDAR CREDENCIALES
                // ===============================
                if (rememberMe) {

                    localStorage.setItem("username", username);
                    localStorage.setItem("password", password);
                    localStorage.setItem("rememberMe", true);

                    $("#checkIcon").show();
                }
                else {

                    localStorage.removeItem("username");
                    localStorage.removeItem("password");
                    localStorage.removeItem("rememberMe");

                    $("#checkIcon").hide();
                }

                // Redirigir
                window.location.href = "Usuarios";
            })

            // ===============================
            // ERROR LOGIN / SERVER
            // ===============================
            .catch(error => {

                console.error("Login error:", error);

                const mensaje =
                    error?.message ||
                    error?.Message ||
                    "Usuario o contraseña incorrectos.";

                $("#errorMessage").text(mensaje);
                $("#diverrorMessage").stop(true, true).fadeIn();

                setTimeout(() => {
                    $("#diverrorMessage").fadeOut();
                }, 3000);
            });

    });
    // Al cambiar el estado del checkbox, mostrar u ocultar el ícono
    $("#rememberMe").on("change", function () {
        var username = $("#username").val(); // Obtener el nombre de usuario
        var password = $("#password").val(); // Obtener la contraseña
        if ($(this).prop('checked')) {
            $("#checkIcon").show(); // Mostrar el ícono verde de check
            localStorage.setItem('username', username);
            localStorage.setItem('password', password);
            localStorage.setItem('rememberMe', true);
        } else {
            $("#checkIcon").hide(); // Ocultar el ícono de check
            localStorage.removeItem('username');
            localStorage.removeItem('password');
            localStorage.removeItem('rememberMe');
        }
    });
});
