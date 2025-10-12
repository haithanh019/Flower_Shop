$(document).ready(function () {
    const host = "https://provinces.open-api.vn/api/";
    const callAPI = async (api) => {
        try {
            const response = await fetch(api);
            return await response.json();
        } catch (error) {
            console.error('API call failed:', error);
        }
    };
    const renderOptions = (data, selectElement, selectValue = null) => {
        selectElement.innerHTML = '<option value="" disabled>-- Chọn --</option>';
        data.forEach(item => {
            const option = new Option(item.name, item.name);
            option.dataset.code = item.code;
            selectElement.add(option);
        });
        if (selectValue) selectElement.value = selectValue;
    };
    // Xử lý modal "Thêm mới"
    $('#addAddressModal').on('shown.bs.modal', function () {
        const citySelect = $("#city-add");
        const districtSelect = $("#district-add");
        const wardSelect = $("#ward-add");
        callAPI(host + '?depth=1').then(data => renderOptions(data, citySelect[0]));
        citySelect.on('change', function () {
            districtSelect.empty(); wardSelect.empty();
            const code = $(this).find(':selected').data('code');
            callAPI(host + 'p/' + code + '?depth=2').then(data => renderOptions(data.districts, districtSelect[0]));
        });
        districtSelect.on('change', function () {
            wardSelect.empty();
            const code = $(this).find(':selected').data('code');
            callAPI(host + 'd/' + code + '?depth=2').then(data => renderOptions(data.wards, wardSelect[0]));
        });
    });
    // Xử lý modal "Chỉnh sửa"
    $('.edit-address-modal').on('shown.bs.modal', async function (event) {
        const button = $(event.relatedTarget);
        const addressId = button.data('address-id');
        const city = button.data('city');
        const district = button.data('district');
        const ward = button.data('ward');
        const citySelect = $(`#city-edit-${addressId}`);
        const districtSelect = $(`#district-edit-${addressId}`);
        const wardSelect = $(`#ward-edit-${addressId}`);

        const cities = await callAPI(host + '?depth=1');
        renderOptions(cities, citySelect[0], city);

        const cityCode = citySelect.find(':selected').data('code');
        if (cityCode) {
            const districts = await callAPI(`${host}p/${cityCode}?depth=2`);
            renderOptions(districts.districts, districtSelect[0], district);
        }

        const districtCode = districtSelect.find(':selected').data('code');
        if (districtCode) {
            const wards = await callAPI(`${host}d/${districtCode}?depth=2`);
            renderOptions(wards.wards, wardSelect[0], ward);
        }

        citySelect.on('change', function () {
            districtSelect.empty(); wardSelect.empty();
            const code = $(this).find(':selected').data('code');
            callAPI(host + 'p/' + code + '?depth=2').then(data => renderOptions(data.districts, districtSelect[0]));
        });

        districtSelect.on('change', function () {
            wardSelect.empty();
            const code = $(this).find(':selected').data('code');
            callAPI(host + 'd/' + code + '?depth=2').then(data => renderOptions(data.wards, wardSelect[0]));
        });
    });


    // --- PHẦN XỬ LÝ MỚI ---

    // Hàm hiển thị thông báo
    const showAlert = (placeholderId, message, type) => {
        const alertPlaceholder = document.getElementById(placeholderId);
        const wrapper = document.createElement('div');
        wrapper.innerHTML = [
            `<div class="alert alert-${type} alert-dismissible" role="alert">`,
            `   <div>${message}</div>`,
            '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
            '</div>'
        ].join('');
        alertPlaceholder.append(wrapper);
    };

    // Xử lý form đổi mật khẩu
    $('#changePasswordModal form').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var alertPlaceholder = $('#changePasswordAlertPlaceholder');
        alertPlaceholder.empty(); // Xóa thông báo cũ

        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    $('#changePasswordModal').modal('hide');
                    form[0].reset();
                    showAlert('profile-alert-placeholder', response.message, 'success');
                }
            },
            error: function (xhr) {
                var response = xhr.responseJSON;
                var errorMsg = 'An unexpected error occurred.';
                if (response && response.errors) {
                    errorMsg = response.errors.join('<br>');
                }
                showAlert('changePasswordAlertPlaceholder', errorMsg, 'danger');
            }
        });
    });

    // Xử lý modal lịch sử đơn hàng
    $('#orderHistoryModal').on('show.bs.modal', function () {
        var modalContent = $('#orderHistoryModalContent');
        modalContent.html('<div class="modal-body text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');

        $.get('/Orders/HistoryPartial', function (data) {
            modalContent.html(data);
        }).fail(function () {
            modalContent.html('<div class="modal-body"><p class="text-danger">Failed to load order history.</p></div>');
        });
    });

    // Xử lý click nút "View Details" trong modal lịch sử
    $('#orderHistoryModal').on('click', '.view-details-btn', function (e) {
        e.preventDefault();
        var orderId = $(this).data('order-id');
        var modalContent = $('#orderDetailModalContent');

        modalContent.html('<div class="modal-body text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        $('#orderDetailModal').modal('show');

        $.get('/Orders/DetailsPartial?id=' + orderId, function (data) {
            modalContent.html(data);
        }).fail(function () {
            modalContent.html('<div class="modal-body"><p class="text-danger">Failed to load order details.</p></div>');
        });
    });

    // Reset form khi modal đổi mật khẩu đóng lại
    $('#changePasswordModal').on('hidden.bs.modal', function () {
        $(this).find('form')[0].reset();
        $('#changePasswordAlertPlaceholder').empty();
    });
});