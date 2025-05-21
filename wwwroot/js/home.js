 
/*let strEnviroment = '/abms/api/CleanroomApi/getTopData?count=';*/
//let strHourlyAveragesChartTemperature1 = '/api/SensorReadingAPI/getHourlyAverages?SensorInfoID=1';

//let strDailyAverages = '/api/CleanroomApi/getDailyAverages';
//let strLastest = '/api/CleanroomApi/getLatest';

//let strDuongTieuChuanHumidity = '/api/CleanroomApi/getDuongTieuChuanDoAm';
//let strDuongTieuChuanTemperature = '/api/CleanroomApi/getDuongTieuChuanNhietDo';

//let strLastestTemplature1 = '/api/SensorReadingAPI/GetLastesBySensorInfoID?SensorInfoID=1';
//let strLastestHumidity1 = '/api/SensorReadingAPI/GetLastesBySensorInfoID?SensorInfoID=4';
//let strLastestTemplature2 = '/api/SensorReadingAPI/GetLastesBySensorInfoID?SensorInfoID=8';
//let strLastestHumidity2 = '/api/SensorReadingAPI/GetLastesBySensorInfoID?SensorInfoID=12';
//let strLastestPressure1 = '/api/SensorReadingAPI/GetLastesBySensorInfoID?SensorInfoID=13';

 


//let strTopRealtimeHumidity1 = "api/SensorReadingAPI/GetTopData?SensorInfoID=4";
//let strTopRealtimeTemperature2 = "api/SensorReadingAPI/GetTopData?SensorInfoID=8";
//let strTopRealtimeHumidity2 = "api/SensorReadingAPI/GetTopData?SensorInfoID=12";
//let strTopRealtimePressure1 = "api/SensorReadingAPI/GetTopData?SensorInfoID=13";

////Hiển thị giá trị mới nhất của nhiệt độ LastestTemplature1
//function LastestTemplature1() {
//    fetch(strLastestTemplature1)
//        .then(response => response.json())
//        .then(data => {
//            if (data === null) {
//                console.error('No latest data found!');
//                return;
//            }
//            document.getElementById("ptc1-point1-temp").textContent = data.readingValue;
//            document.getElementById("ptc1-point1-temp2").textContent = data.readingValue;
//        })
//        .catch(error => {
//            console.error('Error fetching data:', error);
//        });
//}

////Hiển thị giá trị mới nhất của Độ ẩm LastestHumidity1
//function LastestHumidity1() {
//    fetch(strLastestHumidity1)
//        .then(response => response.json())
//        .then(data => {
//            if (data === null) {
//                console.error('No latest data found!');
//                return;
//            }


//            document.getElementById("ptc1-point1-humidity").textContent = data.readingValue;
//            document.getElementById("ptc1-point1-humidity2").textContent = data.readingValue;
//        })
//        .catch(error => {
//            console.error('Error fetching data:', error);
//        });
//}

////Hiển thị giá trị mới nhất của nhiệt độ LastestTemplature2
//function LastestTemplature2() {
//    fetch(strLastestTemplature2)
//        .then(response => response.json())
//        .then(data => {
//            if (data === null) {
//                console.error('No latest data found!');
//                return;
//            }
//            document.getElementById("ptc1-point2-temp").textContent = data.readingValue;
//            document.getElementById("ptc1-point2-temp2").textContent = data.readingValue;

//        })
//        .catch(error => {
//            console.error('Error fetching data:', error);
//        });
//}

////Hiển thị giá trị mới nhất của Độ ẩm LastestHumidity2
//function LastestHumidity2() {
//    fetch(strLastestHumidity2)
//        .then(response => response.json())
//        .then(data => {
//            if (data === null) {
//                console.error('No latest data found!');
//                return;
//            }
//            document.getElementById("ptc1-point2-humidity").textContent = data.readingValue;
//            document.getElementById("ptc1-point2-humidity2").textContent = data.readingValue;
//        })
//        .catch(error => {
//            console.error('Error fetching data:', error);
//        });
//}

