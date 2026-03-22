function openChangePasswordForm() {
    const container = document.getElementById('changePasswordFormContainer');
    container.style.display = 'flex'; // Đảm bảo flex hoạt động
}


function closeChangePasswordForm() {
    const container = document.getElementById('changePasswordFormContainer');

    // Thêm lớp animation
    container.style.animation = 'fadeOut 0.3s ease-in-out forwards';

    // Chờ animation kết thúc rồi mới ẩn
    setTimeout(() => {
        container.style.display = 'none';
        container.style.animation = ''; // Xóa animation để tránh lỗi hiển thị khi mở lại
    }, 300); // Thời gian này phải khớp với thời gian animation (0.3s)
}

$(document).ready(function () {
    $("#changePasswordBtn").click(function (e) {
        e.preventDefault(); // Ngăn chặn hành động mặc định

        $.ajax({
            url: "/Admin/Home/ChangePassword", // Kiểm tra URL đúng với Controller
            type: "GET",
            success: function (response) {
                console.log("Form tải thành công"); // Kiểm tra console
                $("#changePasswordFormContainer").html(response); // Chèn form vào DOM
                $("#changePasswordFormContainer").show(); // Hiển thị form nếu bị ẩn
            },
            error: function (xhr) {
                console.error("Lỗi tải form:", xhr.responseText);
                alert("Không thể tải form đổi mật khẩu.");
            }
        });
    });
});