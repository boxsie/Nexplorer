import Vue from 'vue';
import Moment from 'moment';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import dataTableVue from '../Library/dataTableVue';

import '../../Style/blocks.index.scss';

export class BlockViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                txTableAjaxUrl: '/blocks/getblocks',
                filterCriteria: {
                    txType: null,
                    minAmount: null,
                    maxAmount: null,
                    heightFrom: null,
                    heightTo: null,
                    utcFrom: null,
                    utcTo: null,
                    orderBy: 0
                },
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
                        isCustom: true
                    }
                ],
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
                        key: 'height',
                        class: 'col-3 col-sm-2 text-right text-sm-left',
                        header: '<span class="d-none d-sm-inline fa fa-cube"></span>',
                        render: (data, row) => {
                            return `<span class="d-sm-none inline-icon fa fa-cube"></span>
                                    <a href="/blocks/${data}"><span class="d-none d-sm-inline">#</span>${data}</a>`;
                        }
                    },
                    {
                        key: 'hash',
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
                        key: 'channel',
                        class: 'text-center',
                        header: '<span class="fa fa-exchange"></span>',
                        render: (data, type, row) => {
                            switch (data) {
                            case 0:
                                return `<span class="fa fa-bolt tx-type-icon"></span>`;
                            case 1:
                                return `<span class="fa fa-microchip tx-type-icon"></span>`;
                            case 2:
                                return `<span class="fa fa-hashtag tx-type-icon"></span>`;
                            }
                        }
                    },
                    {
                        key: 'difficulty',
                        class: 'd-none d-sm-table-cell',
                        header: '<span class="fa fa-tachometer"></span>',
                        render: (data, type, row) => {
                            return `<span>${data.toLocaleString()}</span>`;
                        }
                    },
                    {
                        key: 'size',
                        class: 'd-none d-sm-table-cell',
                        header: '<span class="fa fa-hdd-o"></span>',
                        render: (data, type, row) => {
                            return `<span>${this.vm.$layoutHub.parseBytes(data)}</span>`;
                        }
                    },
                    {
                        key: 'transactionCount',
                        class: 'in-out-col',
                        header: '<span class="fa fa-compress"></span>',
                        render: (data, type, row) => {
                            return `<strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    }
                ],
                currentFilter: 'latest',
                blockTableAjaxUrl: '/blocks/getblocks'
            },
            components: {
                txTable: dataTableVue
            },
            methods: {
                reloadData() {
                    this.$refs.blockTable.dataReload();
                },
                selectBlock(block) {
                    window.location.href = `/blocks/${block.height}`;
                },
                filterUpdate(filter) {
                    if (filter) {
                        this.currentFilter = filter;
                    } else {
                        this.currentFilter = 'latest';
                    }
                },
                filterCriteriaUpdate(filterCriteria) {
                    this.filterCriteria = filterCriteria;
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                },
                changeFilter(newFilter) {
                    this.currentFilter = newFilter;
                    this.filterCriteria = JSON.parse(JSON.stringify(defaultCriteria));

                    if (this.currentFilter !== 'custom') {
                        this.reloadData();
                    }
                }
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/blockhub').build();

                this.connection.on('newBlockPubSub', (block) => {
                    this.$refs.blockTable.dataReload();
                });

                this.connection.start();
            }
        });
    }
}
