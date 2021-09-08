$(document).ready(function () {
    $('#detail-header').on('click', function () {
        var elementToToggle = $(this).parent().find('#ticket-detail-body');
        var icon = $(this).find('#detail-icon');

        if (elementToToggle.attr('class').trim() == '') {
            elementToToggle.hide();
            icon.attr('class', 'fas fa-chevron-down');
        } else {
            elementToToggle.show();
            icon.attr('class', 'fas fa-chevron-up');
        }

        elementToToggle.toggleClass('hidden');
    });

    $('#timeline-header').on('click', function () {
        var elementToToggle = $(this).parent().find('#ticket-timeline-body');
        var icon = $(this).find('#timeline-icon');

        if (elementToToggle.attr('class').trim() == '') {
            elementToToggle.hide();
            icon.attr('class', 'fas fa-chevron-down');
        } else {
            elementToToggle.show();
            icon.attr('class', 'fas fa-chevron-up');
        }

        elementToToggle.toggleClass('hidden');
    });
});