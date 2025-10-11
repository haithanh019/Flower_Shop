document.addEventListener('DOMContentLoaded', function () {
    const quantityForms = document.querySelectorAll('.quantity-form');

    quantityForms.forEach(form => {
        const itemId = form.querySelector('input[name="cartItemId"]').value;
        const quantityInput = form.querySelector(`#quantity-${itemId}`);
        const decreaseBtn = form.querySelector('.btn-decrease');
        const increaseBtn = form.querySelector('.btn-increase');

        const submitForm = () => {
            clearTimeout(form.timer);
            form.timer = setTimeout(() => {
                form.submit();
            }, 350);
        };

        decreaseBtn.addEventListener('click', function () {
            let currentValue = parseInt(quantityInput.value, 10);
            if (currentValue > 1) {
                quantityInput.value = currentValue - 1;
                submitForm();
            }
        });

        increaseBtn.addEventListener('click', function () {
            let currentValue = parseInt(quantityInput.value, 10);
            quantityInput.value = currentValue + 1;
            submitForm();
        });

        quantityInput.addEventListener('change', function () {
            let currentValue = parseInt(quantityInput.value, 10);
            if (currentValue < 1 || isNaN(currentValue)) {
                quantityInput.value = 1;
            }
            submitForm();
        });
    });
});