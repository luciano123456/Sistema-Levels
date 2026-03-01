let gridUsuarios;
let personalSelectorData = [];
let personalSeleccionado = null;

const columnConfig = [
    { index: 1, filterType: 'text' },
    { index: 2, filterType: 'text' },
    { index: 3, filterType: 'text' },
    { index: 4, filterType: 'text' },
    { index: 5, filterType: 'text' },
    { index: 6, filterType: 'text' },
    { index: 7, filterType: 'select', fetchDataFunc: listaRolesFilter },
    { index: 8, filterType: 'select', fetchDataFunc: listaEstadosFilter },
    { index: 9, filterType: 'text' }, // (nota: tu tabla llega hasta 8, lo dejo por compatibilidad)
];

$(document).ready(() => {

    listaUsuarios();

    document.querySelectorAll("#modalEdicion input, #modalEdicion select, #modalEdicion textarea").forEach(el => {
        el.setAttribute("autocomplete", "off");
        el.addEventListener("input", () => validarCampoIndividual(el));
        el.addEventListener("change", () => validarCampoIndividual(el));
        el.addEventListener("blur", () => validarCampoIndividual(el));
    });

});

/* =========================
   CRUD
========================= */

function guardarCambios() {
    if (!validarCampos()) return false;

    const idUsuario = $("#txtId").val();

    const nuevoModelo = {
        "Id": idUsuario !== "" ? idUsuario : 0,
        "Usuario": $("#txtUsuario").val(),
        "Nombre": $("#txtNombre").val(),
        "Apellido": $("#txtApellido").val(),
        "DNI": $("#txtDni").val(),
        "Telefono": $("#txtTelefono").val(),
        "Direccion": $("#txtDireccion").val(),
        "IdRol": $("#Roles").val(),
        "IdEstado": $("#Estados").val(),
        "Contrasena": idUsuario === "" ? $("#txtContrasena").val() : "",
        "ContrasenaNueva": $("#txtContrasenaNueva").val(),
        "CambioAdmin": 1
    };

    const url = idUsuario === "" ? "/Usuarios/Insertar" : "/Usuarios/Actualizar";
    const method = idUsuario === "" ? "POST" : "PUT";

    fetch(url, {
        method: method,
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(nuevoModelo)
    })
        .then(r => {
            if (!r.ok) throw new Error(r.statusText);
            return r.json();
        })
        .then(dataJson => {
            let mensaje = idUsuario === "" ? "Usuario registrado correctamente" : "Usuario modificado correctamente";

            if (dataJson.valor === 'Contrasena') {
                errorModal("Contraseña incorrecta");
                return;
            }

            $('#modalEdicion').modal('hide');
            exitoModal(mensaje);
            listaUsuarios();
        })
        .catch(err => {
            console.error('Error:', err);
            errorModal("Ha ocurrido un error.");
        });
}

function nuevoUsuario() {
    limpiarModal();
    listaEstados();
    listaRoles();


    setModalSoloLectura(false); 
    $('#modalEdicion').modal('show');

    $("#btnGuardar").html(`<i class="fa fa-check"></i> Registrar`);
    $("#modalEdicionLabel").text("Nuevo Usuario");

    document.getElementById("divContrasena").removeAttribute("hidden");
    document.getElementById("divContrasenaNueva").setAttribute("hidden", "hidden");
}

async function mostrarModal(modelo) {
    limpiarModal();

    setModalSoloLectura(false);

    const campos = ["Id", "Usuario", "Nombre", "Apellido", "Dni", "Telefono", "Direccion", "Contrasena", "ContrasenaNueva"];
    campos.forEach(campo => {
        $(`#txt${campo}`).val(modelo[campo]);
    });

    await listaEstados();
    await listaRoles();

    // seleccionar rol/estado si vienen
    if (modelo.IdRol != null) $("#Roles").val(modelo.IdRol);
    if (modelo.IdEstado != null) $("#Estados").val(modelo.IdEstado);

    $('#modalEdicion').modal('show');

    $("#btnGuardar").html(`<i class="fa fa-check"></i> Guardar`);
    $("#modalEdicionLabel").text("Editar Usuario");

    document.getElementById("divContrasena").setAttribute("hidden", "hidden");
    document.getElementById("divContrasenaNueva").removeAttribute("hidden");
}

