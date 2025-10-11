document.addEventListener("DOMContentLoaded", function () {
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
        selectElement.innerHTML = '<option value="" disabled>Chọn một tùy chọn</option>';
        data.forEach(item => {
            const option = document.createElement('option');
            option.value = item.name;
            option.textContent = item.name;
            option.setAttribute('data-code', item.code);
            selectElement.appendChild(option);
        });
        if (selectValue) {
            selectElement.value = selectValue;
        } else {
            selectElement.selectedIndex = 0;
        }
    };

    const loadAddressForEdit = async (modalId, cityName, districtName, wardName, detailValue) => {
        const modal = document.getElementById(`editAddressModal-${modalId}`);
        if (!modal) {
            console.error(`Modal with ID #editAddressModal-${modalId} not found.`);
            return;
        }

        const citySelect = document.getElementById(`city-edit-${modalId}`);
        const districtSelect = document.getElementById(`district-edit-${modalId}`);
        const wardSelect = document.getElementById(`ward-edit-${modalId}`);
        const detailInput = modal.querySelector('.detail-input');

        if (detailInput) {
            detailInput.value = detailValue;
        }

        const cities = await callAPI(host + '?depth=1');
        renderOptions(cities, citySelect, cityName);

        if (cityName) {
            const selectedCity = Array.from(citySelect.options).find(opt => opt.value === cityName);
            if (selectedCity) {
                const cityCode = selectedCity.getAttribute('data-code');
                const districts = await callAPI(`${host}p/${cityCode}?depth=2`);
                renderOptions(districts.districts, districtSelect, districtName);

                if (districtName) {
                    const selectedDistrict = Array.from(districtSelect.options).find(opt => opt.value === districtName);
                    if (selectedDistrict) {
                        const districtCode = selectedDistrict.getAttribute('data-code');
                        const wards = await callAPI(`${host}d/${districtCode}?depth=2`);
                        renderOptions(wards.wards, wardSelect, wardName);
                    }
                }
            }
        }
    };

    const addAddressModal = document.getElementById('addAddressModal');
    if (addAddressModal) {
        const addCitySelect = document.getElementById("city-add");
        const addDistrictSelect = document.getElementById("district-add");
        const addWardSelect = document.getElementById("ward-add");

        addAddressModal.addEventListener('shown.bs.modal', function () {
            callAPI(host + '?depth=1').then(data => renderOptions(data, addCitySelect));
            addDistrictSelect.innerHTML = '<option value="" disabled selected>Chọn một tùy chọn</option>';
            addWardSelect.innerHTML = '<option value="" disabled selected>Chọn một tùy chọn</option>';
        });

        addCitySelect.addEventListener('change', function () {
            const selectedOption = this.options[this.selectedIndex];
            addDistrictSelect.innerHTML = '<option value="" disabled selected>Chọn một tùy chọn</option>';
            addWardSelect.innerHTML = '<option value="" disabled selected>Chọn một tùy chọn</option>';
            if (selectedOption.value) {
                callAPI(host + 'p/' + selectedOption.getAttribute('data-code') + '?depth=2')
                    .then(data => renderOptions(data.districts, addDistrictSelect));
            }
        });

        addDistrictSelect.addEventListener('change', function () {
            const selectedOption = this.options[this.selectedIndex];
            addWardSelect.innerHTML = '<option value="" disabled selected>Chọn một tùy chọn</option>';
            if (selectedOption.value) {
                callAPI(host + 'd/' + selectedOption.getAttribute('data-code') + '?depth=2')
                    .then(data => renderOptions(data.wards, addWardSelect));
            }
        });
    }

    document.querySelectorAll('.edit-address-modal').forEach(modal => {
        modal.addEventListener('shown.bs.modal', function () {
            const button = document.querySelector(`[data-bs-target="#${modal.id}"]`);
            const addressId = button.dataset.addressId;
            const city = button.dataset.city;
            const district = button.dataset.district;
            const ward = button.dataset.ward;
            const detail = button.dataset.detail;
            loadAddressForEdit(addressId, city, district, ward, detail);
        });
    });

    document.querySelectorAll('.city-select').forEach(select => {
        select.addEventListener('change', function () {
            const selectedOption = this.options[this.selectedIndex];
            const modalBody = this.closest('.modal-body');
            const districtSelect = modalBody.querySelector('.district-select');
            const wardSelect = modalBody.querySelector('.ward-select');
            districtSelect.innerHTML = '<option value="" disabled selected>Chọn một tùy chọn</option>';
            wardSelect.innerHTML = '<option value="" disabled selected>Chọn một tùy chọn</option>';
            if (selectedOption.value) {
                callAPI(host + 'p/' + selectedOption.getAttribute('data-code') + '?depth=2')
                    .then(data => renderOptions(data.districts, districtSelect));
            }
        });
    });

    document.querySelectorAll('.district-select').forEach(select => {
        select.addEventListener('change', function () {
            const selectedOption = this.options[this.selectedIndex];
            const modalBody = this.closest('.modal-body');
            const wardSelect = modalBody.querySelector('.ward-select');
            wardSelect.innerHTML = '<option value="" disabled selected>Chọn một tùy chọn</option>';
            if (selectedOption.value) {
                callAPI(host + 'd/' + selectedOption.getAttribute('data-code') + '?depth=2')
                    .then(data => renderOptions(data.wards, wardSelect));
            }
        });
    });
});