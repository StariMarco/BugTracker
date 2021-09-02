$(document).ready(function () {
    $('#statusfilter').change(function () {
        $("#width_status_option").html($('#statusfilter option:selected').text());
        $(this).width($("#width_status_select").width() - 25);
    });

    $('#typefilter').change(function () {
        $("#width_type_option").html($('#typefilter option:selected').text());
        $(this).width($("#width_type_select").width() - 25);
    });

    /* All this to show the status in different tags */
    $("#tblProjectTickets").ready(updateStatus);
});

function updateStatus() {
    $("#tblProjectTickets .status-tag").each(function () {
        var statusId = $(this).children("input:first").val();

        switch (statusId) {
            case "1":
                $(this).attr('class', 'todo-tag status-tag');
                break;
            case "2":
                $(this).attr('class', 'in-progress-tag status-tag');
                break;
            case "3":
                $(this).attr('class', 'in-review-tag status-tag');
                break;
            case "4":
                $(this).attr('class', 'done-tag status-tag');
                break;
            default:
                break;
        }

    });
}