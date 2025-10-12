// File: FlowerShop_WebApp/wwwroot/js/toast.js

/**
 * Hiển thị một thông báo toast của Bootstrap.
 * @param {string} message Nội dung thông báo.
 * @param {string} type Loại thông báo ('success', 'danger', 'info', 'warning'). Mặc định là 'success'.
 * @param {number} delay Thời gian hiển thị (mili giây). Mặc định là 5000 (5 giây).
 */
function showToast(message, type = 'success', delay = 5000) {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) return;

    // Tạo các class và icon tương ứng với loại thông báo
    const iconClass = type === 'success' ? 'fa-check-circle' : 'fa-times-circle';
    const toastClass = `text-bg-${type}`;

    const toastId = `toast-${Date.now()}`;

    // Tạo cấu trúc HTML cho toast
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center ${toastClass} border-0" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="${delay}">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas ${iconClass} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    // Thêm toast vào container
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);

    // Xóa element khỏi DOM sau khi đã ẩn đi
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });

    toast.show();
}