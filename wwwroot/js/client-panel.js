/* JS do Painel do Cliente
   - Centralize aqui integrações do calendário, serviços e produtos
   - Este arquivo é carregado em Areas/Public/Views/PublicClientPanel/Index.cshtml
*/

(function () {
    "use strict";

    // Guardas simples para evitar conflitos caso a página seja reutilizada
    if (window.__clientPanelInitialized) return;
    window.__clientPanelInitialized = true;

    // Pontos de extensão: inicializações podem ser plugadas aqui
    // Ex.: initCalendar(); initServices(); initProducts();
})();

// client-panel.js - JavaScript otimizado para o painel do cliente
(function() {
    'use strict';

    // Variáveis globais
    let calendar;
    let currentAppointmentId = null;

    // Inicialização quando o DOM estiver pronto
    document.addEventListener('DOMContentLoaded', function() {
        initializeCalendar();
        loadServices();
        loadProducts();
        setupEventListeners();
        initializeSidebar();
    });

    // ========================================
    // INICIALIZAÇÃO DO CALENDÁRIO
    // ========================================
    function initializeCalendar() {
        const calendarEl = document.getElementById('calendar');
        if (!calendarEl) return;
        
        calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'dayGridMonth',
            locale: 'pt-br',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay'
            },
            events: function(info, successCallback, failureCallback) {
                fetch('/Public/PublicClientPanel/GetCalendarData')
                    .then(response => response.json())
                    .then(data => {
                        if (data.error) {
                            console.error('Erro ao carregar eventos:', data.error);
                            failureCallback(data.error);
                        } else {
                            successCallback(data);
                        }
                    })
                    .catch(error => {
                        console.error('Erro na requisição:', error);
                        failureCallback(error);
                    });
            },
            eventClick: function(info) {
                showAppointmentDetails(info.event);
            },
            eventDidMount: function(info) {
                // Adicionar tooltip com informações do agendamento
                info.el.setAttribute('title', 
                    `${info.event.title}\nProfissional: ${info.event.extendedProps.professional}\nPreço: €${info.event.extendedProps.price}\nStatus: ${info.event.extendedProps.status}`
                );
            }
        });

        calendar.render();
    }

    // ========================================
    // DETALHES DO AGENDAMENTO
    // ========================================
    function showAppointmentDetails(event) {
        const details = `
            <div class="row">
                <div class="col-12">
                    <h6><strong>${event.title}</strong></h6>
                    <p class="text-muted">${event.extendedProps.description || 'Sem descrição'}</p>
                    
                    <dl class="row">
                        <dt class="col-sm-4">Data:</dt>
                        <dd class="col-sm-8">${event.start.toLocaleDateString('pt-PT')}</dd>
                        
                        <dt class="col-sm-4">Horário:</dt>
                        <dd class="col-sm-8">${event.start.toLocaleTimeString('pt-PT', {hour: '2-digit', minute: '2-digit'})}</dd>
                        
                        <dt class="col-sm-4">Duração:</dt>
                        <dd class="col-sm-8">${event.extendedProps.duration || 'Não informado'}</dd>
                        
                        <dt class="col-sm-4">Profissional:</dt>
                        <dd class="col-sm-8">${event.extendedProps.professional}</dd>
                        
                        <dt class="col-sm-4">Preço:</dt>
                        <dd class="col-sm-8">€${event.extendedProps.price}</dd>
                        
                        <dt class="col-sm-4">Status:</dt>
                        <dd class="col-sm-8">
                            <span class="badge bg-${getStatusBadgeClass(event.extendedProps.status)}">
                                ${event.extendedProps.status}
                            </span>
                        </dd>
                    </dl>
                </div>
            </div>
        `;

        document.getElementById('appointment-details').innerHTML = details;
        
        // Mostrar botão de cancelar apenas se o status permitir
        const cancelBtn = document.getElementById('cancel-appointment-btn');
        if (event.extendedProps.status.toLowerCase() === 'agendado' || 
            event.extendedProps.status.toLowerCase() === 'scheduled') {
            cancelBtn.style.display = 'inline-block';
            currentAppointmentId = event.id;
        } else {
            cancelBtn.style.display = 'none';
            currentAppointmentId = null;
        }

        const modal = new bootstrap.Modal(document.getElementById('appointmentModal'));
        modal.show();
    }

    function getStatusBadgeClass(status) {
        switch (status.toLowerCase()) {
            case 'agendado':
            case 'scheduled':
                return 'primary';
            case 'confirmado':
            case 'confirmed':
                return 'success';
            case 'cancelado':
            case 'canceled':
                return 'danger';
            case 'concluído':
            case 'completed':
                return 'info';
            default:
                return 'secondary';
        }
    }

    // ========================================
    // CARREGAMENTO DE PRODUTOS
    // ========================================
    function loadProducts() {
        fetch('/Public/PublicClientPanel/GetProducts')
            .then(response => response.json())
            .then(products => {
                const container = document.getElementById('products-container');
                if (!container) return;
                
                container.innerHTML = '';

                if (products.length === 0) {
                    container.innerHTML = `
                        <div class="col-12">
                            <div class="text-center py-4">
                                <i class="fas fa-shopping-bag fa-3x text-muted mb-3"></i>
                                <h5 class="text-muted">Nenhum produto encontrado</h5>
                                <p class="text-muted">Tente ajustar os filtros ou entre em contato conosco.</p>
                            </div>
                        </div>
                    `;
                    return;
                }

                products.forEach(product => {
                    const productCard = createProductCard(product);
                    container.appendChild(productCard);
                });
            })
            .catch(error => {
                console.error('Erro ao carregar produtos:', error);
                const container = document.getElementById('products-container');
                if (container) {
                    container.innerHTML = 
                        '<div class="col-12"><div class="alert alert-danger"><i class="fas fa-exclamation-triangle"></i> Erro ao carregar produtos. Tente novamente.</div></div>';
                }
            });
    }

    function createProductCard(product) {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4 mb-3';

        col.innerHTML = `
            <div class="card h-100 shadow-sm border-0">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <h6 class="card-title mb-0">${product.name}</h6>
                        <span class="badge bg-info">${product.category}</span>
                    </div>
                    <p class="card-text text-muted small mb-3">${product.description || 'Sem descrição'}</p>
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <small class="text-muted">
                                <i class="fas fa-box"></i> Estoque: ${product.stock}
                            </small>
                        </div>
                        <div class="text-end">
                            <strong class="text-success fs-5">€${parseFloat(product.price || 0).toFixed(2)}</strong>
                        </div>
                    </div>
                </div>
                <div class="card-footer bg-light">
                    <button class="btn btn-info btn-sm w-100" onclick="showProductDetails(${product.id})">
                        <i class="fas fa-info-circle"></i> Ver Detalhes
                    </button>
                </div>
            </div>
        `;

        return col;
    }

    // ========================================
    // CARREGAMENTO DE SERVIÇOS
    // ========================================
    function loadServices() {
        fetch('/Public/PublicClientPanel/GetServices')
            .then(response => response.json())
            .then(services => {
                const container = document.getElementById('services-container');
                if (!container) return;
                
                container.innerHTML = '';

                if (services.length === 0) {
                    container.innerHTML = `
                        <div class="col-12">
                            <div class="text-center py-4">
                                <i class="fas fa-concierge-bell fa-3x text-muted mb-3"></i>
                                <h5 class="text-muted">Nenhum serviço encontrado</h5>
                                <p class="text-muted">Tente ajustar os filtros ou entre em contato conosco.</p>
                            </div>
                        </div>
                    `;
                    return;
                }

                services.forEach(service => {
                    const serviceCard = createServiceCard(service);
                    container.appendChild(serviceCard);
                });
            })
            .catch(error => {
                console.error('Erro ao carregar serviços:', error);
                const container = document.getElementById('services-container');
                if (container) {
                    container.innerHTML = 
                        '<div class="col-12"><div class="alert alert-danger"><i class="fas fa-exclamation-triangle"></i> Erro ao carregar serviços. Tente novamente.</div></div>';
                }
            });
    }

    function createServiceCard(service) {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4 mb-3';

        col.innerHTML = `
            <div class="card h-100 shadow-sm border-0">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <h6 class="card-title mb-0">${service.name}</h6>
                        <span class="badge bg-primary">${service.category}</span>
                    </div>
                    <p class="card-text text-muted small mb-3">${service.description || 'Sem descrição'}</p>
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <small class="text-muted">
                                <i class="fas fa-clock"></i> ${service.duration}
                            </small>
                        </div>
                        <div class="text-end">
                            <strong class="text-success fs-5">€${parseFloat(service.price || 0).toFixed(2)}</strong>
                        </div>
                    </div>
                </div>
                <div class="card-footer bg-light">
                    <a href="/Public/PublicBooking/Schedule/${service.id}" class="btn btn-primary btn-sm w-100">
                        <i class="fas fa-calendar-plus"></i> Agendar
                    </a>
                </div>
            </div>
        `;

        return col;
    }

    // ========================================
    // CONFIGURAÇÃO DE EVENT LISTENERS
    // ========================================
    function setupEventListeners() {
        // Botão de cancelar agendamento
        const cancelBtn = document.getElementById('cancel-appointment-btn');
        if (cancelBtn) {
            cancelBtn.addEventListener('click', function() {
                if (currentAppointmentId) {
                    cancelAppointment(currentAppointmentId);
                }
            });
        }

        // Filtros da sidebar
        const filterForm = document.getElementById('filter-form');
        if (filterForm) {
            filterForm.addEventListener('submit', function(e) {
                e.preventDefault();
                applyFilters();
            });
        }

        const clearFilters = document.getElementById('clear-filters');
        if (clearFilters) {
            clearFilters.addEventListener('click', function() {
                clearFilters();
            });
        }

        // Filtros de produtos
        const productFilterForm = document.getElementById('product-filter-form');
        if (productFilterForm) {
            productFilterForm.addEventListener('submit', function(e) {
                e.preventDefault();
                applyProductFilters();
            });
        }

        const clearProductFilters = document.getElementById('clear-product-filters');
        if (clearProductFilters) {
            clearProductFilters.addEventListener('click', function() {
                clearProductFilters();
            });
        }
    }

    // ========================================
    // CANCELAMENTO DE AGENDAMENTO
    // ========================================
    function cancelAppointment(appointmentId) {
        if (!confirm('Tem certeza que deseja cancelar este agendamento?')) {
            return;
        }

        fetch(`/Public/PublicClientPanel/CancelAppointment/${appointmentId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);
                calendar.refetchEvents();
                bootstrap.Modal.getInstance(document.getElementById('appointmentModal')).hide();
            } else {
                alert('Erro: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Erro ao cancelar agendamento:', error);
            alert('Erro ao cancelar agendamento. Tente novamente.');
        });
    }

    // ========================================
    // FILTROS
    // ========================================
    function applyFilters() {
        const formData = new FormData(document.getElementById('filter-form'));
        const params = new URLSearchParams();
        
        for (let [key, value] of formData.entries()) {
            if (value) params.append(key, value);
        }

        fetch(`/Public/PublicClientPanel/GetServices?${params.toString()}`)
            .then(response => response.json())
            .then(services => {
                const container = document.getElementById('services-container');
                if (!container) return;
                
                container.innerHTML = '';

                services.forEach(service => {
                    const serviceCard = createServiceCard(service);
                    container.appendChild(serviceCard);
                });
            })
            .catch(error => {
                console.error('Erro ao aplicar filtros:', error);
            });
    }

    function clearFilters() {
        const filterForm = document.getElementById('filter-form');
        if (filterForm) {
            filterForm.reset();
            loadServices();
        }
    }

    // ========================================
    // FILTROS DE PRODUTOS
    // ========================================
    function applyProductFilters() {
        const formData = new FormData(document.getElementById('product-filter-form'));
        const params = new URLSearchParams();
        
        for (let [key, value] of formData.entries()) {
            if (value) params.append(key, value);
        }

        fetch(`/Public/PublicClientPanel/GetProducts?${params.toString()}`)
            .then(response => response.json())
            .then(products => {
                const container = document.getElementById('products-container');
                if (!container) return;
                
                container.innerHTML = '';

                if (products.length === 0) {
                    container.innerHTML = `
                        <div class="col-12">
                            <div class="text-center py-4">
                                <i class="fas fa-shopping-bag fa-3x text-muted mb-3"></i>
                                <h5 class="text-muted">Nenhum produto encontrado</h5>
                                <p class="text-muted">Tente ajustar os filtros ou entre em contato conosco.</p>
                            </div>
                        </div>
                    `;
                    return;
                }

                products.forEach(product => {
                    const productCard = createProductCard(product);
                    container.appendChild(productCard);
                });
            })
            .catch(error => {
                console.error('Erro ao aplicar filtros de produtos:', error);
            });
    }

    function clearProductFilters() {
        const productFilterForm = document.getElementById('product-filter-form');
        if (productFilterForm) {
            productFilterForm.reset();
            loadProducts();
        }
    }

    // ========================================
    // GOOGLE CALENDAR
    // ========================================
    function linkGoogleCalendar() {
        // Exportar agendamentos para Google Calendar
        fetch('/Public/PublicClientPanel/ExportToGoogleCalendar')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Criar URL do Google Calendar com os eventos
                    const baseUrl = 'https://calendar.google.com/calendar/render?action=TEMPLATE';
                    const events = data.appointments;
                    
                    if (events.length === 0) {
                        alert('Você não possui agendamentos para exportar.');
                        return;
                    }

                    // Para múltiplos eventos, vamos criar um link para o primeiro e mostrar opções
                    const firstEvent = events[0];
                    const googleUrl = `${baseUrl}&text=${encodeURIComponent(firstEvent.title)}&dates=${firstEvent.start}/${firstEvent.end}&details=${encodeURIComponent(firstEvent.description)}&location=${encodeURIComponent(firstEvent.location)}`;
                    
                    if (confirm(`Você possui ${events.length} agendamento(s). Deseja abrir o Google Calendar para adicionar o primeiro evento?`)) {
                        window.open(googleUrl, '_blank');
                    }
                } else {
                    alert('Erro ao exportar agendamentos: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Erro ao exportar para Google Calendar:', error);
                alert('Erro ao exportar agendamentos. Tente novamente.');
            });
    }

    // ========================================
    // SIDEBAR MOBILE
    // ========================================
    function initializeSidebar() {
        const sidebarToggle = document.getElementById('sidebarToggle');
        const sidebar = document.getElementById('sidebar');
        const overlay = document.getElementById('sidebarOverlay');

        if (sidebarToggle && sidebar) {
            sidebarToggle.addEventListener('click', () => {
                sidebar.classList.toggle('sidebar-open');
                if (overlay) {
                    overlay.classList.toggle('show');
                }
            });
        }

        if (overlay) {
            overlay.addEventListener('click', () => {
                sidebar.classList.remove('sidebar-open');
                overlay.classList.remove('show');
            });
        }

        // Fechar sidebar ao redimensionar
        window.addEventListener('resize', () => {
            if (window.innerWidth >= 992) {
                sidebar.classList.remove('sidebar-open');
                if (overlay) {
                    overlay.classList.remove('show');
                }
            }
        });

        // Marcar link ativo
        const currentPath = window.location.pathname;
        document.querySelectorAll('.menu-link').forEach(link => {
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            }
        });
    }

    // ========================================
    // DETALHES DO PRODUTO
    // ========================================
    function showProductDetails(productId) {
        // Por enquanto, mostrar um alert simples
        // Futuramente pode ser implementado um modal mais elaborado
        alert(`Detalhes do produto ID: ${productId}\n\nEsta funcionalidade pode ser expandida para mostrar informações completas do produto, galeria de imagens, etc.`);
    }

    // ========================================
    // EXPOSIÇÃO DE FUNÇÕES GLOBAIS
    // ========================================
    window.linkGoogleCalendar = linkGoogleCalendar;
    window.showProductDetails = showProductDetails;

})();
