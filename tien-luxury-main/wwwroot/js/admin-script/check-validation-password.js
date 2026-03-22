function validatePassword() {
    const password = document.getElementById("NewPassword").value;
    const confirmPassword = document.getElementById("ConfirmPassword").value;
    const passwordError = $("#changePasswordMessage");

    passwordError.css({
        "color": "red",
        "background-color": "#ffe5e5",
        "border": "1px solid red",
        "border-radius": "7px",
        "padding": "5px",
        "margin-bottom": "20px",
        "display": "none"
    });

    const invalidCharRegex = /[^A-Za-z0-9!@#$%^&*]/; // Regex phủ định để kiểm tra ký tự không hợp lệ
    let errorMessage = "";

    if (password.length < 9) {
        errorMessage = "Mật khẩu phải có ít nhất 9 ký tự.";
    } else if (invalidCharRegex.test(password)) {
        errorMessage = "Mật khẩu chứa ký tự không hợp lệ.";
    } else if (password !== confirmPassword) {
        errorMessage = "Mật khẩu nhập lại không khớp.";
    }

    if (errorMessage) {
        passwordError.text(errorMessage);
        setTimeout(() => passwordError.fadeIn(300), 150);
        return false;
    }

    passwordError.fadeOut(200);
    return true;
}

$(document).ready(function () {
    $("#changePasswordForm").submit(function (e) {
        e.preventDefault(); // Ngăn form load lại trang

        if (!validatePassword()) return; // Kiểm tra mật khẩu trước khi gửi AJAX

        $.ajax({
            url: "/Admin/ChangePassword",
            type: "POST",
            data: $(this).serialize(),
            success: function (response) {
                var messageBox = $("#changePasswordMessage");

                messageBox.css({
                    "border-radius": "5px",
                    "margin-bottom": "20px",
                    "display": "none" // Ẩn trước khi hiển thị từ từ
                }).text(response.message);

                if (!response.success) {
                    messageBox.css({
                        "color": "red",
                        "background-color": "#ffe5e5",
                        "border": "1px solid red"
                    });
                } else {
                    messageBox.css({
                        "color": "green",
                        "background-color": "#e5ffe5",
                        "border": "1px solid green"
                    });

                    setTimeout(() => $("#changePasswordFormContainer").fadeOut(), 1000); // Đóng form sau 1s
                }

                setTimeout(() => messageBox.fadeIn(300), 150); // Hiển thị sau 0.15s
            },
            error: function () {
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        });
    });
});
