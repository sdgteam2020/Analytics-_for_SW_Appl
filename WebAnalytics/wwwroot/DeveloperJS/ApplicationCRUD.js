$(document).ready(function () {
    loadApplications();

    $(".copy-btn").click(function () {

        copyCode(this);
    })

    $("#appForm").on("submit", async function (e) {
        $(".errormsg").html("");
        $(".errormsg").addClass("d-none");
        // Remove Bootstrap invalid class
        $(".is-invalid").removeClass("is-invalid");

        // Clear validation messages
        $("[data-valmsg-for]")
            .removeClass("field-validation-error")
            .addClass("field-validation-valid")
            .text("");
        e.preventDefault(); // stop normal submit

        // client-side validation (optional but nice)
        if (typeof $(this).valid === "function" && !$(this).valid()) {
            return;
        }

        const form = this;

        // build FormData (includes files)
        const fd = new FormData(form);
        const token = $('input[name="__RequestVerificationToken"]').val();
        Swal.fire({
            title: "Do you want to Add Application?",
            text: "You won't be able to revert this!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, Add it!"
        }).then(async (result) => {
            if (result.isConfirmed) {

                try {
                    const response = await fetch('/ApplicationCRUD/AddApplication', {
                        method: 'POST',
                        headers: {
                            "RequestVerificationToken": token // matches [ValidateAntiForgeryToken]
                        },
                        body: fd // don't set content-type; fetch will handle for FormData
                    });

                    // Check if the response is ok (status code 200-299)
                    if (!response.ok) {
                        const text = await response.text();
                        Swal.fire({
                            position: "top-end",
                            icon: "error",
                            title: "Save failed.\n" + text,
                            showConfirmButton: false,
                            timer: 1500
                        });
                        $(".errormsg").html("Save failed.\n" + text);
                        $(".errormsg").removeClass("d-none");
                        return;
                    }

                    // Parse the JSON response
                    const data = await response.json(); // Parse JSON response

                    // Example: { code: 200, message: "...", data: { ... } }
                    if (data.Code === 200) {
                        Swal.fire({
                            position: "top-end",
                            icon: "success",
                            title: data.Message,
                            showConfirmButton: false,
                            timer: 3500
                        });
                        $(".errormsg").html(data.Message);
                        (".errormsg").addClass("d-none");
                        resetappication()
                        loadApplications();

                    } else if (data.Code === 4) {
                        let errorText = data.Data
                            .map(e => e.Field + " : " + e.Message)
                            .join(", ");
                        //Swal.fire({
                        //    position: "top-end",
                        //    icon: "error",
                        //    title: data.Message + "\n " + errorText,
                        //    showConfirmButton: false,
                        //    timer: 3500
                        //});
                       

                        $(".errormsg").text(errorText);
                        if (data.Data && data.Data.length > 0) {

                            data.Data.forEach(function (e) {

                                // Add red border using name attribute
                                $("[name='" + e.Field + "']").addClass("is-invalid");

                            });
                        }
                        // Clear old errors first
                        $(".input-validation-error").removeClass("input-validation-error");
                        $("[data-valmsg-for]").text("");

                        if (data.Data && data.Data.length > 0) {

                            data.Data.forEach(function (e) {

                                // Add red border (Bootstrap compatible)
                                $("[name='" + e.Field + "']")
                                    .addClass("input-validation-error");

                                // Bind error message to asp-validation-for span
                                $("[data-valmsg-for='" + e.Field + "']")
                                    .removeClass("field-validation-valid")
                                    .addClass("field-validation-error")
                                    .text(e.Message);
                            });
                        }
                        $(".errormsg").removeClass("d-none");
                    } else {
                        Swal.fire({
                            position: "top-end",
                            icon: "error",
                            title: data.Message,
                            showConfirmButton: false,
                            timer: 3500
                        });
                        $(".errormsg").html(data.Message);
                        $(".errormsg").removeClass("d-none");
                    }

                } catch (error) {
                    // Handle any errors from the fetch request
                    Swal.fire({
                        text: "An error occurred: " + error.message
                    });
                    $(".errormsg").html(data.Message);
                    $(".errormsg").removeClass("d-none");
                }
            }
        });
    });


    function resetappication() {
        $("#ApplicationId").val(0);
        $("#ApplicationName").val("")
        $("#Description").val("")
        $("#origin").val("")
    }

    $(document).on("click", ".btn-Edit-code", function (e) {
        let editBtn = $(e.target).closest(".btn-Edit-code");
       
        let applicationId = editBtn.data("key");
        $("#ApplicationId").val(applicationId);
        $("#ApplicationName").val($(this).closest('tr').find('#ApplicationName').html())
        $("#Description").val($(this).closest('tr').find('#Description').html())
        $("#origin").val($(this).closest('tr').find('#origin').html())
    });

    $(document).on("click", ".btn-view-code", function (e) {
        let editBtn = $(e.target).closest(".btn-view-code");

        const key = editBtn.data("key");
        // Ensure apiBase ends with a trailing slash
        const base = (typeof apiBase1 === "string" ? apiBase1 : "").replace(/\/?$/, "/");

        // Build endpoints
        const urlIncrement = `${base}ApplicationHit/IncrementHits`;
        const urlSessionStart = `${base}Application/ApplicationSessionStart`;
        const urlSessionEnd = `${base}Application/ApplicationSessionEnd`;
        const urlHitsConUser = `${base}ApplicationHit/HitswithConcurrentuser`;

        const urlIncrementDomainId = `${base}ApplicationHit/IncrementHits`;
        const urlSessionStartwithDomianId = `${base}Application/ApplicationSessionStart`;
        const urlSessionEndwithDomian = `${base}Application/ApplicationSessionEnd`;

        // One request template used in all snippets
       
        const requestDecl = `const myHeadersIncrement = new Headers(); 
        myHeadersIncrement.append("X-API-KEY", '${key}');

        const requestIncrement = {
          method: "POST",
          redirect: "follow",
           headers: myHeadersIncrement
        };`;

        // Snippet: increment hits
        const snippetIncrement = `${requestDecl}

    fetch("${urlIncrement}", requestIncrement)
      .then((response) => response.text())
      .then((result) => console.log(result))
      .catch((error) => console.error(error));`;
        // One request template used in all snippets
        const requestDeclIncrementWithDomainId = `const myHeadersIncrementWithDomainId = new Headers(); 
        myHeadersIncrementWithDomainId.append("X-API-KEY", '${key}');
        const requestIncrementWithDomainId = {
      method: "POST",
      redirect: "follow",
      headers: myHeadersIncrementWithDomainId
    };`;

        // Snippet: increment hits
        const snippetIncrementwithDomainId = `${requestDeclIncrementWithDomainId}

    fetch("${urlIncrementDomainId}", requestIncrementWithDomainId)
      .then((response) => response.text())
      .then((result) => console.log(result))
      .catch((error) => console.error(error));`;

        // Snippet: session start
        const requestDeclStart = `const myHeadersIncrementStart = new Headers(); 
        myHeadersIncrementStart.append("X-API-KEY", '${key}');
        const requestStart = {
      method: "POST",
      redirect: "follow",
      headers: myHeadersIncrementStart
    };`;
        const snippetSessionStart = `${requestDeclStart}

    fetch("${urlSessionStart}", requestStart)
      .then((response) => response.text())
      .then((result) => console.log(result))
      .catch((error) => console.error(error));`;


        // Snippet: session start With Domain
        const requestDeclStartwithDomianid = `const myHeadersIncrementStartdomainId = new Headers(); 
        myHeadersIncrementStartdomainId.append("X-API-KEY", '${key}');
        const requestStartdomainId = {
      method: "POST",
      redirect: "follow",
      headers: myHeadersIncrementStartdomainId
    };`;
        const snippetSessionStartWithDomianId = `${requestDeclStartwithDomianid}

    fetch("${urlSessionStartwithDomianId}", requestStartdomainId)
      .then((response) => response.text())
      .then((result) => console.log(result))
      .catch((error) => console.error(error));`;

        // Snippet: session end
        const requestDeclEnd = `const myHeadersIncrementEND = new Headers(); 
        myHeadersIncrementEND.append("X-API-KEY", '${key}');
        const requestEND = {
      method: "POST",
      redirect: "follow",
      headers: myHeadersIncrementEND
    };`;
        const snippetSessionEnd = `${requestDeclEnd}

    fetch("${urlSessionEnd}", requestEND)
      .then((response) => response.text())
      .then((result) => console.log(result))
      .catch((error) => console.error(error));`;
        // Snippet: session end DomainID
        const requestDeclEndDomainId = `const myHeadersENDDomainId = new Headers(); 
        myHeadersENDDomainId.append("X-API-KEY", '${key}');
        const requestENDDomainId = {
      method: "POST",
      redirect: "follow",
      headers: myHeadersENDDomainId
    };`;
        const snippetSessionEndDomainId = `${requestDeclEndDomainId}

    fetch("${urlSessionEndwithDomian}", requestENDDomainId)
      .then((response) => response.text())
      .then((result) => console.log(result))
      .catch((error) => console.error(error));`;
        // Snippet: get hits + concurrent users (expects JSON)
        const requestDeclall = `const myHeaderstall = new Headers(); 
        myHeaderstall.append("X-API-KEY", '${key}');
        const requestall = {
      method: "POST",
      redirect: "follow",
      headers: myHeaderstall
    };`;
        const snippetHitsConUser = `${requestDeclall}

    fetch("${urlHitsConUser}", requestall)
      .then((response) => response.json())
      .then((data) => {
        console.log("todayHits:", data.todayHits);
        console.log("monthlyHits:", data.monthlyHits);
        console.log("totalHits:", data.totalHits);
        console.log("concurrentuser:", data.concurrentuser);
      })
      .catch((error) => console.error(error));`;

        // Fill code blocks safely
        const elInc = document.getElementById("codeSnippet");
        const elIncHitCounterDomainId = document.getElementById("codeSnippetHitCounterDomainId");
        const elStart = document.getElementById("codeSnippetDashboard");
        const elStartWithDomainId = document.getElementById("codeSnippetDashboardDomainId");
        const elEnd = document.getElementById("codeSnippetLogout");
        const elEnddomainId = document.getElementById("codeSnippetLogoutDomainId");
        const elGet = document.getElementById("codeSnippetGetHitCounter");

        if (elInc) elInc.textContent = snippetIncrement;
        if (elIncHitCounterDomainId) elIncHitCounterDomainId.textContent = snippetIncrementwithDomainId;
        if (elStart) elStart.textContent = snippetSessionStart;
        if (elStartWithDomainId) elStartWithDomainId.textContent = snippetSessionStartWithDomianId;
        if (elEnd) elEnd.textContent = snippetSessionEnd;
        if (elEnddomainId) elEnddomainId.textContent = snippetSessionEndDomainId;
        if (elGet) elGet.textContent = snippetHitsConUser;

        // Show modal
        $("#exampleModal").modal('show');


        return; // stop here so edit handler below doesn't also run
    });
});

