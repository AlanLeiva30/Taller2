(() => {
  'use strict';
  function brandSwagger() {
    const wrapper = document.querySelector('.swagger-ui .topbar-wrapper');
    if (!wrapper || wrapper.querySelector('.aula-api-brand')) return Boolean(wrapper);
    const brand = document.createElement('a');
    brand.href = '/';
    brand.className = 'aula-api-brand';
    brand.textContent = 'aula';
    brand.setAttribute('aria-label', 'Aula, portal de gestión académica');
    const label = document.createElement('span');
    label.textContent = 'DOCUMENTACIÓN API';
    brand.append(label);
    const home = document.createElement('a');
    home.href = '/';
    home.className = 'aula-api-home';
    home.textContent = 'Ir al portal ↗';
    wrapper.prepend(brand);
    wrapper.append(home);
    document.title = 'API · Aula';
    return true;
  }
  function observe() {
    if (brandSwagger()) return;
    const observer = new MutationObserver(() => { if (brandSwagger()) observer.disconnect(); });
    observer.observe(document.body, { childList: true, subtree: true });
    setTimeout(() => observer.disconnect(), 20000);
  }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', observe, { once: true });
  else observe();
})();
