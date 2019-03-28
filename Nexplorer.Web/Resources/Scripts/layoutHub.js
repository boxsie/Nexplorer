import $ from 'jquery';
import Vue from 'vue';
import moment from 'moment';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

export default {
    install(Vue, options) {
        Vue.prototype.$layoutHub = new Vue({
            data: {
                utcMoment: moment(),
                latestBlock: {},
                latestPrice: {},
                latestDiffs: {},
                navTop: 0,
                layoutTop: null,
                mainBodyTop: 0,
                layoutHeight: 40,
                layoutClass: 'position-absolute',
                lastScrollTop: null
            },
            computed: {
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
                parseBlockChannel(channel) {
                    return options.blockChannels[channel];
                },
                parseTxType(txType) {
                    return options.txTypes[txType];
                },
                parseBytes(bytes) {
                    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
                    if (bytes === 0) return '0 Byte';
                    const i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
                    return Math.round(bytes / Math.pow(1024, i), 2) + ' ' + sizes[i];
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
                onScroll(instant) {
                    if (!this.waitingForTimeout && !instant) {
                        this.waitingForTimeout = true;

                        setTimeout(() => {
                            this.waitingForTimeout = false;
                        }, 50);

                        return;
                    }

                    const doc = document.documentElement;
                    const left = (window.pageXOffset || doc.scrollLeft) - (doc.clientLeft || 0);
                    const top = (window.pageYOffset || doc.scrollTop) - (doc.clientTop || 0);

                    this.navTop += (this.lastScrollTop - top) * 0.25;

                    if (top < options.navHeight && this.navTop > top) {
                        this.navTop = -top;
                    } else if (this.navTop > 0) {
                        this.navTop = 0;
                    } else if (this.navTop < -options.navHeight) {
                        this.navTop = -options.navHeight;
                    }

                    if (top > this.mainBodyTop) {
                        this.layoutTop = -(this.navTop + options.navHeight);
                        this.layoutClass = 'position-fixed';
                    } else {
                        this.layoutTop = this.mainBodyTop;
                        this.layoutClass = 'position-absolute';
                    }

                    this.lastScrollTop = top;
                },
                windowResize() {
                    const top = $('#mainBody').offset().top - this.layoutHeight;

                    if (top !== this.mainBodyTop) {
                        this.mainBodyTop = top;
                        this.onScroll(true);
                    }
                }
            },
            created() {
                window.addEventListener('resize', this.windowResize);
                window.addEventListener('scroll', this.onScroll);

                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/layouthub').build();

                this.connection.start()
                    .then(() => {
                        this.connection.on('updateTimestampUtc',
                            (timestamp) => {
                                this.utcMoment = moment(timestamp);
                            });
                        
                        this.connection.invoke('getLatestTimestampUtc').then((timestamp) => {
                            this.utcMoment = moment(timestamp);
                        });

                        this.connection.on('updateLatestBlockData',
                            (latestBlock) => {
                                this.latestBlock = latestBlock;
                            });
                        
                        this.connection.invoke('getLatestBlock').then((latestBlock) => {
                            this.latestBlock = latestBlock;
                        });
                        
                        this.connection.on('updateLatestPriceData',
                            (latestPrice) => {
                                this.latestPrice = latestPrice;
                            });

                        this.connection.invoke('getLatestPrice').then((latestPrice) => {
                            this.latestPrice = latestPrice;
                        });

                        this.connection.on('updateLatestDiffData',
                            (latestDiffs) => {
                                this.latestDiffs = latestDiffs;
                            });

                        this.connection.invoke('getLatestDiffs').then((latestDiffs) => {
                            this.latestDiffs = latestDiffs;
                        });
                    });

                setInterval(() => {
                    this.utcMoment = moment(this.utcMoment.add(1, 's'));
                }, 1000);

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

                    self.windowResize();
                });
            }
        });
    }
};