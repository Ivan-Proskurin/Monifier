function CheckboxedTable(selectionChanged) {

    this.showCheckboxes = function() {
        $(".checkbox-cell").removeAttr("hidden");
        $(".hidecheckboxes").removeAttr("hidden");
        $(".showcheckboxes").attr("hidden", "hidden");
    }

    this.hideCheckboxes = function() {
        $("input[type=checkbox]").prop("checked", false);
        selectionChanged();
        $(".checkbox-cell").attr("hidden", "hidden");
        $(".showcheckboxes").removeAttr("hidden");
        $(".hidecheckboxes").attr("hidden", "hidden");
    }

    this.toggleAllCheckboxes = function() {
        if ($(".toggle-all-chechboxes:checked").length) {
            $(".item-checkbox").prop("checked", true);
        } else {
            $(".item-checkbox").prop("checked", false);
        }
        selectionChanged();
    }

    $(".showcheckboxes").click(this.showCheckboxes);
    $(".hidecheckboxes").click(this.hideCheckboxes);
    $(".toggle-all-chechboxes").change(this.toggleAllCheckboxes);

    this.getSelectedCount = function() {
        return $(".item-checkbox:checked").length;
    }

    this.hideControls = function() {
        $("input[type=checkbox]").prop("checked", false);
        selectionChanged();
        $(".checkbox-cell").attr("hidden", "hidden");
        $(".showcheckboxes").attr("hidden", "hidden");
        $(".hidecheckboxes").attr("hidden", "hidden");
    }

    this.showControls = function() {
        this.hideCheckboxes();
    }

    this.getSelectedCheckboxes = function() {
        return $(".item-checkbox:checked");
    }
}