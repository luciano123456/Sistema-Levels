/* =========================================================
   VentasNuevoModif.js — UI PRO (Paneles + Editor)
   - Clientes (izq) => Ventas por cliente (centro) => Editor (der)
   - Tabs: Datos / Artistas / Personal / Cobros / Notas / Auditoría
   - Cálculos live: ImporteAbonado / Saldo + comisiones
   - Draft localStorage
========================================================= */

let VN = {
    clienteSel: null,
    ventaSel: null,
    clientes: [],
    ventasCliente: [],

    combos: {
        productoras: [],
        ubicaciones: [],
        monedas: [],
        estados: [],
        tiposContrato: [],
        opcionesBinarias: [],
        artistas: [],
        representantes: [],
        personal: [],
        cargos: [],
        tiposComision: [],
        cuentas: []
    },

    detalle: {
        artistas: [],
        personal: [],
        cobros: []
    }
};

// ⚠️ Ajustá endpoints acá una sola vez.
const API = {
    // VentasController
    listaClientes: "/Ventas/ListaClientes",
    listaPorCliente: (idCliente) => `/Ventas/ListaPorCliente?idCliente=${idCliente}`,
    editarInfo: (id) => `/Ventas/EditarInfo?id=${id}`,
    insertar: "/Ventas/Insertar",
    actualizar: "/Ventas/Actualizar",

    // Combos (poné los tuyos reales)
    productoras: "/Productoras/Lista",
    ubicaciones: "/Ubicaciones/Lista",
    monedas: "/PaisesMoneda/Lista",          // si tu endpoint es otro: cambiá
    estadosVenta: "/VentasEstados/Lista",    // idem
    tiposContrato: "/TiposContrato/Lista",
    opcionesBinarias: "/OpcionesBinarias/Lista",

    artistas: "/Artistas/Lista",
    representantes: "/Personal/Lista",

    personal: "/Personal/Lista",
    cargos: "/PersonalCargos/Lista",
    tiposComision: "/TiposComisiones/Lista",

    cuentas: "/MonedasCuentas/Lista" // o /Cuentas/Lista
};

// Token global (vos ya lo usás así en el proyecto)
const authHeaders = () => ({
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
});

const DRAFT_KEY = () => {
    const userId = localStorage.getItem("userId") || "default";
    return `VN_DRAFT_${userId}`;
};

/* =========================
   HELPERS
========================= */

