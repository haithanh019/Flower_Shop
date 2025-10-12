document.addEventListener('DOMContentLoaded', function () {
    const addressRadios = document.querySelectorAll('input[name="SelectedAddressId"]');
    addressRadios.forEach(radio => {
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

    const paymentRadios = document.querySelectorAll('input[name="PaymentMethod"]');
    paymentRadios.forEach(radio => {
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