(() => {
  const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value
    || document.querySelector('#af-token')?.value;

  window.showToast = function (message, type = 'success') {
    const stack = document.getElementById('toast-stack');
    if (!stack) return;
    const el = document.createElement('div');
    el.className = `toast-item ${type === 'error' ? 'error' : ''}`;
    el.textContent = message;
    stack.appendChild(el);
    setTimeout(() => el.remove(), 2800);
  };

  async function postForm(url, data) {
    const body = new URLSearchParams(data);
    if (token) body.append('__RequestVerificationToken', token);
    const res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
      body
    });
    return res.json();
  }

  document.addEventListener('click', async (e) => {
    const addBtn = e.target.closest('[data-add-cart]');
    if (addBtn) {
      e.preventDefault();
      if (addBtn.disabled) return;
      addBtn.disabled = true;
      const old = addBtn.innerHTML;
      addBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
      try {
        const json = await postForm('/cart/add', { productId: addBtn.dataset.addCart, quantity: addBtn.dataset.qty || 1 });
        if (json.login) { location.href = '/account/login?returnUrl=' + encodeURIComponent(location.pathname); return; }
        showToast(json.message, json.ok ? 'success' : 'error');
        if (json.count !== undefined) {
          const badge = document.getElementById('cart-count');
          if (badge) badge.textContent = json.count;
        }
      } catch { showToast('Không thể thêm giỏ hàng.', 'error'); }
      addBtn.disabled = false;
      addBtn.innerHTML = old;
    }

    const wishBtn = e.target.closest('[data-wish]');
    if (wishBtn) {
      e.preventDefault();
      try {
        const json = await postForm('/wishlist/toggle', { productId: wishBtn.dataset.wish });
        if (json.login) { location.href = '/account/login?returnUrl=' + encodeURIComponent(location.pathname); return; }
        showToast(json.message, json.ok ? 'success' : 'error');
        const badge = document.getElementById('wish-count');
        if (badge && json.count !== undefined) badge.textContent = json.count;
      } catch { showToast('Không thể cập nhật yêu thích.', 'error'); }
    }

    const cmpBtn = e.target.closest('[data-compare]');
    if (cmpBtn) {
      e.preventDefault();
      try {
        const json = await postForm('/compare/toggle', { productId: cmpBtn.dataset.compare });
        showToast(json.message, json.ok ? 'success' : 'error');
      } catch { showToast('Không thể so sánh.', 'error'); }
    }
  });

  const search = document.getElementById('site-search');
  const box = document.getElementById('suggest-box');
  let timer;
  if (search && box) {
    search.addEventListener('input', () => {
      clearTimeout(timer);
      const q = search.value.trim();
      if (q.length < 2) { box.style.display = 'none'; return; }
      timer = setTimeout(async () => {
        const res = await fetch('/search/suggest?q=' + encodeURIComponent(q));
        const items = await res.json();
        box.innerHTML = items.length
          ? items.map(i => `<a class="suggest-item" href="/products/${i.slug}"><img src="${i.imageUrl}" alt=""><div><strong>${i.name}</strong><div class="muted">${i.price}</div></div></a>`).join('')
          : '<div class="suggest-item muted">Không có gợi ý</div>';
        box.style.display = 'block';
      }, 220);
    });
    document.addEventListener('click', (e) => {
      if (!e.target.closest('.search-wrap')) box.style.display = 'none';
    });
  }

  const news = document.getElementById('newsletter-form');
  if (news) {
    news.addEventListener('submit', async (e) => {
      e.preventDefault();
      const email = news.querySelector('input[name=email]').value;
      const fd = new FormData(news);
      const res = await fetch('/Home/Newsletter', { method: 'POST', body: new URLSearchParams(fd) });
      const json = await res.json();
      showToast(json.message, json.ok ? 'success' : 'error');
    });
  }
})();
