import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import streamingLineChart from '../Library/streamingChart.js';
import '../../Style/staking.index.scss';

export class StakingIndexViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                
            },
            components: {
                
            },
            methods: {
                
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/mininghub').build();
                
                this.connection.start().then(() => {
                });
            }
        });
    }
}
