var input = $("<textarea>");
input.appendTo("body");

var makeArrayBtn = $("<button> make array</button>");
makeArrayBtn.appendTo("body");

makeArrayBtn.click(function(){
    dataFromInput= input.val();

    var splitted = dataFromInput.split("\n"); 
    console.log(splitted);   
    url = "/Scrape/ScrapeAction"; 
    var data = 
    {
        links: splitted
    }; 
    var result = ajaxPost(url,data); 
    console.log(result); 
});


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


