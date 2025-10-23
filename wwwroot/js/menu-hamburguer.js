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
        
        // Fecha dropdowns ao clicar fora
        if (!$(e.target).closest('.nav-desktop .dropdown').length) {
            $('.nav-desktop .dropdown-content').fadeOut(200);
        }
        
        // Fecha dropdowns mobile ao clicar fora
        if (!$(e.target).closest('.dropdown-mobile').length) {
            $('.dropdown-content-mobile').slideUp(200);
            $('.dropbtn-mobile').removeClass('active');
        }
    });
    
    // Dropdown hover para desktop - com especificidade para nav-desktop
    $('.nav-desktop .dropdown').hover(
        function () {
            if ($(window).width() > 768) {
                $(this).find('.dropdown-content').stop(true, true).fadeIn(200);
            }
        },
        function () {
            if ($(window).width() > 768) {
                $(this).find('.dropdown-content').stop(true, true).fadeOut(200);
            }
        }
    );
    
    // Dropdown click para mobile (desktop)
    $('.nav-desktop .dropdown .dropbtn').click(function (e) {
        if ($(window).width() <= 768) {
            e.preventDefault();
            e.stopPropagation();
            $(this).next('.dropdown-content').fadeToggle(200);
        }
    });
    
    // Dropdown click para mobile (menu mobile)
    $('.dropdown-mobile .dropbtn-mobile').click(function (e) {
        e.preventDefault();
        $(this).toggleClass('active');
        $(this).next('.dropdown-content-mobile').slideToggle(200);
    });
    
    // Fecha dropdowns ao redimensionar a janela
    $(window).resize(function () {
        if ($(window).width() > 768) {
            $('.nav-mobile').removeClass('active');
            $('.menu-hamburguer').removeClass('active');
            $('body').css('overflow', 'auto');
            $('.dropdown-content-mobile').slideUp(200);
            $('.dropbtn-mobile').removeClass('active');
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