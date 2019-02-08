import Vue from 'vue';
import Moment from 'moment';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import dataTableVue from '../Library/dataTableVue';

import '../../Style/blocks.index.scss';

export class BlockViewModel {
    constructor(options) {
        const defaultCriteria = {
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
                blockTableAjaxUrl: '/blocks/getblocks',
                blockTableColumns: [
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
                        data: 'height',
                        render: (data, type, row) => {
                            return `<a href="/blocks/${data}">#${data}</a>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-hashtag"></span>',
                        data: 'hash',
                        render: (data, type, row) => {
                            return `<a class="d-none d-md-block" href="/blocks/${data}">${this.vm.truncateHash(data, 15)}</a>
                                            <a class="d-none d-sm-block d-md-none" href="/blocks/${data}">${this.vm.truncateHash(data, 10)}</a>
                                            <a class="d-sm-none" href="/blocks/${data}">${this.vm.truncateHash(data, 4)}</a>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-exchange"></span>',
                        data: 'channel',
                        class: 'in-out-col',
                        render: (data, type, row) => {
                            switch (data) {
                            case 0:
                                return `<span class="fa fa-hashtag tx-type-icon"></span>`;
                            case 1:
                                return `<span class="fa fa-microchip tx-type-icon"></span>`;
                            case 2:
                                return `<span class="fa fa-bolt tx-type-icon"></span>`;
                            }
                        }
                    },
                    {
                        title: '<span class="fa fa-tachometer"></span>',
                        data: 'difficulty',
                        class: 'd-none d-sm-table-cell',
                        render: (data, type, row) => {
                            return `<span>${data.toLocaleString()}</span>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-hdd-o"></span>',
                        data: 'size',
                        class: 'd-none d-sm-table-cell',
                        render: (data, type, row) => {
                            return `<span>${this.vm.parseBytes(data)}</span>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-compress"></span>',
                        data: 'transactionCount',
                        class: 'in-out-col',
                        render: (data, type, row) => {
                            return `<strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    }
                ]
            },
            components: {
                blockTable: dataTableVue('first_last_numbers')
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
                },
                parseBytes(bytes) {
                    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
                    if (bytes === 0) return '0 Byte';
                    const i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
                    return Math.round(bytes / Math.pow(1024, i), 2) + ' ' + sizes[i];
                }
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/blockhub').build();

                this.connection.on('newBlockPubSub', (block) => {
                    this.$refs.blockTable.refreshPage();
                });

                this.connection.start();
            }
        });
    }
}
