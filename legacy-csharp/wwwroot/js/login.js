$(document).ready(function () {
    // Efeito de parallax simples sem dependências externas
    $(document).mousemove(function (e) {
        var x = e.pageX / 8;
        var y = e.pageY / 12;
        $('body').css('background-position', x + 'px ' + y + 'px');
    });
});