// content.js
chrome.runtime.onMessage.addListener(
    function (request, sender, sendResponse) {
        var xmlhttp = new XMLHttpRequest();
        var theUrl = "http://localhost:8888/";
        //Change request type here
        xmlhttp.open("POST", theUrl);
        xmlhttp.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        //Add variables here
        xmlhttp.send(JSON.stringify({ "URL": window.location.href.toString() }));
    }
);