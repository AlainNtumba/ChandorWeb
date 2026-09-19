(function () {
    const editors = new Map();

    function clamp(value, min, max) {
        return Math.min(max, Math.max(min, value));
    }

    function rotatedSize(state) {
        const quarterTurn = Math.abs(state.rotation % 180) === 90;
        return {
            width: quarterTurn ? state.image.naturalHeight : state.image.naturalWidth,
            height: quarterTurn ? state.image.naturalWidth : state.image.naturalHeight
        };
    }

    function updateBaseScale(state) {
        const size = rotatedSize(state);
        state.baseScale = Math.max(
            state.canvas.width / size.width,
            state.canvas.height / size.height);
    }

    function updateCanvasOrientation(state) {
        const quarterTurn = Math.abs(state.rotation % 180) === 90;
        state.canvas.width = quarterTurn ? state.baseCanvasHeight : state.baseCanvasWidth;
        state.canvas.height = quarterTurn ? state.baseCanvasWidth : state.baseCanvasHeight;
        const stage = state.canvas.parentElement;
        if (stage) {
            stage.style.width = `${state.canvas.width}px`;
            stage.style.maxWidth = '100%';
            stage.style.aspectRatio = `${state.canvas.width} / ${state.canvas.height}`;
        }
    }

    function resetCrop(state) {
        state.crop = {
            x: 0,
            y: 0,
            width: state.canvas.width,
            height: state.canvas.height
        };
        updateCropOverlay(state);
    }

    function updateCropOverlay(state) {
        if (!state.grid || !state.crop) return;
        state.grid.style.left = `${state.crop.x / state.canvas.width * 100}%`;
        state.grid.style.top = `${state.crop.y / state.canvas.height * 100}%`;
        state.grid.style.width = `${state.crop.width / state.canvas.width * 100}%`;
        state.grid.style.height = `${state.crop.height / state.canvas.height * 100}%`;
        state.grid.style.right = 'auto';
        state.grid.style.bottom = 'auto';
    }

    function resizeCrop(state, handle, deltaX, deltaY) {
        const crop = state.crop;
        const minimum = Math.min(60, state.canvas.width, state.canvas.height);

        if (handle === 'move') {
            crop.x = clamp(crop.x + deltaX, 0, state.canvas.width - crop.width);
            crop.y = clamp(crop.y + deltaY, 0, state.canvas.height - crop.height);
            return;
        }

        if (handle.includes('w')) {
            const nextX = clamp(crop.x + deltaX, 0, crop.x + crop.width - minimum);
            crop.width += crop.x - nextX;
            crop.x = nextX;
        }
        if (handle.includes('e')) {
            crop.width = clamp(crop.width + deltaX, minimum, state.canvas.width - crop.x);
        }
        if (handle.includes('n')) {
            const nextY = clamp(crop.y + deltaY, 0, crop.y + crop.height - minimum);
            crop.height += crop.y - nextY;
            crop.y = nextY;
        }
        if (handle.includes('s')) {
            crop.height = clamp(crop.height + deltaY, minimum, state.canvas.height - crop.y);
        }
    }

    function constrainPosition(state) {
        const size = rotatedSize(state);
        const scale = state.baseScale * state.zoom;
        const overflowX = Math.max(0, (size.width * scale - state.canvas.width) / 2);
        const overflowY = Math.max(0, (size.height * scale - state.canvas.height) / 2);
        state.offsetX = clamp(state.offsetX, -overflowX, overflowX);
        state.offsetY = clamp(state.offsetY, -overflowY, overflowY);
    }

    function render(state) {
        constrainPosition(state);
        const context = state.canvas.getContext('2d');
        context.clearRect(0, 0, state.canvas.width, state.canvas.height);
        context.fillStyle = '#111827';
        context.fillRect(0, 0, state.canvas.width, state.canvas.height);
        context.save();
        context.translate(
            state.canvas.width / 2 + state.offsetX,
            state.canvas.height / 2 + state.offsetY);
        context.rotate(state.rotation * Math.PI / 180);
        const scale = state.baseScale * state.zoom;
        context.scale(scale, scale);
        context.drawImage(
            state.image,
            -state.image.naturalWidth / 2,
            -state.image.naturalHeight / 2);
        context.restore();
        updateCropOverlay(state);
    }

    function toBlob(canvas, type, quality) {
        return new Promise((resolve, reject) => {
            canvas.toBlob(blob => blob ? resolve(blob) : reject(new Error('Image export failed.')), type, quality);
        });
    }

    function blobToBase64(blob) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result.split(',')[1]);
            reader.onerror = () => reject(reader.error || new Error('Unable to read optimized image.'));
            reader.readAsDataURL(blob);
        });
    }

    function drawOutput(state, width, height) {
        const output = document.createElement('canvas');
        output.width = width;
        output.height = height;
        const context = output.getContext('2d');
        const outputScale = width / state.crop.width;
        context.fillStyle = '#ffffff';
        context.fillRect(0, 0, width, height);
        context.save();
        context.translate(
            (state.canvas.width / 2 + state.offsetX - state.crop.x) * outputScale,
            (state.canvas.height / 2 + state.offsetY - state.crop.y) * outputScale);
        context.rotate(state.rotation * Math.PI / 180);
        const imageScale = state.baseScale * state.zoom * outputScale;
        context.scale(imageScale, imageScale);
        context.drawImage(
            state.image,
            -state.image.naturalWidth / 2,
            -state.image.naturalHeight / 2);
        context.restore();
        return output;
    }

    window.chandorImageCropper = {
        initialize: function (canvasId, sourceUrl) {
            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                throw new Error('Crop canvas not found.');
            }

            window.chandorImageCropper.dispose(canvasId);

            return new Promise((resolve, reject) => {
                const image = new Image();
                image.onload = () => {
                    const previewScale = Math.min(
                        1,
                        500 / image.naturalWidth,
                        500 / image.naturalHeight);
                    const previewWidth = Math.max(1, Math.round(image.naturalWidth * previewScale));
                    const previewHeight = Math.max(1, Math.round(image.naturalHeight * previewScale));
                    const state = {
                        canvas,
                        image,
                        grid: canvas.parentElement?.querySelector('.image-cropper-grid') || null,
                        baseCanvasWidth: previewWidth,
                        baseCanvasHeight: previewHeight,
                        rotation: 0,
                        zoom: 1,
                        baseScale: 1,
                        offsetX: 0,
                        offsetY: 0,
                        dragging: false,
                        lastX: 0,
                        lastY: 0,
                        cropDragging: false,
                        cropHandle: 'move',
                        handlers: {}
                    };

                    updateCanvasOrientation(state);
                    updateBaseScale(state);
                    resetCrop(state);

                    state.handlers.pointerDown = event => {
                        state.dragging = true;
                        state.lastX = event.clientX;
                        state.lastY = event.clientY;
                        canvas.setPointerCapture(event.pointerId);
                    };
                    state.handlers.pointerMove = event => {
                        if (!state.dragging) return;
                        const rect = canvas.getBoundingClientRect();
                        state.offsetX += (event.clientX - state.lastX) * canvas.width / rect.width;
                        state.offsetY += (event.clientY - state.lastY) * canvas.height / rect.height;
                        state.lastX = event.clientX;
                        state.lastY = event.clientY;
                        render(state);
                    };
                    state.handlers.pointerUp = event => {
                        state.dragging = false;
                        if (canvas.hasPointerCapture(event.pointerId)) {
                            canvas.releasePointerCapture(event.pointerId);
                        }
                    };
                    state.handlers.wheel = event => {
                        event.preventDefault();
                        state.zoom = clamp(state.zoom + (event.deltaY < 0 ? 0.08 : -0.08), 1, 4);
                        render(state);
                    };
                    state.handlers.cropPointerDown = event => {
                        event.preventDefault();
                        event.stopPropagation();
                        state.cropDragging = true;
                        state.cropHandle = event.target.dataset.cropHandle || 'move';
                        state.lastX = event.clientX;
                        state.lastY = event.clientY;
                        state.grid.setPointerCapture(event.pointerId);
                    };
                    state.handlers.cropPointerMove = event => {
                        if (!state.cropDragging) return;
                        event.preventDefault();
                        const rect = canvas.getBoundingClientRect();
                        const deltaX = (event.clientX - state.lastX) * canvas.width / rect.width;
                        const deltaY = (event.clientY - state.lastY) * canvas.height / rect.height;
                        state.lastX = event.clientX;
                        state.lastY = event.clientY;
                        resizeCrop(state, state.cropHandle, deltaX, deltaY);
                        updateCropOverlay(state);
                    };
                    state.handlers.cropPointerUp = event => {
                        state.cropDragging = false;
                        if (state.grid.hasPointerCapture(event.pointerId)) {
                            state.grid.releasePointerCapture(event.pointerId);
                        }
                    };

                    canvas.addEventListener('pointerdown', state.handlers.pointerDown);
                    canvas.addEventListener('pointermove', state.handlers.pointerMove);
                    canvas.addEventListener('pointerup', state.handlers.pointerUp);
                    canvas.addEventListener('pointercancel', state.handlers.pointerUp);
                    canvas.addEventListener('wheel', state.handlers.wheel, { passive: false });
                    if (state.grid) {
                        state.grid.addEventListener('pointerdown', state.handlers.cropPointerDown);
                        state.grid.addEventListener('pointermove', state.handlers.cropPointerMove);
                        state.grid.addEventListener('pointerup', state.handlers.cropPointerUp);
                        state.grid.addEventListener('pointercancel', state.handlers.cropPointerUp);
                    }
                    editors.set(canvasId, state);
                    render(state);
                    resolve();
                };
                image.onerror = () => reject(new Error('Unable to load the selected image.'));
                image.src = sourceUrl;
            });
        },

        setZoom: function (canvasId, zoom) {
            const state = editors.get(canvasId);
            if (!state) return;
            state.zoom = clamp(Number(zoom) || 1, 1, 4);
            render(state);
        },

        rotate: function (canvasId, degrees) {
            const state = editors.get(canvasId);
            if (!state) return;
            state.rotation = (state.rotation + degrees) % 360;
            state.offsetX = 0;
            state.offsetY = 0;
            updateCanvasOrientation(state);
            updateBaseScale(state);
            resetCrop(state);
            render(state);
        },

        reset: function (canvasId) {
            const state = editors.get(canvasId);
            if (!state) return;
            state.rotation = 0;
            state.zoom = 1;
            state.offsetX = 0;
            state.offsetY = 0;
            updateCanvasOrientation(state);
            updateBaseScale(state);
            resetCrop(state);
            render(state);
        },

        exportImage: async function (canvasId, targetBytes) {
            const state = editors.get(canvasId);
            if (!state) {
                throw new Error('Crop editor is not initialized.');
            }

            let availableCropWidth = Math.floor(
                state.crop.width / (state.baseScale * state.zoom));
            let availableCropHeight = Math.floor(
                state.crop.height / (state.baseScale * state.zoom));
            const maximumDimension = 1600;
            const dimensionScale = Math.min(
                1,
                maximumDimension / availableCropWidth,
                maximumDimension / availableCropHeight);
            let width = Math.max(1, Math.round(availableCropWidth * dimensionScale));
            let height = Math.max(1, Math.round(availableCropHeight * dimensionScale));
            let blob = null;

            while (true) {
                const output = drawOutput(state, width, height);
                for (let quality = 0.88; quality >= 0.48; quality -= 0.07) {
                    blob = await toBlob(output, 'image/webp', quality);
                    if (blob.size <= targetBytes) break;
                }

                if (blob.size <= targetBytes || Math.max(width, height) <= 600) break;
                const resizeScale = Math.max(600 / Math.max(width, height), 0.84);
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
        },

        dispose: function (canvasId) {
            const state = editors.get(canvasId);
            if (!state) return;
            const canvas = state.canvas;
            canvas.removeEventListener('pointerdown', state.handlers.pointerDown);
            canvas.removeEventListener('pointermove', state.handlers.pointerMove);
            canvas.removeEventListener('pointerup', state.handlers.pointerUp);
            canvas.removeEventListener('pointercancel', state.handlers.pointerUp);
            canvas.removeEventListener('wheel', state.handlers.wheel);
            if (state.grid) {
                state.grid.removeEventListener('pointerdown', state.handlers.cropPointerDown);
                state.grid.removeEventListener('pointermove', state.handlers.cropPointerMove);
                state.grid.removeEventListener('pointerup', state.handlers.cropPointerUp);
                state.grid.removeEventListener('pointercancel', state.handlers.cropPointerUp);
            }
            editors.delete(canvasId);
        }
    };
})();
