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
                latestDiffs: {}
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
                }
            },
            created() {
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
            }
        });
    }
};