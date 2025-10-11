document.addEventListener('DOMContentLoaded', function () {
    // Xử lý việc chọn địa chỉ đã lưu
    const addressRadios = document.querySelectorAll('input[name="SelectedAddressId"]');
    addressRadios.forEach(radio => {
        // Kiểm tra và đặt trạng thái 'selected' ban đầu khi tải lại trang (nếu có lỗi validation)
        if (radio.checked) {
            radio.closest('.saved-address-item').classList.add('selected');
        }

        radio.addEventListener('change', function () {
            document.querySelectorAll('.saved-address-item').forEach(item => item.classList.remove('selected'));
            if (this.checked) {
                this.closest('.saved-address-item').classList.add('selected');
            }
        });
    });

    // Xử lý việc chọn phương thức thanh toán
    const paymentRadios = document.querySelectorAll('input[name="PaymentMethod"]');
    paymentRadios.forEach(radio => {
        // Kiểm tra và đặt trạng thái 'selected' ban đầu
        if (radio.checked) {
            radio.closest('.payment-method-item').classList.add('selected');
        }

        radio.addEventListener('change', function () {
            document.querySelectorAll('.payment-method-item').forEach(item => item.classList.remove('selected'));
            if (this.checked) {
                this.closest('.payment-method-item').classList.add('selected');
            }
        });
    });
});