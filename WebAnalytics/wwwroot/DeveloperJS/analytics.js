let lineChart   ;
let lineChartDays   ;
let barChart;
let barChartForConcurrentuser;
const intervalInMilliseconds = 10000;
let appColorMap = {};
$(document).ready(function () {

    const myHeadersIncrement = new Headers();
    myHeadersIncrement.append("X-API-KEY", 'a27c3cae-df44-4e40-8d47-d9b25939f21f');

    const requestIncrement = {
        method: "POST",
        redirect: "follow",
        headers: myHeadersIncrement
    };

    fetch("https://192.168.10.40/api/ApplicationHit/IncrementHits", requestIncrement)
        .then((response) => response.text())
        .then((result) => console.log(result))
        .catch((error) => console.error(error));
    const myHeadersIncrementStart = new Headers();
    myHeadersIncrementStart.append("X-API-KEY", 'a27c3cae-df44-4e40-8d47-d9b25939f21f');
    const requestStart = {
        method: "POST",
        redirect: "follow",
        headers: myHeadersIncrementStart
    };

    fetch("https://192.168.10.40/api/Application/ApplicationSessionStart", requestStart)
        .then((response) => response.text())
        .then((result) => console.log(result))
        .catch((error) => console.error(error));

    loadApplications("ddlappliction");
   
    setTimeout(() => {
        showchart(0, $("#ddlMonths").val(), $("#ddlYears").val())
    }, 1000);
    setInterval(function () {
        AllConcurrentuser($("#ddlappliction").val())

    }, intervalInMilliseconds);
   
    $("#ddlappliction").change(function () {
        
        showchart($(this).val(), $("#ddlMonths").val(), $("#ddlYears").val());  // Assuming 'ddlApplication' is a function you want to call
        AllConcurrentuser($(this).val())
        setInterval(function () {
           
            AllConcurrentuser($(this).val())
        }, intervalInMilliseconds);
      
    });

    $("#ddlYears").change(function () {
        showchart(0, $("#ddlMonths").val(), $("#ddlYears").val())
        //GetDataMonthsWise($("#ddlappliction").val(), $(this).val())
        //GetDataDaysWise($("#ddlappliction").val(), $("#ddlYears").val(), $("#ddlMonths").val())
        
    });

    $("#ddlMonths").change(function () {
        showchart(0, $("#ddlMonths").val(), $("#ddlYears").val())
     // GetDataDaysWise($("#ddlappliction").val(), $("#ddlYears").val(), $(this).val())
       
    });
});

async function loadApplications(ddl) {
    const res = await fetch("/ApplicationCRUD/GetApplication", {
        method: "POST",
        headers: {
            "Accept": "application/json",
          
        }
    });
      const data = await res.json();

      const tbody = document.querySelector("#appsTable tbody");
        let listItemddl = "";
        listItemddl += '<option value="0">All</option>';
      data.forEach((app, index) => {
          const tr = document.createElement("tr");
          listItemddl += '<option value="' + app.ApplicationKey + '">' + app.ApplicationName + '</option>';
      
      });
    $("#" + ddl + "").html(listItemddl);
    
    
}
function showchart(Id, Month, Years) {
    
    const requestOptions = {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            applicationKey: Id,
            Years: Years,
            Month: Month
        })
    };

    fetch(`/Home/GetDataSummary/` , requestOptions)
        .then((response) => response.text())
        .then((result) => {
           
            // Get the data from the result
            const parsedResult = JSON.parse(result);

            // Now access the properties on the parsed object
            const dtoApplicationMonthsWiseList = parsedResult.DTOApplicationMonthsWiseList;
            const dtoApplicationTotalCountList = parsedResult.DTOApplicationTotalCountList;
            const dtoApplicationDayWiseList = parsedResult.DTOApplicationDayWiseList;

            const barLabels = dtoApplicationTotalCountList.map(item => item.ApplicationName);  // Application names as the x-axis labels
            const barData = dtoApplicationTotalCountList.map(item => item.TotalHits);  // Total hits for each application
           
            dtoApplicationTotalCountList.forEach(item => {
                appColorMap[item.ApplicationName] = item.ColorCode;
            });
            
            const barColors = barLabels.map(appName => appColorMap[appName]);
            // Ensure the chart is destroyed before creating a new one
            if (barChart) {
                barChart.destroy();
            }
            // Generate a random color for each bar
           // const barColors = barData.map(() => getRandomColor());
            // Create the Bar Chart
            const ctxBar = document.getElementById('barChart').getContext('2d');
            barChart = new Chart(ctxBar, {
                type: 'bar',
                data: {
                    labels: barLabels,  // Application names as the x-axis labels
                    datasets: [{
                        label: 'LifeTime Hits',  // Label for the dataset
                        data: barData,  // Total hits data
                        backgroundColor: barColors,  // Specific colors for each application
                        barThickness: 30,  // Adjust the thickness of the bars
                    }]
                },
                options: {
                    plugins: {
                        title: {
                            display: true,
                            /* text: 'Years Wise Status'*/
                        },
                        legend: {
                            display: false
                        },
                        datalabels: {
                            anchor: 'end',
                            align: 'end',
                            color: '#000',
                            font: {
                                weight: 'bold'
                            },
                            formatter: function (value) {
                                return value;
                            }
                        }
                    }
                }, plugins: [ChartDataLabels] // Register the plugin
            });
            BindchartMonthsWiseHitCounter(dtoApplicationMonthsWiseList)
            BindchartDaysWiseHitCounter(dtoApplicationDayWiseList)
            AllConcurrentuser($("#ddlappliction").val())
        })

        .catch((error) => console.error(error));



}

