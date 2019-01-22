import Vue from 'vue';
import Moment from 'moment';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import dataTableVue from '../Library/dataTableVue';

import '../../Style/transaction.index.scss';

export class TransactionViewModel {
    constructor(options) {
        var defaultCriteria = {
            txType: 'All',
            minAmount: null,
            maxAmount: null,
            heightFrom: null,
            heightTo: null,
            utcFrom: null,
            utcTo: null,
            orderBy: 0
        };

        this.vm = new Vue({
            el: '#main',
            data: {
                currentFilter: 'latest',
                filterCriteria: defaultCriteria,
                txTableAjaxUrl: '/transactions/gettransactions',
                txTableColumns: [
                    {
                        title: '<span class="fa fa-calendar-o"></span>',
                        data: 'timestamp',
                        width: '16%',
                        render: (data, type, row) => {
                            var timestamp = Moment(data).format('DD/MMM/YY HH:mm:ss');
                            return `<span>${timestamp}</span>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-cube"></span>',
                        data: 'blockHeight',
                        render: (data, type, row) => {
                            return `<a href="/blocks/${data}">#${data}</a>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-hashtag"></span>',
                        data: 'transactionHash',
                        render: (data, type, row) => {
                            return `<a class="d-none d-md-block" href="/transactions/${data}">${this.vm.truncateHash(data, 15)}</a>
                                            <a class="d-none d-sm-block d-md-none" href="/transactions/${data}">${this.vm.truncateHash(data, 10)}</a>
                                            <a class="d-sm-none" href="/transactions/${data}">${this.vm.truncateHash(data, 4)}</a>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-exchange"></span>',
                        data: 'transactionType',
                        class: 'in-out-col',
                        render: (data, type, row) => {
                            switch (data) {
                            case 0:
                                return `<span class="fa fa-hashtag tx-type-icon"></span>`;
                            case 1:
                                return `<span class="fa fa-microchip tx-type-icon"></span>`;
                            case 2:
                            case 3:
                                return `<span class="fa fa-bolt tx-type-icon"></span>`;
                            case 4:
                                return `<span class="fa fa-user tx-type-icon"></span>`;
                            }
                        }
                    },
                    {
                        title: '',
                        data: 'transactionType',
                        class: 'd-none d-sm-table-cell',
                        render: (data, type, row) => {
                            switch (data) {
                            case 0:
                                return `Coinbase hash reward`;
                            case 1:
                                return `Coinbase prime reward`;
                            case 2:
                                return `Coinstake reward`;
                            case 3:
                                return `Coinstake genesis`;
                            case 4:
                                return `User transaction`;
                            }
                        }
                    },
                    {
                        title: '<span class="fa fa-compress"></span>',
                        data: 'transactionInputCount',
                        class: 'in-out-col',
                        render: (data, type, row) => {
                            return `<strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-expand"></span>',
                        data: 'transactionOutputCount',
                        class: 'in-out-col',
                        render: (data, type, row) => {
                            return `<strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-balance-scale"></span>',
                        data: 'amount',
                        class: 'amount-col',
                        width: '15%',
                        render: (data, type, row) => {
                            return `<strong>${parseFloat(data.toFixed(2)).toLocaleString()}</strong> <small>NXS</small>`;
                        }
                    }
                ],
            },
            components: {
                AddressTable: dataTableVue(defaultCriteria, 'first_last_numbers')
            },
            methods: {
                reloadData() {
                    const reloadCriteria = JSON.parse(JSON.stringify(this.filterCriteria));

                    if (reloadCriteria.txType === "All")
                        reloadCriteria.txType = null;

                    this.$refs.txTable.dataReload(reloadCriteria, this.currentFilter);
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                },
                changeFilter(newFilter) {
                    this.currentFilter = newFilter;

                    if (this.currentFilter !== 'custom') {
                        this.reloadData();
                    }
                },
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/transactionhub').build();

                this.connection.on('newTxPubSub', (tx) => {
                    this.$refs.txTable.refreshPage();
                });

                this.connection.start();
            }
        });
    }
}
