// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//Đang update phần mềm. Có thể sẽ có một số bất thường về dữ liệu
var giaTri = "";
 

// Hàm kiểm tra và cập nhật nội dung
function updateAlert() {
    if (giaTri) {
        var alertElement = document.getElementById("myAlert");
        alertElement.innerHTML = "<h1 style='color: red; font-weight: bold;'>" + giaTri + "</h1>"; // Thêm thẻ h1 và style trực tiếp
    }
}

// Gọi hàm updateAlert khi trang được tải
window.onload = updateAlert;

/**
   * Preloader
   */
//const preloader = document.querySelector('#preloader');
//if (preloader) {
//    window.addEventListener('load', () => {
//        preloader.remove();
//    });
//}
// Preloader script
document.addEventListener("DOMContentLoaded", () => {
    const preloader = document.querySelector("#preloader");

    if (preloader) {
        // Delay 1 giây trước khi ẩn preloader
        window.addEventListener("load", () => {
            setTimeout(() => {
                preloader.classList.add("hidden");
            }, 200); // 1 giây
        });
    }
});

// Add this to a separate site.js file or embed in your dashboard view
// Thêm JavaScript cho Real - time Dashboard với SignalR
// Initialize SignalR connection
let connection = new signalR.HubConnectionBuilder()
    .withUrl("/sensorHub")
    .withAutomaticReconnect()
    .build();

// Track current room ID
let currentRoomId = 0;

// Start the connection
function startConnection() {
    connection.start()
        .then(() => {
            console.log("SignalR Connected");
            if (currentRoomId > 0) {
                subscribeToRoom(currentRoomId);
            }
        })
        .catch(err => {
            console.error(err);
            setTimeout(startConnection, 5000);
        });
}

// Subscribe to room updates
function subscribeToRoom(roomId) {
    if (currentRoomId !== roomId && currentRoomId > 0) {
        connection.invoke("UnsubscribeFromRoom", currentRoomId);
    }

    currentRoomId = roomId;
    connection.invoke("SubscribeToRoom", roomId)
        .catch(err => console.error(err));
}

// Handle sensor updates
connection.on("ReceiveSensorUpdates", (data) => {
    data.forEach(sensor => {
        updateSensorDisplay(sensor);
        updateSensorChart(sensor);
    });
});

// Handle alerts
connection.on("ReceiveAlerts", (data) => {
    // Show notification for alerts
    if (Notification.permission === "granted") {
        new Notification(`Alert in ${data.roomName}`, {
            body: `${data.alerts.length} sensors reporting abnormal values`,
            icon: '/images/alert-icon.png'
        });
    }

    // Update UI to show alert indicators
    data.alerts.forEach(alert => {
        const sensorCard = document.querySelector(`#sensor-${alert.sensorID}-card`);
        if (sensorCard) {
            sensorCard.classList.remove('border-success', 'border-warning');
            sensorCard.classList.add('border-danger');

            // Blink the card
            sensorCard.classList.add('alert-blink');
            setTimeout(() => {
                sensorCard.classList.remove('alert-blink');
            }, 5000);
        }
    });
});

// Update sensor display
function updateSensorDisplay(sensor) {
    const valueElement = document.querySelector(`#sensor-${sensor.sensorID}-value`);
    if (valueElement) {
        valueElement.textContent = sensor.value;

        document.querySelector(`#sensor-${sensor.sensorID}-time`).textContent =
            `Last updated: ${new Date(sensor.timestamp).toLocaleTimeString()}`;

        const statusClass = sensor.isValid ? 'success' : 'danger';
        const cardElement = document.querySelector(`#sensor-${sensor.sensorID}-card`);
        cardElement.classList.remove('border-success', 'border-warning', 'border-danger');
        cardElement.classList.add(`border-${statusClass}`);
    }
}

// Update sensor chart with new data point
function updateSensorChart(sensor) {
    const chart = charts[sensor.sensorID];
    if (chart) {
        // Only keep the last 100 data points to avoid performance issues
        if (chart.data.labels.length > 100) {
            chart.data.labels.shift();
            chart.data.datasets[0].data.shift();
        }

        // Add new data point
        chart.data.labels.push(new Date(sensor.timestamp).toLocaleTimeString());
        chart.data.datasets[0].data.push(sensor.value);
        chart.update();
    }
}

// Create dashboard summary cards
function createSummaryCards(containerId, data) {
    const container = document.getElementById(containerId);
    if (!container) return;

    container.innerHTML = '';

    const rows = Math.ceil(data.length / 3);
    for (let i = 0; i < rows; i++) {
        const row = document.createElement('div');
        row.className = 'row mb-3';

        for (let j = 0; j < 3; j++) {
            const index = i * 3 + j;
            if (index >= data.length) break;

            const sensor = data[index];
            const col = document.createElement('div');
            col.className = 'col-md-4';

            const statusClass = sensor.isValid ? 'success' : 'danger';

            col.innerHTML = `
                <div id="sensor-${sensor.sensorID}-card" class="card border-${statusClass}">
                    <div class="card-header bg-${statusClass} bg-opacity-25">
                        <h6 class="card-title mb-0">${sensor.sensorName}</h6>
                        <small>${sensor.typeName}</small>
                    </div>
                    <div class="card-body text-center">
                        <h3 id="sensor-${sensor.sensorID}-value" class="display-4">${sensor.value}</h3>
                        <p class="mb-0">${sensor.unit}</p>
                        <small id="sensor-${sensor.sensorID}-time" class="text-muted">
                            Last updated: ${new Date(sensor.timestamp).toLocaleTimeString()}
                        </small>
                    </div>
                </div>
            `;

            row.appendChild(col);
        }

        container.appendChild(row);
    }
}

// Request notification permission on page load
function requestNotificationPermission() {
    if ('Notification' in window) {
        Notification.requestPermission();
    }
}

// Set up document ready handler
$(document).ready(function () {
    // Initialize connection
    startConnection();

    // Set up room selection
    $('.room-select').click(function (e) {
        e.preventDefault();
        const roomId = $(this).data('room-id');
        subscribeToRoom(roomId);
        loadRoomDashboard(roomId);
    });

    // Request notification permission
    requestNotificationPermission();

    // Initial setup for current room
    currentRoomId = $('#dashboard-container').data('room-id') || 0;
    if (currentRoomId > 0) {
        subscribeToRoom(currentRoomId);
    }
});

// Handle connection lost and reconnection
connection.onclose(async () => {
    console.log("SignalR Disconnected");
    await startConnection();
});