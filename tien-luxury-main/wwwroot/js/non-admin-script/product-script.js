//document.addEventListener('DOMContentLoaded', function () {
//    const searchInput = document.getElementById('search-input');
//    const suggestionsBox = document.getElementById('suggestions-box');

//    searchInput.addEventListener('input', function () {
//        const searchTerm = this.value.trim();

//        if (searchTerm.length > 0) {
//            fetch(`/Product/SearchSuggestions?searchTerm=${encodeURIComponent(searchTerm)}`)
//                .then(response => response.json())
//                .then(data => {

//                    console.log(data); // Kiểm tra dữ liệu trả về trong Console

//                    suggestionsBox.innerHTML = '';

//                    if (data.length > 0) {
//                        data.forEach(item => {
//                            const suggestionItem = document.createElement('div');
//                            suggestionItem.classList.add('suggestion-item');
//                            suggestionItem.textContent = item.productName;
//                            suggestionItem.dataset.productId = item.id;

//                            suggestionItem.addEventListener('click', function () {
//                                window.location.href = `/Product/ProductDetail/${item.id}`;
//                            });

//                            suggestionsBox.appendChild(suggestionItem);
//                        });
//                        suggestionsBox.style.display = 'block';
//                    } else {
//                        suggestionsBox.style.display = 'none';
//                    }
//                })
//                .catch(error => {
//                    console.error('Lỗi khi gọi API:', error);
//                });
//        } else {
//            suggestionsBox.style.display = 'none';
//        }
//    });

//    document.addEventListener('click', function (event) {
//        if (!searchInput.contains(event.target) && !suggestionsBox.contains(event.target)) {
//            suggestionsBox.style.display = 'none';
//        }
//    });
//});

document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("live-search-input");
    const suggestionsBox = document.getElementById("search-suggestions");
    let timeoutId; // Biến để delay (debounce)

    searchInput.addEventListener("input", function () {
        const term = this.value.trim();

        // Xóa timeout cũ nếu người dùng đang gõ liên tục
        clearTimeout(timeoutId);

        if (term.length < 1) {
            suggestionsBox.style.display = "none";
            suggestionsBox.innerHTML = "";
            return;
        }

        // Đợi 300ms sau khi ngừng gõ mới gửi request (Tránh spam server)
        timeoutId = setTimeout(() => {
            fetch(`/Product/GetProductSuggestions?term=${encodeURIComponent(term)}`)
                .then(response => response.json())
                .then(data => {
                    suggestionsBox.innerHTML = "";

                    if (data.length > 0) {
                        suggestionsBox.style.display = "block";

                        data.forEach(product => {
                            // Tạo HTML cho từng dòng gợi ý
                            const item = document.createElement("a");
                            item.href = `/Product/ProductDetail/${product.id}`; // Link đến chi tiết
                            item.classList.add("suggestion-item");

                            item.innerHTML = `
                                    <img src="${product.image}" class="suggestion-img" alt="${product.name}">
                                    <div class="suggestion-info">
                                        <h6>${product.name}</h6>
                                        <span>${product.price}</span>
                                    </div>
                                `;
                            suggestionsBox.appendChild(item);
                        });
                    } else {
                        suggestionsBox.style.display = "none"; // Ẩn nếu không có kết quả
                    }
                })
                .catch(err => console.error("Lỗi tìm kiếm:", err));
        }, 300);
    });

    // Ẩn hộp gợi ý khi click ra ngoài
    document.addEventListener("click", function (e) {
        if (!searchInput.contains(e.target) && !suggestionsBox.contains(e.target)) {
            suggestionsBox.style.display = "none";
        }
    });
});