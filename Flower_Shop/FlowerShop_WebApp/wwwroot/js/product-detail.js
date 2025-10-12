function changeImage(imageUrl) {
    const mainImage = document.getElementById('mainImage');
    if (mainImage) {
        mainImage.src = imageUrl;
    }

    const thumbnails = document.querySelectorAll('.thumbnail-item');
    thumbnails.forEach(item => {
        item.classList.remove('active');
        if (item.querySelector('img').src.includes(imageUrl)) {
            item.classList.add('active');
        }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    const thumbnails = document.querySelectorAll('.thumbnail-item img');
    thumbnails.forEach(thumb => {
        thumb.addEventListener('click', function () {
            changeImage(this.src);
        });
    });
});