window.customScripts = {
    preventDropdownClose: () => {
        try {
            document.querySelectorAll(".dropdown-menu.stop").forEach(e => {
                e.addEventListener("click", function (e) {
                    e.stopPropagation();
                });
            });
        } catch (e) { }
    },
    toggleDarkLightMode: () => {
        try {
            const themeColorToggle = document.getElementById("light-dark-mode");
            if (themeColorToggle) {
                themeColorToggle.addEventListener("click", function () {
                    const currentTheme = document.documentElement.getAttribute("data-bs-theme");
                    document.documentElement.setAttribute("data-bs-theme", currentTheme === "light" ? "dark" : "light");
                });
            }
        } catch (e) { }
    },
    toggleSidebar: () => {
        try {
            const collapsedToggle = document.querySelector(".mobile-menu-btn");
            const overlay = document.querySelector(".startbar-overlay");

            const changeSidebarSize = () => {
                if (window.innerWidth >= 310 && window.innerWidth <= 1440) {
                    document.body.setAttribute("data-sidebar-size", "collapsed");
                } else {
                    document.body.setAttribute("data-sidebar-size", "default");
                }
            };

            collapsedToggle?.addEventListener("click", function () {
                document.body.setAttribute("data-sidebar-size",
                    document.body.getAttribute("data-sidebar-size") === "collapsed" ? "default" : "collapsed");
            });

            overlay?.addEventListener("click", () => {
                document.body.setAttribute("data-sidebar-size", "collapsed");
            });

            window.addEventListener("resize", changeSidebarSize);
            changeSidebarSize();
        } catch (e) { }
    },
    activateTooltipsAndPopovers: () => {
        try {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });

            const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
            popoverTriggerList.map(function (popoverTriggerEl) {
                return new bootstrap.Popover(popoverTriggerEl);
            });
        } catch (e) { }
    },
    stickyTopbarOnScroll: () => {
        window.addEventListener("scroll", e => {
            e.preventDefault();
            const topbar = document.getElementById("topbar-custom");
            if (!topbar) return;
            if (document.body.scrollTop >= 50 || document.documentElement.scrollTop >= 50) {
                topbar.classList.add("nav-sticky");
            } else {
                topbar.classList.remove("nav-sticky");
            }
        });
    },
    initVerticalMenu: () => {
        try {
            const collapses = document.querySelectorAll(".navbar-nav li .collapse");
            document.querySelectorAll(".navbar-nav li [data-bs-toggle='collapse']").forEach(e => {
                e.addEventListener("click", function (e) {
                    e.preventDefault();
                });
            });

            collapses.forEach(e => {
                e.addEventListener("show.bs.collapse", function (t) {
                    const o = t.target.closest(".collapse.show");
                    document.querySelectorAll(".navbar-nav .collapse.show").forEach(e => {
                        if (e !== t.target && e !== o) {
                            new bootstrap.Collapse(e).hide();
                        }
                    });
                });
            });

            const links = document.querySelectorAll(".navbar-nav a");
            const currentUrl = window.location.href.split(/[?#]/)[0];

            links.forEach(function (t) {
                if (t.href === currentUrl) {
                    t.classList.add("active");
                    t.parentNode.classList.add("active");
                    let e = t.closest(".collapse");
                    while (e) {
                        e.classList.add("show");
                        e.parentElement.children[0].classList.add("active");
                        e.parentElement.children[0].setAttribute("aria-expanded", "true");
                        e = e.parentElement.closest(".collapse");
                    }
                }
            });
        } catch (e) { }
    }
};
