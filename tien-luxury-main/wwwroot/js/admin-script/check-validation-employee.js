    $(document).ready(function () {
        $(document).on("submit", ".employeeForm", function (e) {
            console.log("AJAX submit event triggered");
            e.preventDefault();

            let formData = new FormData(this);

            let productImageInput = document.getElementById("employeeImage");
            if (productImageInput.files.length > 0) {
                formData.append("employeeImage", productImageInput.files[0]);
            }

            $("#errorMessage").hide().empty();
            $(".error-field").empty();

            $.ajax({
                url: $(this).attr("action"),
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {

                    console.log("Server response:", response);
                    
                    if (response.success) {
                        window.location.href = response.redirectUrl;
                    } else {
                        $("#errorMessage").text(response.message || "Kiểm tra lại các trường đã nhập").show();
                        if (response.errors) {
                            $.each(response.errors, function (key, value) {
                                var fieldName = key.replace("Employee.", "");
                                var errorSpan = $("[data-valmsg-for='Employee." + fieldName + "']");
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