function vnFmtMoneyARS(n) {
    try {
        const v = Math.round(Number(n || 0));
        return v.toLocaleString("es-AR", { style: "currency", currency: "ARS", minimumFractionDigits: 0, maximumFractionDigits: 0 });
    } catch { return "$ 0"; }
}
function vnToNumber(s) {
    if (s == null) return 0;
    const t = String(s).trim();
    if (!t) return 0;
    // acepta "$ 1.234,56" y "1234.56"
    const cleaned = t.replace(/[^\d,-]/g, "").replace(/\./g, "").replace(",", ".");
    const n = Number(cleaned);
    return Number.isFinite(n) ? n : 0;
}
function vnIsoLocalFromDate(d) {
    try {
        const dt = (d instanceof Date) ? d : new Date(d);
        if (isNaN(dt.getTime())) return "";
        const pad = x => String(x).padStart(2, '0');
        return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}T${pad(dt.getHours())}:${pad(dt.getMinutes())}`;
    } catch { return ""; }
}
function vnNowIsoLocal() {
    return vnIsoLocalFromDate(new Date());
}
function vnSetSaving(state, text) {
    const spin = document.getElementById("spinSaving");
    const lbl = document.getElementById("txtEstadoGuardado");
    if (spin) spin.classList.toggle("d-none", !state);
    if (lbl) lbl.textContent = text || (state ? "Guardando..." : "Listo");
}
function vnToastOk(msg) {
    if (typeof exitoModal === "function") exitoModal(msg);
    else alert(msg);
}
function vnToastErr(msg) {
    if (typeof errorModal === "function") errorModal(msg);
    else alert(msg);
}
async function vnConfirm(msg) {
    if (typeof confirmarModal === "function") return await confirmarModal(msg);
    return confirm(msg);
}

function ensureSelect2(selector, options) {
    const $el = $(selector);
    if (!$el.length) return;
    if ($el.data("select2")) $el.select2("destroy");

    $el.select2(Object.assign({
        width: "100%",
        allowClear: true,
        placeholder: "Seleccionar"
    }, options || {}));
}

async function vnFetchJson(url) {
    const r = await fetch(url, { method: "GET", headers: authHeaders() });
    if (!r.ok) throw new Error(r.statusText);
    return await r.json();
}

/* =========================
   INIT
========================= */

$(document).ready(async () => {

    // botones top
    $("#btnNuevo").on("click", () => vnNuevoDesdeCero());
    $("#btnNuevaVentaCliente").on("click", () => vnNuevoParaCliente());
    $("#btnGuardar, #btnGuardar2").on("click", () => vnGuardar());
    $("#btnGuardarDraft").on("click", () => vnGuardarDraft(true));

    $("#btnDraftRestore").on("click", () => vnRestaurarDraft());
    $("#btnDraftClear").on("click", async () => {
        const ok = await vnConfirm("¿Eliminar el borrador guardado?");
        if (!ok) return;
        localStorage.removeItem(DRAFT_KEY());
        vnToastOk("Borrador eliminado.");
    });

    $("#txtBuscarCliente").on("input", () => vnRenderClientes());

    // money input live
    $(document).on("input", ".vn-money", function () {
        // no formateo agresivo mientras escribe; solo recalc
        vnRecalcularTotales();
    });

    // draft autosave (cada 6s si hay venta activa)
    setInterval(() => {
        if (!VN.clienteSel) return;
        if (!vnPuedeEditar()) return;
        vnGuardarDraft(false);
    }, 6000);

    await vnCargarCombosBase();
    await vnCargarClientes();

    // init select2 combos del editor
    vnInitSelectsEditor();

    // Si venís con parámetros
    const init = window.VN_INIT || { id: 0, idCliente: 0 };
    const idVenta = Number(init.id || $("#Venta_Id").val() || 0);
    const idCliente = Number(init.idCliente || $("#Venta_IdCliente").val() || 0);

    if (idCliente > 0) {
        await vnSeleccionarCliente(idCliente, { scroll: true });
    }

    if (idVenta > 0) {
        await vnAbrirVenta(idVenta);
    } else {
        // si no hay venta pero hay cliente, preparar nueva
        if (idCliente > 0) vnNuevoParaCliente();
        else vnSetModo("-");
    }
});

/* =========================
   CARGA CLIENTES / VENTAS
========================= */

async function vnCargarClientes() {
    try {
        const data = await vnFetchJson(API.listaClientes);
        VN.clientes = Array.isArray(data) ? data : [];
        $("#kpiClientes").text(VN.clientes.length);
        vnRenderClientes();
    } catch (e) {
        console.error(e);
        vnToastErr("No se pudieron cargar los clientes.");
    }
}

function vnRenderClientes() {
    const cont = document.getElementById("clientesList");
    if (!cont) return;

    const q = ($("#txtBuscarCliente").val() || "").toLowerCase().trim();

    const list = VN.clientes
        .filter(x => !q || (x.Nombre || "").toLowerCase().includes(q))
        .slice(0, 1000);

    cont.innerHTML = "";

    if (!list.length) {
        cont.innerHTML = `<div class="vn-empty"><i class="fa fa-search"></i> Sin resultados.</div>`;
        return;
    }

    list.forEach(c => {
        const isActive = VN.clienteSel && Number(VN.clienteSel.Id) === Number(c.Id);
        cont.insertAdjacentHTML("beforeend", `
            <div class="vn-item ${isActive ? "active" : ""}" onclick="vnSeleccionarCliente(${c.Id})">
                <div class="title">${c.Nombre || ""}</div>
                <div class="sub">ID: ${c.Id}</div>
            </div>
        `);
    });
}

async function vnSeleccionarCliente(idCliente, opts) {
    opts = opts || {};
    const cli = VN.clientes.find(x => Number(x.Id) === Number(idCliente));
    if (!cli) return;

    VN.clienteSel = cli;
    VN.ventaSel = null;

    $("#pillClienteSel").html(`<i class="fa fa-user"></i> <span>${cli.Nombre}</span>`);
    $("#btnNuevaVentaCliente").prop("disabled", false);
    $("#btnVerCliente").prop("disabled", false);

    vnRenderClientes();
    await vnCargarVentasCliente(idCliente);

    if (opts.scroll) {
        document.querySelector(".vn-mid")?.scrollIntoView({ behavior: "smooth", block: "start" });
    }

    vnSetModo("Cliente seleccionado");
    vnEditorDisabled(true);
    vnLimpiarEditor();
}

async function vnCargarVentasCliente(idCliente) {
    try {
        const data = await vnFetchJson(API.listaPorCliente(idCliente));
        VN.ventasCliente = Array.isArray(data) ? data : [];
        vnRenderVentasCliente();
        vnActualizarKpisVentas();
    } catch (e) {
        console.error(e);
        vnToastErr("No se pudieron cargar las ventas del cliente.");
    }
}

function vnRenderVentasCliente() {
    const cont = document.getElementById("ventasList");
    if (!cont) return;

    cont.innerHTML = "";

    if (!VN.ventasCliente.length) {
        cont.innerHTML = `<div class="vn-empty"><i class="fa fa-folder-open"></i> Este cliente no tiene ventas.</div>`;
        return;
    }

    VN.ventasCliente.forEach(v => {
        const isActive = VN.ventaSel && Number(VN.ventaSel.Id) === Number(v.Id);
        const estado = v.Estado || "";
        const total = vnFmtMoneyARS(v.ImporteTotal || 0);
        const saldo = vnFmtMoneyARS(v.Saldo || 0);

        cont.insertAdjacentHTML("beforeend", `
            <div class="vn-sale ${isActive ? "active" : ""}" onclick="vnAbrirVenta(${v.Id})">
                <div class="top">
                    <div class="name">#${v.Id} • ${v.NombreEvento || ""}</div>
                    <span class="vn-badge">${estado}</span>
                </div>
                <div class="meta">
                    <span><i class="fa fa-calendar"></i> ${vnHumanDate(v.Fecha)}</span>
                    <span><i class="fa fa-money"></i> ${total}</span>
                    <span><i class="fa fa-balance-scale"></i> Saldo: ${saldo}</span>
                </div>
            </div>
        `);
    });
}

function vnActualizarKpisVentas() {
    const cant = VN.ventasCliente.length;
    let total = 0, abon = 0, saldo = 0;
    VN.ventasCliente.forEach(v => {
        total += Number(v.ImporteTotal || 0);
        abon += Number(v.ImporteAbonado || 0);
        saldo += Number(v.Saldo || 0);
    });

    $("#kpiVentas").text(cant);
    $("#kpiTotal").text(vnFmtMoneyARS(total));
    $("#kpiAbonado").text(vnFmtMoneyARS(abon));
    $("#kpiSaldo").text(vnFmtMoneyARS(saldo));
}

function vnHumanDate(fecha) {
    try {
        const d = new Date(fecha);
        if (isNaN(d.getTime())) return "";
        return d.toLocaleString("es-AR");
    } catch { return ""; }
}

/* =========================
   COMBOS BASE
========================= */

async function vnCargarCombosBase() {
    try {
        const [
            productoras, ubicaciones, monedas, estados, tiposContrato, opcionesBinarias,
            artistas, representantes, personal, cargos, tiposComision, cuentas
        ] = await Promise.all([
            vnFetchJson(API.productoras).catch(_ => []),
            vnFetchJson(API.ubicaciones).catch(_ => []),
            vnFetchJson(API.monedas).catch(_ => []),
            vnFetchJson(API.estadosVenta).catch(_ => []),
            vnFetchJson(API.tiposContrato).catch(_ => []),
            vnFetchJson(API.opcionesBinarias).catch(_ => []),

            vnFetchJson(API.artistas).catch(_ => []),
            vnFetchJson(API.representantes).catch(_ => []),

            vnFetchJson(API.personal).catch(_ => []),
            vnFetchJson(API.cargos).catch(_ => []),
            vnFetchJson(API.tiposComision).catch(_ => []),

            vnFetchJson(API.cuentas).catch(_ => [])
        ]);

        VN.combos.productoras = productoras || [];
        VN.combos.ubicaciones = ubicaciones || [];
        VN.combos.monedas = monedas || [];
        VN.combos.estados = estados || [];
        VN.combos.tiposContrato = tiposContrato || [];
        VN.combos.opcionesBinarias = opcionesBinarias || [];

        VN.combos.artistas = artistas || [];
        VN.combos.representantes = representantes || [];

        VN.combos.personal = personal || [];
        VN.combos.cargos = cargos || [];
        VN.combos.tiposComision = tiposComision || [];

        VN.combos.cuentas = cuentas || [];
    } catch (e) {
        console.error(e);
        vnToastErr("No se pudieron cargar combos base.");
    }
}

function vnInitSelectsEditor() {
    // Combos Datos
    vnFillSelect("#IdUbicacion", VN.combos.ubicaciones, "Id", "Descripcion", "Seleccionar");
    vnFillSelect("#IdProductora", VN.combos.productoras, "Id", "Nombre", "Seleccionar");
    vnFillSelect("#IdMoneda", VN.combos.monedas, "Id", "Nombre", "Seleccionar");
    vnFillSelect("#IdEstado", VN.combos.estados, "Id", "Nombre", "Seleccionar");
    vnFillSelect("#IdTipoContrato", VN.combos.tiposContrato, "Id", "Nombre", "Seleccionar");
    vnFillSelect("#IdOpExclusividad", VN.combos.opcionesBinarias, "Id", "Nombre", "—");

    ensureSelect2("#IdUbicacion");
    ensureSelect2("#IdProductora");
    ensureSelect2("#IdMoneda");
    ensureSelect2("#IdEstado");
    ensureSelect2("#IdTipoContrato");
    ensureSelect2("#IdOpExclusividad");
}

function vnFillSelect(sel, data, idField, textField, placeholder) {
    const el = document.querySelector(sel);
    if (!el) return;
    el.innerHTML = "";
    el.append(new Option(placeholder || "Seleccionar", "", true, true));
    (data || []).forEach(x => {
        el.append(new Option(x[textField] ?? x.Nombre ?? "", x[idField] ?? x.Id));
    });
}

/* =========================
   NUEVA / ABRIR VENTA
========================= */

function vnEditorDisabled(disabled) {
    $("#btnGuardar, #btnGuardar2, #btnGuardarDraft").prop("disabled", disabled);
    $("#tabDatos input, #tabDatos select, #tabNotas textarea").prop("disabled", disabled);
    // tablas: se controlan por botones add/remove
    $("#btnAddArtista, #btnAddPersonal, #btnAddCobro").prop("disabled", disabled);
}

function vnSetModo(m) {
    $("#chipModo").text(`Modo: ${m}`);
}

function vnPuedeEditar() {
    return !!VN.clienteSel;
}

function vnLimpiarEditor() {
    // datos
    $("#Venta_Id").val("0");
    $("#Fecha").val(vnNowIsoLocal());
    $("#NombreEvento").val("");
    $("#Duracion").val("");
    $("#IdUbicacion").val("").trigger("change.select2");
    $("#IdProductora").val("").trigger("change.select2");
    $("#IdMoneda").val("").trigger("change.select2");
    $("#IdEstado").val("").trigger("change.select2");
    $("#ImporteTotal").val("");
    $("#IdTipoContrato").val("").trigger("change.select2");
    $("#IdOpExclusividad").val("").trigger("change.select2");
    $("#DiasPrevios").val("");
    $("#FechaHasta").val("");

    $("#NotaInterna").val("");
    $("#NotaCliente").val("");

    // detalle
    VN.detalle.artistas = [];
    VN.detalle.personal = [];
    VN.detalle.cobros = [];

    vnRenderDetalle();
    vnRecalcularTotales();

    // auditoria
    $("#audReg").html(`<i class="fa fa-user"></i> —`);
    $("#audMod").html(`<i class="fa fa-edit"></i> —`);
}

function vnNuevoDesdeCero() {
    VN.clienteSel = null;
    VN.ventaSel = null;
    $("#pillClienteSel").html(`<i class="fa fa-user"></i> <span>Seleccioná un cliente</span>`);
    $("#btnNuevaVentaCliente").prop("disabled", true);
    $("#btnVerCliente").prop("disabled", true);

    vnRenderClientes();
    vnRenderVentasCliente();
    vnActualizarKpisVentas();

    vnSetModo("Elegí cliente");
    vnEditorDisabled(true);
    vnLimpiarEditor();
}

function vnNuevoParaCliente() {
    if (!VN.clienteSel) {
        vnToastErr("Seleccioná un cliente primero.");
        return;
    }

    VN.ventaSel = null;
    $("#Venta_Id").val("0");
    $("#Venta_IdCliente").val(String(VN.clienteSel.Id));

    vnLimpiarEditor();
    vnEditorDisabled(false);

    // defaults smart
    $("#Fecha").val(vnNowIsoLocal());
    vnSetModo(`Nueva venta • ${VN.clienteSel.Nombre}`);

    vnSetSaving(false, "Listo");
}

async function vnAbrirVenta(idVenta) {
    if (!VN.clienteSel) {
        // si no hay cliente seleccionado, intentamos inferirlo con la lista de ventas cargada (no siempre posible)
        // igual pedimos al backend y luego seteamos
    }

    try {
        vnSetSaving(true, "Cargando venta...");
        const v = await vnFetchJson(API.editarInfo(idVenta));

        VN.ventaSel = v;
        $("#Venta_Id").val(String(v.Id || 0));
        $("#Venta_IdCliente").val(String(v.IdCliente || 0));

        // si la venta trae cliente distinto, seleccionarlo
        if (!VN.clienteSel || Number(VN.clienteSel.Id) !== Number(v.IdCliente)) {
            await vnSeleccionarCliente(v.IdCliente);
        }

        // datos
        $("#Fecha").val(vnIsoLocalFromDate(v.Fecha));
        $("#NombreEvento").val(v.NombreEvento || "");
        $("#Duracion").val(vnIsoLocalFromDate(v.Duracion));
        $("#IdUbicacion").val(String(v.IdUbicacion || "")).trigger("change.select2");
        $("#IdProductora").val(String(v.IdProductora || "")).trigger("change.select2");
        $("#IdMoneda").val(String(v.IdMoneda || "")).trigger("change.select2");
        $("#IdEstado").val(String(v.IdEstado || "")).trigger("change.select2");
        $("#IdTipoContrato").val(String(v.IdTipoContrato || "")).trigger("change.select2");
        $("#IdOpExclusividad").val(v.IdOpExclusividad != null ? String(v.IdOpExclusividad) : "").trigger("change.select2");

        $("#DiasPrevios").val(v.DiasPrevios != null ? String(v.DiasPrevios) : "");
        $("#FechaHasta").val(v.FechaHasta ? vnIsoLocalFromDate(v.FechaHasta) : "");

        $("#ImporteTotal").val(String(v.ImporteTotal ?? ""));

        $("#NotaInterna").val(v.NotaInterna || "");
        $("#NotaCliente").val(v.NotaCliente || "");

        // detalle
        VN.detalle.artistas = Array.isArray(v.Artistas) ? v.Artistas : [];
        VN.detalle.personal = Array.isArray(v.Personal) ? v.Personal : [];
        VN.detalle.cobros = Array.isArray(v.Cobros) ? v.Cobros : [];

        vnRenderDetalle();
        vnRecalcularTotales();

        vnEditorDisabled(false);
        vnSetModo(`Editando #${v.Id}`);
        vnRenderVentasCliente();

        // auditoría
        if (v.UsuarioRegistra && v.FechaRegistra) {
            $("#audReg").html(`<i class="fa fa-user"></i> Registrado por <b>${v.UsuarioRegistra}</b> • <b>${vnHumanDate(v.FechaRegistra)}</b>`);
        } else {
            $("#audReg").html(`<i class="fa fa-user"></i> —`);
        }
        if (v.UsuarioModifica && v.FechaModifica) {
            $("#audMod").html(`<i class="fa fa-edit"></i> Última modificación por <b>${v.UsuarioModifica}</b> • <b>${vnHumanDate(v.FechaModifica)}</b>`);
        } else {
            $("#audMod").html(`<i class="fa fa-edit"></i> —`);
        }

        vnSetSaving(false, "Listo");
    } catch (e) {
        console.error(e);
        vnToastErr("No se pudo abrir la venta.");
        vnSetSaving(false, "Error");
    }
}

