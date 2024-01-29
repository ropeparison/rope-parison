//Function for setting datagrid header height when using vertical header text. Decided not to use vertical header text in the end.
function setHeaderHeight(args) {

    var textWidth = document.querySelector(".vertical-header > div").scrollWidth; // Obtain the width of the headerText content.
    var header = document.querySelectorAll(".e-columnheader");
    for (var i = 0; i < header.length; i++) {
        (header.item(i)).style.height = textWidth + 'px'; // Assign the obtained textWidth as the height of the column header.
    }

    //Test
    //confirm("Press a button!");
}

function jsTestFunc() {
    confirm("Press a button!");
}

function getWindowDimensions() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
}; 