(function () {
    function loadImage(sourceUrl) {
        return new Promise((resolve, reject) => {
            const image = new Image();
            image.onload = () => resolve(image);
            image.onerror = () => reject(new Error('Unable to load the selected image.'));
            image.src = sourceUrl;
        });
    }

    function toBlob(canvas, contentType, quality) {
        return new Promise((resolve, reject) => {
            canvas.toBlob(
                blob => blob ? resolve(blob) : reject(new Error('Image compression failed.')),
                contentType,
                quality);
        });
    }

    function blobToBase64(blob) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result.split(',')[1]);
            reader.onerror = () => reject(reader.error || new Error('Unable to read compressed image.'));
            reader.readAsDataURL(blob);
        });
    }

    function render(image, width, height) {
        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        const context = canvas.getContext('2d');
        context.drawImage(image, 0, 0, width, height);
        return canvas;
    }

    window.chandorImageCompression = {
        compress: async function (sourceUrl, targetBytes, maximumDimension) {
            const image = await loadImage(sourceUrl);
            const initialScale = Math.min(
                1,
                maximumDimension / Math.max(image.naturalWidth, image.naturalHeight));
            let width = Math.max(1, Math.round(image.naturalWidth * initialScale));
            let height = Math.max(1, Math.round(image.naturalHeight * initialScale));
            let blob = null;

            while (true) {
                const canvas = render(image, width, height);
                for (let quality = 0.86; quality >= 0.44; quality -= 0.07) {
                    blob = await toBlob(canvas, 'image/webp', quality);
                    if (blob.size <= targetBytes) break;
                }

                if (blob.size <= targetBytes || Math.max(width, height) <= 400) break;
                const resizeScale = Math.max(400 / Math.max(width, height), 0.84);
                width = Math.max(1, Math.round(width * resizeScale));
                height = Math.max(1, Math.round(height * resizeScale));
            }

            return {
                base64: await blobToBase64(blob),
                contentType: blob.type || 'image/webp',
                size: blob.size,
                width,
                height
            };
        }
    };
})();
