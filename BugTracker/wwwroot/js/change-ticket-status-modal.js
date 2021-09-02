$(function () {
    var placeholderElement = $('#modalPlaceholder');

    $('button[data-toggle="ajax-modal"]').click(function (event) {

        var url = $(this).data(url);
        $.get(url).done(function (data) {
            placeholderElement.html(data);
            $('#navbar').css('z-index', 2);
            placeholderElement.find('.modal').show();
        });

    })

    /* Save button */
    placeholderElement.on('click', '[data-save="modal"]', function (event) {
        var form = $(this).parents('modal').find('form');
        var actionUrl = form.attr('asp-action');
        var sendData = form.serialize();

        $.post(actionUrl, sendData).done(function (data) {
            $('#navbar').css('z-index', 500);
            placeholderElement.find('.modal').hide();
            console.log(data);
        })
    });

    // Close button
    placeholderElement.on('click', '[data-dismiss="modal"]', function (event) {
        $('#navbar').css('z-index', 500);
        placeholderElement.find('.modal').hide();
    });

    // Exit on click outside the status-modal-content
    placeholderElement.on('click', '.modal', function (event) {
        $('#navbar').css('z-index', 500);
        placeholderElement.find('.modal').hide();
    });

    /* Avoid hiding the modal when the status-modal-content is clicked */
    placeholderElement.on('click', '.status-modal-content', function (event) {
        event.stopPropagation();
    });


})