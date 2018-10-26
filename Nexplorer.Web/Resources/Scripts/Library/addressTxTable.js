import $ from 'jquery';
import Moment from 'moment';
import BootstrapPager from '../Library/bootstrapPager.js';

export default {
    template: require('../../Markup/address-tx-table.html'),
    props: ['addressHash', 'txType', 'pageCount', 'txPerPage', 'onScreenTxType'],
    data: () => {
        return {
            currentPageTxs: [],
            currentPage: 1
        };
    },
    components: {
        CustomPagerTop: BootstrapPager('custom'),
        CustomPagerBottom: BootstrapPager('custom')
    },
    methods: {
        changePage(newPage) {
            this.currentPageTxs = [];

            $.ajax({
                url: ,
                data: {
                    start: (newPage - 1) * this.txPerPage,
                    count: this.txPerPage,
                    txType: this.txType,
                    addressHash: this.addressHash
                },
                success: (txs) => {
                    this.currentPageTxs = txs;
                    this.currentPage = newPage;
                }
            });
        },
        getDate(date) {
            return Moment(date).format("MMM Do 'YY");
        }
    },
    created() {
        var self = this;

        $(() => {
            self.dataTable = $('#addressTable').DataTable({
                dom: '<"top"f>rt<"bottom"lp><"clear">',
                searching: false,
                ordering: false,
                responsive: true,
                serverSide: true,
                info: true,
                pagingType: "full",
                ajax: {
                    url: '/addresses/getaddresstxs',
                    type: 'POST',
                    data: (data) => {
                        data.filter = self.currentFilter;

                        if (self.currentFilter === 'custom') {
                            data.filterCriteria = self.filterCriteria;
                        }

                        return data;
                    },
                    dataFilter: (data) => {
                        const obj = JSON.parse(data);
                        const pageInfo = self.dataTable.page.info();

                        if (!obj.data) {
                            return data;
                        }

                        if (self.currentFilter === 'custom') {
                            self.showDataTable = true;
                        }

                        for (let i = 0; i < obj.data.length; i++) {
                            obj.data[i].i = (i + 1) + (pageInfo.page * pageInfo.length);
                        }

                        return JSON.stringify(obj);
                    },
                    dataType: 'json'
                },
                order: [[2, 'dsc']],
                columns: [
                    {
                        title: '',
                        data: 'i',
                        render: (data, type, row) => {
                            return `<strong>${data}</strong>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-hashtag"></span>',
                        data: 'hash',
                        render: (data, type, row) => {
                            return `<a class="hidden-xs hidden-sm" href="/addresses/${data}">${data}</a>
                                            <a class="visible-sm" href="/addresses/${data}">${self.truncateHash(data,
                                32)}</a>
                                            <a class="visible-xs" href="/addresses/${data}">${self.truncateHash(data,
                                8)}</a>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-bolt"></span>',
                        data: 'interestRate',
                        render: (data, type, row) => {
                            return data ? `${data.toFixed(3).toLocaleString()}%` : '';
                        }
                    },
                    {
                        title: '<span class="fa fa-cube"></span>',
                        data: 'lastBlockSeen',
                        render: (data, type, row) => {
                            return `<a href="/blocks/${data}">#${data}</a>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-balance-scale"></span>',
                        data: 'balance',
                        render: (data, type, row) => {
                            return `<strong>${parseFloat(data.toFixed(2)).toLocaleString()
                                }</strong> <small>NXS</small>`;
                        }
                    }
                ]
            });
        });
    }
};