/* =========================
   DETALLE: RENDER + ACTIONS
========================= */

$("#btnAddArtista").on("click", () => {
    VN.detalle.artistas.push({
        Id: 0,
        IdArtista: 0,
        IdRepresentante: 0,
        PorcComision: 0,
        TotalComision: 0
    });
    vnRenderDetalle();
});
$("#btnAddPersonal").on("click", () => {
    VN.detalle.personal.push({
        Id: 0,
        IdPersonal: 0,
        IdCargo: 0,
        IdTipoComision: 0,
        PorcComision: 0,
        TotalComision: 0
    });
    vnRenderDetalle();
});
$("#btnAddCobro").on("click", () => {
    VN.detalle.cobros.push({
        Id: 0,
        Fecha: new Date(),
        IdMoneda: Number($("#IdMoneda").val() || 0) || 0,
        IdCuenta: 0,
        Importe: 0,
        Cotizacion: 1,
        Conversion: 0,
        NotaCliente: "",
        NotaInterna: ""
    });
    vnRenderDetalle();
    vnRecalcularTotales();
});

function vnRenderDetalle() {
    vnRenderArtistas();
    vnRenderPersonal();
    vnRenderCobros();

    $("#cntArtistas").text(`(${VN.detalle.artistas.length})`);
    $("#cntPersonal").text(`(${VN.detalle.personal.length})`);
    $("#cntCobros").text(`(${VN.detalle.cobros.length})`);
}

