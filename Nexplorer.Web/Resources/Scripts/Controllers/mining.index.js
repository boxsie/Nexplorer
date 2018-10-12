import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import streamingLineChart from '../Library/streamingChart.js';
import '../../Style/mining.index.scss';

export class MiningIndexViewModel {
    constructor(options) {
        this.blocksVue = new Vue({
            el: '#main',
            data: {
                recentChannelStats: options.recentChannelStats,
                latestChannelStats: options.latestChannelStats,
                latestSuppyRates: options.latestSupplyRates,
                updateDelayMs: 10000,
                chartHeight: 75
            },
            components: {
                hashDifficultyChart: streamingLineChart,
                hashRewardChart: streamingLineChart,
                hashPerSecChart: streamingLineChart,
                hashReserveChart: streamingLineChart,
                primeDifficultyChart: streamingLineChart,
                primeRewardChart: streamingLineChart,
                primePerSecChart: streamingLineChart,
                primeReserveChart: streamingLineChart,
                posDifficultyChart: streamingLineChart
            },
            methods: {
                numberToGiga(num) {
                    return num ? num / 1000000000 : 0;
                },
                getChannelStatInnit(channel, name) {
                    return this.recentChannelStats[channel].map((x) => {
                        return {
                            date: x.createdOn,
                            val: x[name]
                        };
                    });
                },
                getPercentChange(latest, channel, name) {
                    if (this.recentChannelStats) {
                        const cStats = this.recentChannelStats[channel];

                        const startNum = cStats[0][name];

                        return (((latest - startNum) / startNum) * 100).toFixed(4);
                    }

                    return 0;
                },
                formatFractionNumber(num) {
                    return num ? num.toLocaleString('en-US', { style: 'decimal', maximumFractionDigits: 8, minimumFractionDigits: 0 }) : 0;
                }
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/mininghub').build();

                this.connection.on('statPublish', (statDtoJson) => {
                    if (statDtoJson) {
                        const statDto = JSON.parse(statDtoJson);

                        if (statDto.channelStats) {
                            this.latestChannelStats = statDto.channelStats;
                        }

                        if (statDto.supplyRate) {
                            this.latestSuppyRates = statDto.supplyRate;
                        }
                    }
                });

                this.connection.start().then(() => {
                });
            }
        });
    }
}
