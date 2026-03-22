function closeEmployeeForm() {
    const container = document.getElementById('employeeFormContainer');
    // Thêm lớp animation
    container.style.animation = 'fadeOut 0.3s ease-in-out forwards';

    // Chờ animation kết thúc rồi mới ẩn
    setTimeout(() => {
        container.style.display = 'none';
        container.style.animation = '';

    }, 300);
}

$(document).ready(function () {
    $("#addEmployeeBtn").click(function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Admin/EmployeesManagement/AddEmployee",
            type: "GET",
            success: function (data) {
                $("#employeeFormContainer").html(data);
                $("#employeeFormContainer").show();
            },
            error: function () {
                alert("Lỗi khi tải form.");
            }
        });
    });

    // Gắn sự kiện click cho nút "Chỉnh Sửa"
    $(document).on('click', '#updateEmployeeBtn', function (e) {
        e.preventDefault();

        var employeeId = $(this).closest('tr').data('employee-id');

        $.ajax({
            url: "/Admin/EmployeesManagement/UpdateEmployee", // URL đến action xử lý yêu cầu AJAX
            type: "GET",
            data: { id: employeeId },
            success: function (data) {
                $("#employeeFormContainer").html(data);
                $("#employeeFormContainer").show();
            },
            error: function (xhr, status, error) {
                console.error('Error loading update form:', error);
            }
        });
    });

    $(document).on('click', '#deleteEmployeeBtn', function (e) {

        e.preventDefault();

        var employeeId = $(this).closest('tr').data('employee-id');

        $.ajax({
            url: "/Admin/EmployeesManagement/DeleteComfirmation",
            type: "GET",
            data: { id: employeeId },
            success: function (data) {
                $("#employeeFormContainer").html(data);
                $("#employeeFormContainer").show();

                $("#employeeFormContainer").find('.confirm-btn').click(function (e) {

                    e.preventDefault();

                    $.ajax({
                        url: "/Admin/EmployeesManagement/DeleteEmployee",
                        type: 'POST',
                        data: { id: employeeId },
                        success: function () {
                            window.location.href = "/Admin/EmployeesManagement/Index";
                        },
                        error: function () {
                            alert('Có lỗi xảy ra khi xóa nhân viên!');
                            $("#employeeFormContainer").remove();
                        }
                    });

                });

            },
            error: function (xhr, status, error) {
                console.error('Error loading update form:', error);
            }
        });
    });
});