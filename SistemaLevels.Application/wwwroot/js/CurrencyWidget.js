/* =========================================================
   CURRENCY WIDGET GLOBAL PRO — FAST & STABLE (100% completo)
   - token es GLOBAL (NO se declara acá)
   - Pins por usuario (localStorage)
   - Máx 3 monedas
   - Drag & drop para ordenar
   - Auto-refresh configurable por usuario
   - Chequea cada 1 minuto (scheduler)
   - Actualiza SOLO si corresponde por tiempo
   - Fetch paralelo a APIs externas
   - 1 sola llamada al backend: /PaisesMoneda/ActualizarMasivo
   - ARS no se actualiza
   - Redondeo 2 decimales y comparación robusta (no “cambios” falsos)
   - Efecto up/down solo si cambia
   - Sin errores por funciones faltantes
========================================================= */

/* ==============================
   KEYS / STATE
============================== */

const userId = localStorage.getItem("userId") || "default";

const CW_KEYS = {
    PINS: "currencyWidget_" + userId,
    UPDATES: "currencyLastUpdate_" + userId,
    REFRESH_MIN: "currencyRefreshConfig_" + userId,
    MANUAL_SYNC: "currencyWidget_sync_" + userId // (solo señal, por si querés usarla)

};

const CW_DEFAULT_REFRESH_MIN = 6;
const CW_CHECK_EVERY_MS = 60 * 1000; // chequea cada 1 minuto
CW_KEYS.NEXT_UPDATE = "currencyNextUpdate_" + userId;

let monedasGlobal = [];        // viene de /PaisesMoneda/Lista
let monedasPin = [];           // array de ids
let ultimaActualizacion = {};  // { [id]: timestampMs }
let autoCheckTimer = null;
let countdownTimer = null;
let isUpdatingNow = false;
let isUpdatingCurrencies = false;
const widgetDomIndex = {};
let monedasIndex = {};
let nextUpdateTimestamp = 0;

// Multi-tab sync
const CW_CHANNEL_NAME = `cw_channel_${userId}`;
const cwChannel = ("BroadcastChannel" in window) ? new BroadcastChannel(CW_CHANNEL_NAME) : null;

const CW_LEADER_KEY = `cw_leader_${userId}`; // {tabId, ts}
const CW_TAB_ID = `${Date.now()}_${Math.random().toString(16).slice(2)}`;

let isLeaderTab = false;
let leaderHeartbeatTimer = null;
let leaderElectTimer = null;

/* ==============================
   PUBLIC API (para PaisesMoneda.js)
============================== */

window.CurrencyWidget = {
    // fuerza recarga desde server + re-render (por si actualizás desde otra pantalla)

    setRefreshMinutes: async (minutos) => {

        guardarRefreshConfig(minutos);

        // ⭐ RESET INMEDIATO DEL TIMER
        nextUpdateTimestamp = Date.now();

        localStorage.setItem(
            CW_KEYS.NEXT_UPDATE,
            String(nextUpdateTimestamp)
        );

        actualizarCountdownTexto();
        renderRefreshState();

        // ejecuta YA
        await verificarSiDebeActualizar();
    },

    refreshFromServer: async () => {
        await cargarMonedasGlobal();
        renderWidget();
    },

    // cuando PaisesMoneda.js ya tiene la lista, puede pasársela para no re-fetch
    setMonedas: (lista) => {
        if (Array.isArray(lista)) {
            monedasGlobal = lista;
            renderWidget();
        }
    },

    // cuando PaisesMoneda.js guarda una moneda, que impacte arriba (sin recargar)
    notifyMonedaUpdate: (id, nuevaCotizacion, timestampMs) => {
        const m = monedasGlobal.find(x => Number(x.Id) === Number(id));
        if (m) {
            const prev = toMoneyNumber(m.Cotizacion);
            const next = toMoneyNumber(nuevaCotizacion);

            m.Cotizacion = next;

            // si te pasan timestamp úsalo, si no, ahora
            guardarUltimaActualizacion(Number(id), timestampMs || Date.now());

            renderWidget();

            // efecto solo si cambia realmente
            if (!sameMoney(prev, next)) {
                aplicarEfectoCambio(Number(id), prev, next);
            }
        }
    },

    // util: ids pineados
    getPinnedIds: () => [...monedasPin],

    // util: forzar re-render
    render: () => renderWidget()
};

/* ==============================
   INIT
============================== */

