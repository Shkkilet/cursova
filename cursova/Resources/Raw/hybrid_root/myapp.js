//function SendMessageToCSharp() {
//    var message = document.getElementById('messageInput').value;
//    HybridWebView.SendRawMessageToDotNet(message);
//}
const map = L.map('map').setView([50.45, 30.52], 12);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; OpenStreetMap contributors'
}).addTo(map);
const geocoder = L.Control.geocoder({
    defaultMarkGeocode: false
})
    .on('markgeocode', function (e) {
        const lat = e.geocode.center.lat;
        const lon = e.geocode.center.lng;
        const name = e.geocode.name;

        // Zoom to selected place
        map.setView([lat, lon], 14);

        // Create marker for search result
        const marker = L.marker([lat, lon]).addTo(map).bindPopup(name).openPopup();

        // Notify MAUI about selected city
        notifyHostCity(name, lat, lon);
    })
    .addTo(map);

let startMarker = null;
let endMarker = null;
let control = null;
let mode = null; // "start" | "end" | null

const btnStart = document.getElementById('btnStart');
const btnEnd = document.getElementById('btnEnd');
const btnClear = document.getElementById('btnClear');

btnStart.addEventListener('click', () => { setMode('start'); });
btnEnd.addEventListener('click', () => { setMode('end'); });
btnClear.addEventListener('click', clearAll);

function setMode(m) {
    mode = m;
    btnStart.classList.toggle('active', m === 'start');
    btnEnd.classList.toggle('active', m === 'end');
}

function clearAll() {
    if (startMarker) { map.removeLayer(startMarker); startMarker = null; }
    if (endMarker) { map.removeLayer(endMarker); endMarker = null; }
    if (control) { map.removeControl(control); control = null; }
    setMode(null);
    // notify host we cleared
    sendToHost('cleared');
}
async function reverseGeocode(lat, lon) {
    const url = `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}`;
    const res = await fetch(url);
    const data = await res.json();
    return data.display_name || "Unknown location";
}

map.on('click', async function (e) {
    const lat = e.latlng.lat;
    const lon = e.latlng.lng;

    // ќтримуЇмо назву м≥сц€ через Nominatim reverse geocoding
    let name = "Unknown location";
    try {
        const res = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}`);
        const data = await res.json();
        if (data && data.display_name) {
            name = data.display_name;
        }
    } catch (err) {
        console.error("Reverse geocode error:", err);
    }

    // ƒодаЇмо маркер на карту та передаЇмо дан≥
    if (mode === 'start') {
        if (startMarker) map.removeLayer(startMarker);
        startMarker = L.marker([lat, lon], { draggable: true }).addTo(map).bindPopup(name).openPopup();
        startMarker.on('dragend', async function (ev) {
            const p = ev.target.getLatLng();
            let newName = name;
            try {
                const r = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${p.lat}&lon=${p.lng}`);
                const d = await r.json();
                if (d && d.display_name) newName = d.display_name;
            } catch { }
            notifyHostCoords('start', p.lat, p.lng, newName);
        });
        notifyHostCoords('start', lat, lon, name);

    } else if (mode === 'end') {
        if (endMarker) map.removeLayer(endMarker);
        endMarker = L.marker([lat, lon], { draggable: true }).addTo(map).bindPopup(name).openPopup();
        endMarker.on('dragend', async function (ev) {
            const p = ev.target.getLatLng();
            let newName = name;
            try {
                const r = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${p.lat}&lon=${p.lng}`);
                const d = await r.json();
                if (d && d.display_name) newName = d.display_name;
            } catch { }
            notifyHostCoords('end', p.lat, p.lng, newName);
        });
        notifyHostCoords('end', lat, lon, name);

    } else {
        // €кщо режим не встановлено, ставимо спочатку start, пот≥м end
        if (!startMarker) {
            startMarker = L.marker([lat, lon], { draggable: true }).addTo(map).bindPopup(name).openPopup();
            startMarker.on('dragend', async function (ev) {
                const p = ev.target.getLatLng();
                let newName = name;
                try {
                    const r = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${p.lat}&lon=${p.lng}`);
                    const d = await r.json();
                    if (d && d.display_name) newName = d.display_name;
                } catch { }
                notifyHostCoords('start', p.lat, p.lng, newName);
            });
            notifyHostCoords('start', lat, lon, name);

        } else if (!endMarker) {
            endMarker = L.marker([lat, lon], { draggable: true }).addTo(map).bindPopup(name).openPopup();
            endMarker.on('dragend', async function (ev) {
                const p = ev.target.getLatLng();
                let newName = name;
                try {
                    const r = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${p.lat}&lon=${p.lng}`);
                    const d = await r.json();
                    if (d && d.display_name) newName = d.display_name;
                } catch { }
                notifyHostCoords('end', p.lat, p.lng, newName);
            });
            notifyHostCoords('end', lat, lon, name);
        }
    }

    setMode(null);
    btnStart.classList.remove('active');
    btnEnd.classList.remove('active');

    // будуЇмо маршрут €кщо обидва маркери Ї
    if (startMarker && endMarker) {
        buildRouteFromMarkers();
    }
});

function notifyMarkerMoved(type, latlng) {
    notifyHostCoords(type, latlng.lat, latlng.lng);
    if (startMarker && endMarker) buildRouteFromMarkers();
}

function notifyHostCoords(type, lat, lon, name) {
    const url =
        `invoke://set?` +
        `type=${encodeURIComponent(type)}` +
        `&lat=${encodeURIComponent(lat)}` +
        `&lon=${encodeURIComponent(lon)}` +
        `&name=${encodeURIComponent(name || "")}`;

    window.location.href = url;
}


function sendToHost(action) {
    const url = `invoke://action?action=${encodeURIComponent(action)}`;
    window.location.href = url;
}

function notifyHostCity(city, lat, lon) {
    const url = `invoke://city?name=${encodeURIComponent(city)}&lat=${lat}&lon=${lon}`;
    window.location.href = url;
}


window.buildRoute = function (startArr, endArr, stops) {
    // доступна дл€ виклику з MAUI €кщо потр≥бно
    if (control) map.removeControl(control);
    const waypoints = [L.latLng(startArr[0], startArr[1])];
    (stops || []).forEach(s => waypoints.push(L.latLng(s[0], s[1])));
    waypoints.push(L.latLng(endArr[0], endArr[1]));

    control = L.Routing.control({
        waypoints: waypoints,
        lineOptions: { styles: [{ color: 'blue', opacity: 0.6, weight: 5 }] },
        routeWhileDragging: false,
        draggableWaypoints: true
    }).addTo(map);
};

function buildRouteFromMarkers() {
    const s = startMarker.getLatLng();
    const e = endMarker.getLatLng();
    // €кщо хочете Ч ≥ тут можна викликати MAUI, але зараз ми малюЇмо маршрут на сторон≥ JS
    if (control) map.removeControl(control);
    control = L.Routing.control({
        waypoints: [L.latLng(s.lat, s.lng), L.latLng(e.lat, e.lng)],
        lineOptions: { styles: [{ color: 'blue', opacity: 0.6, weight: 5 }] },
        routeWhileDragging: false,
        draggableWaypoints: true
    }).addTo(map);
}