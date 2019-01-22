import Swiper from 'swiper/dist/js/swiper';

export default {
    template: require('../../Markup/swiper-vue.html'),
    mounted() {
        this.swiper = new Swiper('.swiper-container', {
            slidesPerView: 4,
            spaceBetween: 0,
            freeMode: true,
            freeModeSticky: true,
            grabCursor: true,
            speed: 250,
            autoplay: {
                delay: 4000,
                disableOnInteraction: true
            },
            breakpoints: {
                500: {
                    slidesPerView: 1
                },
                768: {
                    slidesPerView: 2
                },
                1100: {
                    slidesPerView: 3
                }
            }
        });
    }
};

