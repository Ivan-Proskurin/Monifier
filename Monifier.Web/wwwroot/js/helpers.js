function activateLink(linkId) {
    $(".navbar-nav li.nav-item").removeClass("active");
    $(linkId).addClass("active");
}

function makeInputAutocomplete(inputId, list, onSelect) {
    inputId = inputId.replace(".", "_");
    var input = document.getElementById(inputId);

    var comboplete = new Awesomplete(input, {
        minChars: 0,
        autoFirst: true,
        sort: false
    });
    if (list) {
        comboplete.list = list;
    }

    $("div.awesomplete").addClass("input-group");
    comboplete.ul.setAttribute("style", "margin-top: 45px;");
    
    comboplete.selectItem = function (item) {

        function find(list, item) {
            for (var i = 0; i < list.length; i++){
                if (list[i].value === item) return i;
            }
            return -1;
        }
        
        comboplete.evaluate();
        var index = find(comboplete._list, item);
        if (index < 0) {
            comboplete.close();
            return -1;
        }
        comboplete.index = index;
        comboplete.select();
        return index;
    };
    
    var $input = $(input);
    var val = $input.val();
    if (val){
        $input.val("");
        if (comboplete.selectItem(val) < 0){
            $input.val(val);
        }
    }

    function openDropdown() {
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
    }
    
    if (onSelect) {
        input.addEventListener("awesomplete-selectcomplete", onSelect);
    }

    input.addEventListener("click", function () {
        openDropdown();
    });

    $("#" + inputId + "_Btn").click(function () {
        openDropdown();
    });

    return comboplete;
}

function makeInputNumeric(input, allowNegative = false) {
    if (typeof input === "string"){
        input = document.getElementById(input.replace(".", "_"));
    }
    input.onkeydown = function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        var allowCodes = [46, 8, 9, 27, 13, 110, 190, 188];
        if (allowNegative) allowCodes.push(173);
        if ($.inArray(e.keyCode, allowCodes) !== -1 ||
            // Allow: Ctrl/cmd+A
            (e.keyCode == 65 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: Ctrl/cmd+C
            (e.keyCode == 67 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: Ctrl/cmd+X
            (e.keyCode == 88 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: home, end, left, right
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    }
}

function makeInputDatetimePicker(inputId, select) {
    inputId = inputId.replace(".", "_");
    $("#" + inputId).datetimepicker({
        onSelectDate: function (dt) { propagateDateTimeToPicker(inputId, dt, select) },
        onSelectTime: function (dt) { propagateDateTimeToPicker(inputId, dt, select) },
        dayOfWeekStart: 1,
        closeOnDateSelect: true,
        format: "Y.m.d H:i"
    });
    $("#" + inputId + "_Btn").click(function() {
        $("#" + inputId).focus();
    });
}

function propagateDateTimeToPicker(inputId, dt, select) {
    $("#" + inputId).val(formatDateTime(dt));
    if (select) select();
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

function sendAjax(url, args, success) {
    $.ajax({
        type: "POST",
        url: url,
        beforeSend: function(xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: args,
        contentType: "application/json",
        dataType: "json",
        success: success,
        failure: function(response) {
            console.log(response);
        }
    });
}