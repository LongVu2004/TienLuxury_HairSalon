$(document).ready(function () {
    $(document).on("submit", ".serviceForm", function (e) {
        e.preventDefault();
        $("#errorMessage").hide().empty();
        $(".error-field").empty();

        $.ajax({
            url: $(this).attr("action"),
            type: "POST",
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    window.location.reload();
                } else {
                    $("#errorMessage")
                        .text(response.message)
                        .show();
                }
            },
            error: function () {
                $("#errorMessage")
                    .text("Đã xảy ra lỗi hệ thống. Vui lòng thử lại.")
                    .show();
            }
        });
    });
});