async function loadApplications() {
    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const res = await fetch("/ApplicationCRUD/GetApplication", {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "RequestVerificationToken": token
            }
        });

        if (!res.ok) throw new Error("Failed to load applications");

        const data = await res.json();
        const tbody = document.querySelector("#appsTable tbody");
        tbody.innerHTML = "";
        data.forEach((app, index) => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
          <td>${index + 1}</td>
          <td id="ApplicationName">${app.ApplicationName}</td>
          <td id="Description">${app.Description || ""}</td>
          <td id="ApplicationKey">${app.ApplicationKey || ""}</td>
          <td id="origin">${app.origin || ""}</td>
              
     <td><button type="button" class="btn btn-outline-info btn-view-code" data-key="${app.ApplicationKey || ""}">Code Snippet</button>
     <button type="button" class="btn btn-outline-primary btn-Edit-code" data-key="${app.ApplicationId || ""}">Edit</button>
     </td>
        `;
            tbody.appendChild(tr);
        });
       
        // render table...
    } catch (err) {
        console.error(err);
        alert("Unable to load applications");
    }
}


function copyCode(button) {
    
    const code = button.closest('.code-snippet').querySelector('code').innerText;
    navigator.clipboard.writeText(code).then(() => {
        button.innerText = 'Copied!';
        setTimeout(() => button.innerText = 'Copy', 1500);
    });
}