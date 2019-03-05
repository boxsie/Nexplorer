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
                        key: 'height',
                        class: 'col-6 col-sm-2 col-lg-1',
                        header: '<span class="d-none d-sm-inline fa fa-cube"></span>',
                        render: (data, row) => {
                            return `<span class="d-sm-none inline-icon fa fa-cube"></span>
                                    <a href="/blocks/${data}">${data}</a>`;
                        }
                    },
                    {
                        key: 'timestamp',
                        class: 'col-6 col-sm-2',
                        header: '<span class="d-none d-sm-inline fa fa-calendar-o"></span>',
                        render: (data, row) => {
                            var timestamp = Moment(data).format('DD/MMM/YY HH:mm:ss');
                            return `<span>${timestamp}</span>`;
                        }
                    },
                    {
                        key: 'hash',
                        class: 'col-12 col-sm-3',
                        header: '<span class="d-none d-sm-inline fa fa-hashtag"></span>',
                        render: (data, row) => {
                            return `<span class="d-sm-none inline-icon fa fa-hashtag"></span>
                                    <a class="d-none d-md-block" href="/transactions/${data}">${this.vm.truncateHash(data, 18)}</a>
                                    <a class="d-none d-sm-block d-md-none" href="/transactions/${data}">${this.vm.truncateHash(data, 14)}</a>
                                    <a class="d-sm-none" href="/transactions/${data}">${this.vm.truncateHash(data, 24)}</a>`;
                        }
                    },
                    {
                        key: 'channel',
                        class: 'col-4 col-sm-2',
                        header: '<span class="d-none d-sm-inline fa fa-exchange"></span>',
                        render: (data, type, row) => {
                            switch (data) {
                                case 0:
                                    return `<span class="fa fa-bolt inline-icon"></span> POS`;
                            case 1:
                                    return `<span class="fa fa-microchip inline-icon"></span> Prime`;
                            case 2:
                                    return `<span class="fa fa-hashtag inline-icon"></span> Hash`;
                            }
                        }
                    },
                    {
                        key: 'difficulty',
                        class: '',
                        header: '<span class="d-none d-sm-inline fa fa-tachometer"></span>',
                        render: (data, type, row) => {
                            return `<span class="d-sm-none fa fa-tachometer inline-icon"></span> <span>${data.toLocaleString()}</span>`;
                        }
                    },
                    {
                        key: 'size',
                        class: '',
                        header: '<span class="d-none d-sm-inline fa fa-hdd-o"></span>',
                        render: (data, type, row) => {
                            return `<span class="d-sm-none fa fa-hdd-o inline-icon"></span> <span>${this.vm.$layoutHub.parseBytes(data)}</span>`;
                        }
                    },
                    {
                        key: 'transactionCount',
                        class: 'in-out-col',
                        header: '<span class="d-none d-sm-inline fa fa-compress"></span>',
                        render: (data, type, row) => {
                            return `<span class="d-sm-none fa fa-compress inline-icon"></span> <strong>${data ? data.toLocaleString() : 0}</strong>`;
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
