const myHeadersIncrementEND = new Headers();
myHeadersIncrementEND.append("X-API-KEY", 'a27c3cae-df44-4e40-8d47-d9b25939f21f');
const requestEND = {
    method: "POST",
    redirect: "follow",
    headers: myHeadersIncrementEND
};

fetch("https://hitcounter.army.mil/api/Application/ApplicationSessionEnd", requestEND)
    .then((response) => response.text())
    .then((result) => console.log(result))
    .catch((error) => console.error(error));