function vnRenderArtistas() {
    const tb = document.getElementById("tbArtistas");
    if (!tb) return;
    tb.innerHTML = "";

    VN.detalle.artistas.forEach((it, idx) => {
        tb.insertAdjacentHTML("beforeend", `
            <tr>
                <td>
                    <select class="form-select vn-input vn-mini vn-artista" data-idx="${idx}"></select>
                </td>
                <td>
                    <select class="form-select vn-input vn-mini vn-rep" data-idx="${idx}"></select>
                </td>
                <td>
                    <input class="form-control vn-input vn-mini vn-porcA" data-idx="${idx}" type="number" min="0" step="0.01" value="${it.PorcComision ?? 0}">
                </td>
                <td>
                    <input class="form-control vn-input vn-mini vn-totalA vn-money" data-idx="${idx}" type="text" value="${it.TotalComision ?? 0}">
                </td>
                <td class="text-end">
                    <button class="btn btn-outline-danger vn-btn vn-mini" onclick="vnDelArtista(${idx})">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>
        `);
    });

    // fill selects + select2
    tb.querySelectorAll("select.vn-artista").forEach(sel => {
        const idx = Number(sel.dataset.idx);
        vnFillSelectDom(sel, VN.combos.artistas, "Id", "Nombre", "Seleccionar");
        sel.value = String(VN.detalle.artistas[idx].IdArtista || "");
        $(sel).select2({ width: "100%", allowClear: true, placeholder: "Artista" })
            .on("change", function () {
                VN.detalle.artistas[idx].IdArtista = Number(this.value || 0);
            });
    });

    tb.querySelectorAll("select.vn-rep").forEach(sel => {
        const idx = Number(sel.dataset.idx);
        vnFillSelectDom(sel, VN.combos.representantes, "Id", "Nombre", "Seleccionar");
        sel.value = String(VN.detalle.artistas[idx].IdRepresentante || "");
        $(sel).select2({ width: "100%", allowClear: true, placeholder: "Representante" })
            .on("change", function () {
                VN.detalle.artistas[idx].IdRepresentante = Number(this.value || 0);
            });
    });

    tb.querySelectorAll("input.vn-porcA").forEach(inp => {
        inp.addEventListener("input", function () {
            const idx = Number(this.dataset.idx);
            VN.detalle.artistas[idx].PorcComision = Number(this.value || 0);
        });
    });
    tb.querySelectorAll("input.vn-totalA").forEach(inp => {
        inp.addEventListener("input", function () {
            const idx = Number(this.dataset.idx);
            VN.detalle.artistas[idx].TotalComision = vnToNumber(this.value);
        });
    });
}