document.addEventListener("DOMContentLoaded", async () => {
    // carga base

    nextUpdateTimestamp = obtenerProximoUpdate() || 0;

    await cargarMonedasGlobal();
    cargarPins();
    cargarUltimasActualizaciones();

    // render inicial
    renderWidget();

    // enganchar UI de refresh si existe (select + countdown)
    bindRefreshUI();

    // chequeo al cargar (si corresponde por tiempo)
    await verificarSiDebeActualizar();

    // loop de chequeo (cada 1 min)
    iniciarAutoCheck();

    // countdown visual (cada 1s)
    iniciarCountdown();
});

/* ==============================
   CONFIG REFRESH MINUTES
============================== */

function obtenerRefreshConfig() {
    const val = localStorage.getItem(CW_KEYS.REFRESH_MIN);
    const n = parseInt(val, 10);
    return Number.isFinite(n) && n > 0 ? n : CW_DEFAULT_REFRESH_MIN;
}

function guardarRefreshConfig(minutos) {
    const n = parseInt(minutos, 10);
    const safe = Number.isFinite(n) && n > 0 ? n : CW_DEFAULT_REFRESH_MIN;
    localStorage.setItem(CW_KEYS.REFRESH_MIN, String(safe));

    // re-render para actualizar el label/contador
    renderWidget();
}

/* ==============================
   FETCH MONEDAS (TU API, CON TOKEN)
============================== */

async function cargarMonedasGlobal() {

    try {
        const resp = await fetch("/PaisesMoneda/Lista", {
            headers: { "Authorization": "Bearer " + token }
        });

        if (!resp.ok) return;

        const data = await resp.json();

        if (Array.isArray(data)) {

            monedasGlobal = data;

            // ✅ INDEX O(1)
            monedasIndex = {};
            for (const m of data)
                monedasIndex[m.Id] = m;
        }

    } catch (e) {
        console.warn("CurrencyWidget cargarMonedasGlobal", e);
    }
}
/* ==============================
   LOCAL STORAGE
============================== */

function cargarPins() {
    try {
        const data = localStorage.getItem(CW_KEYS.PINS);
        const arr = data ? JSON.parse(data) : [];
        monedasPin = Array.isArray(arr) ? arr.map(x => Number(x)).filter(Boolean) : [];
    } catch {
        monedasPin = [];
    }
}

function guardarPins() {
    localStorage.setItem(CW_KEYS.PINS, JSON.stringify(monedasPin));
}

function cargarUltimasActualizaciones() {
    try {
        const data = localStorage.getItem(CW_KEYS.UPDATES);
        const obj = data ? JSON.parse(data) : {};
        ultimaActualizacion = obj && typeof obj === "object" ? obj : {};

        // 🔥 inicializa timestamps faltantes
        for (const id of monedasPin) {
            if (!ultimaActualizacion[id]) {
                ultimaActualizacion[id] = Date.now();
            }
        }

        localStorage.setItem(CW_KEYS.UPDATES, JSON.stringify(ultimaActualizacion));
    } catch {
        ultimaActualizacion = {};
    }


}

function guardarUltimaActualizacion(id, whenMs) {
    ultimaActualizacion[id] = whenMs || Date.now();
    localStorage.setItem(CW_KEYS.UPDATES, JSON.stringify(ultimaActualizacion));
}

/* ==============================
   PIN (se usa desde PaisesMoneda.js)
============================== */

function togglePin(id) {
    id = Number(id);

    if (monedasPin.includes(id)) {
        monedasPin = monedasPin.filter(x => x !== id);
    } else {
        if (monedasPin.length >= 3) {
            advertenciaModal("Máximo 3 monedas");
            return;
        }
        monedasPin.push(id);
    }

    guardarPins();

    // si estás en pantalla monedas: re-dibuja cards
    if (typeof listarMonedas === "function") {
        try { listarMonedas(); } catch { }
    }

    renderWidget();
}

// expongo para que tu botón pin lo use directo
window.togglePin = togglePin;

/* ==============================
   AUTO CHECK (cada 1 minuto)
============================== */

function iniciarAutoCheck() {
    if (autoCheckTimer) clearInterval(autoCheckTimer);

    autoCheckTimer = setInterval(async () => {
        await verificarSiDebeActualizar();
    }, CW_CHECK_EVERY_MS);
}

/* ==============================
   DECIDE SI ACTUALIZAR (según última actualización real)
============================== */

