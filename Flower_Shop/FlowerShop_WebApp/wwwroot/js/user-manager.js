$(function () {
    const userModal = new bootstrap.Modal(document.getElementById('userModal'));
    const modalContent = $('#userModalContent');
    const tableContainer = $('#userTableContainer');

    function refreshTable() {
        // Lấy giá trị search hiện tại từ URL để giữ nguyên khi tải lại
        const currentUrl = new URL(window.location.href);
        const searchString = currentUrl.searchParams.get('searchString') || '';

        $.get(`/Admin/Users/Index?searchString=${encodeURIComponent(searchString)}`, function (data) {
            var newTableHtml = $(data).find('#userTableContainer').html();
            tableContainer.html(newTableHtml);
        });
    }

    // Mở modal để Sửa
    $(document).on('click', '[data-bs-toggle="modal"][data-bs-target="#userModal"]', function () {
        const url = $(this).data('url');
        modalContent.html('<div class="modal-body text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        userModal.show();

        $.get(url, function (data) {
            modalContent.html(data);
            $.validator.unobtrusive.parse(modalContent.find('form'));
        }).fail(function () {
            modalContent.html('<div class="modal-body"><p class="text-danger">Không thể tải nội dung.</p></div>');
        });
    });

    // Xử lý submit form Sửa
    $(document).on('submit', '#userForm', function (e) {
        e.preventDefault();
        var form = $(this);
        var alertPlaceholder = $('#modal-alert-placeholder');
        alertPlaceholder.empty();

        if (!form.valid()) {
            return;
        }

        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    userModal.hide();
                    showToast(response.message, 'success');
                    refreshTable();
                }
            },
            error: function (xhr) {
                var response = xhr.responseJSON;
                var errorMsg = 'Đã có lỗi xảy ra.';
                if (response && response.errors) {
                    errorMsg = response.errors.join('<br>');
                }
                alertPlaceholder.html(`<div class="alert alert-danger">${errorMsg}</div>`);
            }
        });
    });

    // Đóng modal thì xóa nội dung
    $('#userModal').on('hidden.bs.modal', function () {
        modalContent.empty();
    });
});