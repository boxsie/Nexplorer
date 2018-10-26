import $ from 'jquery';
import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import moment from 'moment';
import dt from 'datatables.net-bs4';

import swiperVue from '../Library/swiperVue.js';
import swiperVueSlide from '../Library/swiperVueSlide.js';
import doughnutChart from '../Library/doughnutChartVue.js';

import '../../Style/addresses.index.scss';
import 'datatables.net-bs4/css/dataTables.bootstrap4.css'; 

export class AddressIndexViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                totalAddresses: ' - ',
                averageBalance: ' - ',
                newAddressRate: ' - ',
                amountStaking: ' - ',
                stakeablePercentage: ' - ',
                totalStakedCoins: ' - ',
                percentageDormant: ' - ',
                zeroBalance: ' - ',
                currentFilter: 'all',
                dataTable: null,
                showDataTable: true,
                filterCriteria: {
                    minBalance: null,
                    maxBalance: null,
                    heightFrom: null,
                    heightTo: null,
                    isStaking: false,
                    isNexus: false,
                    orderBy: 0
                },
                nxsDistributionChartData: {
                    labels: options.distributionBandData.map(function (x) {
                        return options.distributionBands[x.distributionBand];
                    }),
                    datasets: [
                        {
                            backgroundColor: ["#EC9787", "#00A591", "#6B5B95", "#DBB1CD", "#944743", "#6F9FD8", "#BFD641"],
                            fillOpacity: .3,
                            data: options.distributionBandData.map(function (x) {
                                return x.coinPercent;
                            })
                        }
                    ]
                },
                addressDistributionChartData: {
                    labels: options.distributionBandData.map(function (x) {
                        return options.distributionBands[x.distributionBand];
                    }),
                    datasets: [
                        {
                            backgroundColor: ["#EC9787", "#00A591", "#6B5B95", "#DBB1CD", "#944743", "#6F9FD8", "#BFD641"],
                            fillOpacity: .3,
                            data: options.distributionBandData.map(function (x) {
                                return x.addressPercent;
                            })
                        }
                    ]
                },
                distributionChartOptions: {
                    maintainAspectRatio: false,
                    scales: {
                        xAxes: [{ gridLines: { display: false, drawBorder: false }, ticks: { display: false } } ],
                        yAxes: [{ gridLines: { display: false, drawBorder: false }, ticks: { display: false } } ]
                    },
                    legend: {
                        position: 'left',
                        labels: {
                            boxWidth: 20
                        }
                    },
                    tooltips: {
                        callbacks: {
                            label: function (tooltipItem, data) {
                                const val = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
                                return `${options.distributionBands[tooltipItem.index]} NXS: ${val.toFixed(3).toLocaleString()}%`;
                            }
                        }
                    }
                }
            },
            computed: {
                timestampUtc() {
                    return this.$layoutHub.utcMoment.format("DD/MM HH:mm:ss");
                }
            },
            components: {
                Swiper: swiperVue,
                SwiperSlide: swiperVueSlide,
                NxsDistributionChart: doughnutChart,
                AddressDistributionChart: doughnutChart
            },
            methods: {
                updateStats(statDtoJson) {
                    if (!statDtoJson) {
                        return;
                    }
                    const statDto = JSON.parse(statDtoJson);

                    this.totalAddresses = statDto.addressCount.toLocaleString();
                    this.averageBalance = `${statDto.averageBalance.toFixed(3).toLocaleString()} NXS`;
                    this.newAddressRate = `${statDto.createdPerHour.toLocaleString()} p/hour`;
                    this.amountStaking = statDto.stakingCount.toLocaleString();
                    this.totalStakedCoins = `${statDto.totalStakedCoins.toFixed(3).toLocaleString()} NXS`;
                    this.stakeablePercentage = `${((statDto.balanceOverOneThousand / statDto.addressCount) * 100).toFixed(3).toLocaleString()} %`;
                    this.percentageDormant = `${((statDto.dormantOverOneYear / statDto.addressCount) * 100).toFixed(3).toLocaleString()} %`;
                    this.zeroBalance = `${((statDto.zeroBalance / statDto.addressCount) * 100).toFixed(3).toLocaleString()} %`;
                },
                changeFilter(newFilter) {
                    this.currentFilter = newFilter;

                    if (this.currentFilter === 'custom') {
                        this.showDataTable = false;
                    } else {
                        this.dataTable.ajax.reload();
                        this.showDataTable = true;
                    }
                },
                filterCustom() {
                    this.dataTable.ajax.reload();
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                }
            },
            created() {
                var self = this;

                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/addresshub').build();

                this.connection.on('statPublish', (statsJson) => this.updateStats(statsJson));

                this.connection.start().then(() => {
                    this.connection.invoke('getLatestAddressStats').then((statsJson) => this.updateStats(statsJson));
                });

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
                            url: 'addresses/getaddresses',
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
                                            <a class="visible-sm" href="/addresses/${data}">${self.truncateHash(data, 32)}</a>
                                            <a class="visible-xs" href="/addresses/${data}">${self.truncateHash(data, 8)}</a>`;
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
                                    return `<strong>${parseFloat(data.toFixed(2)).toLocaleString()}</strong> <small>NXS</small>`;
                                }
                            }
                        ]
                    });
                });
            }
        });
    }
}
