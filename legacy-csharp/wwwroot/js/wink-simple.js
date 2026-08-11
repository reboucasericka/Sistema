// wink-simple.js

$(document).ready(function () {

    //  Abre o menu lateral ao clicar no ícone hambúrguer
    $('.bar, .bar_mobile').click(function () {
        $('#menu_bar').addClass('active');
        $('#mask').addClass('active');
        $('body').css('overflow', 'hidden'); // Evita scroll no fundo
    });

    //  Fecha o menu lateral ao clicar no X ou fora dele
    $('.close_menu, .close, #mask').click(function () {
        $('#menu_bar').removeClass('active');
        $('#mask').removeClass('active');
        $('body').css('overflow', 'auto');
    });

    // Dropdown com hover (desktop)
    $('.dropdown-trigger').hover(
        function () {
            if ($(window).width() > 768) {
                $(this).find('.about_dropdown').stop(true, true).slideDown(200);
            }
        },
        function () {
            if ($(window).width() > 768) {
                $(this).find('.about_dropdown').stop(true, true).slideUp(200);
            }
        }
    );

    //Dropdown com clique (mobile)
    $('.dropdown-trigger').on('click', function (e) {
        if ($(window).width() <= 768) {
            e.preventDefault();
            $(this).find('.about_dropdown').stop(true, true).slideToggle(200);
        }
    });

    // ✅ Inicializa o slider se existir
    if ($('.slider_home').length) {
        $('.slider_home').slick({
            dots: false,
            infinite: true,
            speed: 1000,
            slidesToShow: 1,
            slidesToScroll: 1,
            autoplay: true,
            autoplaySpeed: 5000,
            fade: true,
            cssEase: 'linear',
            prevArrow: '<button type="button" class="slick-prev"><i class="fas fa-chevron-left"></i></button>',
            nextArrow: '<button type="button" class="slick-next"><i class="fas fa-chevron-right"></i></button>',
            responsive: [
                {
                    breakpoint: 768,
                    settings: {
                        arrows: false
                    }
                }
            ]
        });
    }

    //Função para alternar visibilidade do menu (evita duplicação)
    function toggleMenuVisibility() {
        if ($(window).width() <= 768) {
            $('.menu').hide();
            $('.bar_mobile').show();
        } else {
            $('.menu').show();
            $('.bar_mobile').hide();
        }
    }

    toggleMenuVisibility(); // Executa ao carregar
    $(window).resize(toggleMenuVisibility); // Executa ao redimensionar

});