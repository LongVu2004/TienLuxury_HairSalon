$(document).ready(function () {
    $(document).on("submit", ".productForm", function (e) {
        console.log("AJAX submit event triggered"); // Kiểm tra sự kiện có chạy không
        e.preventDefault(); // Chặn hành vi gửi form mặc định

        let formData = new FormData(this); // Lấy dữ liệu form, bao gồm cả file

        let productImageInput = document.getElementById("productImage");
        if (productImageInput.files.length > 0) {
            formData.append("productImage", productImageInput.files[0]); // Thêm file vào FormData
        }

        $("#errorMessage").hide().empty();
        $(".error-field").empty();

        $.ajax({
            url: $(this).attr("action"),
            type: "POST",
            data: formData,
            processData: false, // Bắt buộc khi gửi FormData
            contentType: false, // Bắt buộc khi gửi FormData
            success: function (response) {
                console.log("Server response:", response);
                if (response.success) {
                    window.location.href = response.redirectUrl; // Chuyển hướng nếu thành công
                } else {
                    $("#errorMessage").text(response.message || "Kiểm tra lại các trường đã nhập").show();
                    if (response.errors) {
                        $.each(response.errors, function (key, value) {
                            var fieldName = key.replace("Product.", "");
                            var errorSpan = $("[data-valmsg-for='Product." + fieldName + "']");
                            if (errorSpan.length) {
                                errorSpan.text(value[0]);
                            }
                        });
                    }
                }
            },
            error: function (xhr, status, error) {
                console.log("AJAX error:", xhr.responseText);
                $("#errorMessage").text("Đã xảy ra lỗi hệ thống. Vui lòng thử lại.").show();
            }
        });
    });
});
