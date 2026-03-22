let chart;
let currentData = { labels: [], data: [] }; // Lưu trữ toàn bộ dữ liệu 30 ngày
let currentType = 'day';

// Biến quản lý khung nhìn (Window)
let windowSize = 7; // Mặc định hiển thị 7 cột
let startIndex = 0;
let endIndex = 0;

const options = {
    series: [{ name: 'Doanh thu', data: [] }],
    chart: {
        type: 'area',
        height: 350,
        toolbar: { show: false },
        fontFamily: 'Poppins, sans-serif',
        animations: { enabled: true, easing: 'easeinout', speed: 300 } // Animation mượt
    },
    colors: ['#d2b48c'],
    dataLabels: { enabled: false },
    stroke: { curve: 'smooth', width: 3 },
    fill: {
        type: 'gradient',
        gradient: { shadeIntensity: 1, opacityFrom: 0.7, opacityTo: 0.2, stops: [0, 90, 100] }
    },
    xaxis: {
        categories: [],
        axisBorder: { show: false },
        axisTicks: { show: false },
        labels: { style: { colors: '#888' } }
    },
    yaxis: {
        labels: {
            style: { colors: '#888' },
            formatter: val => val.toLocaleString('vi-VN') + ' đ'
        }
    },
    grid: { borderColor: '#f1f1f1', strokeDashArray: 4 },
    tooltip: {
        theme: 'dark',
        y: { formatter: val => val.toLocaleString('vi-VN') + ' đ' }
    }
};

function loadChart(type, btnElement) {
    if (btnElement) {
        document.querySelectorAll('.btn-outline-gold').forEach(btn => btn.classList.remove('active'));
        btnElement.classList.add('active');
    }
    currentType = type;

    // Gọi API lấy dữ liệu
    fetch('/Admin/Home/GetRevenueData?type=' + type)
        .then(response => response.json())
        .then(data => {
            currentData = data; // Lưu lại dữ liệu gốc

            if (type === 'day') {
                // Logic riêng cho "Ngày": Chỉ hiện 7 ngày cuối cùng
                windowSize = 7;
                endIndex = data.labels.length;
                startIndex = Math.max(0, endIndex - windowSize);
            } else {
                // Tháng/Năm thì hiện tất cả
                startIndex = 0;
                endIndex = data.labels.length;
            }

            updateChartDisplay();
        });
}

// Hàm cắt dữ liệu để hiển thị
function updateChartDisplay() {
    // Cắt mảng dữ liệu dựa trên startIndex và endIndex
    const slicedLabels = currentData.labels.slice(startIndex, endIndex);
    const slicedData = currentData.data.slice(startIndex, endIndex);

    chart.updateOptions({
        xaxis: { categories: slicedLabels },
        series: [{ data: slicedData }]
    });
}

document.addEventListener("DOMContentLoaded", function () {
    chart = new ApexCharts(document.querySelector("#revenueChart"), options);
    chart.render();

    // Load mặc định
    loadChart('day', document.querySelector('.btn-outline-gold.active'));

    // --- XỬ LÝ SỰ KIỆN LĂN CHUỘT (SCROLL) ---
    const chartDiv = document.querySelector("#revenueChart");

    chartDiv.addEventListener('wheel', function (e) {
        // Chỉ áp dụng lăn chuột cho chế độ "Ngày"
        if (currentType !== 'day') return;

        e.preventDefault(); // Chặn cuộn trang web

        // Xác định hướng lăn (Lên hay Xuống)
        // deltaY < 0: Lăn lên -> Lùi về quá khứ
        // deltaY > 0: Lăn xuống -> Tiến tới tương lai
        const direction = e.deltaY < 0 ? -1 : 1;

        // Tính toán Index mới
        let newStart = startIndex + direction;
        let newEnd = newStart + windowSize;

        // Kiểm tra giới hạn (Không được lùi quá ngày đầu, ko tiến quá ngày cuối)
        if (newStart >= 0 && newEnd <= currentData.labels.length) {
            startIndex = newStart;
            endIndex = newEnd;
            updateChartDisplay();
        }
    });
});