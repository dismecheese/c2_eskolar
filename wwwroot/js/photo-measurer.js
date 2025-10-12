// Auto-measure images and set a CSS variable --aspect on the closest .single-photo-container
// Usage: include this script on pages with .single-photo elements or bundle it in your site's JS

(function () {
  'use strict';

  // Helper: process a single img element and attach handlers if not yet processed
  function processImage(img) {
    if (!img || img.dataset.photoMeasured === '1') return;

    // mark as processed to avoid duplicate work
    img.dataset.photoMeasured = '1';

    function applyAspect() {
      try {
        const w = img.naturalWidth || 0;
        const h = img.naturalHeight || 0;
        if (!w || !h) return;

        const container = img.closest('.single-photo-container');
        if (!container) return;

        // Set both variants for compatibility with CSS
        container.style.setProperty('--aspect', `${w}/${h}`);
        container.style.setProperty('--aspect-ratio', `${w}/${h}`);

        const src = img.currentSrc || img.src || '';
        if (src) {
          // Use URL wrapped in url("...") to be safe
          container.style.setProperty('--bg', `url("${src}")`);
        }

        if (!img.getAttribute('width') && !img.getAttribute('height')) {
          img.setAttribute('width', w);
          img.setAttribute('height', h);
        }
      } catch (err) {
        // ignore
      }
    }

    // If already loaded, apply immediately; otherwise wait for load
    if (img.complete && img.naturalWidth && img.naturalHeight) {
      applyAspect();
    } else {
      img.addEventListener('load', applyAspect, { once: true });
      img.addEventListener('error', () => {}, { once: true });
    }

    // Observe src/data-src changes
    const obs = new MutationObserver(mutations => {
      for (const m of mutations) {
        if (m.type === 'attributes' && (m.attributeName === 'src' || m.attributeName === 'data-src')) {
          if (img.complete && img.naturalWidth && img.naturalHeight) applyAspect();
          else img.addEventListener('load', applyAspect, { once: true });
        }
      }
    });
    obs.observe(img, { attributes: true, attributeFilter: ['src', 'data-src'] });
  }

  // Scan the document for any .single-photo images that haven't been processed yet
  function scanForImages() {
    const imgs = document.querySelectorAll('.single-photo');
    if (!imgs) return;
    imgs.forEach(img => processImage(img));
  }

  // Observe the document for added nodes so we can pick up images inserted by Blazor at runtime
  const bodyObserver = new MutationObserver(mutations => {
    let shouldScan = false;
    for (const m of mutations) {
      if (m.type === 'childList' && m.addedNodes && m.addedNodes.length > 0) {
        shouldScan = true;
        break;
      }
      if (m.type === 'attributes' && (m.attributeName === 'class' || m.attributeName === 'src' || m.attributeName === 'data-src')) {
        shouldScan = true;
        break;
      }
    }
    if (shouldScan) scanForImages();
  });

  // Start scanning once DOM is ready and set up observers
  function init() {
    scanForImages();
    if (document.body) {
      bodyObserver.observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['class', 'src', 'data-src'] });
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