////Hiển thị giá trị mới nhất của Độ ẩm LastestPresure1
//function LastestPresure1() {
//    fetch(strLastestPressure1)
//        .then(response => response.json())
//        .then(data => {
//            if (data === null) {
//                console.error('No latest data found!');
//                return;
//            }

//            document.getElementById("ptc1-point3-pressure").textContent = data.readingValue;
//            document.getElementById("ptc1-point3-pressure2").textContent = data.readingValue;

//            document.getElementById("ptc1-point1-update-time").textContent = formatDateTime(data.readingTime);
//            setTimeout(() => {
//                console.log('Updated data at', new Date().toLocaleTimeString());
//            }, 3000);

//        })
//        .catch(error => {
//            console.error('Error fetching data:', error);
//        });
//}

//    // Update data every 5 seconds
//    setInterval(LastestTemplature1, 3000);
//    setInterval(LastestTemplature2, 3000);
//    setInterval(LastestHumidity1, 3000);
//    setInterval(LastestHumidity2, 3000);
//    setInterval(LastestPresure1, 3000);






///* ChartTemperature2 realtime*/
//const ctxchartTemperature2 = document.getElementById('ChartTemperature2').getContext('2d');
//const chartTemperature2 = new Chart(ctxchartTemperature2, {
//    type: 'line',
//    data: {
//        labels: [],
//        datasets: [
//            {
//                label: 'Point 1',
//                data: [],
//                borderColor: 'red',
//                borderWidth: 2,
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                // pointHoverRadius: 0 // Loại bỏ điểm marker khi hover
//            },

//            {
//                label: 'Max Standard',
//                data: [],
//                borderColor: 'black',
//                borderDash: [5, 5], // Đường nét đứt
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                borderWidth: 2 // Độ dày của đường viền
//            },
//            {
//                label: 'Min Standard',
//                data: [],
//                borderColor: 'black',
//                borderDash: [5, 5], // Đường nét đứt
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                borderWidth: 2 // Độ dày của đường viền
//            }

//        ]
//    },
//    options: {
//        responsive: true,
//        scales: {
//            x: {
//                type: 'time',
//                time: {
//                    unit: 'minute', // Hiển thị trục x theo đơn vị giây

//                    displayFormats: {
//                        second: 'HH:mm' // Định dạng hiển thị trục x
//                    }
//                },
//                title: {
//                    display: true,
//                    text: 'Time'
//                }
//            },
//            y: {
//                beginAtZero: true,
//                min: 10, // Giá trị tối thiểu trên trục Y
//                max: 40, // Giá trị tối đa trên trục Y
//                title: {
//                    display: true,
//                    text: 'Temperature'
//                }
//            }
//        }
//    },

//    plugins: {
//        annotation: {
//            annotations: {
//                maxLine: {
//                    type: 'line',
//                    yMin: 0,
//                    yMax: 0,
//                    borderColor: 'black',
//                    borderWidth: 1,
//                    borderDash: [5, 5],
//                    label: {
//                        content: 'Standard Max',
//                        enabled: true,
//                        position: 'end'
//                    }
//                },
//                minLine: {
//                    type: 'line',
//                    yMin: 0,
//                    yMax: 0,
//                    borderColor: 'black',
//                    borderWidth: 1,
//                    borderDash: [5, 5],
//                    label: {
//                        content: 'Standard Min',
//                        enabled: true,
//                        position: 'end'
//                    }
//                }
//            }
//        }
//    }

//});

