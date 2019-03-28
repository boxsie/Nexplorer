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
                userSettings: options.userSettings,
                lastPrice: 0,
                waitingForTimeout: false
            },
            computed: {
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
                closeCookieWarning() {
                    $(this.$refs.cookieWarning).height(0);

                    $.ajax({
                        url: '/home/dismissCookieWarning',
                        method: 'POST'
                    });
                }
            },
            created() {
                document.addEventListener('click', this.documentClick);
                document.addEventListener('touchstart', this.documentClick);
            }
        });
    }
}