// Nội dung file admin.js
document.addEventListener('DOMContentLoaded', function () {
    // Tìm tất cả các form có thuộc tính data-confirm
    const confirmForms = document.querySelectorAll('form[data-confirm]');

    confirmForms.forEach(form => {
        form.addEventListener('submit', function (event) {
            const message = form.getAttribute('data-confirm');
            if (!confirm(message)) {
                event.preventDefault(); // Hủy bỏ việc submit form nếu người dùng không đồng ý
            }
        });
    });
});