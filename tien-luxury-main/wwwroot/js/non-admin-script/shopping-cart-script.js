document.addEventListener('DOMContentLoaded', function () {
    const paymentOptions = document.querySelectorAll('.payment-option');
    const paymentMethodInput = document.getElementById('payment-method');

    // ====================================================
    // 1. XỬ LÝ CHỌN PHƯƠNG THỨC THANH TOÁN
    paymentOptions.forEach(option => {
        option.addEventListener('click', function () {
            const method = this.dataset.method;
            paymentOptions.forEach(opt => opt.classList.remove('selected'));
            this.classList.add('selected');

            if (method === 'bank') paymentMethodInput.value = 'Bank';
            else if (method === 'momo') paymentMethodInput.value = 'Momo';
            else paymentMethodInput.value = 'COD';
        });
    });

    // 2. LƯU DỮ LIỆU KHI BẤM ĐẶT HÀNG
    const checkoutBtn = document.querySelector('.checkout-btn');
    if (checkoutBtn) {
        checkoutBtn.addEventListener('click', function (e) {
            if (!paymentMethodInput.value) {
                e.preventDefault();
                alert('Vui lòng chọn phương thức thanh toán');
                return;
            }
            const formData = {
                name: $('input[name="CustomerName"]').val(),
                phone: $('input[name="PhoneNumber"]').val(),
                email: $('input[name="Email"]').val(),
                address: $('#input-address').val(),
                province: $('#province-select').val(),
                district: $('#district-select').val(),
                ward: $('#ward-select').val(),
                paymentMethod: paymentMethodInput.value
            };
            localStorage.setItem('draft_order', JSON.stringify(formData));
        });
    }

    // 3. TÍNH TỔNG TIỀN & SỐ LƯỢNG
    const updateTotal = () => {
        let total = 0;
        document.querySelectorAll('.cart-item').forEach(item => {
            const price = parseInt(item.querySelector('.cart-item-price').textContent.replace(/[^0-9]/g, ''));
            const quantity = parseInt(item.querySelector('.quantity-input').value);
            total += price * quantity;
        });
        const totalEl = document.getElementById('total-payment');
        if (totalEl) totalEl.textContent = total.toLocaleString('vi-VN') + ' ₫';
    };

    document.querySelectorAll('.quantity-btn').forEach(button => {
        button.addEventListener('click', function () {
            const input = this.parentElement.querySelector('.quantity-input');
            let value = parseInt(input.value);
            if (this.classList.contains('increase')) value++;
            else if (this.classList.contains('decrease') && value > 1) value--;
            input.value = value;
            updateTotal();
        });
    });

    document.querySelectorAll('.remove-item').forEach(button => {
        button.addEventListener('click', function () {
            this.closest('.cart-item').remove();
            updateTotal();
        });
    });

    updateTotal();

    // 4. XỬ LÝ ĐỊA CHỈ (API PROVINCES)
    let provincesData = [];
    const provinceSelect = $("#province-select");
    const districtSelect = $("#district-select");
    const wardSelect = $("#ward-select");

    $.ajax({
        url: 'https://provinces.open-api.vn/api/?depth=3',
        method: 'GET',
        dataType: 'json',
        success: function (data) {
            provincesData = data;
            provinceSelect.empty().append('<option value="">---</option>');
            data.forEach(province => {
                provinceSelect.append(`<option value="${province.code}">${province.name}</option>`);
            });
            restoreFormData();
        },
    });

    function restoreFormData() {
        const savedData = localStorage.getItem('draft_order');
        if (savedData) {
            const data = JSON.parse(savedData);
            if (!$('input[name="CustomerName"]').val()) $('input[name="CustomerName"]').val(data.name);
            if (!$('input[name="PhoneNumber"]').val()) $('input[name="PhoneNumber"]').val(data.phone);
            if (!$('input[name="Email"]').val()) $('input[name="Email"]').val(data.email);
            $('#input-address').val(data.address);

            if (data.province) {
                provinceSelect.val(data.province);
                const selectedProvince = provincesData.find(p => p.code == data.province);
                if (selectedProvince) {
                    districtSelect.empty().append('<option value="">---</option>');
                    selectedProvince.districts.forEach(district => {
                        districtSelect.append(`<option value="${district.code}">${district.name}</option>`);
                    });

                    if (data.district) {
                        districtSelect.val(data.district);
                        const selectedDistrict = selectedProvince.districts.find(d => d.code == data.district);
                        if (selectedDistrict) {
                            wardSelect.empty().append('<option value="">---</option>');
                            selectedDistrict.wards.forEach(ward => {
                                wardSelect.append(`<option value="${ward.code}">${ward.name}</option>`);
                            });
                            if (data.ward) wardSelect.val(data.ward);
                        }
                    }
                }
                updateFullAddress();
            }

            if (data.paymentMethod) {
                paymentMethodInput.value = data.paymentMethod;
                const methodKey = data.paymentMethod.toLowerCase();
                const option = document.querySelector(`.payment-option[data-method="${methodKey}"]`);
                if (option) {
                    paymentOptions.forEach(opt => opt.classList.remove('selected'));
                    option.classList.add('selected');
                }
            }
        }
    }

    provinceSelect.change(function () {
        const provinceCode = $(this).val();
        districtSelect.empty().append('<option value="">---</option>');
        wardSelect.empty().append('<option value="">---</option>');
        if (provinceCode) {
            const selectedProvince = provincesData.find(p => p.code == provinceCode);
            selectedProvince.districts.forEach(district => {
                districtSelect.append(`<option value="${district.code}">${district.name}</option>`);
            });
        }
        updateFullAddress();
    });

    districtSelect.change(function () {
        const districtCode = $(this).val();
        wardSelect.empty().append('<option value="">---</option>');
        if (districtCode) {
            const provinceCode = provinceSelect.val();
            const selectedProvince = provincesData.find(p => p.code == provinceCode);
            const selectedDistrict = selectedProvince.districts.find(d => d.code == districtCode);
            selectedDistrict.wards.forEach(ward => {
                wardSelect.append(`<option value="${ward.code}">${ward.name}</option>`);
            });
        }
        updateFullAddress();
    });

    wardSelect.change(updateFullAddress);
    $("#input-address").on('input', updateFullAddress);

    function updateFullAddress() {
        const province = provinceSelect.find('option:selected').text();
        const district = districtSelect.find('option:selected').text();
        const ward = wardSelect.find('option:selected').text();
        const specificLocation = $("#input-address").val().trim();

        if (province !== "---" && district !== "---" && ward !== "---" && specificLocation) {
            const fullAddress = `${specificLocation}, ${ward}, ${district}, ${province}`;
            $("#full-address").val(fullAddress);
        }
    }

    // 5. XỬ LÝ VOUCHER (KIỂM TRA THỦ CÔNG)
    const btnCheckVoucher = document.getElementById('btnCheckVoucher');

    if (btnCheckVoucher) {
        btnCheckVoucher.addEventListener('click', function () {
            var codeInput = document.getElementById("voucherInput");
            var code = codeInput.value.trim();
            var msg = document.getElementById("voucherMessage");
            var btn = this;

            var subTotalEl = document.getElementById("subTotal");
            if (!subTotalEl) return;

            var originalTotal = parseFloat(subTotalEl.getAttribute("data-original"));

            if (!code) {
                msg.className = "mt-2 d-block text-danger fw-bold";
                msg.innerText = "Vui lòng nhập mã voucher!";
                return;
            }

            btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            btn.disabled = true;

            fetch('/Voucher/CheckVoucher', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ code: code, orderTotal: originalTotal })
            })
                .then(res => res.json())
                .then(data => {
                    btn.innerHTML = "Kiểm tra";
                    btn.disabled = false;

                    if (data.success) {
                        msg.className = "mt-2 d-block text-success fw-bold";
                        msg.innerHTML = `<i class="fa-solid fa-check-circle"></i> ${data.message}`;

                        document.getElementById("discountRow").style.display = "flex";
                        document.getElementById("discountAmount").innerText = "-" + data.discountAmount.toLocaleString('vi-VN') + "đ";
                        document.getElementById("finalTotal").innerText = data.finalTotal.toLocaleString('vi-VN') + "đ";

                        // Đổi trạng thái nút thành "Đã dùng"
                        codeInput.readOnly = true;
                        btn.innerText = "Đã dùng";
                        btn.style.backgroundColor = "#28a745";
                        btn.style.color = "white";
                        btn.disabled = true;
                    } else {
                        msg.className = "mt-2 d-block text-danger fw-bold";
                        msg.innerHTML = `<i class="fa-solid fa-circle-exclamation"></i> ${data.message}`;

                        document.getElementById("discountRow").style.display = "none";
                        document.getElementById("finalTotal").innerText = originalTotal.toLocaleString('vi-VN') + "đ";
                    }
                })
                .catch(err => {
                    btn.innerHTML = "Kiểm tra";
                    btn.disabled = false;
                    console.error(err);
                    msg.className = "mt-2 d-block text-danger fw-bold";
                    msg.innerText = "Lỗi kết nối server.";
                });
        });
    }

    // 6. XỬ LÝ VOUCHER MODAL
    const btnOpenModal = document.getElementById("btnOpenVoucherModal");
    const voucherContainer = document.getElementById("voucherListContainer");

    // Chỉ cần mở modal bằng bootstrap JS, không cần xử lý tay display:block
    // (Lưu ý: Bạn cần import Bootstrap JS trong View để attribute data-bs-toggle hoạt động, hoặc dùng code dưới)
    var myVoucherModal = null; // Biến giữ instance modal

    if (btnOpenModal) {
        btnOpenModal.addEventListener("click", function () {
            // Khởi tạo Bootstrap Modal
            var modalEl = document.getElementById('voucherModal');
            myVoucherModal = new bootstrap.Modal(modalEl);
            myVoucherModal.show();

            // Load dữ liệu
            loadVouchers();
        });
    }

    function closeModal() {
        if (myVoucherModal) myVoucherModal.hide();
    }

    // Hàm tải danh sách voucher
    function loadVouchers() {
        if (!voucherContainer) return;

        voucherContainer.innerHTML = '<div class="text-center p-3"><i class="fas fa-spinner fa-spin"></i> Đang tải...</div>';

        fetch('/Voucher/GetAvailableVouchers')
            .then(res => res.json())
            .then(response => {
                if (response.success && response.data.length > 0) {
                    let html = '';
                    response.data.forEach(v => {
                        let discountText = v.discountType === "PERCENT"
                            ? `Giảm ${v.value}%`
                            : `Giảm ${v.value.toLocaleString()}đ`;

                        let maxText = (v.discountType === "PERCENT" && v.maxDiscountAmount)
                            ? ` (Tối đa ${v.maxDiscountAmount.toLocaleString()}đ)` : "";

                        // --- LOGIC XỬ LÝ ĐÃ DÙNG (IsUsed) ---
                        // Lưu ý: JSON trong JS thường là camelCase (isUsed)
                        const isUsed = v.isUsed;
                        const disabledClass = isUsed ? 'disabled-ticket' : '';
                        const btnText = isUsed ? 'Đã dùng' : 'Dùng ngay';

                        // Tạo thẻ HTML
                        html += `
                        <div class="voucher-ticket ${disabledClass}" data-code="${v.code}">
                            <div class="ticket-left">
                                <div style="font-weight:bold; font-size: 1.1rem;">${v.discountType === "PERCENT" ? "%" : "VNĐ"}</div>
                                <div style="font-size: 0.7rem;">${v.code}</div>
                            </div>
                            <div class="ticket-right">
                                <div class="ticket-title">${discountText} ${maxText}</div>
                                <div class="ticket-desc">Đơn tối thiểu: ${v.minOrderAmount.toLocaleString()}đ</div>
                                <div class="d-flex justify-content-between align-items-center">
                                    <div class="ticket-expiry">HSD: ${new Date(v.endDate).toLocaleDateString('vi-VN')}</div>
                                    <button class="ticket-btn">${btnText}</button>
                                </div>
                            </div>
                        </div>`;
                    });
                    voucherContainer.innerHTML = html;

                    // Gắn sự kiện click
                    document.querySelectorAll('.voucher-ticket').forEach(ticket => {
                        ticket.addEventListener('click', function () {
                            // QUAN TRỌNG: Nếu có class disabled thì chặn luôn
                            if (this.classList.contains('disabled-ticket')) {
                                return;
                            }

                            const code = this.getAttribute('data-code');
                            const input = document.getElementById("voucherInput");

                            if (input) {
                                input.value = code;
                                closeModal(); // Đóng modal

                                // Tự động kích hoạt nút kiểm tra
                                if (btnCheckVoucher) btnCheckVoucher.click();
                            }
                        });
                    });

                } else {
                    voucherContainer.innerHTML = `
                        <div class="text-center p-5">
                            <i class="fas fa-ticket-alt text-muted fa-3x mb-3"></i>
                            <p class="text-muted">Hiện chưa có mã giảm giá nào khả dụng.</p>
                        </div>`;
                }
            })
            .catch(err => {
                console.error(err);
                voucherContainer.innerHTML = '<p class="text-danger text-center">Lỗi tải dữ liệu.</p>';
            });
    }
});