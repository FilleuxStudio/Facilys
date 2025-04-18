window.modifyBodyForModal = function (isModalOpen) {
    try {
        const body = document.body;

        if (isModalOpen) {
            body.classList.add("modal-open");
            body.style.overflow = "hidden";
            body.style.paddingRight = "0px";
        } else {
            body.classList.remove("modal-open");
            body.style.overflow = "";
            body.style.paddingRight = "";
        }
    } catch (e) {
        console.error("Error in modifyBodyForModal:", e);
    }
};