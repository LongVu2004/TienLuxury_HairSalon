document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    const content = document.getElementById('content');
    const toggleBtn = document.getElementById('toggle-btn');
    const settingsBtn = document.querySelector('.settings-btn');
    const submenu = document.getElementById('settings-submenu');
    const menuLinks = document.querySelectorAll('.sidebar a:not(.settings-btn)');

    // Toggle sidebar
    toggleBtn.addEventListener('click', function () {
        sidebar.classList.toggle('expanded');
        content.classList.toggle('expanded');

        // Save state to localStorage
        const expanded = sidebar.classList.contains('expanded');
        localStorage.setItem('sidebarExpanded', expanded);
    });

    // Toggle submenu
    settingsBtn.addEventListener('click', function (e) {
        e.preventDefault();
        submenu.classList.toggle('active');
    });

    // Set active link
    menuLinks.forEach(link => {
        link.addEventListener('click', function () {
            menuLinks.forEach(item => item.classList.remove('active-link'));
            settingsBtn.classList.remove('active-link');
            this.classList.add('active-link');
        });
    });

    // Add smooth animation for hover effect
    const sidebarLinks = document.querySelectorAll('.sidebar a');
    sidebarLinks.forEach(link => {
        link.addEventListener('mouseenter', function () {
            this.style.transition = 'all 0.3s ease';
        });

        link.addEventListener('mouseleave', function () {
            this.style.transition = 'all 0.3s ease';
        });
    });

    function updateDateTime() {
        const now = new Date();
        const date = now.toLocaleDateString('vi-VN');
        const time = now.toLocaleTimeString('vi-VN');
        document.getElementById("datetime").textContent = `${date} - ${time}`;
      }
    updateDateTime();
    setInterval(updateDateTime, 1000);

});
