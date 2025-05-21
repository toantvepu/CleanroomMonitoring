let sensorChart = null;
/**
 * Khởi tạo đồ thị
 * @param {any} data
 * @returns
 */

function getMinMax(arr) {
    if (!arr.length) return { min: 0, max: 0 };
    let min = Math.min(...arr);
    let max = Math.max(...arr);
    // Nếu min khác 0 thì trừ 10, nếu min = 0 thì giữ nguyên
    min = min === 0 ? 0 : min - 5;
    max = max - Math.round ((max%5))+ 10; //làm tròn trục Y cho đồ thị đẹp
    return { min, max };
}
function initChart(data) {
    if (!data) {
        console.error("No chart data provided");
        return;
    }

    const temperatureData = data.temperature || [];
    const humidityData = data.humidity || [];
    const pressureData = data.pressure || [];

    const temperatureValues = temperatureData.map(r => r.Value);
    const humidityValues = humidityData.map(r => r.Value);
    const pressureValues = pressureData.map(r => r.Value);
    const labels = humidityData.map(r => r.Time);

    // Tính min/max cho từng loại
    const tempMinMax = getMinMax(temperatureValues);
    const humMinMax = getMinMax(humidityValues);
    const presMinMax = getMinMax(pressureValues);

    // Nếu chỉ hiển thị 1 loại, lấy min/max của loại đó
    // Nếu hiển thị nhiều loại, lấy min nhỏ nhất và max lớn nhất của tất cả
    let min = Math.min(tempMinMax.min, humMinMax.min, presMinMax.min);
    let max = Math.max(tempMinMax.max, humMinMax.max, presMinMax.max);

    const ctx = document.getElementById('sensorReadingsChart').getContext('2d');

    // Destroy chart nếu đã tồn tại (tránh bị chồng nhiều lần)
    if (sensorChart !== null) {
        sensorChart.destroy();
    }

    sensorChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Temperature (°C)',
                    data: temperatureValues,
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 2,
                    tension: 0,
                    pointRadius: 1,
                },
                {
                    label: 'Humidity (%)',
                    data: humidityValues,
                    backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 2,
                    tension: 0,
                    pointRadius: 1,
                    hidden: false
                },
                {
                    label: 'Pressure (Pa)',
                    data: pressureValues,
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 2,
                    tension: 0,
                    pointRadius: 1,
                    hidden: false
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    min: min,
                    max: max,
                    title: {
                        display: true,
                        text: 'Giá trị'
                    },
                    ticks: {
                        stepSize: 5 // Khoảng cách giữa các giá trị: 0, 5, 10, 15, 20
                    }
                }
            },
            interaction: {
                mode: 'index',
                intersect: false
            },
            plugins: {
                legend: { display: true },
                tooltip: { enabled: true }
            }
        }
    });
}

// Sau khi tải xong trang (lần đầu), khởi tạo chart
document.addEventListener("DOMContentLoaded", () => {
    if (window.sensorChartData) {
        initChart(window.sensorChartData);
    }

    // Gắn lại event nút toggle
    document.querySelectorAll("#btnTemperature, #btnHumidity, #btnPressure, #btnAll").forEach(btn => {
        btn.addEventListener("click", function () {
            const type = this.id.replace("btn", "").toLowerCase();
            sensorChart.data.datasets.forEach((ds, idx) => {
                if (type === "all") {
                    ds.hidden = false;
                } else {
                    ds.hidden = idx !== { temperature: 0, humidity: 1, pressure: 2 }[type];
                }
            });
            sensorChart.update();
            document.querySelectorAll('.btn-group .btn').forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
        });
    });
});


/*Khởi tạo các nút bấm*/
function initSensorChartButtons() {
    document.getElementById('btnTemperature')?.addEventListener('click', function () {
        sensorChart.data.datasets[0].hidden = false;
        sensorChart.data.datasets[1].hidden = true;
        sensorChart.data.datasets[2].hidden = true;
        sensorChart.update();
        setActiveButton(this);
    });

    document.getElementById('btnHumidity')?.addEventListener('click', function () {
        sensorChart.data.datasets[0].hidden = true;
        sensorChart.data.datasets[1].hidden = false;
        sensorChart.data.datasets[2].hidden = true;
        sensorChart.update();
        setActiveButton(this);
    });

    document.getElementById('btnPressure')?.addEventListener('click', function () {
        sensorChart.data.datasets[0].hidden = true;
        sensorChart.data.datasets[1].hidden = true;
        sensorChart.data.datasets[2].hidden = false;
        sensorChart.update();
        setActiveButton(this);
    });

    document.getElementById('btnAll')?.addEventListener('click', function () {
        sensorChart.data.datasets[0].hidden = false;
        sensorChart.data.datasets[1].hidden = false;
        sensorChart.data.datasets[2].hidden = false;
        sensorChart.update();
        setActiveButton(this);
    });

    function setActiveButton(activeButton) {
        document.querySelectorAll('.btn-group .btn').forEach(btn => {
            btn.classList.remove('active');
        });
        activeButton.classList.add('active');
    }
}