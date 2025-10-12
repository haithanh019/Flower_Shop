$(function () {
    const categoryModal = new bootstrap.Modal(document.getElementById('categoryModal'));
    const modalContent = $('#categoryModalContent');
    const tableContainer = $('#categoryTableContainer');

    function refreshTable() {
        $.get('/Admin/Categories/Index', function (data) {
            // Lấy nội dung của bảng từ trang được tải lại
            var newTableHtml = $(data).find('#categoryTableContainer').html();
            tableContainer.html(newTableHtml);
        });
    }

    // Mở modal để Thêm/Sửa
    $(document).on('click', '[data-bs-toggle="modal"][data-bs-target="#categoryModal"]', function () {
        const url = $(this).data('url');
        modalContent.html('<div class="modal-body text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        categoryModal.show();

        $.get(url, function (data) {
            modalContent.html(data);
            // Kích hoạt lại unobtrusive validation cho form mới
            $.validator.unobtrusive.parse(modalContent.find('form'));
        }).fail(function () {
            modalContent.html('<div class="modal-body"><p class="text-danger">Không thể tải nội dung.</p></div>');
        });
    });

    // Xử lý submit form Thêm/Sửa
    $(document).on('submit', '#categoryForm', function (e) {
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
                    categoryModal.hide();
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

    // Xử lý nút Xóa
    $(document).on('click', '.btn-delete', function () {
        const id = $(this).data('id');
        const name = $(this).data('name');

        if (confirm(`Bạn có chắc chắn muốn xóa danh mục "${name}" không?`)) {
            const token = $('input[name="__RequestVerificationToken"]').val();
            $.ajax({
                url: `/Admin/Categories/Delete/${id}`,
                method: 'POST',
                data: {
                    __RequestVerificationToken: token
                },
                success: function (response) {
                    if (response.success) {
                        showToast(response.message, 'success');
                        refreshTable();
                    }
                },
                error: function (xhr) {
                    var response = xhr.responseJSON;
                    showToast(response.message || 'Lỗi khi xóa.', 'danger');
                }
            });
        }
    });
});