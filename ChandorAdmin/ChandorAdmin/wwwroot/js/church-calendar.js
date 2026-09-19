window.chandorCalendar = {
    setSaveLoading: function (isLoading) {
        const selector = '.e-schedule-dialog .e-footer-content .e-primary';

        if (isLoading) {
            const button = document.querySelector(selector);
            if (!button || button.dataset.chandorSaveLoading === 'true') {
                return;
            }

            button.dataset.chandorSaveLoading = 'true';
            button.dataset.chandorOriginalContent = button.innerHTML;
            button.setAttribute('aria-busy', 'true');
            button.setAttribute('aria-disabled', 'true');
            button.style.pointerEvents = 'none';
            button.innerHTML =
                '<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>' +
                '<span>Chargement...</span>';
            return;
        }

        document.querySelectorAll('[data-chandor-save-loading="true"]').forEach(function (button) {
            button.innerHTML = button.dataset.chandorOriginalContent || 'Save';
            button.removeAttribute('aria-busy');
            button.removeAttribute('aria-disabled');
            button.style.removeProperty('pointer-events');
            delete button.dataset.chandorSaveLoading;
            delete button.dataset.chandorOriginalContent;
        });
    }
};