async function verificarSiDebeActualizar() {

    if (isUpdatingNow) return;
    if (!monedasPin.length) return;

    if (!nextUpdateTimestamp)
        nextUpdateTimestamp = obtenerProximoUpdate();

    if (Date.now() < nextUpdateTimestamp)
        return;

    const ids = monedasPin.filter(id => {
        const m = monedasIndex[id];
        return m && !m.Nombre?.toUpperCase().includes("ARS");
    });

    if (!ids.length) return;

    await actualizarMasivo(ids);
}
/* ==============================
   UPDATE MASIVO — FAST
   1) fetch paralelo a APIs externas
   2) arma lista {Id, Cotizacion}
   3) 1 PUT /PaisesMoneda/ActualizarMasivo
   4) actualiza memoria local + timestamps + efecto up/down
============================== */
async function actualizarMasivo(ids) {

    console.time("CW_total");

    if (isUpdatingNow) return;
    if (!Array.isArray(ids) || ids.length === 0) return;

    isUpdatingNow = true;
    isUpdatingCurrencies = true;
    renderRefreshState();

    const start = performance.now();

    try {

        // 🔥 timeout global de toda la tanda (corta SI O SI)
        const batchController = new AbortController();
        const batchTimeoutMs = 3000;
        const batchTimer = setTimeout(() => batchController.abort(), batchTimeoutMs);

        const tasks = ids.map(async (id) => {

            const m = monedasIndex[id];
            if (!m) return null;

            const url = getExternalUrlByNombre(m.Nombre);
            if (!url) return null;

            // fetchExternalRate ya tiene su timeout individual
            const next = await fetchExternalRate(url);
            if (next == null) return null;

            // batch abort (por si querés cortar todo)
            if (batchController.signal.aborted) return null;

            return { id: Number(m.Id), next: round2(next) };
        });

        let results = [];
        try {
            results = await Promise.allSettled(tasks);
        } catch {
            // si algo raro pasa, no colgar
            results = [];
        } finally {
            clearTimeout(batchTimer);
        }

        const updates = [];
        const changes = [];

        for (const r of results) {

            if (r.status !== "fulfilled" || !r.value) continue;

            const { id, next } = r.value;

            const m = monedasGlobal.find(x => Number(x.Id) === id);
            if (!m) continue;

            const prev = round2(toMoneyNumber(m.Cotizacion));

            if (sameMoney(prev, next)) {
                guardarUltimaActualizacion(id, Date.now());
                continue;
            }

            updates.push({ Id: id, Cotizacion: next });
            changes.push({ id, prev, next });
        }

        if (updates.length) {

            // backend con timeout -> no cuelga nunca
            const ok = await putActualizarMasivo(updates);

            // si backend falló, igual actualizá UI local (decisión tuya)
            // si querés NO hacerlo, comentá este bloque
            const now = Date.now();
            for (const u of updates) {
                const m = monedasGlobal.find(x => Number(x.Id) === u.Id);
                if (m) m.Cotizacion = u.Cotizacion;
                guardarUltimaActualizacion(u.Id, now);
            }

            // ✅ render 1 sola vez
            renderWidget();

            // ⭐ avisar a toda la app
            window.dispatchEvent(new CustomEvent("cw:monedasActualizadas", {
                detail: {
                    cambios: changes
                }
            }));

            // efecto (máx 3)
            for (const c of changes) {
                aplicarEfectoCambio(c.id, c.prev, c.next);
            }

        } else {
            renderWidget();
        }

    } catch (e) {
        console.warn("CurrencyWidget actualizarMasivo", e);
    } finally {

        isUpdatingNow = false;
        isUpdatingCurrencies = false;

        guardarProximoUpdate();
        renderRefreshState();

        const ms = Math.round(performance.now() - start);
        console.log("CurrencyWidget actualizarMasivo total ms:", ms);
    }
}
function guardarProximoUpdate() {

    const refreshMin = obtenerRefreshConfig();

    nextUpdateTimestamp =
        Date.now() + (refreshMin * 60000);

    localStorage.setItem(
        CW_KEYS.NEXT_UPDATE,
        String(nextUpdateTimestamp)
    );
}
function obtenerProximoUpdate() {
    const v = localStorage.getItem(CW_KEYS.NEXT_UPDATE);
    return v ? Number(v) : 0;
}

