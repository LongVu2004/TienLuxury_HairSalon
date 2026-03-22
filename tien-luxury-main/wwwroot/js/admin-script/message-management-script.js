document.addEventListener('DOMContentLoaded', function () {
    const messageItems = document.querySelectorAll('.message-item');

    messageItems.forEach(item => {
        item.addEventListener('click', function () {
            // Remove active class
            messageItems.forEach(i => i.classList.remove('active'));
            this.classList.add('active');

            // Hiện phần detail nếu đang bị ẩn
            const detailBox = document.getElementById('message-detail');
            detailBox.classList.remove('hidden');

            // Lấy dữ liệu từ data attributes
            const name = this.dataset.name;
            const email = this.dataset.email;
            const phone = this.dataset.phone;
            const content = this.dataset.content;

            document.getElementById('detail-name').textContent = name;
            document.getElementById('detail-email').textContent = email;
            document.getElementById('detail-phone').textContent = phone;
            document.getElementById('content-paragraph').textContent = content;
        });
    });


    document.querySelector('.delete-btn').addEventListener('click', function () {
        if (confirm('Bạn có chắc chắn muốn xóa đánh giá này?')) {
            $.ajax({
                url: "/Admin/MessagesManagement/DeleteMessage",
                type: "POST",
                data: {
                    id: document.querySelector('.message-item.active').dataset.id
                },
                success: function (response) {
                    if (response.success) {
                        alert("Xóa đánh giá thành công!");

                        const activeItem = document.querySelector('.message-item.active');
                        if (activeItem) {
                            activeItem.remove();
                        }
                        
                        // window.location.href = response.redirectUrl;

                        document.getElementById('message-detail').classList.add('hidden');
                    } else {
                        alert("Xóa đánh giá thất bại, vui lòng thử lại!");
                    }

                },
                error: function () {
                    alert("Đã xảy ra lỗi, vui lòng thử lại!");
                }
            });
        }
    });
});