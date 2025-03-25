/**
 * Main JavaScript for Intern Management System
 */

// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Handle expandable menu items
    const menuItems = document.querySelectorAll('.nav-group > .nav-link');

    menuItems.forEach(function (item) {
        item.addEventListener('click', function (e) {
            const submenu = this.getAttribute('href');
            if (submenu.startsWith('#')) {
                e.preventDefault();
                const targetMenu = document.querySelector(submenu);
                if (targetMenu) {
                    if (targetMenu.classList.contains('show')) {
                        // Close this submenu
                        targetMenu.classList.remove('show');
                        this.setAttribute('aria-expanded', 'false');
                    } else {
                        // Close other open submenus
                        document.querySelectorAll('.sidebar-menu .collapse.show').forEach(function (menu) {
                            if (menu !== targetMenu) {
                                menu.classList.remove('show');
                                const menuTrigger = document.querySelector(`[href="#${menu.id}"]`);
                                if (menuTrigger) {
                                    menuTrigger.setAttribute('aria-expanded', 'false');
                                }
                            }
                        });

                        // Open this submenu
                        targetMenu.classList.add('show');
                        this.setAttribute('aria-expanded', 'true');
                    }
                }
            }
        });
    });

    // Update current time
    function updateDateTime() {
        const now = new Date();
        const dateStr = now.toISOString().split('T')[0];
        const timeStr = now.toTimeString().substring(0, 5);

        const dateElement = document.getElementById('current-date');
        const timeElement = document.getElementById('current-time');

        if (dateElement) dateElement.textContent = dateStr;
        if (timeElement) timeElement.textContent = timeStr;
    }

    // Update time initially and then every minute
    updateDateTime();
    setInterval(updateDateTime, 60000);

    // Mark current active menu item
    const highlightCurrentPage = function () {
        const currentPath = window.location.pathname;

        document.querySelectorAll('.sidebar-menu .nav-link').forEach(function (link) {
            const href = link.getAttribute('href');
            if (href && !href.startsWith('#') && currentPath.includes(href)) {
                link.classList.add('active');

                // If the active link is in a submenu, expand the parent menu
                const parentCollapseMenu = link.closest('.collapse');
                if (parentCollapseMenu) {
                    parentCollapseMenu.classList.add('show');
                    const parentNavLink = document.querySelector(`[href="#${parentCollapseMenu.id}"]`);
                    if (parentNavLink) {
                        parentNavLink.setAttribute('aria-expanded', 'true');
                    }
                }
            }
        });
    };

    highlightCurrentPage();
});