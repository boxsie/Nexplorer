import Vue from 'vue';
import Moment from 'moment';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import dataTableVue from '../Library/dataTableVue';

import '../../Style/transactions.index.scss';

export class TransactionViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                dtOptions: {
                    defaultCriteria: {
                        txType: null,
                        minAmount: null,
                        maxAmount: null,
                        heightFrom: null,
                        heightTo: null,
                        utcFrom: null,
                        utcTo: null,
                        orderBy: 0
                    },
                    ajaxUrl: '/transactions/gettransactions',
                    filters: [
                        {
                            name: 'Latest',
                            criteria: {}
                        },
                        {
                            name: 'User',
                            criteria: { txType: '4' }
                        },
                        {
                            name: 'Custom',
                            isUserFilter: true
                        }
                    ]
                },
                columns: [
                    {
                        key: 'timestamp',
                        class: 'col-5 col-sm-2',
                        header: '<span class="d-none d-sm-inline fa fa-calendar-o"></span>',
                        render: (data, row) => {
                            var timestamp = Moment(data).format('DD/MMM/YY HH:mm:ss');
                            return `<span>${timestamp}</span>`;
                        }
                    },
                    {
                        key: 'transactionHash',
                        class: 'col-4 col-sm-2',
                        header: '<span class="d-none d-sm-inline fa fa-hashtag"></span>',
                        render: (data, row) => {
                            return `<span class="d-sm-none inline-icon fa fa-hashtag"></span>
                                    <a class="d-none d-md-block" href="/transactions/${data}">${this.vm.truncateHash(data, 10)}</a>
                                    <a class="d-none d-sm-block d-md-none" href="/transactions/${data}">${this.vm.truncateHash(data, 8)}</a>
                                    <a class="d-sm-none" href="/transactions/${data}">${this.vm.truncateHash(data, 8)}</a>`;
                        }
                    },
                    {
                        key: 'blockHeight',
                        class: 'col-3 col-sm-2 text-right text-sm-left',
                        header: '<span class="d-none d-sm-inline fa fa-cube"></span>',
                        render: (data, row) => {
                            return `<span class="d-sm-none inline-icon fa fa-cube"></span>
                                    <a href="/blocks/${data}"><span class="d-none d-sm-inline">#</span>${data}</a>`;
                        }
                    },
                    {
                        key: 'transactionType',
                        class: 'col-6 col-sm-2 in-out text-left text-sm-center text-md-left',
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
                        class: 'd-none d-sm-block in-out',
                        header: '<span class="fa fa-compress"></span>',
                        render: (data, row) => {
                            return `<strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    },
                    {
                        key: 'transactionOutputCount',
                        class: 'd-none d-sm-block in-out',
                        header: '<span class="fa fa-expand"></span>',
                        render: (data, row) => {
                            return `<strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    },
                    {
                        key: 'amount',
                        class: 'col-6 col-sm-2 text-right',
                        header: '<span class="d-none d-sm-inline fa fa-balance-scale"></span>',
                        render: (data, row) => {
                            return `<strong>${parseFloat(data.toFixed(2)).toLocaleString()}</strong> <small>NXS</small>`;
                        }
                    }
                ]
            },
            components: {
                txTable: dataTableVue
            },
            methods: {
                reloadData() {
                    this.$refs.txTable.reloadData();
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                },
                selectTransaction(tx) {
                    window.location.href = `/transactions/${tx.transactionHash}`;
                },
                createInOutText(faType, txType) {
                    return `<span class="fa ${faType} inline-icon"></span> <span class="d-sm-none d-md-inline">${txType}</span>`;
                }
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/transactionhub').build();

                this.connection.on('newTxPubSub', (tx) => {
                    if (this.currentFilter !== 'custom') {
                        this.reloadData();
                    }
                });

                this.connection.start();
            }
        });
    }
}
