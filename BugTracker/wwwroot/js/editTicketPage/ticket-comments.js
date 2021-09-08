const showEditComment = (parentId) => {
    const parentElement = document.getElementById(parentId);
    const elementToShow = parentElement.querySelector("#commentEdit");
    const elementToHide = parentElement.querySelector("#commentDefault");

    elementToShow.style.display = "block";
    elementToHide.style.display = "none";
};

const showDefaultComment = (parentId) => {
    const parentElement = document.getElementById(parentId);
    const elementToHide = parentElement.querySelector("#commentEdit");
    const elementToShow = parentElement.querySelector("#commentDefault");

    elementToShow.style.display = "block";
    elementToHide.style.display = "none";
};