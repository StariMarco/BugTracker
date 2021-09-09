const getTicketData = () => {
    const ticketId = document.getElementById("inputTicketId").value;
    const projectId = document.getElementById("inputProjectId").value;
    const currentUserId = document.getElementById("currentUserId").value;

    const title = document.getElementById("inputTitle").value;
    const description = document.getElementById("inputDescription").value;
    const createdAt = document.getElementById("inputCreatedAt").value.toString();
    const closedAt = document.getElementById("inputClosedAt").value.toString();

    const reportedId = document.getElementById("inputReporter").value.toString();
    const developerId = document.getElementById("inputDeveloper").value.toString();
    const reviewerId = document.getElementById("inputReviewer").value.toString();

    const statusId = document.getElementById("inputStatus").value;
    const priorityId = document.getElementById("inputPriority").value;
    const typeId = document.getElementById("inputType").value;

    const canEditTicket = document.getElementById("canEditTicket").value;

    return {
        id: ticketId,
        title: title,
        description: description,
        createdAt: createdAt,
        closedAt: closedAt,
        projectId: projectId,
        reporterId: reportedId,
        developerId: developerId,
        reviewerId: reviewerId,
        statusId: statusId,
        priorityId: priorityId,
        typeId: typeId,
        currentUserId: currentUserId,
        canEditTicket: canEditTicket
    };
}