document.addEventListener("DOMContentLoaded", function () {
    const themeToggleBtn = document.getElementById("theme-toggle");
    const body = document.body;

    // Kiểm tra trạng thái lưu trữ trước đó
    if (localStorage.getItem("theme") === "dark") {
        body.classList.add("dark-mode");
        themeToggleBtn.textContent = "☀️"; // Icon mặt trời cho chế độ sáng
    }

    // Bắt sự kiện khi nhấn nút
    themeToggleBtn.addEventListener("click", function () {
        if (body.classList.contains("dark-mode")) {
            body.classList.remove("dark-mode");
            localStorage.setItem("theme", "light");
            themeToggleBtn.textContent = "🌙"; // Chuyển sang biểu tượng mặt trăng
        } else {
            body.classList.add("dark-mode");
            localStorage.setItem("theme", "dark");
            themeToggleBtn.textContent = "☀️"; // Chuyển sang biểu tượng mặt trời
        }
    });
});
