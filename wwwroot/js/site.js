// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ══════════════════════════════════════════════
//  DARK / LIGHT MODE TOGGLE
// ══════════════════════════════════════════════
(function () {
    const STORAGE_KEY = 'etkincity-theme';
    const html = document.documentElement;

    // Sayfa yüklendiğinde kayıtlı temayı uygula (flash önleme inline script'te yapılıyor,
    // burada sadece buton durumunu güncelliyoruz)
    function applyTheme(theme) {
        if (theme === 'dark') {
            html.setAttribute('data-theme', 'dark');
        } else {
            html.removeAttribute('data-theme');
        }
        localStorage.setItem(STORAGE_KEY, theme);
    }

    document.addEventListener('DOMContentLoaded', function () {
        const btn = document.getElementById('theme-toggle');
        if (!btn) return;

        // Mevcut temayı al
        const currentTheme = localStorage.getItem(STORAGE_KEY) || 'light';
        applyTheme(currentTheme);

        // Toggle tıklama
        btn.addEventListener('click', function () {
            const isDark = html.getAttribute('data-theme') === 'dark';
            applyTheme(isDark ? 'light' : 'dark');

            // Buton animasyonu
            btn.style.transform = 'rotate(360deg) scale(1.15)';
            setTimeout(function () {
                btn.style.transform = '';
            }, 380);
        });
    });
})();