function vnRenderPersonal() {
    const tb = document.getElementById("tbPersonal");
    if (!tb) return;
    tb.innerHTML = "";

    VN.detalle.personal.forEach((it, idx) => {
        tb.insertAdjacentHTML("beforeend", `
            <tr>
                <td><select class="form-select vn-input vn-mini vn-personal" data-idx="${idx}"></select></td>
                <td><select class="form-select vn-input vn-mini vn-cargo" data-idx="${idx}"></select></td>
                <td><select class="form-select vn-input vn-mini vn-tc" data-idx="${idx}"></select></td>
                <td><input class="form-control vn-input vn-mini vn-porcP" data-idx="${idx}" type="number" min="0" step="0.01" value="${it.PorcComision ?? 0}"></td>
                <td><input class="form-control vn-input vn-mini vn-totalP vn-money" data-idx="${idx}" type="text" value="${it.TotalComision ?? 0}"></td>
                <td class="text-end">
                    <button class="btn btn-outline-danger vn-btn vn-mini" onclick="vnDelPersonal(${idx})">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>
        `);
    });

    tb.querySelectorAll("select.vn-personal").forEach(sel => {
        const idx = Number(sel.dataset.idx);
        vnFillSelectDom(sel, VN.combos.personal, "Id", "Nombre", "Seleccionar");
        sel.value = String(VN.detalle.personal[idx].IdPersonal || "");
        $(sel).select2({ width: "100%", allowClear: true, placeholder: "Personal" })
            .on("change", function () {
                VN.detalle.personal[idx].IdPersonal = Number(this.value || 0);
            });
    });

    tb.querySelectorAll("select.vn-cargo").forEach(sel => {
        const idx = Number(sel.dataset.idx);
        vnFillSelectDom(sel, VN.combos.cargos, "Id", "Nombre", "Seleccionar");
        sel.value = String(VN.detalle.personal[idx].IdCargo || "");
        $(sel).select2({ width: "100%", allowClear: true, placeholder: "Cargo" })
            .on("change", function () {
                VN.detalle.personal[idx].IdCargo = Number(this.value || 0);
            });
    });

    tb.querySelectorAll("select.vn-tc").forEach(sel => {
        const idx = Number(sel.dataset.idx);
        vnFillSelectDom(sel, VN.combos.tiposComision, "Id", "Nombre", "Seleccionar");
        sel.value = String(VN.detalle.personal[idx].IdTipoComision || "");
        $(sel).select2({ width: "100%", allowClear: true, placeholder: "Tipo comisión" })
            .on("change", function () {
                VN.detalle.personal[idx].IdTipoComision = Number(this.value || 0);
            });
    });

    tb.querySelectorAll("input.vn-porcP").forEach(inp => {
        inp.addEventListener("input", function () {
            const idx = Number(this.dataset.idx);
            VN.detalle.personal[idx].PorcComision = Number(this.value || 0);
        });
    });
    tb.querySelectorAll("input.vn-totalP").forEach(inp => {
        inp.addEventListener("input", function () {
            const idx = Number(this.dataset.idx);
            VN.detalle.personal[idx].TotalComision = vnToNumber(this.value);
        });
    });
}

