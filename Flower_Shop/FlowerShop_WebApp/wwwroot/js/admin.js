document.addEventListener('DOMContentLoaded', function () {
    const confirmForms = document.querySelectorAll('form[data-confirm]');

    confirmForms.forEach(form => {
        form.addEventListener('submit', function (event) {
            const message = form.getAttribute('data-confirm');
            if (!confirm(message)) {
                event.preventDefault(); 
            }
        });
    });
});