// Write your Javascript code.

(function ($) {

    $.fn.setOptions = function (data) {
        var element = this;
        element.find("option").remove();
        $.each(data, function () {
            element.append('<option value="' + this.value + '"'
                + (this.selected ? ' selected' : '')
                + (this.disabled ? ' disabled' : '')
                + '>' + this.text + '</option>')
        })
        return this;
    };

}(jQuery));

function isSelected(str) {
    return (str && str !== "0");
}