// menu-hamburguer.js - Otimizado e unificado
$(document).ready(function () {
    
    // Toggle do menu hambúrguer
    $('.menu-hamburguer').click(function () {
        $(this).toggleClass('active');
        $('.nav-mobile').toggleClass('active');
        
        // Previne scroll do body quando menu está aberto
        if ($('.nav-mobile').hasClass('active')) {
            $('body').css('overflow', 'hidden');
        } else {
            $('body').css('overflow', 'auto');
        }
    });
    
    // Fecha o menu ao clicar em um link
    $('.nav-mobile a').click(function () {
        $('.menu-hamburguer').removeClass('active');
        $('.nav-mobile').removeClass('active');
        $('body').css('overflow', 'auto');
    });
    
    // Fecha o menu ao clicar fora dele
    $(document).click(function (e) {
        if (!$(e.target).closest('.menu-hamburguer, .nav-mobile').length) {
            $('.menu-hamburguer').removeClass('active');
            $('.nav-mobile').removeClass('active');
            $('body').css('overflow', 'auto');
        }
    });
    
    // Dropdown hover para desktop
    $('.dropdown').hover(
        function () {
            if ($(window).width() > 768) {
                $(this).find('.dropdown-content').stop(true, true).slideDown(200);
            }
        },
        function () {
            if ($(window).width() > 768) {
                $(this).find('.dropdown-content').stop(true, true).slideUp(200);
            }
        }
    );
    
    // Dropdown click para mobile
    $('.dropdown .dropbtn').click(function (e) {
        if ($(window).width() <= 768) {
            e.preventDefault();
            $(this).next('.dropdown-content').slideToggle(200);
        }
    });
    
    // Fecha dropdowns ao redimensionar a janela
    $(window).resize(function () {
        if ($(window).width() > 768) {
            $('.nav-mobile').removeClass('active');
            $('.menu-hamburguer').removeClass('active');
            $('body').css('overflow', 'auto');
        }
    });
    
    // Smooth scroll para links internos
    $('a[href^="#"]').click(function (e) {
        e.preventDefault();
        var target = $(this.getAttribute('href'));
        if (target.length) {
            $('html, body').animate({
                scrollTop: target.offset().top - 80
            }, 1000);
        }
    });
    
    // Adiciona classe ativa ao link da página atual
    var currentPath = window.location.pathname;
    $('.nav-desktop a, .nav-mobile a').each(function () {
        if ($(this).attr('href') === currentPath) {
            $(this).addClass('active');
        }
    });
});