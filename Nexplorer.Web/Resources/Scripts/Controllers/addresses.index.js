import $ from 'jquery';
import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import moment from 'moment';

import swiperVue from '../Library/swiperVue';
import swiperVueSlide from '../Library/swiperVueSlide';
import doughnutChart from '../Library/doughnutChartVue';
import dataTableVue from '../Library/dataTableVue';

import '../../Style/addresses.index.scss';

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
                dtOptions: {
                    ajaxUrl: '/addresses/getaddresses',
                    showIndex: true,
                    defaultCriteria: {
                        minBalance: null,
                        maxBalance: null,
                        heightFrom: null,
                        heightTo: null,
                        isStaking: false,
                        isNexus: false,
                        orderBy: 0
                    },
                    filters: [
                        {
                            name: 'All',
                            criteria: {}
                        },
                        {
                            name: 'Recent',
                            criteria: { orderBy: '2' }
                        },
                        {
                            name: 'Staking',
                            criteria: { isStaking: true, orderBy: '4' }
                        },
                        {
                            name: 'Nexus',
                            criteria: { isNexus: true }
                        },
                        {
                            name: 'Custom',
                            isUserFilter: true
                        }
                    ]
                },
                columns: [
                    {
                        key: 'hash',
                        class: 'col-12 col-md-6 col-lg-7',
                        header: '<span class="d-none d-md-inline fa fa-hashtag"></span>',
                        render: (data, row) => {
                            return `<span class="d-md-none inline-icon fa fa-hashtag"></span>
                                    <a class="d-none d-sm-inline d-md-none d-lg-inline" href="/addresses/${data}">${data}</a>
                                    <a class="d-none d-md-inline d-lg-none" href="/addresses/${data}">${this.vm.truncateHash(data, 35)}</a>
                                    <a class="d-sm-none" href="/addresses/${data}">${this.vm.truncateHash(data, 32)}</a>`;
                        }
                    },
                    {
                        key: 'lastBlockSeen',
                        class: 'col col-md-2',
                        header: '<span class="d-none d-md-inline fa fa-cube"></span>',
                        render: (data, row) => {
                            return `<span class="d-md-none inline-icon fa fa-cube"></span>
                                    <a href="/blocks/${data}">${data}</a>`;
                        }
                    },
                    {
                        key: 'interestRate',
                        class: 'col-3 col-md-1',
                        header: '<span class="d-none d-md-inline fa fa-bolt"></span>',
                        render: (data, row) => {
                            return data ? `<span class="d-md-none inline-icon fa fa-bolt"></span> ${data.toFixed(3).toLocaleString()}%` : '';
                        }
                    },
                    {
                        key: 'balance',
                        class: 'text-right',
                        header: '<span class="d-none d-md-inline fa fa-balance-scale"></span>',
                        render: (data, row) => {
                            return `<strong>${parseFloat(data.toFixed(2)).toLocaleString()}</strong> <small>NXS</small>`;
                        }
                    }
                ],
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
                AddressDistributionChart: doughnutChart,
                AddressTable: dataTableVue
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
                    this.totalStakedCoins = `${parseInt(statDto.totalStakedCoins).toLocaleString()} NXS`;
                    this.stakeablePercentage = `${((statDto.balanceOverOneThousand / statDto.addressCount) * 100).toFixed(3).toLocaleString()} %`;
                    this.percentageDormant = `${((statDto.dormantOverOneYear / statDto.addressCount) * 100).toFixed(3).toLocaleString()} %`;
                    this.zeroBalance = `${((statDto.zeroBalance / statDto.addressCount) * 100).toFixed(3).toLocaleString()} %`;
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                },
                selectAddress(address) {
                    window.location.href = `/transactions/${address.hash}`;
                },
            },
            created() {
                const self = this;

                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/addresshub').build();

                this.connection.on('statPublish', (statsJson) => this.updateStats(statsJson));

                this.connection.start().then(() => {
                    this.connection.invoke('getLatestAddressStats').then((statsJson) => this.updateStats(statsJson));
                });
            }
        });
    }
}
