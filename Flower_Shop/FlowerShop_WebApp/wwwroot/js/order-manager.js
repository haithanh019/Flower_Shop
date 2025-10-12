$(function () {
    const orderModal = new bootstrap.Modal(document.getElementById('orderModal'));
    const modalContent = $('#orderModalContent');
    const tableContainer = $('#orderTableContainer');
    const filterForm = $('#filterForm');
    const filterSelect = filterForm.find('select[name="statusFilter"]');

    function refreshTable(page = 1) {
        const filter = filterSelect.val();
        const url = `/Admin/Orders/Index?statusFilter=${filter}&pageNumber=${page}`;

        $.get(url, function (data) {
            tableContainer.html(data);
        }).fail(function () {
            showToast('Không thể tải danh sách đơn hàng.', 'danger');
        });
    }

    // Lọc khi thay đổi dropdown
    filterSelect.on('change', function () {
        refreshTable(1); // Luôn về trang 1 khi lọc
    });

    // Phân trang
    $(document).on('click', '.pagination a', function (e) {
        e.preventDefault();
        const page = $(this).data('page');
        refreshTable(page);
    });

    // Mở modal để xem chi tiết
    $(document).on('click', '.btn-view-details', function () {
        const url = $(this).data('url');
        modalContent.html('<div class="modal-body text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        orderModal.show();

        $.get(url, function (data) {
            modalContent.html(data);
            $.validator.unobtrusive.parse(modalContent.find('form'));
        }).fail(function () {
            modalContent.html('<div class="modal-body"><p class="text-danger">Không thể tải chi tiết đơn hàng.</p></div>');
        });
    });

    // Xử lý submit form cập nhật trạng thái
    $(document).on('submit', '#orderStatusForm', function (e) {
        e.preventDefault();
        var form = $(this);
        var alertPlaceholder = $('#modal-alert-placeholder');
        alertPlaceholder.empty();

        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    orderModal.hide();
                    showToast(response.message, 'success');
                    refreshTable($('.pagination .active a').data('page') || 1); 
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
    $('#orderModal').on('hidden.bs.modal', function () {
        modalContent.empty();
    });
});