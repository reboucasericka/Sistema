// painel.js - Otimizado para Chart.js 3.x
$(document).ready(function () {
    // Inicialização dos gráficos de pizza usando Chart.js 3.x
    if (typeof Chart !== 'undefined') {
        // Gráfico 1 - Agendamentos
        var ctx1 = document.getElementById('demo-pie-1');
        if (ctx1) {
            new Chart(ctx1, {
                type: 'doughnut',
                data: {
                    datasets: [{
                        data: [75, 25],
                        backgroundColor: ['#2dde98', '#eee'],
                        borderWidth: 0
                    }]
                },
                options: {
                    cutout: '70%',
                    plugins: {
                        legend: { display: false }
                    }
                }
            });
        }

        // Gráfico 2 - Clientes
        var ctx2 = document.getElementById('demo-pie-2');
        if (ctx2) {
            new Chart(ctx2, {
                type: 'doughnut',
                data: {
                    datasets: [{
                        data: [60, 40],
                        backgroundColor: ['#8e43e7', '#eee'],
                        borderWidth: 0
                    }]
                },
                options: {
                    cutout: '70%',
                    plugins: {
                        legend: { display: false }
                    }
                }
            });
        }

        // Gráfico 3 - Serviços
        var ctx3 = document.getElementById('demo-pie-3');
        if (ctx3) {
            new Chart(ctx3, {
                type: 'doughnut',
                data: {
                    datasets: [{
                        data: [80, 20],
                        backgroundColor: ['#ffc168', '#eee'],
                        borderWidth: 0
                    }]
                },
                options: {
                    cutout: '70%',
                    plugins: {
                        legend: { display: false }
                    }
                }
            });
        }
    }
});