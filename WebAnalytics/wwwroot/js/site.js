

$(document).ready(function () {
    let currentPath = window.location.pathname.toLowerCase();

    $('.navbar-nav .nav-link').each(function () {
        let linkPath = $(this).attr('href').toLowerCase();
        $(this).removeClass('active'); // Add active to link
        if (currentPath === linkPath) {
            $(this).addClass('active'); // Add active to link
            $(this).closest('.nav-item').addClass('active'); // Optional: to <li>
        }
    });
    const myHeaderstall = new Headers();
    myHeaderstall.append("X-API-KEY", 'a27c3cae-df44-4e40-8d47-d9b25939f21f');
    const requestall = {
        method: "POST",
        redirect: "follow",
        headers: myHeaderstall
    };

    fetch("https://localhost:7144/api/ApplicationHit/HitswithConcurrentuser", requestall)
        .then((response) => response.json())
        .then((data) => {
            $("#TodayHits").html(data.TodayHits);
            $("#MonthlyHits").html(data.MonthlyHits);
            $("#TotalHits").html(data.TotalHits);
            $("#ConcurrentUsers").html(data.Concurrentuser);
        })
        .catch((error) => console.error(error));
});
