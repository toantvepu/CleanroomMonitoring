// Lưu trữ các biểu đồ trong một Map để dễ dàng cập nhật
const charts = new Map();

// Lấy thông tin API từ data attributes
const sensorContainer = document.getElementById('sensorContainer');
const baseUrl = sensorContainer?.dataset.apiBaseUrl || '';
const apiPath = sensorContainer?.dataset.apiPath || 'api/SensorDataApi/realtime';

// Hàm tạo mẫu HTML cho một cảm biến
function createSensorTemplate(sensor) {
    const sensorTypeId = sensor.sensorType.replace(/\s+/g, '_');

    return `
        <div class="sensor-card" id="sensor_${sensorTypeId}">
            <div class="card mb-3">
                <div class="card-body">
                    <div class="d-flex align-items-center mb-2">
                        <div class="me-3">
                            <i class="${sensor.icon} fs-3 text-${sensor.statusClass}"></i>
                        </div>
                        <div>
                            <h6 class="m-0">${sensor.sensorType}</h6>
                            <div id="value_${sensorTypeId}" class="fs-4 text-${sensor.statusClass} fw-bold">
                                ${sensor.currentValue.toFixed(2)} ${sensor.unit}
                            </div>
                        </div>
                        <div class="ms-auto text-end">
                            <small class="d-block text-muted">
                                ${sensor.activeSensorCount} of ${sensor.sensorCount} sensors reporting
                            </small>
                            <small class="d-block text-muted">
                                ${sensor.lastUpdated ? new Date(sensor.lastUpdated).toLocaleTimeString() : "No data"}
                            </small>
                        </div>
                    </div>

                    <div class="gauge-container mb-2">
                        <div class="gauge">
                            <div id="gauge_${sensorTypeId}" class="gauge-fill" style="width: ${sensor.gaugePercentage}%;"></div>
                        </div>

                        ${sensor.minThreshold !== null && sensor.maxThreshold !== null ? `
                            <div class="gauge-labels d-flex justify-content-between">
                                <small>${sensor.minThreshold.toFixed(1)}</small>
                                <small>${sensor.maxThreshold.toFixed(1)}</small>
                            </div>
                        ` : ''}

                        ${(sensor.warningMinThreshold !== null || sensor.warningMaxThreshold !== null) &&
            (sensor.minThreshold !== null && sensor.maxThreshold !== null) ? `
                            <div class="gauge-zones">
                                ${sensor.warningMinThreshold !== null ? `
                                    <div class="gauge-zone zone-warning" style="left: 0; width: ${((sensor.warningMinThreshold - sensor.minThreshold) /
                (sensor.maxThreshold - sensor.minThreshold)) * 100
            }%;"></div>
                                ` : ''}
                                
                                ${sensor.warningMaxThreshold !== null ? `
                                    <div class="gauge-zone zone-warning" style="right: 0; width: ${((sensor.maxThreshold - sensor.warningMaxThreshold) /
                (sensor.maxThreshold - sensor.minThreshold)) * 100
            }%;"></div>
                                ` : ''}
                            </div>
                        ` : ''}
                    </div>

                    <div class="mini-chart">
                        <canvas id="chart_${sensorTypeId}" height="60"></canvas>
                    </div>
                </div>
            </div>
        </div>
    `;
}

// Hàm khởi tạo các biểu đồ
function initializeCharts(sensorData) {
    // Xóa các biểu đồ cũ
    charts.forEach(chart => chart.destroy());
    charts.clear();

    // Đảm bảo sensorReadings là một mảng
    if (!sensorData.sensorReadings || !Array.isArray(sensorData.sensorReadings) || sensorData.sensorReadings.length === 0) {
        console.warn('No valid sensor readings available for charts');
        return;
    }

    // Tạo các biểu đồ mới
    sensorData.sensorReadings.forEach(sensor => {
        const sensorTypeId = sensor.sensorType.replace(/\s+/g, '_');
        const canvas = document.getElementById(`chart_${sensorTypeId}`);

        if (!canvas) return;

        // Đảm bảo chartData là một mảng
        if (!sensor.chartData || !Array.isArray(sensor.chartData) || sensor.chartData.length === 0) {
            console.warn(`No chart data available for sensor: ${sensor.sensorType}`);
            return;
        }

        const ctx = canvas.getContext('2d');
        const chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: sensor.chartData.map(d => d.label),
                datasets: [{
                    label: sensor.sensorType,
                    data: sensor.chartData.map(d => d.value),
                    borderColor: getStatusColor(sensor.statusClass),
                    backgroundColor: getStatusColor(sensor.statusClass, 0.2),
                    borderWidth: 1,
                    tension: 0.4,
                    pointRadius: 0,
                    pointHitRadius: 5
                }]
            },
            options: {
                plugins: {
                    legend: { display: false },
                    tooltip: { enabled: true }
                },
                scales: {
                    x: { display: false },
                    y: {
                        display: false,
                        min: sensor.minThreshold ? sensor.minThreshold * 0.9 : undefined,
                        max: sensor.maxThreshold ? sensor.maxThreshold * 1.1 : undefined
                    }
                },
                maintainAspectRatio: false,
                responsive: true
            }
        });

        // Lưu biểu đồ vào Map để cập nhật sau này
        charts.set(sensor.sensorType, chart);
    });
}

