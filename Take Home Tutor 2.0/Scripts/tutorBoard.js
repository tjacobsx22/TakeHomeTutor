document.body.onmouseover = function () {
    if (!document.getElementById("test")) {
        var div = document.createElement("div");
        div.setAttribute('id', 'test');
        document.body.appendChild(div);
    }
    document.getElementById("test").innerHTML = new Date().getTime();
}
    
        
    

