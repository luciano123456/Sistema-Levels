/* ==========================================
   VENTAS INDEX PRO
========================================== */

let VI = {
    clientes: [],
    ventas: [],
    ventasOriginal: [],
    clienteSel: null
};

const API = {
    clientes: "/Ventas/ListaClientes",
    ventas: "/Ventas/Lista",
    ventasPorCliente: id => `/Ventas/ListaPorCliente?idCliente=${id}`
};

const authHeaders = () => ({
    'Authorization': 'Bearer ' + token
});

/* =========================
   INIT
========================= */

$(document).ready(async () => {

    $("#btnRefresh").on("click", cargarTodo);
    $("#btnNuevaVenta").on("click", () => {
        window.location = "/Ventas/NuevoModif";
    });

    $("#txtBuscarCliente").on("input", renderClientes);
    $("#txtBuscarVenta").on("input", renderVentas);

    await cargarTodo();
});

/* =========================
   LOAD
========================= */

async function cargarTodo() {

    await Promise.all([
        cargarClientes(),
        cargarVentas()
    ]);
}

async function cargarClientes() {
    const r = await fetch(API.clientes, { headers: authHeaders() });
    VI.clientes = await r.json();
    $("#kpiClientes").text(VI.clientes.length);
    renderClientes();
}

async function cargarVentas() {
    const r = await fetch(API.ventas, { headers: authHeaders() });
    VI.ventas = await r.json();
    VI.ventasOriginal = [...VI.ventas];
    renderVentas();
    actualizarKpis();
}

/* =========================
   CLIENTES
========================= */

function renderClientes() {

    const cont = $("#clientesList");
    cont.html("");

    const q = $("#txtBuscarCliente").val().toLowerCase();

    VI.clientes
        .filter(x => !q || x.Nombre.toLowerCase().includes(q))
        .forEach(c => {

            const active = VI.clienteSel?.Id === c.Id ? "active" : "";

            cont.append(`
                <div class="vi-item ${active}"
                     onclick="seleccionarCliente(${c.Id})">
                    <b>${c.Nombre}</b>
                </div>
            `);
        });
}

async function seleccionarCliente(id) {

    if (VI.clienteSel && VI.clienteSel.Id === id) {
        // toggle OFF
        VI.clienteSel = null;
        $("#lblFiltroCliente").text("Todos los clientes");
        VI.ventas = [...VI.ventasOriginal];
        renderClientes();
        renderVentas();
        actualizarKpis();
        return;
    }

    VI.clienteSel = VI.clientes.find(x => x.Id === id);

    $("#lblFiltroCliente")
        .text(VI.clienteSel.Nombre);

    renderClientes();

    const r = await fetch(API.ventasPorCliente(id),
        { headers: authHeaders() });

    VI.ventas = await r.json();

    renderVentas();
    actualizarKpis();
}

/* =========================
   VENTAS
========================= */

function renderVentas() {

    const cont = $("#ventasList");
    cont.html("");

    const q = $("#txtBuscarVenta").val().toLowerCase();

    const list = VI.ventas.filter(v =>
        !q ||
        (v.NombreEvento || "").toLowerCase().includes(q)
    );

    if (!list.length) {
        cont.html(`<div class="text-center opacity-75 mt-3">
            Sin ventas
        </div>`);
        return;
    }

    list.forEach(v => {

        cont.append(`
            <div class="vi-sale"
                 onclick="abrirVenta(${v.Id})">

                <div class="top">
                    <div class="name">
                        #${v.Id} • ${v.NombreEvento}
                    </div>
                    <div>${v.Estado || ""}</div>
                </div>

                <div class="meta">
                    ${fmtDate(v.Fecha)}
                    • ${fmtMoney(v.ImporteTotal)}
                    • Saldo ${fmtMoney(v.Saldo)}
                </div>

            </div>
        `);
    });
}

/* =========================
   KPIS
========================= */

function actualizarKpis() {

    let total = 0, abon = 0, saldo = 0;

    VI.ventas.forEach(v => {
        total += Number(v.ImporteTotal || 0);
        abon += Number(v.ImporteAbonado || 0);
        saldo += Number(v.Saldo || 0);
    });

    $("#kpiVentas").text(VI.ventas.length);
    $("#kpiTotal").text(fmtMoney(total));
    $("#kpiAbonado").text(fmtMoney(abon));
    $("#kpiSaldo").text(fmtMoney(saldo));
}

/* =========================
   HELPERS
========================= */

function abrirVenta(id) {
    window.location = `/Ventas/NuevoModif?id=${id}`;
}

function fmtMoney(n) {
    return Number(n || 0).toLocaleString("es-AR",
        { style: "currency", currency: "ARS", maximumFractionDigits: 0 });
}

function fmtDate(d) {
    try {
        return new Date(d).toLocaleDateString("es-AR");
    } catch { return ""; }
}