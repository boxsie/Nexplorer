import Vue from 'vue';
import Moment from 'moment';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import dataTableVue from '../Library/dataTableVue';

import '../../Style/blocks.block.scss';

export class BlockViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                confirmations: options.confirmations,
                showNext: options.showNext,
                dtOptions: {
                    ajaxUrl: '/transactions/gettransactions',
                    filterClass: 'col-sm-6 no-bg p-0'
                },
                filterCriteria: {
                    txType: 'All',
                    minAmount: null,
                    maxAmount: null,
                    heightFrom: options.height,
                    heightTo: options.height,
                    utcFrom: null,
                    utcTo: null,
                    orderBy: 0
                },
                ignoreKeys: ['heightFrom', 'heightTo'],
                filters: [],
                columns: [
                    {
                        key: 'timestamp',
                        class: 'col-5 col-sm-2 text-right text-sm-left order-2 order-sm-2',
                        header: '<span class="d-none d-sm-inline fa fa-calendar-o"></span>',
                        render: (data, row) => {
                            var timestamp = Moment(data).format('DD/MMM/YY HH:mm:ss');
                            return `<span>${timestamp}</span>`;
                        }
                    },
                    {
                        key: 'transactionHash',
                        class: 'col-7 col-sm-4 order-1 order-sm-2',
                        header: '<span class="d-none d-sm-inline fa fa-hashtag"></span>',
                        render: (data, row) => {
                            return `<span class="d-sm-none inline-icon fa fa-hashtag"></span>
                                    <a class="d-none d-md-inline" href="/transactions/${data}">${this.vm.truncateHash(data, 26)}</a>
                                    <a class="d-md-none" href="/transactions/${data}">${this.vm.truncateHash(data, 20)}</a>`;
                        }
                    },
                    {
                        key: 'transactionType',
                        class: 'col-6 col-sm-2 in-out text-left text-sm-center text-md-left order-3',
                        header: '<span class="d-none d-sm-inline fa fa-exchange"></span>',
                        render: (data, row) => {
                            switch (data) {
                                case 0:
                                    return this.vm.createInOutText('fa-hashtag', 'Coinbase hash');
                                case 1:
                                    return this.vm.createInOutText('fa-microchip', 'Coinbase prime');
                                case 2:
                                    return this.vm.createInOutText('fa-bolt', 'Coinstake genesis');
                                case 3:
                                    return this.vm.createInOutText('fa-bolt', 'Coinstake');
                                case 4:
                                    return this.vm.createInOutText('fa-user', 'User transaction');
                            }

                            return 'Unknown';
                        }
                    },
                    {
                        key: 'transactionInputCount',
                        class: 'd-none d-sm-block in-out order-4',
                        header: '<span class="fa fa-compress"></span>',
                        render: (data, row) => {
                            return `<strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    },
                    {
                        key: 'transactionOutputCount',
                        class: 'd-none d-sm-block in-out order-5',
                        header: '<span class="fa fa-expand"></span>',
                        render: (data, row) => {
                            return `<strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    },
                    {
                        key: 'amount',
                        class: 'col-6 col-sm-2 text-right order-6',
                        header: '<span class="d-none d-sm-inline fa fa-balance-scale"></span>',
                        render: (data, row) => {
                            return `<strong>${parseFloat(data.toFixed(2)).toLocaleString()}</strong> <small>NXS</small>`;
                        }
                    }
                ]
            },
            computed: {
                confirmationText() {
                    return this.confirmations.toLocaleString();
                }
            },
            components: {
                txTable: dataTableVue
            },
            methods: {
                selectTransaction(tx) {
                    window.location.href = `/transactions/${tx.transactionHash}`;
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                },
                createInOutText(faType, txType) {
                    return `<span class="fa ${faType} inline-icon"></span> <span class="d-sm-none d-md-inline">${txType}</span>`;
                }
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/blockhub').build();

                this.connection.on('newBlockPubSub', (block) => {
                    this.confirmations++;
                    this.showNext = true;
                });

                this.connection.start();
            }
        });
    }
}