function GetDataMonthsWise(applictionKey, years) {
   
    const requestOptions = {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            "applicationKey": applictionKey,
            "years": years
        })
    };

    fetch(`/Home/GetYearWise`, requestOptions)
        .then((response) => response.text())
        .then((result) => {
            const parsedResult = JSON.parse(result);
            BindchartMonthsWiseHitCounter(parsedResult)
        }
    )
        .catch((error) => console.error(error));
}
function GetDataDaysWise(applictionKey, years, month) {

    const myHeaders = new Headers();
    myHeaders.append("Content-Type", "application/json");

    const raw = JSON.stringify({
        "applicationKey": applictionKey,
        "years": years,
        "month": month
    });

    const requestOptions = {
        method: "POST",
        headers: myHeaders,
        body: raw,
        redirect: "follow"
    };

    fetch(`/Home/GetDaysWise`, requestOptions)
        .then((response) => response.text())
        .then((result) => {
            const parsedResult = JSON.parse(result);
            BindchartDaysWiseHitCounter(parsedResult)
        }
        )
        .catch((error) => console.error(error));
}

function BindchartMonthsWiseHitCounter(dtoApplicationMonthsWiseList) {
   
    if (dtoApplicationMonthsWiseList.length==0) {
        if (lineChart) {
            lineChart.destroy();
        }

    }
    else {
        const allMonths = [
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        ];
        const lineLabels = allMonths;
        $("#ddlYears").val(dtoApplicationMonthsWiseList[0].Years);
        // Prepare Labels (Months)
       // const lineLabels = [...new Set(dtoApplicationMonthsWiseList.map(item => item.MonthsName))];  // Unique months

        // Organize data by ApplicationName
        const applications = [...new Set(dtoApplicationMonthsWiseList.map(item => item.ApplicationName))];  // Unique application names

        const datasets = applications.map(application => {
            // Filter the data for each application
            const appData = dtoApplicationMonthsWiseList.filter(item => item.ApplicationName === application);

            const barColors = applications.map(appName => appColorMap[appName]);
            // Create a dataset for each application
            return {
                label: application,  // Application Name
                data: lineLabels.map(month => {
                    const entry = appData.find(item => item.MonthsName === month);
                    return entry ? entry.TotalHits : 0;  // If no data, return 0
                }),
                fill: false,
                borderColor: barColors,  // Random color for each line
                tension: 0.1,  // Line tension for smoothness
                barThickness: 30,
            };
        });// Create the Line Chart
        if (lineChart) {
            lineChart.destroy();
        }
        const ctxLine = document.getElementById('lineChart').getContext('2d');
        lineChart = new Chart(ctxLine, {
            type: 'line',
            data: {
                labels: lineLabels,  // Months as x-axis labels
                datasets: datasets  // Datasets for each application
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    tooltip: {
                        callbacks: {
                            label: function (tooltipItem) {
                                return tooltipItem.dataset.label + ': ' + tooltipItem.raw;
                            }
                        }
                    }
                },
                animation: {
                    duration: 2000,  // Duration of the animation
                    easing: 'easeOutElastic',  // Elastic easing for the line chart
                    onComplete: function () {
                        console.log('Line chart animation completed!');
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
}
function BindchartDaysWiseHitCounter(dtoApplicationDayWiseList) {
   
    if (dtoApplicationDayWiseList.length == 0) {
        if (lineChartDays) {
            lineChartDays.destroy();
        }
        // Get the current month (0-indexed, January is 0, December is 11)
        let currentMonth = new Date().getMonth();

        // Set the value of the dropdown to the current month
       // $("#ddlMonths").val(currentMonth + 1); // Adding 1 because months are 1-based in the dropdown

    }
    else {
        
       // $("#ddlMonths").val(dtoApplicationDayWiseList[0].Month);
        // Prepare Labels (Months)
        const lineLabels = [];
        const now = new Date();
        const year = now.getFullYear();
        const month = dtoApplicationDayWiseList[0].Month;

        // Get total days in the month
        const daysInMonth = new Date(year, month, 0).getDate();
        
        for (let day = 1; day <= daysInMonth; day++) {
            lineLabels.push(day);
        }


        // Organize data by ApplicationName
        const applications = [...new Set(dtoApplicationDayWiseList.map(item => item.ApplicationName))];  // Unique application names
        const barColors = applications.map(appName => appColorMap[appName]);
        const datasets = applications.map(application => {
            // Filter the data for each application
            const appData = dtoApplicationDayWiseList.filter(item => item.ApplicationName === application);
         
            // Create a dataset for each application
            return {
                label: application,  // Application Name
                data: lineLabels.map(days => {
                    const entry = appData.find(item => item.Days === days);
                    
                    return entry ?  entry.TotalHits : 0;  // If no data, return 0
                }),
                fill: false,
                borderColor: barColors,  // Random color for each line
                tension: 0.1,  // Line tension for smoothness
                barThickness: 30,
            };
        });// Create the Line Chart
        if (lineChartDays) {
            lineChartDays.destroy();
        }
        const ctxLine = document.getElementById('lineChartDays').getContext('2d');
        lineChartDays = new Chart(ctxLine, {
            type: 'line',
            data: {
                labels: lineLabels,  // Months as x-axis labels
                datasets: datasets  // Datasets for each application
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    tooltip: {
                        callbacks: {
                            label: function (tooltipItem) {
                                return tooltipItem.dataset.label + ': ' + tooltipItem.raw;
                            }
                        }
                    }
                },
                animation: {
                    duration: 2000,  // Duration of the animation
                    easing: 'easeOutElastic',  // Elastic easing for the line chart
                    onComplete: function () {
                        console.log('Line chart animation completed 1111!');
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                // Add click event
                onClick: function (evt, elements) {
                    if (elements.length > 0) {
                        // Get the first clicked element
                        const element = elements[0];
                        const datasetIndex = element.datasetIndex;
                        const dataIndex = element.index;

                        const datasetLabel = this.data.datasets[datasetIndex].label;
                        const value = this.data.datasets[datasetIndex].data[dataIndex];
                        const label = this.data.labels[dataIndex];

                        console.log(`Clicked on: Dataset = ${datasetLabel}, Label = ${label}, Value = ${value}`);

                        // Example: Alert user
                      //  alert(`You clicked on ${datasetLabel} (${label}): ${value}`);

                        showPopupdatewise(datasetLabel,label)
                    }
                }
            }
        });

    }
}
// Helper function to generate random colors for each line
function getRandomColor() {
    const letters = '0123456789ABCDEF';
    let color = '#';
    for (let i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}// Function to assign a unique color to each application
function getColorForApplications(barLabels) {
    // Define a color palette for applications
    const colorPalette = {
        "E-ISAC": "rgba(75, 192, 192, 0.6)",  // Greenish blue for E-ISAC
        "Application 1": "rgba(255, 99, 132, 0.6)",  // Red for Application 1
        "Application 2": "rgba(255, 159, 64, 0.6)",  // Orange for Application 2
        "Application 3": "rgba(153, 102, 255, 0.6)",  // Purple for Application 3
        // Add more applications and their colors here
    };

    // Return an array of colors corresponding to each application
    return barLabels.map(label => colorPalette[label] || "rgba(54, 162, 235, 0.6)");  // Default to blue if no color is defined
}

function AllConcurrentuser(Id) {
   
    const requestOptions = {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            applicationKey: Id
        })
    };

    fetch(`/Home/AllConcurrentuser`, requestOptions)
        .then((response) => response.text())
        .then((result) => {

            // Get the data from the result
            const parsedResult = JSON.parse(result);
          
            // Now access the properties on the parsed object
            // Assume parsedResult is an array of objects, e.g.,
            // [{ applicationName: 'App A', total: 150, applicationId: 'app-a-123' }, ...]
            // Assume appColorMap is a map of application names to colors.

            // It's more efficient to create a single structured data array for Chart.js
            const chartData = parsedResult.map(item => ({
                label: item.ApplicationName,
                data: item.Total,
                applicationId: item.ApplicationId, // Add the applicationId here
                color: appColorMap[item.ApplicationName]
            }));

            // Application names as the x-axis labels
            const barLabels = chartData.map(item => item.label);
            // Total hits for each application
            const barData = chartData.map(item => item.data);
            // Specific colors for each application
            const barColors = chartData.map(item => item.color);

            // Ensure the chart is destroyed before creating a new one
            if (barChartForConcurrentuser) {
                barChartForConcurrentuser.destroy();
            }

            // Create the Bar Chart
            const ctxBarall = document.getElementById('barChartForConcurrentuser').getContext('2d');
            barChartForConcurrentuser = new Chart(ctxBarall, {
                type: 'bar',
                data: {
                    labels: barLabels, // Application names as the x-axis labels
                    datasets: [{
                        label: 'LifeTime Hits', // Label for the dataset
                        data: barData, // Total hits data
                        backgroundColor: barColors, // Specific colors for each application
                        barThickness: 30, // Adjust the thickness of the bars
                    }]
                },
                options: {
                    plugins: {
                        title: {
                            display: true,
                        },
                        legend: {
                            display: false
                        },
                        datalabels: {
                            anchor: 'end',
                            align: 'end',
                            color: '#000',
                            font: {
                                weight: 'bold'
                            },
                            formatter: function (value) {
                                return value;
                            }
                        }
                    },
                    // Add click event
                    onClick: function (evt, elements) {
                        // Check if any bar was clicked
                        if (elements.length > 0) {
                            const element = elements[0];
                            const dataIndex = element.index;

                            // Retrieve the corresponding data from the original structured array
                            const clickedItem = chartData[dataIndex];

                            const label = clickedItem.label;
                            const value = clickedItem.data;
                            const applicationId = clickedItem.applicationId;

                            // Log all the retrieved information
                            //console.log(`Clicked on: Label = ${label}, Value = ${value}, Application ID = ${applicationId}`);

                            // Example: Alert user with the new information
                            // Note: It's better to use a modal or a custom UI instead of alert() in real applications.
                            // Since this is just an example, we will use alert() as requested.
                            // showPopup() is a placeholder function that would display a modal or a custom UI.
                            //alert(`You clicked on ${label} (ID: ${applicationId}): ${value}`);

                            // If you have a showPopup function, you can pass the application ID to it.
                            showPopup(applicationId, label)
                        }
                    }
                },
                plugins: [ChartDataLabels] // Register the plugin
            });
           

        })

        .catch((error) => console.error(error));

}

async function showPopup(applicationId, label) {

    $("#concurrentUsersOffcanvas").modal("show");
   
    try {
        const response = await fetch(`/Home/ConcurrentuserList?ApplicationId=${applicationId}`, {
            method: 'Post', // GET is fine for retrieving data
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Network response was not OK');
        }

        const data = await response.json();
        // Assuming `data` is an array of objects like:
        // [{ userId: 'user-123', sessionStartTime: '2023-10-27T10:00:00Z' }, ...]

        // Display data in modal
        // This line finds the container where the cards will be placed.
        // You must use a unique ID or a parent element that already exists in your HTML.
        const container = document.querySelector('#timelineList');
      //  const containerlines = document.querySelector('#timelineusers .lines');
        document.querySelectorAll('.applicationname')
            .forEach(el => el.textContent = label);

        // Clear any existing content in the container before adding new cards.
        container.innerHTML = '';

        if (data && data.length > 0) {
            // Initialize an empty string to build the HTML for all cards.
            let cardsHtml = '';
            //let cardsHtmllines = '';
           
            const timelineList = document.getElementById('timelineList');
          
          
            timelineList.innerHTML = '';

            data.forEach((item, index) => {
                let date = new Date(item.LastUpdated);
                const containerapplicationname = document.querySelector('.subtitle');
                containerapplicationname.innerHTML = "Concurrent Users Log";//date.toDDMMYYYY_HHMMSS();

               
                const listItem = document.createElement('li');
                listItem.className = `timeline-item `;
                listItem.innerHTML = `
                    <div class="marker"></div>
                    <div class="card">
                        <div class="time">IP Address : ${item.IpAddress}</div>
          
                        <p>Last Updated : ${date.toDDMMYYYY_HHMMSS()}</p>
                    </div>
                `;
                timelineList.appendChild(listItem);
            });
        }
        
 else {
            container.innerHTML = '<p>Data Not Found.</p>';
        }

      

    } catch (error) {
        console.error('Error fetching concurrent users:', error);
        alert('Failed to load concurrent users.');
    }
}

async function showPopupdatewise(Appname, day) {

    const year = $("#ddlYears").val();
    const month = $("#ddlMonths").val();

    day = String(day).padStart(2, '0');  // ensure 2 digits
    const mm = String(month).padStart(2, '0');

    // Build date string
    const dateStr = `${year}-${mm}-${day}`;
    $("#concurrentUsersOffcanvas").modal("show");

    try {
        const response = await fetch(`/Home/ConcurrentuserListDatewise?ApplicationName=${Appname}&date=${dateStr}`, {
            method: 'Post', // GET is fine for retrieving data
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Network response was not OK');
        }

        const data = await response.json();
        // Assuming `data` is an array of objects like:
        // [{ userId: 'user-123', sessionStartTime: '2023-10-27T10:00:00Z' }, ...]

        // Display data in modal
        // This line finds the container where the cards will be placed.
        // You must use a unique ID or a parent element that already exists in your HTML.
        const container = document.querySelector('#timelineList');
        //  const containerlines = document.querySelector('#timelineusers .lines');
        document.querySelectorAll('.applicationname')
            .forEach(el => el.textContent = Appname);

        // Clear any existing content in the container before adding new cards.
        container.innerHTML = '';

        if (data && data.length > 0) {
            // Initialize an empty string to build the HTML for all cards.
            let cardsHtml = '';
            //let cardsHtmllines = '';

            const timelineList = document.getElementById('timelineList');


            timelineList.innerHTML = '';
            let count = 0;
            data.forEach((item, index) => {
                let date = new Date(item.HitDate);
                if (count == 0) {
                    const containerapplicationname = document.querySelector('.subtitle');
                    containerapplicationname.innerHTML = "hit on : " + `${day}-${mm}-${year}`;//date.toDDMMYYYY_HHMMSS();
                    count == 1;

                }


                const listItem = document.createElement('li');
                listItem.className = `timeline-item `;
               
                listItem.innerHTML = `
                    <div class="marker"></div>
                    <div class="card">
                        <div class="time">IP Address : ${item.IpAddress}</div>
                       
                       <p>Time : ${date.toHHMMSS()}</p>
                    </div>
                `;
                timelineList.appendChild(listItem);
            });
        }

        else {
            container.innerHTML = '<p>Data Not Found.</p>';
        }



    } catch (error) {
        console.error('Error fetching concurrent users:', error);
        alert('Failed to load concurrent users.');
    }
}

// Format as dd-MM-yyyy HH:mm:ss
Date.prototype.toDDMMYYYY_HHMMSS = function () {
    const dd = String(this.getDate()).padStart(2, '0');
    const mm = String(this.getMonth() + 1).padStart(2, '0'); // months are 0-based
    const yyyy = this.getFullYear();
    const hh = String(this.getHours()).padStart(2, '0');
    const mi = String(this.getMinutes()).padStart(2, '0');
    const ss = String(this.getSeconds()).padStart(2, '0');

    return `${dd}-${mm}-${yyyy} ${hh}:${mi}:${ss}`;
};
// Format as dd-MM-yyyy HH:mm:ss
Date.prototype.toHHMMSS = function () {

    const hh = String(this.getHours()).padStart(2, '0');
    const mi = String(this.getMinutes()).padStart(2, '0');
    const ss = String(this.getSeconds()).padStart(2, '0');

    return `${hh}:${mi}:${ss}`;
};
// Scroll functions
function scrollToTop() {
    const scrollContainer = document.getElementById('timelineScroll');
    scrollContainer.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}

function scrollToBottom() {
    const scrollContainer = document.getElementById('timelineScroll');
    scrollContainer.scrollTo({
        top: scrollContainer.scrollHeight,
        behavior: 'smooth'
    });
}
