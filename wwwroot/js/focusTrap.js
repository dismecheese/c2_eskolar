export function activate(container) {
  if (!container) return null;
  const previousActive = document.activeElement;
  const focusableSelector = [
    'a[href]',
    'area[href]',
    'input:not([disabled]):not([type="hidden"])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    'button:not([disabled])',
    'iframe',
    'object',
    'embed',
    '[contenteditable]',
    '[tabindex]:not([tabindex="-1"])'
  ].join(',');

  function getFocusable() {
    return Array.from(container.querySelectorAll(focusableSelector))
      .filter(el => (el.offsetWidth > 0 || el.offsetHeight > 0) || el === document.activeElement);
  }

  function handleKey(e) {
    if (e.key !== 'Tab') return;
    const focusables = getFocusable();
    if (!focusables.length) {
      e.preventDefault();
      return;
    }
    const idx = focusables.indexOf(document.activeElement);
    if (e.shiftKey) {
      if (idx <= 0) {
        e.preventDefault();
        focusables[focusables.length - 1].focus();
      }
    } else {
      if (idx === -1 || idx === focusables.length - 1) {
        e.preventDefault();
        focusables[0].focus();
      }
    }
  }

  function handleFocusIn(e) {
    if (!container.contains(e.target)) {
      const focusables = getFocusable();
      if (focusables.length) focusables[0].focus();
      e.preventDefault();
    }
  }

  document.addEventListener('keydown', handleKey, true);
  document.addEventListener('focusin', handleFocusIn, true);

  const auto = container.querySelector('[data-autofocus]');
  const closeBtn = container.querySelector('.viewer-close, .modal-close, button[aria-label="Close"]');
  const focusables = getFocusable();
  try {
    (auto || closeBtn || focusables[0] || container).focus();
  } catch (err) {}

  return {
    deactivate: () => {
      document.removeEventListener('keydown', handleKey, true);
      document.removeEventListener('focusin', handleFocusIn, true);
      try { previousActive && previousActive.focus(); } catch (err) {}
    }
  };
}

window.__focusTrap = window.__focusTrap || {};
window.__focusTrap.activate = async (element) => {
  return (await import('/js/focusTrap.js')).activate(element);
};