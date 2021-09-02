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

    // Exit on click outside the modal-content
    placeholderElement.on('click', '.modal', function (event) {
        $('#navbar').css('z-index', 500);
        placeholderElement.find('.modal').hide();
    });

    /* Avoid hiding the modal when the modal-content is clicked */
    placeholderElement.on('click', '.modal-content', function (event) {
        event.stopPropagation();
    });

    placeholderElement.on('click', '#tblSelectUser tr', function (data) {
        const currentRow = $(this).closest('tr');
        const userId = currentRow.find('td:eq(0)').text();
        const userName = currentRow.find('td:eq(1)').text();

        placeholderElement.find('#userIdInput').val(userId);
        placeholderElement.find('#selectedUserName').text(userName);
    })

})