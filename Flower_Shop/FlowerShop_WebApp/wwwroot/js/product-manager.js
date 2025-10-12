$(function () {
    const productModal = new bootstrap.Modal(document.getElementById('productModal'));
    const modalContent = $('#productModalContent');
    const tableContainer = $('#productTableContainer');

    function refreshTable() {
        $.get('/Admin/Products/Index', function (data) {
            var newTableHtml = $(data).find('#productTableContainer').html();
            tableContainer.html(newTableHtml);
        });
    }

    // Mở modal để Thêm/Sửa
    $(document).on('click', '[data-bs-toggle="modal"][data-bs-target="#productModal"]', function () {
        const url = $(this).data('url');
        modalContent.html('<div class="modal-body text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        productModal.show();

        $.get(url, function (data) {
            modalContent.html(data);
            $.validator.unobtrusive.parse(modalContent.find('form'));
        }).fail(function () {
            modalContent.html('<div class="modal-body"><p class="text-danger">Không thể tải nội dung.</p></div>');
        });
    });

    // Xử lý submit form Thêm/Sửa với file upload
    $(document).on('submit', '#productForm', function (e) {
        e.preventDefault();
        var form = $(this);
        var alertPlaceholder = $('#modal-alert-placeholder');
        alertPlaceholder.empty();

        if (!form.valid()) {
            return;
        }

        // Sử dụng FormData để gửi file
        var formData = new FormData(this);

        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: formData,
            processData: false, // Quan trọng: không xử lý dữ liệu
            contentType: false, // Quan trọng: không set content type
            success: function (response) {
                if (response.success) {
                    productModal.hide();
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
    $(document).on('click', '.btn-delete-image', function () {
        var button = $(this);
        var productId = button.data('product-id');
        var imageUrl = button.data('image-url');

        if (confirm("Bạn có chắc chắn muốn xóa ảnh này không?")) {
            const token = $('#productForm input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: '/Admin/Products/DeleteImage',
                method: 'POST',
                data: {
                    __RequestVerificationToken: token,
                    productId: productId,
                    imageUrl: imageUrl
                },
                success: function (response) {
                    if (response.success) {
                        showToast(response.message, 'success');
                        // Xóa ảnh khỏi giao diện
                        button.closest('.image-thumbnail-container').remove();
                        // Nếu không còn ảnh nào, hiển thị thông báo
                        if ($('#existing-images-container').children().length === 0) {
                            $('#existing-images-container').after('<p class="text-muted" id="no-images-text">Sản phẩm này chưa có ảnh.</p>');
                        }
                    }
                },
                error: function (xhr) {
                    var response = xhr.responseJSON;
                    showToast(response.message || 'Lỗi khi xóa ảnh.', 'danger');
                }
            });
        }
    });
    // Xử lý nút Xóa
    $(document).on('click', '.btn-delete', function () {
        const id = $(this).data('id');
        const name = $(this).data('name');

        if (confirm(`Bạn có chắc chắn muốn xóa sản phẩm "${name}" không?`)) {
            const token = $('input[name="__RequestVerificationToken"]').first().val(); 
            $.ajax({
                url: `/Admin/Products/Delete/${id}`,
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

    // Đóng modal thì xóa nội dung
    $('#productModal').on('hidden.bs.modal', function () {
        modalContent.empty();
    });
});