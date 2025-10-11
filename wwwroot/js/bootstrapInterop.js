window.bootstrapInterop = {
    showModal: function (id) {
        var element = document.getElementById(id);
        if (!element) return;
        var modal = new bootstrap.Modal(element);
        modal.show();
    },
    hideModal: function (id) {
        var element = document.getElementById(id);
        if (!element) return;
        var modal = bootstrap.Modal.getInstance(element);
        if (modal) modal.hide();
    }
};