//function updateChartTemperature2() {
//    count = document.getElementById('dataCount').value;
//    let tem = "";
//    tem = strTopRealtimeTemperature2 + "&Count=" + count;
//    console.log("Tem=" + tem);
//    fetch(tem)
//        .then(response => response.json())
//        .then(data => {
//            chartTemperature2.data.labels = data.map(item => new Date(item.readingTime));
//            chartTemperature2.data.datasets[0].data = data.map(item => item.readingValue);
//            chartTemperature2.update();
//            setTimeout(() => {
//                console.log('Updated chart Temperature at', new Date().toLocaleTimeString());
//            });
//        }).catch(error => {
//            console.error('There was a problem with the fetch operation:', error);
//        });
//    fetch(strDuongTieuChuanTemperature)
//        .then(response => response.json())
//        .then(data => {
//            const maxStandard = data.tieuChuanMax;
//            const minStandard = data.tieuChuanMin;
//            const labels = chartTemperature.data.labels;
//            chartTemperature2.data.datasets[1].data = labels.map(() => maxStandard);
//            chartTemperature2.data.datasets[2].data = labels.map(() => minStandard);

//            chartTemperature2.update();
//        }).catch(error => {
//            console.error('There was a problem with the fetch operation:', error);
//        });
//}
//// Update chart every 2 seconds

//setInterval(() => updateChartTemperature2(), 10000);

//// Initial update
//updateChartTemperature2();

///*#Hết ChartTemperature*/

///*Bắt đầu ChartHumidity*/
 
//const ctxchartHumidity1 = document.getElementById('ChartHumidity1').getContext('2d');
//const chartHumidity1 = new Chart(ctxchartHumidity1, {
//    type: 'line',
//    data: {
//        labels: [],
//        datasets: [
//            {
//                label: 'Point 1',
//                data: [],
//                borderColor: 'red',
//                borderWidth: 2,
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                // pointHoverRadius: 0 // Loại bỏ điểm marker khi hover
//            },

//            {
//                label: 'Max Standard',
//                data: [],
//                borderColor: 'black',
//                borderDash: [5, 5], // Đường nét đứt
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                borderWidth: 2 // Độ dày của đường viền
//            },
//            {
//                label: 'Min Standard',
//                data: [],
//                borderColor: 'black',
//                borderDash: [5, 5], // Đường nét đứt
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                borderWidth: 2 // Độ dày của đường viền
//            }

//        ]
//    },
//    options: {
//        responsive: true,
//        scales: {
//            x: {
//                type: 'time',
//                time: {
//                    unit: 'minute', // Hiển thị trục x theo đơn vị giây

//                    displayFormats: {
//                        second: 'HH:mm' // Định dạng hiển thị trục x
//                    }
//                },
//                title: {
//                    display: true,
//                    text: 'Time'
//                }
//            },
//            y: {
//                beginAtZero: true,
//                min: 10, // Giá trị tối thiểu trên trục Y
//                max: 60, // Giá trị tối đa trên trục Y
//                title: {
//                    display: true,
//                    text: 'Humidity'
//                }
//            }
//        }
//    },

//    plugins: {
//        annotation: {
//            annotations: {
//                maxLine: {
//                    type: 'line',
//                    yMin: 0,
//                    yMax: 0,
//                    borderColor: 'black',
//                    borderWidth: 1,
//                    borderDash: [5, 5],
//                    label: {
//                        content: 'Standard Max',
//                        enabled: true,
//                        position: 'end'
//                    }
//                },
//                minLine: {
//                    type: 'line',
//                    yMin: 0,
//                    yMax: 0,
//                    borderColor: 'black',
//                    borderWidth: 1,
//                    borderDash: [5, 5],
//                    label: {
//                        content: 'Standard Min',
//                        enabled: true,
//                        position: 'end'
//                    }
//                }
//            }
//        }
//    }

//});

