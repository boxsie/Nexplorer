import $ from 'jquery';
import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import layoutHub from './layoutHub.js';

import 'font-awesome/scss/font-awesome.scss';
import '../Style/layout.scss';

export class LayoutViewModel {
    constructor(options) {
        Vue.use(layoutHub);

        this.vm = new Vue({
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

        $(() => {
            this.vm.windowResize();
        });
    }
}