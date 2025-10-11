// Hàm để thay đổi ảnh chính khi click vào thumbnail
function changeImage(imageUrl) {
    const mainImage = document.getElementById('mainImage');
    if (mainImage) {
        mainImage.src = imageUrl;
    }

    // Cập nhật trạng thái 'active' cho thumbnail
    const thumbnails = document.querySelectorAll('.thumbnail-item');
    thumbnails.forEach(item => {
        item.classList.remove('active');
        // So sánh URL một cách an toàn hơn
        if (item.querySelector('img').src.includes(imageUrl)) {
            item.classList.add('active');
        }
    });
}

// Gắn sự kiện click cho các thumbnail khi trang đã tải xong
document.addEventListener('DOMContentLoaded', function () {
    const thumbnails = document.querySelectorAll('.thumbnail-item img');
    thumbnails.forEach(thumb => {
        thumb.addEventListener('click', function () {
            changeImage(this.src);
        });
    });
});