function renderRefreshState() {

    const el = document.getElementById("refreshCountdown");
    if (!el) return;

    if (isUpdatingCurrencies) {

        el.innerHTML = `
            <div class="refresh-loading">
                <div class="refresh-bar"></div>
                <span>Actualizando cotizaciones...</span>
            </div>
        `;
        return;
    }

    actualizarCountdownTexto();

    document.querySelector(".refresh-premium")
        ?.classList.toggle("updating", isUpdatingCurrencies);
}
async function putActualizarMasivo(lista) {

    const controller = new AbortController();
    const timeoutMs = 2500; // backend debe responder rápido; ajustá
    const t = setTimeout(() => controller.abort(), timeoutMs);

    try {
        const resp = await fetch("/PaisesMoneda/ActualizarMasivo", {
            method: "PUT",
            headers: {
                "Authorization": "Bearer " + token,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(lista),
            signal: controller.signal
        });

        if (!resp.ok) return false;

        const json = await resp.json();
        return !!json?.valor;

    } catch (e) {
        return false;
    } finally {
        clearTimeout(t);
    }
}
/* ==============================
   FETCH EXTERNAL API
============================== */

function getExternalUrlByNombre(nombre) {
    const n = String(nombre || "").toUpperCase().trim();

    // hardcode como pediste
    if (n.includes("DOLAR")) return "https://dolarapi.com/v1/dolares/oficial";
    if (n.includes("REAL")) return "https://dolarapi.com/v1/cotizaciones/brl";
    if (n.includes("UY")) return "https://dolarapi.com/v1/cotizaciones/uyu";

    // ARS no actualiza
    if (n.includes("ARS")) return null;

    return null;
}

async function fetchExternalRate(url) {

    const controller = new AbortController();
    const timeoutMs = 1800; // 1.8s (ajustá: 1200–2500)
    const t = setTimeout(() => controller.abort(), timeoutMs);

    try {
        const resp = await fetch(url, {
            method: "GET",
            cache: "no-store",
            signal: controller.signal
        });

        if (!resp.ok) return null;

        const data = await resp.json();

        const raw =
            data?.venta ??
            data?.price ??
            data?.rate ??
            data?.value ??
            null;

        if (raw == null) return null;

        const n = Number(raw);
        return Number.isFinite(n) ? n : null;

    } catch (e) {
        // abort / network -> null
        return null;
    } finally {
        clearTimeout(t);
    }
}

/* ==============================
   RENDER WIDGET
============================== */

function renderWidget() {



    const cont = document.getElementById("currencyWidget");
    if (!cont) return;

    // ✅ ELIMINAR pills que ya no están pineadas
    Object.keys(widgetDomIndex).forEach(id => {

        if (!monedasPin.includes(Number(id))) {

            const pill = widgetDomIndex[id];

            if (pill && pill.parentNode)
                pill.parentNode.removeChild(pill);

            delete widgetDomIndex[id];
        }
    });

    for (const id of monedasPin) {

        const m = monedasGlobal.find(x => Number(x.Id) === Number(id));
        if (!m) continue;

        let pill = widgetDomIndex[id];

        // ✅ crear SOLO una vez
        if (!pill) {

            pill = document.createElement("div");
            pill.className = "currency-pill";
            pill.draggable = true;
            pill.dataset.id = id;

            cont.appendChild(pill);
            widgetDomIndex[id] = pill;

            activarDragSingle(pill);
        }

        actualizarContenidoPill(pill, m);
    }
}

function activarDragSingle(pill) {

    pill.addEventListener("dragstart", e =>
        e.dataTransfer.setData("text/plain", pill.dataset.id)
    );

    pill.addEventListener("dragover", e => e.preventDefault());

    pill.addEventListener("drop", e => {
        e.preventDefault();
        reorderPins(
            e.dataTransfer.getData("text/plain"),
            pill.dataset.id
        );
    });
}

function reorderPins(draggedId, targetId) {

    draggedId = Number(draggedId);
    targetId = Number(targetId);

    const fromIndex = monedasPin.indexOf(draggedId);
    const toIndex = monedasPin.indexOf(targetId);

    if (fromIndex === -1 || toIndex === -1) return;

    // ordenar array
    monedasPin.splice(fromIndex, 1);
    monedasPin.splice(toIndex, 0, draggedId);

    guardarPins();

    // ✅ mover nodos reales del DOM
    const cont = document.getElementById("currencyWidget");
    if (!cont) return;

    monedasPin.forEach(id => {
        const pill = widgetDomIndex[id];
        if (pill) cont.appendChild(pill);
    });
}

function actualizarContenidoPill(pill, m) {

    // cache de refs por pill
    if (!pill._cw) {
        pill.innerHTML = `
            <div class="currency-badge"></div>
            <div class="currency-content">
                <span class="currency-name"></span>
                <span class="currency-value"></span>
                <span class="currency-updated"></span>
            </div>
        `;

        pill._cw = {
            badge: pill.querySelector(".currency-badge"),
            name: pill.querySelector(".currency-name"),
            value: pill.querySelector(".currency-value"),
            updated: pill.querySelector(".currency-updated"),
        };
    }

    const nombre = String(m.Nombre || "");
    pill._cw.badge.textContent = nombre.substring(0, 2).toUpperCase();
    pill._cw.name.textContent = nombre;
    pill._cw.value.textContent = format2(m.Cotizacion);
    pill._cw.updated.textContent = obtenerTextoActualizacion(m.Id);
}

window.CurrencyWidgetManualRefresh = async function (id) {
    // manual: actualiza solo esa, pero usa el mismo flujo masivo para no duplicar lógica
    await actualizarMasivo([Number(id)]);
};

/* ==============================
   FECHA / TEXTO ACTUALIZACIÓN
============================== */

function obtenerTextoActualizacion(id) {
    const ts = Number(ultimaActualizacion[id] || 0);
    if (!ts) return "Sin actualizar";

    const d = new Date(ts);
    const hh = String(d.getHours()).padStart(2, "0");
    const mm = String(d.getMinutes()).padStart(2, "0");
    return `Actualizado ${hh}:${mm}`;
}

/* ==============================
   DRAG & DROP ORDENAMIENTO
============================== */

function activarDrag() {
    const pills = document.querySelectorAll(".currency-pill");
    if (!pills.length) return;

    pills.forEach(pill => {
        pill.addEventListener("dragstart", (e) => {
            e.dataTransfer.setData("text/plain", pill.dataset.id);
        });

        pill.addEventListener("dragover", (e) => e.preventDefault());

        pill.addEventListener("drop", (e) => {
            e.preventDefault();

            const draggedId = Number(e.dataTransfer.getData("text/plain"));
            const targetId = Number(pill.dataset.id);

            const fromIndex = monedasPin.indexOf(draggedId);
            const toIndex = monedasPin.indexOf(targetId);

            if (fromIndex === -1 || toIndex === -1) return;

            monedasPin.splice(fromIndex, 1);
            monedasPin.splice(toIndex, 0, draggedId);

            guardarPins();
            renderWidget();
        });
    });
}

/* ==============================
   EFECTO UP / DOWN (solo cuando cambia)
============================== */

function aplicarEfectoCambio(id, anterior, nuevo) {
    const pill = document.querySelector(`.currency-pill[data-id='${id}']`);
    if (!pill) return;

    const valueEl = pill.querySelector(".currency-value");
    if (!valueEl) return;

    pill.classList.remove("up", "down");
    valueEl.classList.remove("value-up", "value-down");


    // limpiar icono anterior si quedó
    const oldIcon = valueEl.querySelector("i");
    if (oldIcon) oldIcon.remove();

    if (nuevo > anterior) {
        pill.classList.add("up");
        valueEl.classList.add("value-up");
        valueEl.insertAdjacentHTML("beforeend", ` <i class="fa fa-arrow-up"></i>`);
    } else if (nuevo < anterior) {
        pill.classList.add("down");
        valueEl.classList.add("value-down");
        valueEl.insertAdjacentHTML("beforeend", ` <i class="fa fa-arrow-down"></i>`);
    } else {
        return;
    }

    setTimeout(() => {
        pill.classList.remove("up", "down");
        valueEl.classList.remove("value-up", "value-down");
        const ic = valueEl.querySelector("i");
        if (ic) ic.remove();
    }, 2000);
}

/* ==============================
   COUNTDOWN (Actualiza en mm:ss)
   - usa la próxima moneda más “cercana” a necesitar update
   - no queda en 0 clavado
============================== */

function iniciarCountdown() {

    if (countdownTimer)
        clearInterval(countdownTimer);

    countdownTimer = setInterval(() => {

        if (isUpdatingCurrencies) {
            renderRefreshState();
            return;
        }

        actualizarCountdownTexto();

    }, 1000);
}

function actualizarCountdownTexto() {

    const el = document.getElementById("refreshCountdown");
    if (!el) return;

    if (isUpdatingCurrencies) return;

    const nextTs = obtenerProximoUpdate();

    if (!nextTs) {
        el.textContent = "";
        return;
    }

    const remaining = nextTs - Date.now();

    if (remaining <= 0 && !isUpdatingNow) {

        el.textContent = "Actualizando...";

        // ⭐ dispara update inmediatamente
        verificarSiDebeActualizar();

        return;
    }
    const totalSeconds = Math.floor(remaining / 1000);

    const mm = Math.floor(totalSeconds / 60);
    const ss = totalSeconds % 60;

    el.textContent =
        `Actualiza en ${mm.toString().padStart(2, '0')}:${ss.toString().padStart(2, '0')}`;
}

function bindRefreshUI() {
    const sel = document.getElementById("currencyRefreshSelect");
    if (!sel) return;

    // si no tiene opciones, las crea (más opciones, no solo hasta 15)
    if (sel.options.length === 0) {
        const opts = [1, 2, 3, 5, 6, 10, 15, 20, 30, 45, 60];
        for (const m of opts) {
            const o = document.createElement("option");
            o.value = String(m);
            o.textContent = `${m} min`;
            sel.appendChild(o);
        }
    }

    // set valor guardado
    const current = obtenerRefreshConfig();
    sel.value = String(current);

    sel.addEventListener("change", async () => {
        guardarRefreshConfig(sel.value);

        // al cambiar config, re-evaluo si ya venció y debo actualizar
        await verificarSiDebeActualizar();
    });

    actualizarRefreshUILabel();
}

function actualizarRefreshUILabel() {
    // si querés mostrar el “1 min” al lado, usá #refreshLabel (opcional)
    const lbl = document.getElementById("refreshLabel");
    if (!lbl) return;
    lbl.textContent = `${obtenerRefreshConfig()} min`;
}

/* ==============================
   HELPERS (números / formato / comparación)
============================== */

function toMoneyNumber(v) {
    // acepta number o string tipo "1.234,56"
    if (v == null) return 0;

    if (typeof v === "number") return Number.isFinite(v) ? v : 0;

    const s = String(v).trim();
    if (!s) return 0;

    // normaliza: quita miles y usa punto decimal
    // "1.234,56" => "1234.56"
    const normalized = s.replace(/\./g, "").replace(",", ".");
    const n = Number(normalized);
    return Number.isFinite(n) ? n : 0;
}

function round2(n) {
    n = Number(n);
    if (!Number.isFinite(n)) return 0;
    return Math.round((n + Number.EPSILON) * 100) / 100;
}

function sameMoney(a, b) {
    return round2(a) === round2(b);
}

function format2(v) {
    const n = round2(toMoneyNumber(v));
    return n.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function escapeHtml(str) {
    return String(str)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

    function readLeader() {
  try { return JSON.parse(localStorage.getItem(CW_LEADER_KEY) || "null"); }
  catch { return null; }
}

function writeLeader(obj) {
  localStorage.setItem(CW_LEADER_KEY, JSON.stringify(obj));
}

function electLeader() {
  const now = Date.now();
  const leader = readLeader();

  // si no hay líder o está “muerto” (sin latido)
  if (!leader || (now - Number(leader.ts || 0)) > 3500) {
    writeLeader({ tabId: CW_TAB_ID, ts: now });
  }

  const cur = readLeader();
  isLeaderTab = !!cur && cur.tabId === CW_TAB_ID;

  if (isLeaderTab) startLeaderHeartbeat();
  else stopLeaderHeartbeat();
}

function startLeaderHeartbeat() {
  if (leaderHeartbeatTimer) return;

  leaderHeartbeatTimer = setInterval(() => {
    // reafirma liderazgo
    writeLeader({ tabId: CW_TAB_ID, ts: Date.now() });
  }, 1000);
}

function stopLeaderHeartbeat() {
  if (leaderHeartbeatTimer) {
    clearInterval(leaderHeartbeatTimer);
    leaderHeartbeatTimer = null;
  }
}

function initLeaderElection() {
  electLeader();

  // re-elección periódica
  if (leaderElectTimer) clearInterval(leaderElectTimer);
  leaderElectTimer = setInterval(electLeader, 2000);

  // si cambia leader en otra pestaña
  window.addEventListener("storage", (e) => {
    if (e.key === CW_LEADER_KEY) electLeader();
  });

  // al cerrar, si eras líder, liberás
  window.addEventListener("beforeunload", () => {
    const leader = readLeader();
    if (leader?.tabId === CW_TAB_ID) localStorage.removeItem(CW_LEADER_KEY);
  });
}