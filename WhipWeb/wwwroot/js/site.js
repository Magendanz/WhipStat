// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

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