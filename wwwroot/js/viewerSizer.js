export function attach(container) {
  if (!container) return;
  const img = container.querySelector('.viewer-image');
  function applySizing(image) {
    try {
      const nw = image.naturalWidth || image.width;
      const nh = image.naturalHeight || image.height;
      if (nw && nh) {
        container.style.maxWidth = Math.min(window.innerWidth * 0.98, Math.min(1100, nw)) + 'px';
        container.style.maxHeight = Math.min(window.innerHeight * 0.94, nh) + 'px';
      }
    } catch (e) {}
  }
  if (img) {
    if (img.complete) applySizing(img);
    else img.addEventListener('load', () => applySizing(img));
  }
}

export function detach(container) {
  try {
    // no-op for now
  } catch (e) {}
}