//function updateChartHumidity1() {
//    count = document.getElementById('dataCount').value;
//    let tem = "";
//    tem = strTopRealtimeHumidity1 + "&Count=" + count;
//    console.log("Tem=" + tem);
//    fetch(tem)
//        .then(response => response.json())
//        .then(data => {
//            chartHumidity1.data.labels = data.map(item => new Date(item.readingTime));
//            chartHumidity1.data.datasets[0].data = data.map(item => item.readingValue);
//            chartHumidity1.update();
//            setTimeout(() => {
//                console.log('Updated chart Humidity at', new Date().toLocaleTimeString());
//            });
//        }).catch(error => {
//            console.error('There was a problem with the fetch operation:', error);
//        });
//    fetch(strDuongTieuChuanHumidity)
//        .then(response => response.json())
//        .then(data => {
//            const maxStandard = data.tieuChuanMax;
//            const minStandard = data.tieuChuanMin;
//            const labels = chartHumidity.data.labels;
//            chartHumidity1.data.datasets[1].data = labels.map(() => maxStandard);
//            chartHumidity1.data.datasets[2].data = labels.map(() => minStandard);

//            chartHumidity1.update();
//        }).catch(error => {
//            console.error('There was a problem with the fetch operation:', error);
//        });
//}
//setInterval(() => updateChartHumidity1(), 10000);

//// Initial update
//updateChartHumidity1();

///* ChartHumidity2 realtime*/
//const ctxchartHumidity2 = document.getElementById('ChartHumidity2').getContext('2d');
//const chartHumidity2 = new Chart(ctxchartHumidity2, {
//    type: 'line',
//    data: {
//        labels: [],
//        datasets: [
//            {
//                label: 'Point 2',
//                data: [],
//                borderColor: 'red',
//                borderWidth: 2,
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                // pointHoverRadius: 0 // Loại bỏ điểm marker khi hover
//            },

//            {
//                label: 'Max Standard',
//                data: [],
//                borderColor: 'black',
//                borderDash: [5, 5], // Đường nét đứt
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                borderWidth: 2 // Độ dày của đường viền
//            },
//            {
//                label: 'Min Standard',
//                data: [],
//                borderColor: 'black',
//                borderDash: [5, 5], // Đường nét đứt
//                fill: false,
//                pointRadius: 0, // Loại bỏ điểm marker
//                borderWidth: 2 // Độ dày của đường viền
//            }

//        ]
//    },
//    options: {
//        responsive: true,
//        scales: {
//            x: {
//                type: 'time',
//                time: {
//                    unit: 'minute', // Hiển thị trục x theo đơn vị giây

//                    displayFormats: {
//                        second: 'HH:mm' // Định dạng hiển thị trục x
//                    }
//                },
//                title: {
//                    display: true,
//                    text: 'Time'
//                }
//            },
//            y: {
//                beginAtZero: true,
//                min: 10, // Giá trị tối thiểu trên trục Y
//                max: 60, // Giá trị tối đa trên trục Y
//                title: {
//                    display: true,
//                    text: 'Humidity'
//                }
//            }
//        }
//    },

//    plugins: {
//        annotation: {
//            annotations: {
//                maxLine: {
//                    type: 'line',
//                    yMin: 0,
//                    yMax: 0,
//                    borderColor: 'black',
//                    borderWidth: 1,
//                    borderDash: [5, 5],
//                    label: {
//                        content: 'Standard Max',
//                        enabled: true,
//                        position: 'end'
//                    }
//                },
//                minLine: {
//                    type: 'line',
//                    yMin: 0,
//                    yMax: 0,
//                    borderColor: 'black',
//                    borderWidth: 1,
//                    borderDash: [5, 5],
//                    label: {
//                        content: 'Standard Min',
//                        enabled: true,
//                        position: 'end'
//                    }
//                }
//            }
//        }
//    }

//});

//function updateChartHumidity2() {
//    count = document.getElementById('dataCount').value;
//    let tem = "";
//    tem = strTopRealtimeHumidity2 + "&Count=" + count;
//    console.log("Tem=" + tem);
//    fetch(tem)
//        .then(response => response.json())
//        .then(data => {
//            chartHumidity2.data.labels = data.map(item => new Date(item.readingTime));
//            chartHumidity2.data.datasets[0].data = data.map(item => item.readingValue);
//            chartHumidity2.update();
//            setTimeout(() => {
//                console.log('Updated chart Humidity at', new Date().toLocaleTimeString());
//            });
//        }).catch(error => {
//            console.error('There was a problem with the fetch operation:', error);
//        });
//    fetch(strDuongTieuChuanHumidity)
//        .then(response => response.json())
//        .then(data => {
//            const maxStandard = data.tieuChuanMax;
//            const minStandard = data.tieuChuanMin;
//            const labels = chartHumidity.data.labels;
//            chartHumidity2.data.datasets[1].data = labels.map(() => maxStandard);
//            chartHumidity2.data.datasets[2].data = labels.map(() => minStandard);