// Hàm cập nhật hiển thị dữ liệu cảm biến
function updateSensorDisplay(data) {
    // Kiểm tra dữ liệu hợp lệ
    if (!data) {
        console.error('Invalid data received for updating sensor display');
        return;
    }

    // Cập nhật tên phòng
    const roomNameElement = document.querySelector('#roomName');
    if (roomNameElement && data.roomName) {
        roomNameElement.textContent = data.roomName;
    }

    // Cập nhật thời gian cập nhật gần nhất
    const lastUpdateTimeElement = document.querySelector('#lastUpdateTime');
    if (lastUpdateTimeElement && data.lastUpdated) {
        lastUpdateTimeElement.textContent = new Date(data.lastUpdated).toLocaleTimeString();
    }

    // Đảm bảo sensorReadings là một mảng
    if (!data.sensorReadings || !Array.isArray(data.sensorReadings) || data.sensorReadings.length === 0) {
        console.warn('No valid sensor readings available for update');
        return;
    }

    // Cập nhật từng loại cảm biến
    data.sensorReadings.forEach(sensor => {
        if (!sensor || !sensor.sensorType) {
            console.warn('Invalid sensor data found');
            return;
        }

        const sensorTypeId = sensor.sensorType.replace(/\s+/g, '_');

        // Cập nhật giá trị hiện tại
        const valueElement = document.querySelector(`#value_${sensorTypeId}`);
        if (valueElement) {
            valueElement.textContent = `${sensor.currentValue.toFixed(2)} ${sensor.unit}`;
            valueElement.className = `fs-4 text-${sensor.statusClass} fw-bold`;
        }

        // Cập nhật gauge
        const gaugeElement = document.querySelector(`#gauge_${sensorTypeId}`);
        if (gaugeElement) {
            gaugeElement.style.width = `${sensor.gaugePercentage}%`;
        }

        // Cập nhật biểu đồ
        const chart = charts.get(sensor.sensorType);
        if (chart && sensor.chartData && Array.isArray(sensor.chartData) && sensor.chartData.length > 0) {
            chart.data.labels = sensor.chartData.map(d => d.label);
            chart.data.datasets[0].data = sensor.chartData.map(d => d.value);
            chart.data.datasets[0].borderColor = getStatusColor(sensor.statusClass);
            chart.data.datasets[0].backgroundColor = getStatusColor(sensor.statusClass, 0.2);
            chart.update();
        }
    });
}

// Hàm khởi tạo giao diện với dữ liệu cảm biến ban đầu
function initializeSensorDisplay(data) {
    // Kiểm tra dữ liệu hợp lệ
    if (!data) {
        console.error('Invalid data received for initializing sensor display');
        displayErrorMessage('Invalid data received from server');
        return;
    }

    // Cập nhật tên phòng
    const roomNameElement = document.querySelector('#roomName');
    if (roomNameElement && data.roomName) {
        roomNameElement.textContent = data.roomName;
    }

    // Cập nhật thời gian cập nhật gần nhất
    const lastUpdateTimeElement = document.querySelector('#lastUpdateTime');
    if (lastUpdateTimeElement && data.lastUpdated) {
        lastUpdateTimeElement.textContent = new Date(data.lastUpdated).toLocaleTimeString();
    }

    // Tạo container cho từng loại cảm biến
    const sensorContainers = document.querySelector('#sensorContainers');
    if (!sensorContainers) {
        console.error('Sensor containers element not found');
        return;
    }

    sensorContainers.innerHTML = '';

    // Kiểm tra sensorReadings có phải là mảng không
    if (!data.sensorReadings || !Array.isArray(data.sensorReadings) || data.sensorReadings.length === 0) {
        sensorContainers.innerHTML = `
            <div class="alert alert-warning">
                <i class="bi bi-exclamation-triangle me-2"></i> No active sensors found for this selection.
            </div>
        `;
        return;
    }

    // Tạo HTML cho từng loại cảm biến
    let sensorsHtml = '';
    try {
        sensorsHtml = data.sensorReadings.map(sensor => createSensorTemplate(sensor)).join('');
    } catch (error) {
        console.error('Error creating sensor templates:', error);
        displayErrorMessage('Error processing sensor data');
        return;
    }

    sensorContainers.innerHTML = sensorsHtml;

    // Khởi tạo các biểu đồ
    try {
        initializeCharts(data);
    } catch (error) {
        console.error('Error initializing charts:', error);
        displayErrorMessage('Error initializing sensor charts');
    }
}

