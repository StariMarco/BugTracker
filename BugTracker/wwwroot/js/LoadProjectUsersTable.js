var dataTable;

$(document).ready(function () {
    loadDataTable("GetProjectUserList");
});

const loadDataTable = (url) => {
    dataTable = $('#tblProjectUsers').DataTable({
        "ajax": {
            "url": "/Project/Users/1/" + url
        },
        "columns": [
            { "data": "fullname", "width": "20%" },
            { "data": "email", "width": "20%" },
            { "data": "role", "width": "20%" },
            {
                "data": "userId",
                "width": "20%",
                "render": function (data) {
                    return `
                        <a href="#">Edit</a>
                    `;
                },
        ]
    });
}