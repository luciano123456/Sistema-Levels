/* =========================================================
   UBICACIONES.JS — VERSION FINAL COMPLETA
========================================================= */

let gridUbicaciones;
let filtrosUbicacionesActivos = false;

/* =========================================================
   CONFIG COLUMN FILTERS
========================================================= */

const columnConfig = [
    { index: 1, filterType: 'text' }, // Descripción
    { index: 2, filterType: 'text' }, // Espacio
    { index: 3, filterType: 'text' }, // Dirección
];

/* =========================================================
   INIT
========================================================= */

$(document).ready(function () {

    listaUbicaciones();

    // validación campo a campo
    document.querySelectorAll("#modalEdicion input, #modalEdicion textarea").forEach(el => {
        el.setAttribute("autocomplete", "off");
        el.addEventListener("input", () => validarCampoIndividual(el));
        el.addEventListener("blur", () => validarCampoIndividual(el));
    });

});

/* =========================================================
   CRUD
========================================================= */

function nuevoUbicacion() {

    limpiarModal();
    setModalSoloLectura(false);

    $('#modalEdicion').modal('show');

    $("#btnGuardar")
        .attr("onclick", "guardarUbicacion()")
        .html(`<i class="fa fa-check"></i> Registrar`)
        .show();

    $("#modalEdicionLabel").text("Nueva Ubicación");
}

function guardarUbicacion() {

    if (!validarCampos())
        return false;

    const id = $("#txtId").val();

    const modelo = {
        Id: id !== "" ? parseInt(id, 10) : 0,
        Descripcion: $("#txtDescripcion").val(),
        Espacio: $("#txtEspacio").val(),
        Direccion: $("#txtDireccion").val()
    };

    const esNuevo = id === "";

    const url = esNuevo
        ? "/Ubicaciones/Insertar"
        : "/Ubicaciones/Actualizar";

    const method = esNuevo ? "POST" : "PUT";

    fetch(url, {
        method,
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(modelo)
    })
        .then(async r => {
            if (!r.ok) throw new Error("Error HTTP");
            return await r.json();
        })
        .then(data => {

            if (!data.valor) {

                mostrarErrorCampos(
                    data.mensaje,
                    data.idReferencia,
                    data.tipo || "validacion"
                );

                return;
            }

            cerrarErrorCampos();
            $('#modalEdicion').modal('hide');

            exitoModal(
                data.mensaje ||
                (esNuevo
                    ? "Ubicación registrada correctamente"
                    : "Ubicación modificada correctamente")
            );

            listaUbicaciones();
        })
        .catch(err => {
            console.error(err);
            errorModal("Ha ocurrido un error.");
        });
}

const editarUbicacion = id => {
    fetch("/Ubicaciones/EditarInfo?id=" + id, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    })
        .then(r => {
            if (!r.ok) throw new Error("Error");
            return r.json();
        })
        .then(data => {

            if (!data)
                throw new Error("Error");

            limpiarModal();
            setModalSoloLectura(false);

            $("#txtId").val(data.Id || "");
            $("#txtDescripcion").val(data.Descripcion || "");
            $("#txtEspacio").val(data.Espacio || "");
            $("#txtDireccion").val(data.Direccion || "");

            $('#modalEdicion').modal('show');

            $("#btnGuardar")
                .attr("onclick", "guardarUbicacion()")
                .html(`<i class="fa fa-check"></i> Guardar`)
                .show();

            $("#modalEdicionLabel").text("Editar Ubicación");
        })
        .catch(_ => errorModal("Ha ocurrido un error."));
};

async function eliminarUbicacion(id) {

    const confirmado = await confirmarModal("¿Desea eliminar esta ubicación?");
    if (!confirmado) return;

    try {

        const response = await fetch("/Ubicaciones/Eliminar?id=" + id, {
            method: "DELETE",
            headers: {
                'Authorization': 'Bearer ' + token
            }
        });

        if (!response.ok)
            throw new Error("Error HTTP");

        const data = await response.json();

        if (!data.valor) {
            mostrarErrorCampos(data.mensaje, data.idReferencia);
            return;
        }

        listaUbicaciones();
        exitoModal(data.mensaje || "Ubicación eliminada correctamente");

    } catch (e) {
        errorModal("Ha ocurrido un error.");
    }
}

/* =========================================================
   VER FICHA
========================================================= */

