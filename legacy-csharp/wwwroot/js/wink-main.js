// Wink Main JavaScript

$(document).ready(function() {
    
    // Menu Toggle
    $('.bar, .bar_mobile').click(function() {
        $('#menu_bar').addClass('active');
        $('#mask').addClass('active');
        $('body').css('overflow', 'hidden');
    });
    
    // Close Menu
    $('.close_menu, .close, #mask').click(function() {
        $('#menu_bar').removeClass('active');
        $('#mask').removeClass('active');
        $('body').css('overflow', 'auto');
    });
    
    // Dropdown Toggle
    $('.dropdown-trigger').hover(
        function() {
            $(this).find('.about_dropdown').show();
        },
        function() {
            $(this).find('.about_dropdown').hide();
        }
    );
    
    // Slider Initialization
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
    
    // Smooth Scrolling
    $('a[href^="#"]').on('click', function(event) {
        var target = $(this.getAttribute('href'));
        if (target.length) {
            event.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 80
            }, 1000);
        }
    });
    
    // Header Scroll Effect
    $(window).scroll(function() {
        if ($(this).scrollTop() > 100) {
            $('header').addClass('scrolled');
        } else {
            $('header').removeClass('scrolled');
        }
    });
    
    // Animation on Scroll
    function animateOnScroll() {
        $('.animated').each(function() {
            var elementTop = $(this).offset().top;
            var elementBottom = elementTop + $(this).outerHeight();
            var viewportTop = $(window).scrollTop();
            var viewportBottom = viewportTop + $(window).height();
            
            if (elementBottom > viewportTop && elementTop < viewportBottom) {
                $(this).addClass('animate');
            }
        });
    }
    
    $(window).on('scroll', animateOnScroll);
    animateOnScroll(); // Run on page load
    
    // Video Play Functionality
    $('.play').click(function() {
        // Add video play functionality here
        console.log('Video play clicked');
    });
    
    // Mobile Menu Toggle
    if ($(window).width() <= 768) {
        $('.menu').hide();
        $('.bar_mobile').show();
    } else {
        $('.menu').show();
        $('.bar_mobile').hide();
    }
    
    $(window).resize(function() {
        if ($(window).width() <= 768) {
            $('.menu').hide();
            $('.bar_mobile').show();
        } else {
            $('.menu').show();
            $('.bar_mobile').hide();
        }
    });
    
});

// CSS for scrolled header
const style = document.createElement('style');
style.textContent = `
    header.scrolled {
        background: rgba(255, 255, 255, 0.95);
        backdrop-filter: blur(10px);
        box-shadow: 0 2px 20px rgba(0,0,0,0.1);
    }
    
    .animate {
        animation-duration: 1s;
        animation-fill-mode: both;
    }
`;
document.head.appendChild(style);