//            chartHumidity2.update();
//        }).catch(error => {
//            console.error('There was a problem with the fetch operation:', error);
//        });
//}
//// Update chart every 2 seconds

//setInterval(() => updateChartHumidity2(), 10000);

//// Initial update
//updateChartHumidity2();



// //Hết Chart Humidity



//const ctxPressure = document.getElementById('environmentChartPressure').getContext('2d');
//const ChartPressure = new Chart(ctxPressure, {
//    type: 'line',
//    data: {
//        labels: [],
//        datasets: [

//            { label: 'Pressure PTCA/Solder Point 3', data: [], borderColor: 'orange', fill: false }
//        ]
//    },
//    options: {
//        responsive: true,
//        scales: {
//            x: {
//                type: 'time',
//                time: {
//                    unit: 'second', // Hiển thị trục x theo đơn vị giây

//                    displayFormats: {
//                        second: 'HH:mm:ss' // Định dạng hiển thị trục x
//                    }
//                }
//            },
//            y: {
//                beginAtZero: true,
//                min: 10, // Giá trị tối thiểu trên trục Y
//                max: 30, // Giá trị tối đa trên trục Y
//                title: {
//                    display: true,
//                    text: 'Pressure'
//                }
//            }
//        }
//    }


//});

//function updateChartChartPressure() {
//    console.log(count);
//    count = document.getElementById('dataCount').value;
//    let strPressure = `/api/SensorReadingAPI/GetTopData?SensorInfoID=13&Count=${count}`;
//    console.log(strPressure);
//    fetch(strPressure)
//        .then(response => response.json())
//        .then(data => {
//            ChartPressure.data.labels = data.map(item => new Date(item.readingTime)); 
//            ChartPressure.data.datasets[0].data = data.map(item => item.readingValue);
//            ChartPressure.update();
//        });
//}

//// Update ChartHumidity every 2 seconds 
//setInterval(() => updateChartChartPressure(), 3000);
//// Initial update
//updateChartChartPressure();
///* Hết ChartHumidity*/



///*BEGIN Hiển thị chart dữ liệu theo giờ*/
//$(document).ready(function () {

//    // Tính toán ngày đầu tiên và ngày cuối cùng của tháng hiện tại
//    var today = new Date();
//    var firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
//    var lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);

//    // Định dạng ngày theo chuẩn yyyy-mm-dd
//    var formatDate = function (date) {
//        var day = ("0" + date.getDate()).slice(-2);
//        var month = ("0" + (date.getMonth() + 1)).slice(-2);
//        return date.getFullYear() + "-" + month + "-" + day;
//    };

//    // Gán giá trị mặc định cho startDate và endDate
//    $('#startDateHourly').val(formatDate(firstDayOfMonth));
//    $('#endDateHourly').val(formatDate(lastDayOfMonth));

//    // Biến để lưu trữ biểu đồ. XỬ lý cho lỗi Canvas is already in use.
//    var hourlyAveragesChartTemperature1;

//    // Hàm để tải dữ liệu và vẽ biểu đồ
//    function loadData(startDate, endDate) {

//        $.ajax({
//            url: strHourlyAveragesChartTemperature1,
//            method: 'GET',
//            data: {
//                startDate: startDate,
//                endDate: endDate
//            },
//            success: function (data) {
//                var labels = data.map(function (item) {
//                    var date = new Date(item.year, item.month - 1, item.day, item.hour);
//                    return date.toLocaleString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false });
//                });