const editarUsuario = id => {
    $('.rp-actions-dropdown').hide();

    fetch("/Usuarios/EditarInfo?id=" + id, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    })
        .then(r => {
            if (!r.ok) throw new Error("Ha ocurrido un error.");
            return r.json();
        })
        .then(dataJson => {
            if (dataJson) mostrarModal(dataJson);
            else throw new Error("Ha ocurrido un error.");
        })
        .catch(_ => errorModal("Ha ocurrido un error."));
};

async function eliminarUsuario(id) {
    $('.rp-actions-dropdown').hide();

    const confirmado = await confirmarModal("¿Desea eliminar este usuario?");
    if (!confirmado) return;

    try {
        const response = await fetch("/Usuarios/Eliminar?id=" + id, {
            method: "DELETE",
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) throw new Error("Error al eliminar el Usuario.");

        const dataJson = await response.json();
        if (dataJson.valor) {
            listaUsuarios();
            exitoModal("Usuario eliminado correctamente");
        }
    } catch (e) {
        console.error("Ha ocurrido un error:", e);
        errorModal("Ha ocurrido un error.");
    }
}

/* =========================
   LISTA + DATATABLE
========================= */

async function listaUsuarios() {
    let paginaActual = gridUsuarios != null ? gridUsuarios.page() : 0;

    const response = await fetch(`/Usuarios/Lista`, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) throw new Error(`Error en la solicitud: ${response.statusText}`);

    const data = await response.json();
    await configurarDataTable(data);

    if (paginaActual > 0) {
        gridUsuarios.page(paginaActual).draw('page');
    }
}

function rpBadgeEstado(estado) {
    const s = (estado || "").toString().toLowerCase();
    if (s.includes("bloq")) return `<span class="rp-badge rp-badge-danger">Bloqueado</span>`;
    if (s.includes("acti")) return `<span class="rp-badge rp-badge-success">Activo</span>`;
    return `<span class="rp-badge rp-badge-soft">${estado || "—"}</span>`;
}

async function configurarDataTable(data) {

    if (!gridUsuarios) {

        // Header filtros clon
        $('#grd_Usuarios thead tr').clone(true).addClass('filters').appendTo('#grd_Usuarios thead');

        gridUsuarios = $('#grd_Usuarios').DataTable({
            data: data,
            language: {
                sLengthMenu: "Mostrar MENU registros",
                url: "//cdn.datatables.net/plug-ins/2.0.7/i18n/es-MX.json"
            },
            scrollX: true,
            scrollCollapse: true,

            columns: [
                {
                    data: "Id",
                    title: '',
                    width: "1%",
                    render: function (data) {
                        return renderAccionesGrid(data, {
                            ver: "verUsuario",
                            editar: "editarUsuario",
                            eliminar: "eliminarUsuario"
                        });
                    },
                    orderable: false,
                    searchable: false,
                },
                { data: 'Usuario' },
                { data: 'Nombre' },
                { data: 'Apellido' },
                { data: 'Dni' },
                { data: 'Telefono' },
                { data: 'Direccion' },
                { data: 'UsuariosRol' },
                {
                    data: 'Estado',
                    render: function (data) {
                        return rpBadgeEstado(data);
                    }
                },
            ],

            dom: 'Bfrtip',
            buttons: [
                {
                    extend: 'excelHtml5',
                    text: 'Excel',
                    filename: 'Reporte Usuarios',
                    title: '',
                    className: 'rp-dt-btn'
                },
                {
                    extend: 'pdfHtml5',
                    text: 'PDF',
                    filename: 'Reporte Usuarios',
                    title: '',
                    className: 'rp-dt-btn'
                },
                {
                    extend: 'print',
                    text: 'Imprimir',
                    title: '',
                    className: 'rp-dt-btn'
                },
                'pageLength'
            ],

            orderCellsTop: true,
            fixedHeader: true,

            initComplete: async function () {
                const api = this.api();

                // filtros por columna
                for (const config of columnConfig) {

                    // si index excede columnas existentes, ignorar
                    if (config.index > 8) continue;

                    const cell = $('.filters th').eq(config.index);

                    if (config.filterType === 'select') {

                        const select = $(`<select class="rp-filter-select" id="filter${config.index}">
                                            <option value="">Todos</option>
                                          </select>`)
                            .appendTo(cell.empty())
                            .on('change', async function () {
                                const val = $(this).val();
                                const selectedText = $(this).find('option:selected').text();

                                await api.column(config.index)
                                    .search(val ? '^' + selectedText + '$' : '', true, false)
                                    .draw();
                            });

                        const datos = await config.fetchDataFunc();
                        (datos || []).forEach(item => {
                            select.append(`<option value="${item.Id}">${item.Nombre}</option>`);
                        });

                    } else {

                        const input = $(`<input class="rp-filter-input" type="text" placeholder="Buscar...">`)
                            .appendTo(cell.empty())
                            .off('keyup change')
                            .on('keyup change', function (e) {
                                e.stopPropagation();
                                const cursorPosition = this.selectionStart;

                                api.column(config.index)
                                    .search(this.value ? this.value : '', true, false)
                                    .draw();

                                this.setSelectionRange(cursorPosition, cursorPosition);
                            });
                    }
                }

                // primer filtro vacío (acciones)
                $('.filters th').eq(0).html('');

                configurarOpcionesColumnas();

                setTimeout(() => gridUsuarios.columns.adjust(), 10);

                actualizarKpis(data)
            }
        });

    } else {
        gridUsuarios.clear().rows.add(data).draw();
    }
}

/* =========================
   ROLES / ESTADOS
========================= */

async function listaRoles() {
    const response = await fetch(`/Roles/Lista`);
    const data = await response.json();

    $('#Roles option').remove();
    const select = document.getElementById("Roles");

    // placeholder
    const op0 = document.createElement("option");
    op0.value = "";
    op0.text = "Seleccionar";
    select.appendChild(op0);

    for (let i = 0; i < data.length; i++) {
        const option = document.createElement("option");
        option.value = data[i].Id;
        option.text = data[i].Nombre;
        select.appendChild(option);
    }
}

async function listaEstados() {
    const response = await fetch(`/EstadosUsuarios/Lista`);
    const data = await response.json();

    $('#Estados option').remove();
    const select = document.getElementById("Estados");

    // placeholder
    const op0 = document.createElement("option");
    op0.value = "";
    op0.text = "Seleccionar";
    select.appendChild(op0);

    for (let i = 0; i < data.length; i++) {
        const option = document.createElement("option");
        option.value = data[i].Id;
        option.text = data[i].Nombre;
        select.appendChild(option);
    }
}

async function listaEstadosFilter() {
    const response = await fetch(`/EstadosUsuarios/Lista`);
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

async function listaRolesFilter() {
    const response = await fetch(`/Roles/Lista`);
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

/* =========================
   CONFIG COLUMNAS
========================= */

function configurarOpcionesColumnas() {
    const grid = $('#grd_Usuarios').DataTable();
    const columnas = grid.settings().init().columns;
    const container = $('#configColumnasMenu');

    const storageKey = `Usuarios_Columnas`;
    const savedConfig = JSON.parse(localStorage.getItem(storageKey)) || {};

    container.empty();

    columnas.forEach((col, index) => {
        if (col.data && col.data !== "Id") {

            const isChecked = savedConfig[`col_${index}`] !== undefined ? savedConfig[`col_${index}`] : true;
            grid.column(index).visible(isChecked);

            const name = (index === 6) ? "Direccion" : (col.data || "Col");

            container.append(`
                <li class="rp-dd-item">
                    <label class="rp-dd-label">
                        <input type="checkbox" class="toggle-column" data-column="${index}" ${isChecked ? 'checked' : ''}>
                        <span>${name}</span>
                    </label>
                </li>
            `);
        }
    });

    $('.toggle-column').on('change', function () {
        const columnIdx = parseInt($(this).data('column'), 10);
        const isChecked = $(this).is(':checked');

        savedConfig[`col_${columnIdx}`] = isChecked;
        localStorage.setItem(storageKey, JSON.stringify(savedConfig));

        grid.column(columnIdx).visible(isChecked);
    });
}

/* =========================
   ACCIONES DROPDOWN
========================= */

function toggleAcciones(id) {
    var $dropdown = $(`.acciones-menu[data-id="${id}"] .acciones-dropdown`);

    // Si está visible, lo ocultamos, si está oculto lo mostramos
    if ($dropdown.is(":visible")) {
        $dropdown.hide();
    } else {
        // Ocultar todos los dropdowns antes de mostrar el seleccionado
        
        $dropdown.show();
    }
}

$(document).on('click', function (e) {
    // Verificar si el clic está fuera de cualquier dropdown
    if (!$(e.target).closest('.acciones-menu').length) {
         // Cerrar todos los dropdowns
    }
});
/* =========================
   VALIDACIONES (TU LÓGICA)
========================= */

function limpiarModal() {
    const formulario = document.querySelector("#modalEdicion");
    if (!formulario) return;

    formulario.querySelectorAll("input, select, textarea").forEach(el => {
        if (el.tagName === "SELECT") el.selectedIndex = 0;
        else el.value = "";

        el.classList.remove("is-invalid", "is-valid");
    });

    const errorMsg = document.getElementById("errorCampos");
    if (errorMsg) errorMsg.classList.add("d-none");
}

function validarCampoIndividual(el) {

    const obligatorios = [
        "txtNombre",
        "txtUsuario",
        "txtApellido",
        "txtDni",
        "txtContrasena",
        "Roles",
        "Estados"
    ];

    if (!obligatorios.includes(el.id)) return;

    const valor = el.value ? el.value.trim() : "";
    const feedback = el.nextElementSibling;

    if (feedback && feedback.classList.contains("invalid-feedback")) {
        feedback.textContent = "Campo obligatorio";
    }

    if (valor === "" || valor === "Seleccionar" || valor === null) {
        el.classList.remove("is-valid");
        el.classList.add("is-invalid");
    } else {
        el.classList.remove("is-invalid");
        el.classList.add("is-valid");
    }

    verificarErroresGenerales();
}

function verificarErroresGenerales() {
    const errorMsg = document.getElementById("errorCampos");
    const hayInvalidos = document.querySelectorAll("#modalEdicion .is-invalid").length > 0;
    if (!errorMsg) return;

    if (!hayInvalidos) errorMsg.classList.add("d-none");
}

function validarCampos() {

    const idUsuario = $("#txtId").val();

    const campos = [
        "#txtNombre",
        "#txtUsuario",
        "#txtApellido",
        "#txtDni",
        "#Roles",
        "#Estados"
    ];

    // contraseña solo si es nuevo
    if (idUsuario === "") {
        campos.push("#txtContrasena");
    }

    let valido = true;

    campos.forEach(selector => {

        const campo = document.querySelector(selector);
        if (!campo) return;

        const valor = campo.value ? campo.value.trim() : "";
        const feedback = campo.nextElementSibling;

        if (!valor || valor === "Seleccionar") {

            campo.classList.add("is-invalid");
            campo.classList.remove("is-valid");

            if (feedback && feedback.classList.contains("invalid-feedback")) {
                feedback.textContent = "Campo obligatorio";
            }

            valido = false;

        } else {
            campo.classList.remove("is-invalid");
            campo.classList.add("is-valid");
        }
    });

    document
        .getElementById("errorCampos")
        ?.classList.toggle("d-none", valido);

    return valido;
}



function actualizarKpis(data) {
    const cant = Array.isArray(data) ? data.length : 0;
    const el = document.getElementById('kpiCantUsuarios');
    if (el) el.textContent = cant;
}

const verUsuario = id => {

    fetch("/Usuarios/EditarInfo?id=" + id, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    })
        .then(r => {
            if (!r.ok) throw new Error("Ha ocurrido un error.");
            return r.json();
        })
        .then(async dataJson => {
            if (!dataJson) throw new Error("Ha ocurrido un error.");

            await mostrarModal(dataJson);

            // Pasar a modo solo lectura
            setModalSoloLectura(true);

            document.getElementById("divContrasenaNueva").setAttribute("hidden", "hidden");

            $("#modalEdicionLabel").text("Ver Usuario");
        })
        .catch(_ => errorModal("Ha ocurrido un error."));
};





async function abrirSelectorPersonal() {

    $('#modalSelectorPersonal').modal('show');

    const r = await fetch('/Personal/Lista', {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    personalSelectorData = await r.json();

    renderPersonalSelector(personalSelectorData);
}

function renderPersonalSelector(data) {

    const container = $("#listaPersonalSelector");
    container.empty();

    data.forEach(p => {

        container.append(`
    <div class="rp-personal-card" data-id="${p.Id}">

        <div class="rp-personal-name">
            ${p.Nombre}
        </div>

        <div class="rp-personal-info">
            <i class="fa fa-id-card"></i>
            ${p.Dni ?? p.NumeroDocumento ?? "-"}
        </div>

        <div class="rp-personal-info">
            <i class="fa fa-phone"></i>
            ${p.Telefono ?? "-"}
        </div>

        <div class="rp-personal-info">
            <i class="fa fa-envelope"></i>
            ${p.Email ?? "-"}
        </div>

    </div>
`);
    });

    // selección visual
    // CLICK → solo seleccionar
    $(".rp-personal-card").on("click", function () {

        $(".rp-personal-card").removeClass("selected");

        $(this).addClass("selected");

        const id = $(this).data("id");

        personalSeleccionado =
            personalSelectorData.find(x => x.Id === id);
    });


    // DOBLE CLICK → seleccionar + aplicar
    $(".rp-personal-card").on("dblclick", function () {

        $(".rp-personal-card").removeClass("selected");
        $(this).addClass("selected");

        const id = $(this).data("id");

        personalSeleccionado =
            personalSelectorData.find(x => x.Id === id);

        aplicarPersonalSeleccionado();
    });
}

$("#buscarPersonalSelector").on("keyup", function () {

    const txt = $(this).val().toLowerCase();

    const filtrado = personalSelectorData.filter(p =>
        (p.Nombre || "").toLowerCase().includes(txt) ||
        (p.Dni || "").toLowerCase().includes(txt)
    );

    renderPersonalSelector(filtrado);
});

function aplicarPersonalSeleccionado() {

    if (!personalSeleccionado) {
        errorModal("Seleccione un personal.");
        return;
    }

    const p = personalSeleccionado;

    $("#txtNombre").val(p.Nombre ?? "");
    $("#txtDni").val(p.Dni || p.NumeroDocumento || "");
    $("#txtTelefono").val(p.Telefono ?? "");
    $("#txtDireccion").val(p.Direccion ?? "");
    $("#txtCorreo").val(p.Email ?? "");

    $('#modalSelectorPersonal').modal('hide');
}

