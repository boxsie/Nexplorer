import $ from 'jquery';
import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import layoutHub from './layoutHub';

import 'font-awesome/scss/font-awesome.scss';
import '../Style/layout.scss';

export class LayoutViewModel {
    constructor(options) {
        Vue.use(layoutHub);

        this.navVm = new Vue({
            el: '#navbar',
            data: {
                isSignedIn: options.isSignedIn,
                searchOpen: false,
                userOpen: false,
                searchFocusDelay: 400,
                navExpanded: true,
                xsNav: false
            },
            computed: {
                expandableNavCss() {
                    return this.xsNav ? '' : this.navExpanded ? 'nav-pad-out' : 'nav-pad-in';
                },
                expandableIconCss() {
                    return this.xsNav ? '' : this.navExpanded ? 'icon-pad-out' : 'icon-pad-in';
                },
                userMenuCss() {
                    const openClose = this.userOpen ? 'open' : 'closed';
                    const signInOut = this.isSignedIn ? 'signed-in' : '';

                    return openClose + ' ' + signInOut;
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

                    const target = e.target;

                    if (searchEl !== target && !searchEl.contains(target)) {
                        this.searchOpen = false;
                        this.checkForNavExpand();
                    }

                    if (userEl !== target && !userEl.contains(target)) {
                        this.userOpen = false;
                    }
                },
                checkForNavExpand() {
                    if (!this.searchOpen && $(document).scrollTop() > 50) {
                        this.navExpanded = false;
                    } else if (!this.navExpanded) {
                        this.navExpanded = true;
                    }
                },
                windowResize() {
                    if (this.isBreakpoint('xs')) {
                        this.xsNav = true;

                        if (this.navExpanded) {
                            this.navExpanded = false;
                        }
                    } else {
                        this.xsNav = false;
                        this.checkForNavExpand();
                    }
                }
            },
            created() {
                window.addEventListener('resize', this.windowResize);
                window.addEventListener('scroll', this.checkForNavExpand);
                document.addEventListener('click', this.documentClick);
            }
        });

        this.tickerVm = new Vue({
            el: '#layout',
            data: {
                userSettings: options.userSettings,
                lastPrice: 0
            },
            computed: {
                height() {
                    this.pulseElement($(this.$refs.tickerHeight));
                    return this.$layoutHub.latestBlock.height ? this.$layoutHub.latestBlock.height.toLocaleString() : ' - ';
                },
                heightUrl() {
                    return this.$layoutHub.latestBlock.height ? `/blocks/${this.$layoutHub.latestBlock.height}` : '';
                },
                price() {
                    if (this.$layoutHub.latestPrice.last && this.$layoutHub.latestPrice.last !== this.lastPrice) {
                        this.lastPrice = this.$layoutHub.latestPrice.last;
                        this.pulseElement($(this.$refs.tickerPrice));

                        return this.$layoutHub.latestPrice.last.toFixed(8);
                    }

                    return this.lastPrice ? this.lastPrice : ' - ';
                },
                diffPos() {
                    this.pulseElement($(this.$refs.tickerDiffPos));
                    return this.$layoutHub.latestDiffs.pos ? this.parseDifficulty(this.$layoutHub.latestDiffs.pos) : ' - ';
                },
                diffHash() {
                    this.pulseElement($(this.$refs.tickerDiffHash));
                    return this.$layoutHub.latestDiffs.hash ? this.parseDifficulty(this.$layoutHub.latestDiffs.hash) : ' - ';
                },
                diffPrime() {
                    this.pulseElement($(this.$refs.tickerDiffPrime));
                    return this.$layoutHub.latestDiffs.prime ? this.parseDifficulty(this.$layoutHub.latestDiffs.prime) : ' - ';
                }
            },
            methods: {
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
                var self = this;

                $(() => {
                    var diffs = $(this.$refs.mobileDiffs).children();
                    var i = 0;
                    var currentDiff = diffs[i];

                    console.log(diffs);

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