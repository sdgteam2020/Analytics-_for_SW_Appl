
apiBase = "https://hitcounter.army.mil/api/";
$(document).ready(function () {
    var currentPath = window.location.pathname.toLowerCase();

    $('.navbar-nav .nav-link').each(function () {
        var linkPath = $(this).attr('href').toLowerCase();
        $(this).removeClass('active'); // Add active to link
        if (currentPath === linkPath) {
            $(this).addClass('active'); // Add active to link
            $(this).closest('.nav-item').addClass('active'); // Optional: to <li>
        }
    });
});
