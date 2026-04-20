$(document).ready(function () {
    GetAllUser();

    $(document).on('click', '.updateuser', async function () {
      
        const result = await Swal.fire({
            title: "Are you sure?",
            text: "You want to update Status",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes"
        });
        if (result.isConfirmed) {
            const userId = $(this).data('id');

            let isActive = true;
            
            if ($(this).val().trim() == "Active")
                isActive = false;
          



            const token = $('input[name="__RequestVerificationToken"]').val();

            try {
                const resp = await fetch("/Account/UpdateApprovalStatus", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",  // Ensure content-type is set to JSON
                        "RequestVerificationToken": token   // Pass the CSRF token in the header
                    },
                    body: JSON.stringify({
                        Id: userId,
                        Active: isActive
                    })
                });

                if (!resp.ok) {
                    // non-200 status (e.g., 400/500)
                    const text = await resp.text();
                    Swal.fire({
                        position: "top-end",
                        icon: "error",
                        title: "Save failed.\n" + text,
                        showConfirmButton: false,
                        timer: 1500
                    });


                    return;
                }

                // expecting your DTOGenericResponse JSON from the controller
                const data = await resp.json();
                if (data.Code == 200) {
                    GetAllUser();
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: data.Message || "Update successfully.",
                        showConfirmButton: false,
                        timer: 1500
                    });

                } else {
                    Swal.fire({
                        position: "top-end",
                        icon: "error",
                        title: data.Message || "Update failed.",
                        showConfirmButton: false,
                        timer: 1500
                    });


                }
            } catch (err) {

                Swal.fire({
                    position: "top-end",
                    icon: "error",
                    title: "Network error while saving",
                    showConfirmButton: false,
                    timer: 1500
                });
            }
        }

    });
});


function GetAllUser() {
    var listItem = "";
    var userdata = {
        "Id": 0,  // You can add more properties if needed
    };

    $.ajax({
        url: '/Account/GetAllUser',
        contentType: 'application/x-www-form-urlencoded',
        data: userdata,
        type: 'POST',
        success: function (response) {
            if (response && response.length > 0) {  // Check if there are any records
                let listItems = response.map((item, index) => {
                    // Set the slider background color based on Active status
                    let sliderColor = item.Active ? "bg-success" : "bg-danger";
                    let sliderChecked = item.Active ? "checked" : "";
                    let isactive = item.Active ? "Active" : "Deactive";
                    return `
                        <tr>
                            <td class="align-middle">${index + 1}</td>
                            <td class="align-middle"><span class="divName">${item.UserName}</span></td>
                            
                             <td class="align-middle"><span class="divName">${item.RankName}</span></td>
                            <td class="align-middle"><span class="divName">${item.Name}</span></td>
                           
                            <td class="align-middle">
                             <div class="d-flex align-items-center gap-2">
                             <div class="form-check form-switch d-flex align-items-center gap-2">
                        <input class="btn text-white updateuser ${sliderColor}"
                               type="button"
                               id="userSwitch_${item.Id}"
                               data-id="${item.Id}"
                               value=" ${isactive}" />
                               </div>
                            
                            </div>
                          </td>
                        </tr>
                    `;
                }).join("");

               
                $("#UserApprovalBody").html(listItems);
               
               // feather.replace();  // Re-render feather icons after adding new HTML

              
            } else {
                listItem = "<tr><td class='text-center' colspan='7'>No Record Found</td></tr>";
                $("#UserApprovalBody").html(listItem);
            }
        },
        error: function (result) {
            Swal.fire({
                text: "An error occurred while fetching data."
            });
        }
    });
}