function vnRenderCobros() {
    const tb = document.getElementById("tbCobros");
    if (!tb) return;
    tb.innerHTML = "";

    VN.detalle.cobros.forEach((it, idx) => {
        const fechaIso = vnIsoLocalFromDate(it.Fecha || new Date());
        tb.insertAdjacentHTML("beforeend", `
            <tr>
                <td><input class="form-control vn-input vn-mini vn-fechaC" data-idx="${idx}" type="datetime-local" value="${fechaIso}"></td>
                <td><select class="form-select vn-input vn-mini vn-monC" data-idx="${idx}"></select></td>
                <td><select class="form-select vn-input vn-mini vn-cuentaC" data-idx="${idx}"></select></td>
                <td><input class="form-control vn-input vn-mini vn-impC vn-money" data-idx="${idx}" type="text" value="${it.Importe ?? 0}"></td>
                <td><input class="form-control vn-input vn-mini vn-cotC" data-idx="${idx}" type="number" step="0.0001" min="0" value="${it.Cotizacion ?? 1}"></td>
                <td><input class="form-control vn-input vn-mini vn-convC vn-money" data-idx="${idx}" type="text" value="${it.Conversion ?? 0}"></td>
                <td class="text-end">
                    <button class="btn btn-outline-danger vn-btn vn-mini" onclick="vnDelCobro(${idx})">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>
        `);
    });

    // selects + events
    tb.querySelectorAll("select.vn-monC").forEach(sel => {
        const idx = Number(sel.dataset.idx);
        vnFillSelectDom(sel, VN.combos.monedas, "Id", "Nombre", "Seleccionar");
        sel.value = String(VN.detalle.cobros[idx].IdMoneda || "");
        $(sel).select2({ width: "100%", allowClear: true, placeholder: "Moneda" })
            .on("change", function () {
                VN.detalle.cobros[idx].IdMoneda = Number(this.value || 0);
                vnRecalcularCobro(idx);
            });
    });

    tb.querySelectorAll("select.vn-cuentaC").forEach(sel => {
        const idx = Number(sel.dataset.idx);
        // cuentas: ajustá fields si tu endpoint devuelve distinto
        vnFillSelectDom(sel, VN.combos.cuentas, "Id", "Nombre", "Seleccionar");
        sel.value = String(VN.detalle.cobros[idx].IdCuenta || "");
        $(sel).select2({ width: "100%", allowClear: true, placeholder: "Cuenta" })
            .on("change", function () {
                VN.detalle.cobros[idx].IdCuenta = Number(this.value || 0);
            });
    });

    tb.querySelectorAll("input.vn-fechaC").forEach(inp => {
        inp.addEventListener("change", function () {
            const idx = Number(this.dataset.idx);
            VN.detalle.cobros[idx].Fecha = new Date(this.value);
        });
    });

    tb.querySelectorAll("input.vn-impC").forEach(inp => {
        inp.addEventListener("input", function () {
            const idx = Number(this.dataset.idx);
            VN.detalle.cobros[idx].Importe = vnToNumber(this.value);
            vnRecalcularCobro(idx);
        });
    });

    tb.querySelectorAll("input.vn-cotC").forEach(inp => {
        inp.addEventListener("input", function () {
            const idx = Number(this.dataset.idx);
            VN.detalle.cobros[idx].Cotizacion = Number(this.value || 1) || 1;
            vnRecalcularCobro(idx);
        });
    });

    tb.querySelectorAll("input.vn-convC").forEach(inp => {
        inp.addEventListener("input", function () {
            const idx = Number(this.dataset.idx);
            VN.detalle.cobros[idx].Conversion = vnToNumber(this.value);
            vnRecalcularTotales();
        });
    });
}

