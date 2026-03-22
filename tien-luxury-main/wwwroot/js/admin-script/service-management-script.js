function closeServiceForm() {
    const container = document.getElementById('serviceFormContainer');
    // Thêm lớp animation
    container.style.animation = 'fadeOut 0.3s ease-in-out forwards';

    // Chờ animation kết thúc rồi mới ẩn
    setTimeout(() => {
        container.style.display = 'none';
        container.style.animation = '';

    }, 300);
}

$(document).ready(function () {
    $("#addServiceBtn").click(function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Admin/ServicesManagement/AddService",
            type: "GET",
            success: function (data) {
                $("#serviceFormContainer").html(data);
                $("#serviceFormContainer").show();
            },
            error: function () {
                alert("Lỗi khi tải form.");
            }
        });
    });

    // Gắn sự kiện click cho nút "Chỉnh Sửa"
    $(document).on('click', '#updateServiceBtn', function (e) {
        e.preventDefault();

        var serviceId = $(this).closest('tr').data('service-id'); // Lấy ID của dịch vụ

        $.ajax({
            url: "/Admin/ServicesManagement/UpdateService", // URL đến action xử lý yêu cầu AJAX
            type: "GET",
            data: { id: serviceId },
            success: function (data) {
                $("#serviceFormContainer").html(data);
                $("#serviceFormContainer").show();
            },
            error: function (xhr, status, error) {
                console.error('Error loading update form:', error);
            }
        });
    });

    $(document).on('click', '#deleteServiceBtn', function (e) {
        e.preventDefault();
        var serviceId = $(this).closest('tr').data('service-id');
        $.ajax({
            url: "/Admin/ServicesManagement/DeleteService",
            type: "GET",
            data: { id: serviceId },
            success: function (data) {
                $("#serviceFormContainer").html(data);
                $("#serviceFormContainer").show();
            },
            error: function (xhr, status, error) {
                console.error('Error loading update form:', error);
            }
        });
    });
});