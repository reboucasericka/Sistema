/* JS do Agendamento Público (PublicBooking)
   - Movido da view Index.cshtml para centralização e manutenção
*/

(function () {
    "use strict";

    if (window.__publicBookingInitialized) return;
    window.__publicBookingInitialized = true;

    $(document).ready(function () {
        // Toggle dos serviços (se existir markup correspondente)
        $('.service-toggle').on('click', function () {
            const serviceId = $(this).data('service-id');
            const details = $(this).closest('.service-card').find('.service-details');
            const icon = $(this).find('i');

            if (details.is(':visible')) {
                details.slideUp();
                icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
            } else {
                if (details.find('.professionals-list').children().length === 0) {
                    loadServiceDetails(serviceId);
                }
                details.slideDown();
                icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
            }
        });
    });

    function loadServiceDetails(serviceId) {
        $.ajax({
            url: '/PublicBooking/GetServiceDetails',
            type: 'GET',
            data: { serviceId: serviceId },
            headers: (window.UIHelpers && UIHelpers.withCsrf()) || {},
            success: function (response) {
                if (response.success) {
                    displayProfessionals(serviceId, response.professionals);
                }
            },
            error: function () {
                console.error('Erro ao carregar detalhes do serviço');
            }
        });
    }

    function displayProfessionals(serviceId, professionals) {
        const container = $(`.service-card[data-service-id="${serviceId}"] .professionals-list`);
        let html = '';

        professionals.forEach(function (professional) {
            const recommendationsText = professional.recommendations > 0
                ? `${professional.recommendations} recomendações`
                : 'Nenhuma recomendação';

            const hasAvailableTimes = professional.availableTimes.length > 0;
            const timeButtons = hasAvailableTimes
                ? professional.availableTimes.map(time =>
                    `<button class="btn btn-success btn-sm me-2 mb-2 time-slot" data-time="${time}">${time}</button>`
                ).join('')
                : '<button class="btn btn-secondary btn-sm" disabled>HORÁRIOS INDISPONÍVEIS</button>';

            html += `
                <div class="professional-card border rounded p-3 mb-3">
                    <div class="d-flex align-items-center mb-2">
                        <img src="${professional.photo}" class="rounded-circle me-3" width="40" height="40" alt="${professional.name}">
                        <div>
                            <h6 class="mb-0">${professional.name}</h6>
                            <small class="text-muted">${professional.specialty}</small>
                        </div>
                        <div class="ms-auto">
                            <small class="text-muted">${recommendationsText}</small>
                        </div>
                    </div>
                    <div class="time-slots">
                        ${timeButtons}
                    </div>
                </div>
            `;
        });

        container.html(html);

        container.find('.time-slot').on('click', function () {
            const time = $(this).data('time');
            const serviceId = $(this).closest('.service-card').data('service-id');
            bookAppointment(serviceId, time);
        });
    }

    function bookAppointment(serviceId, time) {
        // Em ambientes MVC, a checagem de autenticação é melhor no servidor.
        // Mantemos redirecionamento simples para a rota de schedule.
        window.location.href = `/Public/PublicBooking/Schedule/${serviceId}`;
    }
})();