function vnFillSelectDom(el, data, idField, textField, placeholder) {
    el.innerHTML = "";
    el.append(new Option(placeholder || "Seleccionar", "", true, true));
    (data || []).forEach(x => el.append(new Option(x[textField] ?? x.Nombre ?? "", x[idField] ?? x.Id)));
}

function vnDelArtista(i) {
    VN.detalle.artistas.splice(i, 1);
    vnRenderDetalle();
}
function vnDelPersonal(i) {
    VN.detalle.personal.splice(i, 1);
    vnRenderDetalle();
}
function vnDelCobro(i) {
    VN.detalle.cobros.splice(i, 1);
    vnRenderDetalle();
    vnRecalcularTotales();
}

/* =========================
   CÁLCULOS
========================= */

function vnRecalcularCobro(idx) {
    const c = VN.detalle.cobros[idx];
    if (!c) return;

    const imp = Number(c.Importe || 0);
    const cot = Number(c.Cotizacion || 1) || 1;

    // default: Conversion = Importe si no hay cambio
    if (!c.Conversion || Number(c.Conversion) <= 0) {
        c.Conversion = imp; // tu backend también lo setea así
    } else {
        // si querés usar cotización realmente: descomentá
        // c.Conversion = Math.round((imp * cot) * 100) / 100;
    }

    vnRenderCobros(); // refresca el input conversion (simple y claro)
    vnRecalcularTotales();
}

function vnRecalcularTotales() {
    const importeTotal = vnToNumber($("#ImporteTotal").val());
    const abonado = (VN.detalle.cobros || []).reduce((a, x) => a + Number(x.Importe || 0), 0);
    const saldo = importeTotal - abonado;

    $("#lblAbonado, #lblAbonado2").text(vnFmtMoneyARS(abonado));
    $("#lblSaldo, #lblSaldo2").text(vnFmtMoneyARS(saldo));
}

/* =========================
   VALIDACIÓN
========================= */

function vnValidar() {
    let ok = true;

    const req = [
        { sel: "#Fecha", name: "Fecha" },
        { sel: "#NombreEvento", name: "Nombre evento" },
        { sel: "#IdUbicacion", name: "Ubicación" },
        { sel: "#IdProductora", name: "Productora" },
        { sel: "#IdMoneda", name: "Moneda" },
        { sel: "#IdEstado", name: "Estado" },
        { sel: "#IdTipoContrato", name: "Tipo contrato" },
        { sel: "#ImporteTotal", name: "Importe total" }
    ];

    const errores = [];

    req.forEach(r => {
        const el = document.querySelector(r.sel);
        if (!el) return;
        const val = (el.value ?? "").toString().trim();
        const isEmpty = !val || val === "0";
        el.classList.toggle("is-invalid", isEmpty);
        if (isEmpty) { ok = false; errores.push(r.name); }
    });

    if (!VN.clienteSel) {
        ok = false;
        errores.push("Cliente");
    }

    // validación mínima cobros: si hay cobros, cada uno debe tener cuenta + moneda + importe
    (VN.detalle.cobros || []).forEach((c, idx) => {
        if (Number(c.Importe || 0) <= 0) { ok = false; errores.push(`Cobro #${idx + 1} (importe)`); }
        if (Number(c.IdCuenta || 0) <= 0) { ok = false; errores.push(`Cobro #${idx + 1} (cuenta)`); }
        if (Number(c.IdMoneda || 0) <= 0) { ok = false; errores.push(`Cobro #${idx + 1} (moneda)`); }
    });

    if (!ok) {
        vnToastErr("Faltan datos obligatorios:\n- " + errores.join("\n- "));
    }

    return ok;
}

/* =========================
   MAP MODEL + GUARDAR
========================= */

function vnBuildModel() {
    const idVenta = Number($("#Venta_Id").val() || 0);

    const model = {
        Id: idVenta,

        Fecha: new Date($("#Fecha").val()),
        IdUbicacion: Number($("#IdUbicacion").val() || 0),
        NombreEvento: $("#NombreEvento").val() || "",
        Duracion: $("#Duracion").val() ? new Date($("#Duracion").val()) : new Date($("#Fecha").val()),

        IdCliente: Number(VN.clienteSel?.Id || 0),
        IdProductora: Number($("#IdProductora").val() || 0),
        IdMoneda: Number($("#IdMoneda").val() || 0),
        IdEstado: Number($("#IdEstado").val() || 0),

        ImporteTotal: vnToNumber($("#ImporteTotal").val()),
        NotaInterna: $("#NotaInterna").val() || null,
        NotaCliente: $("#NotaCliente").val() || null,

        IdTipoContrato: Number($("#IdTipoContrato").val() || 0),
        IdOpExclusividad: $("#IdOpExclusividad").val() ? Number($("#IdOpExclusividad").val()) : null,
        DiasPrevios: $("#DiasPrevios").val() ? Number($("#DiasPrevios").val()) : null,
        FechaHasta: $("#FechaHasta").val() ? new Date($("#FechaHasta").val()) : null,

        Artistas: (VN.detalle.artistas || []).map(x => ({
            Id: Number(x.Id || 0),
            IdArtista: Number(x.IdArtista || 0),
            IdRepresentante: Number(x.IdRepresentante || 0),
            PorcComision: Number(x.PorcComision || 0),
            TotalComision: Number(x.TotalComision || 0)
        })),

        Personal: (VN.detalle.personal || []).map(x => ({
            Id: Number(x.Id || 0),
            IdPersonal: Number(x.IdPersonal || 0),
            IdCargo: Number(x.IdCargo || 0),
            IdTipoComision: Number(x.IdTipoComision || 0),
            PorcComision: Number(x.PorcComision || 0),
            TotalComision: Number(x.TotalComision || 0)
        })),

        Cobros: (VN.detalle.cobros || []).map(x => ({
            Id: Number(x.Id || 0),
            Fecha: x.Fecha ? new Date(x.Fecha) : new Date(),
            IdMoneda: Number(x.IdMoneda || 0),
            IdCuenta: Number(x.IdCuenta || 0),
            Importe: Number(x.Importe || 0),
            Cotizacion: Number(x.Cotizacion || 1) || 1,
            Conversion: Number(x.Conversion || x.Importe || 0),
            NotaCliente: x.NotaCliente || null,
            NotaInterna: x.NotaInterna || null
        }))
    };

    return model;
}

