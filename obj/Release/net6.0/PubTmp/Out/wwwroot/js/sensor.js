const connection = new signalR.HubConnectionBuilder()
    .withUrl("/sensorHub")
    .build();

// Kết nối đến SignalR
connection.start()
    .then(() => console.log("Connected to SignalR"))
    .catch(err => console.error(err));

// Lắng nghe sự kiện từ server
connection.on("ReceiveSensorData", function (sensorData) {
    const tableBody = document.getElementById("sensorTableBody");
    tableBody.innerHTML = ""; // Xóa dữ liệu cũ

    sensorData.forEach(data => {
        const row = `<tr>
            <td>${data.SensorInfoID}</td>
            <td>${data.readingValue}</td>
            <td>${new Date(data.readingTime).toLocaleString()}</td>
        </tr>`;
        tableBody.innerHTML += row;
    });
});
