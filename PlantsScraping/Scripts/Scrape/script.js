var input = $("<textarea style='display:none;'>");
input.appendTo("body");

var makeArrayBtn = $("<button style='display:none;'> make array</button>");
makeArrayBtn.appendTo("body");

makeArrayBtn.click(function(){
    dataFromInput= input.val();
    getData(dataFromInput.trim()); 
});


function getFiles()
{
    var url = "/Scripts/Scrape/links%20array.txt"; 
    url = "/Scripts/Scrape/links_test.txt";
    var text_file = ajaxGet(url);
    var data = getData(text_file.trim()); 
    exportToExcel(data); 
}

function getData(dataFromInput)
{
    var splitted = dataFromInput.split("\n"); 
    url = "/Scrape/ScrapeAction"; 
    var data = 
    {
        links: splitted
    }; 
    return ajaxPost(url,data);  
}

function exportToExcel(data)
{
    var url = "/Scrape/DownloadExcel/?file_name="+data; 
    window.location.href = url; 
}


function ajaxPost(url,data)
{
    var result = null; 
    $.ajax 
    (
        {
            url: url,
            data: data,
            type: "post",
            async: false,
            success: function(success)
            {
                console.log("success"); 
                result = success; 
            }, 
            error: function(error)
            {
                console.log("error"); 
                result = error; 
            }
        }
    ); 
    return result; 
}

function ajaxGet(url)
{
    var result = null; 
    $.ajax 
    (
        {
            url: url,
            type: "get",
            async: false,
            success: function(success)
            {
                console.log("success"); 
                result = success; 
            }, 
            error: function(error)
            {
                console.log("error"); 
                result = error; 
            }
        }
    ); 
    return result; 
}


