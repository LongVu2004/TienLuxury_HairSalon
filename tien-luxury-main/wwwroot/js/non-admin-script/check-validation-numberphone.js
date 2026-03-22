document.addEventListener("DOMContentLoaded", function () {
    // Gắn sự kiện cho form
    document.getElementById("inputPhoneNumberForm").addEventListener("submit", function (event) {
        event.preventDefault();
        validatePhone();
    });

    function validatePhone() {
        const phoneInput = document.getElementById("phoneInput").value;
        const phoneError = document.getElementById("phoneError");
        const phonePattern = /^\d{10,11}$/;

        if (phonePattern.test(phoneInput)) {
            phoneError.style.display = "none";
            $.ajax({
                type: "POST",
                url: '/Home/Reservation',
                data: { PhoneNumber: $('#phoneInput').val() },
                success: function (response) {
                    window.location.href = response.redirectUrl;
                },
                error: function (xhr, status, error) {
                    alert("Có lỗi xảy ra: " + error);
                }
            });
        } else {
            phoneError.style.display = "block";
        }
    }

    // Gắn sự kiện kiểm tra khi người dùng nhập
    document.getElementById("phoneInput").addEventListener("input", function() {
        const phoneInput = this.value;
        const phoneError = document.getElementById("phoneError");
        const phonePattern = /^\d{10,11}$/;

        if (phoneInput === "" || phonePattern.test(phoneInput)) {
            phoneError.style.display = "none";
        } else {
            phoneError.style.display = "block";
        }
    });

});