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
                dtOptions: {
                    ajaxUrl: '/blocks/getblocks',
                    defaultCriteria: {
                        channel: 'All',
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
                            name: 'POS',
                            criteria: { channel: '0' }
                        },
                        {
                            name: 'Prime',
                            criteria: { channel: '1' }
                        },
                        {
                            name: 'Hash',
                            criteria: { channel: '2' }
                        },
                        {
                            name: 'Custom',
                            isUserFilter: true
                        }
                    ]
                },
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
                        class: 'col-6 col-sm-2 text-right text-sm-left',
                        header: '<span class="d-none d-sm-inline fa fa-calendar-o"></span>',
                        render: (data, row) => {
                            var timestamp = Moment(data).format('DD/MMM/YY HH:mm:ss');
                            return `<span>${timestamp}</span>`;
                        }
                    },
                    {
                        key: 'hash',
                        class: 'col-8 col-sm-2 col-lg-5',
                        header: '<span class="d-none d-sm-inline fa fa-hashtag"></span>',
                        render: (data, row) => {
                            return `<span class="d-sm-none inline-icon fa fa-hashtag"></span>
                                    <a class="d-none d-lg-block" href="/blocks/${row.height}">${this.vm.truncateHash(data, 42)}</a>
                                    <a class="d-none d-md-block d-lg-none" href="/blocks/${row.height}">${this.vm.truncateHash(data, 10)}</a>
                                    <a class="d-none d-sm-block d-md-none" href="/blocks/${row.height}">${this.vm.truncateHash(data, 8)}</a>
                                    <a class="d-sm-none" href="/blocks/${row.height}">${this.vm.truncateHash(data, 20)}</a>`;
                        }
                    },
                    {
                        key: 'channel',
                        class: 'col-4 col-sm-2 col-lg-1 text-right text-sm-left',
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
                        class: 'col col-lg-1 text-sm-right',
                        header: '<span class="d-none d-sm-inline fa fa-tachometer"></span>',
                        render: (data, type, row) => {
                            return `<span class="d-sm-none fa fa-tachometer inline-icon"></span> <span>${data.toLocaleString()}</span>`;
                        }
                    },
                    {
                        key: 'size',
                        class: 'col col-lg-1 text-sm-right',
                        header: '<span class="d-none d-sm-inline fa fa-hdd-o"></span>',
                        render: (data, type, row) => {
                            return `<span class="d-sm-none fa fa-hdd-o inline-icon"></span> <span>${this.vm.$layoutHub.parseBytes(data)}</span>`;
                        }
                    },
                    {
                        key: 'transactionCount',
                        class: 'col col-lg-1 text-right',
                        header: '<span class="d-none d-sm-inline fa fa-compress"></span>',
                        render: (data, type, row) => {
                            return `<span class="d-sm-none fa fa-compress inline-icon"></span> <strong>${data ? data.toLocaleString() : 0}</strong>`;
                        }
                    }
                ]
            },
            components: {
                blockTable: dataTableVue
            },
            methods: {
                selectBlock(block) {
                    window.location.href = `/blocks/${block.height}`;
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                }
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/blockhub').build();

                this.connection.on('newBlockPubSub', (block) => {                });

                this.connection.start();
            }
        });
    }
}
