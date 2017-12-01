function activateLink(linkId) {
    $(".navbar-nav li.nav-item").removeClass("active");
    $(linkId).addClass("active");
}

function makeInputAutocomplete(inputId, list, onSelect, containerClass) {

    var input = document.getElementById(inputId);

    var comboplete = new Awesomplete(input, {
        minChars: 0,
        autoFirst: true,
        sort: false
    });
    if (list) {
        comboplete.list = list;
    }

    if (onSelect) {
        input.addEventListener("awesomplete-selectcomplete", onSelect);
    }

    var cmb = $("div.awesomplete");
    if (!containerClass) containerClass = "col-md-9";
    cmb.addClass(containerClass);

    var container = input.parentElement;
    var validation = $("span[data-valmsg-for=" + inputId + "]");
    validation.appendTo(container);

    input.addEventListener("click", function () {
        if (comboplete.ul.childNodes.length === 0) {
            comboplete.minChars = 0;
            comboplete.evaluate();
        }
        else if (comboplete.ul.hasAttribute('hidden')) {
            comboplete.open();
        }
        else {
            comboplete.close();
        }
    });

    return comboplete;
}

function makeInputNumeric(inputId, allowNegative = false) {
    var input = document.getElementById(inputId);
    var $input = $("#" + inputId);
    input.onkeydown = function (evt) {
        var charCode = (evt.which) ? evt.which : event.keyCode;
        if (charCode == 32) return false;
        if (charCode == 8) return true;
        var last = event.char;
        var value = $input.val();
        if (last == "-" && !allowNegative) return false;
        if (last == "-" && value.length == 0) return true;
        var start = input.selectionStart;
        var end = input.selectionEnd;
        value = value.slice(0, start) + last + value.slice(end);
        value = value.replace(",", ".");
        var num = +value;
        return !isNaN(num);
    };
}

function makeInputDatetimePicker(inputId) {
    $("#" + inputId).datetimepicker({
        onSelectDate: function (dt) { propagateDateTimeToPicker(inputId, dt) },
        onSelectTime: function (dt) { propagateDateTimeToPicker(inputId, dt) },
        dayOfWeekStart: 1,
        closeOnDateSelect: true,
        format: "Y.m.d H:i"
    });
}

function propagateDateTimeToPicker(inputId, dt) {
    $("#" + inputId).val(formatDateTime(dt));
}

function formatDateTime(dt) {
    var year = dt.getFullYear();
    var month = dt.getMonth() + 1;
    if (month < 10) month = "0" + month;
    var day = dt.getDate();
    if (day < 10) day = "0" + day;
    var hours = dt.getHours();
    if (hours == 0) hours = "00";
    else if (hours < 0) hours = "0" + hours;
    return year + "." + month + "." + day + " " + hours + ":00";
}