//                var avgValue = data.map(function (item) {
//                    return item.avgValue;
//                });

//                var maxValue = data.map(function (item) {
//                    return item.maxValue;
//                });

//                var minValue = data.map(function (item) {
//                    return item.minValue;
//                });

                

//                // Hủy biểu đồ cũ nếu tồn tại
//                if (hourlyAveragesChartTemperature1) {
//                    hourlyAveragesChartTemperature1.destroy();
//                }

//                var ctxhourlyAveragesChartTemperature1 = document.getElementById('hourlyAveragesChartTemperature1').getContext('2d');
//                hourlyAveragesChartTemperature1 = new Chart(ctxhourlyAveragesChartTemperature1, {
//                    type: 'line',
//                    data: {
//                        labels: labels,
//                        datasets: [
//                            {
//                                label: 'avgValue',
//                                data: avgValue,
//                                borderColor: 'rgba(255, 99, 132, 1)',
//                                backgroundColor: 'rgba(255, 99, 132, 0.2)',
//                                fill: false,
//                                pointRadius: 0,
//                                borderWidth: 2, // Độ dày của đường viền
//                            },
//                            {
//                                label: 'maxValue',
//                                data: maxValue,
//                                borderColor: 'rgba(54, 162, 235, 1)',
//                                backgroundColor: 'rgba(54, 162, 235, 0.2)',
//                                fill: false,
//                                pointRadius: 0,
//                                borderWidth: 2, // Độ dày của đường viền
//                            },
//                            {
//                                label: 'minValue',
//                                data: minValue,
//                                borderColor: 'rgba(75, 192, 192, 1)',
//                                backgroundColor: 'rgba(75, 192, 192, 0.2)',
//                                fill: false,
//                                pointRadius: 0,
//                                borderWidth: 2, // Độ dày của đường viền
//                            },
                          
//                        ]
//                    },
//                    options: {
//                        responsive: true,
//                        scales: {
//                            x: {
//                                type: 'category',
//                                labels: labels
//                            },
//                            y: {
//                                beginAtZero: true
//                            }
//                        }
//                    }
//                });
//            },
//            error: function (xhr, status, error) {
//                console.error("Error: " + error);
//            }
//        });
//    }

//    // Gọi hàm loadData khi trang được tải với giá trị mặc định
//    loadData(formatDate(firstDayOfMonth), formatDate(lastDayOfMonth));

//    // Gọi hàm loadData khi nút Load Data được nhấn
//    $('#loadDataTheoGio').click(function () {
//        console.log("OK");
//        var startDate = $('#startDateHourly').val();
//        var endDate = $('#endDateHourly').val();
//        loadData(startDate, endDate);
//    });

//});

///*#END Hiển thị chart dữ liệu theo giờ*/

///*BEGIN Hiển thị chart dữ liệu theo Ngày*/
//$(document).ready(function () {

//    // Tính toán ngày đầu tiên và ngày cuối cùng của tháng hiện tại
//    var today = new Date();
//    var firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
//    var lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);

//    // Định dạng ngày theo chuẩn yyyy-mm-dd
//    var formatDate = function (date) {
//        var day = ("0" + date.getDate()).slice(-2);
//        var month = ("0" + (date.getMonth() + 1)).slice(-2);
//        return date.getFullYear() + "-" + month + "-" + day;
//    };

//    // Gán giá trị mặc định cho startDate và endDate
//    $('#startDateDaily').val(formatDate(firstDayOfMonth));
//    $('#endDateDaily').val(formatDate(lastDayOfMonth));


//    // Biến để lưu trữ biểu đồ. XỬ lý cho lỗi Canvas is already in use.
//    var chartDailyAveragesChart;
//    function loadData(startDate, endDate) {
//        $.ajax({
//            url: strDailyAverages,
//            method: 'GET',
//            data: {
//                startDate: startDate,
//                endDate: endDate
//            },
//            success: function (data) {
//                var labels = data.map(function (item) {
//                    var date = formatDateTimeToDateType(item.date);
//                    return date;
//                });

