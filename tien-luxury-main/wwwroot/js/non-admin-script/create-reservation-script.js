
document.addEventListener("DOMContentLoaded", function () {
    const reservationDate = document.getElementById("reservationDate");

    const now = new Date();

    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');

    reservationDate.min = `${year}-${month}-${day}T${hours}:${minutes}`;

    const nextMonth = new Date();
    nextMonth.setMonth(now.getMonth() + 1);

    // Nếu ngày hiện tại là 31 và tháng tiếp theo không có ngày 31, cần điều chỉnh lại
    if (nextMonth.getDate() !== now.getDate()) {
        nextMonth.setDate(0); // Đặt về ngày cuối cùng của tháng trước (ví dụ: 30 hoặc 28/29)
    }

    const nextYear = nextMonth.getFullYear();
    const nextMonthValue = String(nextMonth.getMonth() + 1).padStart(2, '0');
    const nextDay = String(nextMonth.getDate()).padStart(2, '0');
    
    // Đặt max là thời điểm sau 1 tháng so với hiện tại, đến cuối ngày đó
    reservationDate.max = `${nextYear}-${nextMonthValue}-${nextDay}T23:59`;


    document.querySelectorAll('.service-grid').forEach(grid => {
        let scrollAmount = 0;
        let isScrolling = false;

        grid.addEventListener('wheel', function (event) {
            const target = event.target;

            // Kiểm tra nếu chuột không nằm trên hình ảnh (`.image-container`)
            if (!target.closest('.image-container')) {
                event.preventDefault();
                scrollAmount += event.deltaY;

                if (!isScrolling) smoothScroll(this);
            }
        });

        function smoothScroll(element) {
            isScrolling = true;

            const scrollStep = 10; // Điều chỉnh tốc độ cuộn (càng nhỏ càng mượt)
            const distance = scrollAmount / 8;

            if (Math.abs(distance) > 1) {
                element.scrollLeft += distance;
                scrollAmount -= distance;
                window.requestAnimationFrame(() => smoothScroll(element));
            } else {
                isScrolling = false;
                scrollAmount = 0;
            }
        }
    });
});
