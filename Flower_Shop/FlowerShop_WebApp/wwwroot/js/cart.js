document.addEventListener('DOMContentLoaded', function () {
    const quantityForms = document.querySelectorAll('.quantity-form');

    quantityForms.forEach(form => {
        const itemId = form.querySelector('input[name="cartItemId"]').value;
        const quantityInput = form.querySelector(`#quantity-${itemId}`);
        const decreaseBtn = form.querySelector('.btn-decrease');
        const increaseBtn = form.querySelector('.btn-increase');

        // Hàm để gửi form
        const submitForm = () => {
            // Sử dụng một debounce nhỏ để tránh gửi quá nhiều request khi người dùng click nhanh
            clearTimeout(form.timer);
            form.timer = setTimeout(() => {
                form.submit();
            }, 350);
        };

        // Bắt sự kiện click nút giảm số lượng
        decreaseBtn.addEventListener('click', function () {
            let currentValue = parseInt(quantityInput.value, 10);
            if (currentValue > 1) {
                quantityInput.value = currentValue - 1;
                submitForm();
            }
        });

        // Bắt sự kiện click nút tăng số lượng
        increaseBtn.addEventListener('click', function () {
            let currentValue = parseInt(quantityInput.value, 10);
            quantityInput.value = currentValue + 1;
            submitForm();
        });

        // Bắt sự kiện khi người dùng tự nhập số và thay đổi
        quantityInput.addEventListener('change', function () {
            let currentValue = parseInt(quantityInput.value, 10);
            if (currentValue < 1 || isNaN(currentValue)) {
                quantityInput.value = 1;
            }
            submitForm();
        });
    });
});