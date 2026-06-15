let map;
let markers = new Map();
let currentDotNetHelper = null;

window.checkLeafletLoaded = () => {
    return typeof L !== 'undefined';
};

window.loadLeaflet = () => {
    return new Promise((resolve, reject) => {
        if (typeof L !== 'undefined') {
            resolve();
            return;
        }
        
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
        document.head.appendChild(link);
        
        const script = document.createElement('script');
        script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
        script.onload = () => {
            setTimeout(resolve, 100);
        };
        script.onerror = reject;
        document.body.appendChild(script);
    });
};

window.initMap = async (dotNetHelper, tripId, points) => {
    console.log("initMap called");
    currentDotNetHelper = dotNetHelper;
    
    let retries = 0;
    while (typeof L === 'undefined' && retries < 20) {
        await new Promise(r => setTimeout(r, 100));
        retries++;
        console.log(`Waiting for Leaflet... attempt ${retries}`);
    }
    
    if (typeof L === 'undefined') {
        console.error("Leaflet failed to load!");
        return;
    }
    
    console.log("Leaflet loaded, creating map...");
    
    let centerLat = 52.2297;
    let centerLng = 21.0122;
    
    if (points && points.length > 0) {
        centerLat = points[0].latitude;
        centerLng = points[0].longitude;
    }
    
    const mapElement = document.getElementById('map');
    if (!mapElement) {
        console.error("Map element not found!");
        return;
    }
    
    map = L.map('map').setView([centerLat, centerLng], 12);
    
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);
    
    if (points) {
        points.forEach(point => {
            addMarker(point);
        });
    }
    
    map.on('click', async (e) => {
        if (currentDotNetHelper) {
            await currentDotNetHelper.invokeMethodAsync('OnMapClick', e.latlng.lat, e.latlng.lng);
        }
    });
    
    console.log("Map initialized successfully");
};

window.updateMarkers = (tripId, points) => {
    if (!map) {
        console.warn("Map not initialized yet");
        return;
    }
    
    markers.forEach((marker, id) => {
        if (map) map.removeLayer(marker);
    });
    markers.clear();
    
    if (points) {
        points.forEach(point => {
            addMarker(point);
        });
    }
};

function addMarker(point) {
    if (!map) return;
    
    let markerColor = 'blue';
    switch(point.tag) {
        case 'Restauracja': markerColor = 'red'; break;
        case 'Hotel': markerColor = 'green'; break;
        case 'Zabytki': markerColor = 'purple'; break;
        case 'Natura': markerColor = 'green'; break;
        default: markerColor = 'blue';
    }
    
    const icon = L.divIcon({
        className: 'custom-marker',
        html: `<div style="background-color: ${markerColor}; width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; font-size: 14px; border: 2px solid white; box-shadow: 0 2px 5px rgba(0,0,0,0.3);">📍</div>`,
        iconSize: [24, 24],
        popupAnchor: [0, -12]
    });
    
    const marker = L.marker([point.latitude, point.longitude], { icon: icon })
        .bindPopup(`
            <div style="min-width: 200px; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;">
                <strong style="font-size: 16px;">${escapeHtml(point.name || 'Bez nazwy')}</strong><br/>
                <span style="display: inline-block; background: #e9ecef; padding: 2px 8px; border-radius: 12px; font-size: 11px; margin: 8px 0;">
                    ${escapeHtml(point.tag || 'Atrakcja')}
                </span>
                <p style="margin: 8px 0; font-size: 13px; color: #666;">${escapeHtml(point.description || 'Brak opisu')}</p>
                <div style="display: flex; gap: 8px; margin-top: 12px;">
                    <button onclick="editPointForm(${point.id})" style="flex: 1; padding: 6px 12px; font-size: 13px; background: #2563eb; color: white; border: none; border-radius: 8px; cursor: pointer;">
                        ✏️ Edytuj
                    </button>
                    <button onclick="deletePoint(${point.id})" style="flex: 1; padding: 6px 12px; font-size: 13px; background: #fee2e2; color: #dc2626; border: none; border-radius: 8px; cursor: pointer;">
                        🗑 Usuń
                    </button>
                </div>
            </div>
        `)
        .addTo(map);
    
    markers.set(point.id, marker);
}

window.editPointForm = (pointId) => {
    if (currentDotNetHelper) {
        currentDotNetHelper.invokeMethodAsync('OpenEditForm', pointId);
    }
};
window.removeMarker = (pointId) => {
    const marker = markers.get(pointId);
    if (marker && map) {
        map.removeLayer(marker);
        markers.delete(pointId);
    }
};

window.updateMarkerPopup = (pointId, name, description) => {
    const marker = markers.get(pointId);
    if (marker) {
        marker.setPopupContent(`
            <div>
                <strong>${escapeHtml(name)}</strong><br/>
                <p>${escapeHtml(description)}</p>
                <button onclick="editPoint(${pointId})">Edytuj</button>
                <button onclick="deletePoint(${pointId})">Usuń</button>
            </div>
        `);
    }
};

window.editPoint = (pointId) => {
    const name = prompt('Nowa nazwa punktu:');
    const desc = prompt('Nowy opis:');
    if (name && currentDotNetHelper) {
        currentDotNetHelper.invokeMethodAsync('UpdatePoint', pointId, name, desc);
    }
};

window.deletePoint = (pointId) => {
    if (confirm('Czy na pewno chcesz usunąć ten punkt?')) {
        if (currentDotNetHelper) {
            currentDotNetHelper.invokeMethodAsync('RemovePoint', pointId);
        }
    }
};

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}


window.flyToLocation = (lat, lng) => {
    if (map) {
        map.flyTo([lat, lng], 15, {
            duration: 1.5
        });
    }
};
window.getCurrentPosition = () => {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject(new Error("Geolokalizacja nie jest wspierana"));
            return;
        }
        
        navigator.geolocation.getCurrentPosition(
            (position) => {
                resolve({
                    Latitude: position.coords.latitude,
                    Longitude: position.coords.longitude
                });
            },
            (error) => {
                reject(new Error(error.message));
            }
        );
    });
};

window.downloadFile = (filename, content) => {
    const blob = new Blob([content], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
};