async function vnGuardar() {
    if (!vnValidar()) return;

    const model = vnBuildModel();
    const isNew = Number(model.Id || 0) === 0;

    try {
        vnSetSaving(true, "Guardando...");
        const url = isNew ? API.insertar : API.actualizar;
        const method = isNew ? "POST" : "PUT";

        const r = await fetch(url, {
            method,
            headers: authHeaders(),
            body: JSON.stringify(model)
        });

        if (!r.ok) throw new Error(r.statusText);

        const data = await r.json();

        if (!data.valor) {
            // ServiceResult
            vnToastErr(data.mensaje || "No se pudo guardar.");
            vnSetSaving(false, "Error");
            return;
        }

        vnToastOk(data.mensaje || "Guardado OK");

        // refrescar listas
        await vnCargarVentasCliente(Number(model.IdCliente));
        if (data.idReferencia && Number(data.idReferencia) > 0) {
            await vnAbrirVenta(Number(data.idReferencia));
        } else {
            // por si no devuelve id
            await vnCargarVentasCliente(Number(model.IdCliente));
        }

        // si guardó ok, podés limpiar borrador
        localStorage.removeItem(DRAFT_KEY());

        vnSetSaving(false, "Guardado");
    } catch (e) {
        console.error(e);
        vnToastErr("Error guardando la venta.");
        vnSetSaving(false, "Error");
    }
}

/* =========================
   DRAFT (localStorage)
========================= */

function vnGuardarDraft(showToast) {
    try {
        if (!VN.clienteSel) return;

        const model = vnBuildModel();
        const payload = {
            ts: new Date().toISOString(),
            cliente: VN.clienteSel,
            model,
            detalle: VN.detalle
        };

        localStorage.setItem(DRAFT_KEY(), JSON.stringify(payload));
        if (showToast) vnToastOk("Borrador guardado.");
        $("#lblEstadoGuardado").removeClass("text-danger").addClass("text-muted");
        vnSetSaving(false, "Borrador guardado");
    } catch (e) {
        console.error(e);
    }
}

function vnRestaurarDraft() {
    try {
        const raw = localStorage.getItem(DRAFT_KEY());
        if (!raw) { vnToastErr("No hay borrador guardado."); return; }

        const d = JSON.parse(raw);
        if (!d || !d.cliente || !d.model) { vnToastErr("Borrador inválido."); return; }

        // seleccionar cliente
        const idCliente = Number(d.cliente.Id || 0);
        if (idCliente > 0) {
            vnSeleccionarCliente(idCliente).then(() => {
                // aplicar modelo
                vnNuevoParaCliente();

                $("#Venta_Id").val(String(d.model.Id || 0));
                $("#Fecha").val(vnIsoLocalFromDate(d.model.Fecha));
                $("#NombreEvento").val(d.model.NombreEvento || "");
                $("#Duracion").val(vnIsoLocalFromDate(d.model.Duracion));
                $("#IdUbicacion").val(String(d.model.IdUbicacion || "")).trigger("change.select2");
                $("#IdProductora").val(String(d.model.IdProductora || "")).trigger("change.select2");
                $("#IdMoneda").val(String(d.model.IdMoneda || "")).trigger("change.select2");
                $("#IdEstado").val(String(d.model.IdEstado || "")).trigger("change.select2");
                $("#IdTipoContrato").val(String(d.model.IdTipoContrato || "")).trigger("change.select2");
                $("#IdOpExclusividad").val(d.model.IdOpExclusividad != null ? String(d.model.IdOpExclusividad) : "").trigger("change.select2");
                $("#DiasPrevios").val(d.model.DiasPrevios != null ? String(d.model.DiasPrevios) : "");
                $("#FechaHasta").val(d.model.FechaHasta ? vnIsoLocalFromDate(d.model.FechaHasta) : "");
                $("#ImporteTotal").val(String(d.model.ImporteTotal ?? ""));

                $("#NotaInterna").val(d.model.NotaInterna || "");
                $("#NotaCliente").val(d.model.NotaCliente || "");

                VN.detalle = d.detalle || VN.detalle;
                vnRenderDetalle();
                vnRecalcularTotales();

                vnSetModo("Restaurado de borrador");
                vnToastOk("Borrador restaurado.");
            });
        }
    } catch (e) {
        console.error(e);
        vnToastErr("No se pudo restaurar el borrador.");
    }
}