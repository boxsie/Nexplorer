import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import datePickerVue from '../Library/datePickerVue.js';
import streamingLineChart from '../Library/streamingChart.js';
import '../../Style/staking.index.scss';

export class StakingIndexViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                ud: [
                    {
                        startDate: '27/03/2019',
                        endDate: '30/03/2019'
                    },
                    {
                        startDate: '02/04/2019',
                        endDate: '03/04/2019'
                    },
                    {
                        startDate: '03/04/2019',
                        endDate: '04/04/2019'
                    },
                    {
                        startDate: '04/04/2019',
                        endDate: '07/04/2019'
                    },
                    {
                        startDate: '09/04/2019',
                        endDate: '16/04/2019'
                    },
                    {
                        startDate: '19/04/2019',
                        endDate: '22/04/2019'
                    }
                ],
                dpOptions: {
                    allowPast: false,
                    maxDate: '11/08/2020'
                }
            },
            components: {
                datePicker: datePickerVue
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
