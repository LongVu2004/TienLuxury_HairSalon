document.addEventListener("DOMContentLoaded", function () {
    var myCarousel = document.querySelector("#carouselExampleIndicators");
    var carousel = new bootstrap.Carousel(myCarousel, {
        interval: 3000,
        ride: "carousel",
    });

    let indicatorsContainer = document.querySelector(".carousel-indicators");
    let carouselItems = document.querySelectorAll(".carousel-inner .carousel-item");

    // Xóa các button cũ (nếu có)
    indicatorsContainer.innerHTML = "";

    // Tạo button tương ứng với số lượng ảnh
    carouselItems.forEach((item, index) => {
        let button = document.createElement("button");
        button.type = "button";
        button.setAttribute("data-bs-target", "#carouselExampleIndicators");
        button.setAttribute("data-bs-slide-to", index);

        // Đánh dấu active cho button đầu tiên
        if (index === 0) {
            button.classList.add("active");
        }

        indicatorsContainer.appendChild(button);
    });

    let stars = document.querySelectorAll("#ratingStars span");
    let delay = 500; // 0.5s mỗi lần tô màu
    let loopDelay = 1000; // 1s nghỉ trước khi lặp lại
    let selectedRating = 0;

    function autoFillStars() {
        stars.forEach((star, index) => {
            setTimeout(() => {
                star.classList.add("selected");
            }, index * delay);
        });

        // Sau khi tô hết 5 sao, xóa màu và lặp lại
        setTimeout(() => {
            stars.forEach((star) => star.classList.remove("selected"));
            autoFillStars(); // Gọi lại chính nó để lặp vô hạn
        }, stars.length * delay + loopDelay); // Thời gian tổng cộng + thời gian nghỉ
    }

    function updateStars(rating, isHovering) {
        stars.forEach((star, index) => {
            if (index < rating) {
                star.classList.add(isHovering ? "hovered" : "selected");
            } else {
                star.classList.remove("hovered", "selected");
            }
        });
    }

    autoFillStars();
 
    stars.forEach((star) => {
        star.addEventListener("mouseover", function () {
            let rating = parseInt(star.getAttribute("data-value"));
            updateStars(rating, true);
        });

        star.addEventListener("mouseleave", function () {
            updateStars(selectedRating, false);
        });

        star.addEventListener("click", function () {
            selectedRating = parseInt(star.getAttribute("data-value"));
            updateStars(selectedRating, false);
            alert(`Bạn đã đánh giá ${selectedRating} sao!`);
        });
    });

    //Nhanvien
    const carousels = document.querySelectorAll(".employee-carousel");

    carousels.forEach((carousel) => {
        const prevBtn = document.querySelector(
            `button[data-target="${carousel.id}"].left`
        );
        const nextBtn = document.querySelector(
            `button[data-target="${carousel.id}"].right`
        );

        let scrollAmount = 0;
        const cardWidth = carousel.querySelector(".employee-card").offsetWidth + 20;

        nextBtn.addEventListener("click", () => {
            if (scrollAmount < carousel.scrollWidth - carousel.clientWidth) {
                scrollAmount += cardWidth;
                carousel.scrollTo({
                    left: scrollAmount,
                    behavior: "smooth",
                });
            }
        });

        prevBtn.addEventListener("click", () => {
            if (scrollAmount > 0) {
                scrollAmount -= cardWidth;
                carousel.scrollTo({
                    left: scrollAmount,
                    behavior: "smooth",
                });
            }
        });
    });

    

});
function checkLoginAction(e) {
    // Nếu đã đăng nhập -> Cho phép form hoạt động bình thường
    if (isUserLoggedIn === true) {
        return true;
    }

    // Nếu CHƯA đăng nhập -> Chặn ngay lập tức
    e.preventDefault(); // <--- LỆNH QUAN TRỌNG NHẤT

    // Hiện popup đẹp
    Swal.fire({
        title: 'Bạn chưa đăng nhập!',
        text: "Vui lòng đăng nhập để mua sắm hoặc thêm vào giỏ hàng.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d4af37', // Màu vàng Luxury
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Đăng nhập ngay',
        cancelButtonText: 'Để sau'
    }).then((result) => {
        if (result.isConfirmed) {
            // Bấm đăng nhập mới chuyển trang
            window.location.href = '/Account/Login';
        }
        // Bấm hủy thì ở yên đó, không làm gì cả
    });

    return false;
}