// Hiển thị thông báo lỗi
function displayErrorMessage(message) {
    const sensorContainers = document.querySelector('#sensorContainers');
    if (sensorContainers) {
        sensorContainers.innerHTML = `
            <div class="alert alert-danger">
                <i class="bi bi-exclamation-triangle me-2"></i> ${message || 'Failed to load sensor data. Please check your connection.'}
            </div>
        `;
    }
}

// Hàm lấy màu dựa trên trạng thái
function getStatusColor(status, alpha = 1) {
    const colors = {
        'success': `rgba(40, 167, 69, ${alpha})`,
        'warning': `rgba(255, 193, 7, ${alpha})`,
        'danger': `rgba(220, 53, 69, ${alpha})`,
        'secondary': `rgba(108, 117, 125, ${alpha})`
    };

    return colors[status] || colors['secondary'];
}

// Hàm lấy dữ liệu mới từ API
function fetchSensorData() {
    const roomSelector = document.querySelector('#roomSelector');
    const roomId = roomSelector?.value || '0';
   
    // Xây dựng URL đầy đủ cho API call
    const url = `${baseUrl}${apiPath}?roomId=${roomId}`;
    console.log('Fetching data from:', url); // Log URL cho debug 

    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Network response was not ok: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Received data:', data); // Log to debug

            // Kiểm tra tính hợp lệ của dữ liệu
            if (!data) {
                throw new Error('Empty response received');
            }

            // Xử lý đối tượng tham chiếu từ ASP.NET Core
            if (data.sensorReadings && data.sensorReadings.$values) {
                console.log('Converting reference object to array');
                data.sensorReadings = data.sensorReadings.$values;
            }

            // Đảm bảo sensorReadings là một mảng
            if (!data.sensorReadings) {
                data.sensorReadings = [];
                console.warn('No sensorReadings in response, using empty array');
            } else if (!Array.isArray(data.sensorReadings)) {
                console.error('sensorReadings is not an array:', data.sensorReadings);
                // Cố gắng chuyển đổi nếu không phải mảng
                try {
                    if (typeof data.sensorReadings === 'object') {
                        data.sensorReadings = Object.values(data.sensorReadings);
                    } else {
                        data.sensorReadings = [];
                    }
                } catch (e) {
                    console.error('Failed to convert sensorReadings to array:', e);
                    data.sensorReadings = [];
                }
            }

            // Xử lý chartData cho từng sensor
            if (Array.isArray(data.sensorReadings)) {
                data.sensorReadings.forEach(sensor => {
                    if (sensor.chartData && sensor.chartData.$values) {
                        sensor.chartData = sensor.chartData.$values;
                    }
                });
            }

            // Kiểm tra xem đã có biểu đồ nào chưa
            if (charts.size === 0) {
                // Lần đầu tiên, khởi tạo giao diện
                initializeSensorDisplay(data);
            } else {
                // Các lần sau, chỉ cập nhật dữ liệu
                updateSensorDisplay(data);
            }
        })
        .catch(error => {
            console.error('Error fetching sensor data:', error);
            // Hiển thị thông báo lỗi
            displayErrorMessage('Failed to load sensor data: ' + error.message);
        });
}

// Khởi tạo khi trang được tải
document.addEventListener('DOMContentLoaded', function () {
    // Lấy dữ liệu lần đầu
    fetchSensorData();

    // Thiết lập tự động làm mới mỗi 30 giây
    setInterval(fetchSensorData, 30000);

    // Thiết lập sự kiện thay đổi phòng
    const roomSelector = document.querySelector('#roomSelector');
    if (roomSelector) {
        roomSelector.addEventListener('change', fetchSensorData);
    }
});