//                var avgTemperaturePoint1 = data.map(function (item) {
//                    return item.avgTemperaturePoint1;
//                });

//                var avgHumidityPoint1 = data.map(function (item) {
//                    return item.avgHumidityPoint1;
//                });

//                var avgTemperaturePoint2 = data.map(function (item) {
//                    return item.avgTemperaturePoint2;
//                });

//                var avgHumidityPoint2 = data.map(function (item) {
//                    return item.avgHumidityPoint2;
//                });

//                var avgPressurePoint3 = data.map(function (item) {
//                    return item.avgPressurePoint3;
//                });
//                // Hủy biểu đồ cũ nếu tồn tại
//                if (chartDailyAveragesChart) {
//                    chartDailyAveragesChart.destroy();
//                }
//                var ctxdailyAveragesChart = document.getElementById('dailyAveragesChart').getContext('2d');
//                chartDailyAveragesChart = new Chart(ctxdailyAveragesChart, {
//                    type: 'line',
//                    data: {
//                        labels: labels,
//                        datasets: [
//                            {
//                                label: 'Temperature Point 1',
//                                data: avgTemperaturePoint1,
//                                borderColor: 'rgba(255, 99, 132, 1)',
//                                backgroundColor: 'rgba(255, 99, 132, 0.2)',
//                                fill: false
//                            },
//                            {
//                                label: 'Humidity Point 1',
//                                data: avgHumidityPoint1,
//                                borderColor: 'rgba(54, 162, 235, 1)',
//                                backgroundColor: 'rgba(54, 162, 235, 0.2)',
//                                fill: false
//                            },
//                            {
//                                label: 'Temperature Point 2',
//                                data: avgTemperaturePoint2,
//                                borderColor: 'rgba(75, 192, 192, 1)',
//                                backgroundColor: 'rgba(75, 192, 192, 0.2)',
//                                fill: false
//                            },
//                            {
//                                label: 'Humidity Point 2',
//                                data: avgHumidityPoint2,
//                                borderColor: 'rgba(153, 102, 255, 1)',
//                                backgroundColor: 'rgba(153, 102, 255, 0.2)',
//                                fill: false
//                            },
//                            {
//                                label: 'Pressure Point 3',
//                                data: avgPressurePoint3,
//                                borderColor: 'rgba(255, 159, 64, 1)',
//                                backgroundColor: 'rgba(255, 159, 64, 0.2)',
//                                fill: false
//                            }
//                        ]
//                    },
//                    options: {
//                        responsive: true,
//                        scales: {
//                            x: {
//                                type: 'category',
//                                labels: labels
//                            },
//                            y: {
//                                beginAtZero: true
//                            }
//                        }
//                    }
//                });
//            }
//        });
//    }

//    // Gọi hàm loadData khi trang được tải với giá trị mặc định
//    loadData(formatDate(firstDayOfMonth), formatDate(lastDayOfMonth));

//    // Gọi hàm loadData khi nút Load Data được nhấn
//    $('#loadDataTheoDaily').click(function () {
//        console.log("OK");
//        var startDate = $('#startDateDaily').val();
//        var endDate = $('#endDateDaily').val();
//        loadData(startDate, endDate);
//    });
//});
///*END Hiển thị chart dữ liệu theo Ngày*/


 

//Xử lý thời gian kiểu ISO thành kiểu mong muốn YYYY-MM-DD HH:MM:SS.
function formatDateTime(timestamp) {
    const date = new Date(timestamp);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Tháng bắt đầu từ 0
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
}

function formatDateTimeToHourType(timestamp) {
    const date = new Date(timestamp);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Tháng bắt đầu từ 0
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');

    return `${year}-${month}-${day}  ${hours}`;
}



function formatDateTimeToDateType(timestamp) {
    const date = new Date(timestamp);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Tháng bắt đầu từ 0
    const day = String(date.getDate()).padStart(2, '0');


    return `${year}-${month}-${day}`;
}