const verFicha = id => {

    fetch("/Ubicaciones/EditarInfo?id=" + id, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
        .then(r => {
            if (!r.ok) throw new Error("Error");
            return r.json();
        })
        .then(async data => {

            if (!data)
                throw new Error("Error");

            limpiarModal();

            $("#txtId").val(data.Id || "");
            $("#txtDescripcion").val(data.Descripcion || "");
            $("#txtEspacio").val(data.Espacio || "");
            $("#txtDireccion").val(data.Direccion || "");

            $('#modalEdicion').modal('show');

            setModalSoloLectura(true);
            $("#btnGuardar").hide();

            $("#modalEdicionLabel").text("Ver Ubicación");
        })
        .catch(_ => errorModal("Ha ocurrido un error."));
};

/* =========================================================
   LISTA + DATATABLE
========================================================= */

async function listaUbicaciones() {

    let paginaActual = gridUbicaciones != null ? gridUbicaciones.page() : 0;

    const response = await fetch(`/Ubicaciones/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    if (!response.ok)
        return errorModal("Error cargando datos");

    const data = await response.json();
    await configurarDataTable(data);

    if (paginaActual > 0)
        gridUbicaciones.page(paginaActual).draw('page');
}

async function configurarDataTable(data) {

    if (!gridUbicaciones) {

        const $thead = $('#grd_Ubicaciones thead');

        if ($thead.find('tr.filters').length === 0) {
            $thead.find('tr').first().clone(true).addClass('filters').appendTo($thead);
        }

        gridUbicaciones = $('#grd_Ubicaciones').DataTable({

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
                            ver: "verFicha",
                            editar: "editarUbicacion",
                            eliminar: "eliminarUbicacion"
                        });
                    },
                    orderable: false,
                    searchable: false
                },
                { data: 'Descripcion' },
                { data: 'Espacio' },
                { data: 'Direccion' }
            ],

            dom: 'Bfrtip',
            buttons: [
                { text: 'Excel', action: () => abrirModalExportacion(gridUbicaciones, 'excel', 'Ubicaciones') },
                { text: 'PDF', action: () => abrirModalExportacion(gridUbicaciones, 'pdf', 'Ubicaciones') },
                { text: 'Imprimir', action: () => abrirModalExportacion(gridUbicaciones, 'print', 'Ubicaciones') },
                'pageLength'
            ],

            orderCellsTop: true,
            fixedHeader: true,

            initComplete: function () {

                const api = this.api();

                columnConfig.forEach(cfg => {

                    const cell = $('.filters th').eq(cfg.index);
                    cell.empty();

                    $('<input class="rp-filter-input" type="text" placeholder="Buscar...">')
                        .appendTo(cell)
                        .on('keyup change', function () {
                            api.column(cfg.index).search(this.value).draw(false);
                        });
                });

                $('.filters th').eq(0).html('');

                configurarOpcionesColumnas();
                actualizarKpis(data);
            }
        });

    } else {
        gridUbicaciones.clear().rows.add(data).draw();
        actualizarKpis(data);
    }
}

/* =========================================================
   CONFIG COLUMNAS
========================================================= */

function configurarOpcionesColumnas() {

    const grid = $('#grd_Ubicaciones').DataTable();
    const columnas = grid.settings().init().columns;
    const container = $('#configColumnasMenu');

    const storageKey = `Ubicaciones_Columnas`;
    const savedConfig = JSON.parse(localStorage.getItem(storageKey)) || {};

    container.empty();

    columnas.forEach((col, index) => {

        if (col.data && col.data !== "Id") {

            const isChecked = savedConfig[`col_${index}`] !== undefined ? savedConfig[`col_${index}`] : true;
            grid.column(index).visible(isChecked);

            const name = $('#grd_Ubicaciones thead tr').first().find('th').eq(index).text();

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

    $('.toggle-column').off("change").on('change', function () {

        const columnIdx = parseInt($(this).data('column'), 10);
        const isChecked = $(this).is(':checked');

        savedConfig[`col_${columnIdx}`] = isChecked;
        localStorage.setItem(storageKey, JSON.stringify(savedConfig));

        grid.column(columnIdx).visible(isChecked);
    });
}

/* =========================================================
   VALIDACIONES
========================================================= */

function setEstadoCampo(el, esValido) {

    el.classList.toggle("is-invalid", !esValido);
    el.classList.toggle("is-valid", esValido);

    const $wrap = $(el).closest(".mb-3, .form-group, .col, .col-md-6, .rp-field, .rp-form-group");
    const $msg = $wrap.find(".invalid-feedback").first();

    if ($msg.length)
        $msg.toggleClass("d-none", esValido);
}

function validarCampoIndividual(el) {

    const obligatorios = [
        "txtDescripcion",
        "txtEspacio",
        "txtDireccion"
    ];

    if (!obligatorios.includes(el.id))
        return;

    const valor = (el.value ?? "").toString().trim();
    const esValido = valor !== "";

    setEstadoCampo(el, esValido);
}

function validarCampos() {

    const campos = [
        { selector: "#txtDescripcion", nombre: "Descripción" },
        { selector: "#txtEspacio", nombre: "Espacio" },
        { selector: "#txtDireccion", nombre: "Dirección" }
    ];

    let errores = [];

    campos.forEach(c => {

        const el = document.querySelector(c.selector);
        if (!el) return;

        const valor = (el.value ?? "").toString().trim();
        const esValido = valor !== "";

        setEstadoCampo(el, esValido);

        if (!esValido)
            errores.push(c.nombre);
    });

    if (errores.length > 0) {

        mostrarErrorCampos(
            `Debes completar los campos requeridos:<br>
             <strong>${errores.join(", ")}</strong>`,
            null,
            "validacion"
        );

        return false;
    }

    cerrarErrorCampos();
    return true;
}

function limpiarModal() {

    const formulario = document.querySelector("#modalEdicion");
    if (!formulario) return;

    formulario.querySelectorAll("input, textarea").forEach(el => {

        el.value = "";
        el.classList.remove("is-invalid", "is-valid");
    });

    $("#errorCampos").addClass("d-none");
}

/* =========================================================
   KPI
========================================================= */

function actualizarKpis(data) {
    const cant = Array.isArray(data) ? data.length : 0;
    $('#kpiCantUbicaciones').text(cant);
}