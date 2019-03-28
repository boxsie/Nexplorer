import $ from 'jquery';
import Vue from 'vue';
import 'bootstrap';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import Ads from 'vue-google-adsense';

import layoutHub from './layoutHub';

import '../Style/layout.scss';

export class LayoutViewModel {
    constructor(options) {
        Vue.use(require('vue-script2'));
        Vue.use(Ads.Adsense);
        Vue.use(Ads.InArticleAdsense);
        Vue.use(Ads.InFeedAdsense);
        Vue.use(layoutHub, options);

        this.navVm = new Vue({
            el: '#header',
            data: {
                isSignedIn: options.isSignedIn,
                searchOpen: false,
                userOpen: false,
                searchFocusDelay: 400,
                navTop: 0,
                layoutTop: 0,
                mainBodyTop: 0,
                layoutHeight: 40,
                lastScrollTop: null,
                xsNav: false,
                userSettings: options.userSettings,
                lastPrice: 0
            },
            computed: {
                userMenuCss() {
                    const openClose = this.userOpen ? 'open' : 'closed';
                    const signInOut = this.isSignedIn ? 'signed-in' : '';

                    return openClose + ' ' + signInOut;
                },
                height() {
                    this.pulseElement($(this.$refs.tickerHeight));

                    if (!this.$layoutHub.latestBlock || !this.$layoutHub.latestBlock.height) {
                        return ' - ';
                    }

                    return this.$layoutHub.latestBlock.height.toLocaleString();
                },
                heightUrl() {
                    if (!this.$layoutHub.latestBlock || !this.$layoutHub.latestBlock.height) {
                        return '';
                    }

                    return this.$layoutHub.latestBlock.height ? `/blocks/${this.$layoutHub.latestBlock.height}` : '';
                },
                price() {
                    if (!this.$layoutHub.latestPrice || !this.$layoutHub.latestPrice.last || this.$layoutHub.latestPrice.last === this.lastPrice) {
                        return this.lastPrice ? this.lastPrice : ' - ';
                    }

                    this.lastPrice = this.$layoutHub.latestPrice.last;
                    this.pulseElement($(this.$refs.tickerPrice));

                    return this.$layoutHub.latestPrice.last.toFixed(8);
                },
                diffPos() {
                    if (!this.$layoutHub.latestDiffs || !this.$layoutHub.latestDiffs.pos) {
                        return ' - ';
                    }

                    this.pulseElement($(this.$refs.tickerDiffPos));
                    return this.parseDifficulty(this.$layoutHub.latestDiffs.pos);
                },
                diffHash() {
                    if (!this.$layoutHub.latestDiffs || !this.$layoutHub.latestDiffs.hash) {
                        return ' - ';
                    }

                    this.pulseElement($(this.$refs.tickerDiffHash));
                    return this.parseDifficulty(this.$layoutHub.latestDiffs.hash);
                },
                diffPrime() {
                    if (!this.$layoutHub.latestDiffs || !this.$layoutHub.latestDiffs.prime) {
                        return ' - ';
                    }

                    this.pulseElement($(this.$refs.tickerDiffPrime));
                    return this.parseDifficulty(this.$layoutHub.latestDiffs.prime);
                }
            },
            methods: {
                isBreakpoint(alias) {
                    return $('.device-' + alias).is(':visible');
                },
                openSearch() {
                    this.searchOpen = true;
                    
                    setTimeout(() => {
                        this.$refs.searchTerm.focus();
                    }, this.searchFocusDelay);

                    if (!this.isBreakpoint('xs')) {
                        this.navExpanded = true;
                    }
                },
                openUser() {
                    if (!this.userOpen) {
                        this.userOpen = true;
                    } else {
                        this.userOpen = false;
                    }
                },
                documentClick(e) {
                    const searchEl = this.$refs.navSearch;
                    const userEl = this.$refs.userMenu;
                    const navLinksEl = this.$refs.navbarLinks;

                    const target = e.target;

                    if (searchEl !== target && !searchEl.contains(target)) {
                        this.searchOpen = false;
                    }

                    if (userEl !== target && !userEl.contains(target)) {
                        this.userOpen = false;
                    }

                    if (navLinksEl !== target && !navLinksEl.contains(target)) {
                        $(navLinksEl).collapse('hide');
                    }
                },
                onScroll(e) {
                    if (!this.$refs.nav.clientHeight) {
                        return;
                    }

                    const doc = document.documentElement;
                    const left = (window.pageXOffset || doc.scrollLeft) - (doc.clientLeft || 0);
                    const top = (window.pageYOffset || doc.scrollTop) - (doc.clientTop || 0);
                    
                    this.navTop += (this.lastScrollTop - top) * 0.25;

                    if (top < this.$refs.nav.clientHeight && this.navTop > top) {
                        this.navTop = -top;
                    } else if (this.navTop > 0) {
                        this.navTop = 0;
                    } else if (this.navTop < -this.$refs.nav.clientHeight) {
                        this.navTop = -this.$refs.nav.clientHeight;
                    }

                    if (top < this.mainBodyTop) {
                        this.layoutTop = this.mainBodyTop - top;
                    } else {
                        this.layoutTop = -(this.navTop + this.$refs.nav.clientHeight);
                    }

                    this.lastScrollTop = top;
                },
                windowResize() {
                    if (this.isBreakpoint('xs')) {
                        this.xsNav = true;

                        if (this.navExpanded) {
                            this.navExpanded = false;
                        }
                    } else {
                        this.xsNav = false;
                    }

                    const top = $('#mainBody').offset().top - this.layoutHeight;

                    if (top !== this.mainBodyTop) {
                        this.mainBodyTop = top;
                        this.onScroll();
                    }
                },
                parseDifficulty(diff) {
                    return parseFloat(diff.toFixed(8)).toLocaleString(undefined, { 'minimumFractionDigits': 2, 'maximumFractionDigits': 6 });
                },
                pulseElement($elem) {
                    $elem.addClass('pulse');

                    setTimeout(() => {
                            $elem.removeClass('pulse');
                        },
                        1000);
                },
                closeCookieWarning() {
                    $(this.$refs.cookieWarning).height(0);

                    $.ajax({
                        url: '/home/dismissCookieWarning',
                        method: 'POST'
                    });
                }
            },
            created() {
                window.addEventListener('resize', this.windowResize);
                window.addEventListener('scroll', this.onScroll);
                document.addEventListener('click', this.documentClick);
                document.addEventListener('touchstart', this.documentClick);
                
                const self = this;

                $(() => {
                    var diffs = $(this.$refs.mobileDiffs).children();
                    var i = 0;
                    var currentDiff = diffs[i];

                    setInterval(() => {
                        $(currentDiff).fadeOut('fast', () => {
                            i++;

                            if (i >= diffs.length) {
                                i = 0;
                            }

                            currentDiff = diffs[i];
                            $(currentDiff).fadeIn('fast');
                        });
                    }, 5000);
                });
            }
        });

        $(() => {
            this.navVm.windowResize();
        });
    }
}