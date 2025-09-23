$(document).ready(function () {
    $('#demo-pie-1').pieChart({
        barColor: '#2dde98',
        trackColor: '#eee',
        lineCap: 'round',
        lineWidth: 8,
        onStep: function (from, to, percent) {
            $(this.element).find('.pie-value').text(Math.round(percent) + '%');
        }
    });

    $('#demo-pie-2').pieChart({
        barColor: '#8e43e7',
        trackColor: '#eee',
        lineCap: 'butt',
        lineWidth: 8,
        onStep: function (from, to, percent) {
            $(this.element).find('.pie-value').text(Math.round(percent) + '%');
        }
    });

    $('#demo-pie-3').pieChart({
        barColor: '#ffc168',
        trackColor: '#eee',
        lineCap: 'square',
        lineWidth: 8,
        onStep: function (from, to, percent) {
            $(this.element).find('.pie-value').text(Math.round(percent) + '%');
        }
    });
});
