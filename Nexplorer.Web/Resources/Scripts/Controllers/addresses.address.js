import $ from 'jquery';
import Qrious from 'qrious';
import Moment from 'moment';
import Avatars from '@dicebear/avatars';
import SpriteCollection from '@dicebear/avatars-identicon-sprites';

import dataTableVue from '../Library/dataTableVue';
import collapsableListVue from '../Library/collapsableListVue';
import currencyHelper from '../Library/currencyHelper';
import activityChart from '../Library/addressActivityChart';

import '../../Style/addresses.address.scss';

export class AddressViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                addressHash: options.addressHash,
                nxsBalance: options.nxsBalance.toLocaleString(),
                usdValue: options.usdValue.toLocaleString(),
                totalTxCount: options.totalTxCount.toLocaleString(),
                isAddressWatched: options.isAddressWatched,
                addressOnShow: options.addressAlias,
                lastStakeBlockTimestamp: '',
                lastBlockSeenTimestamp: Moment(options.lastBlockSeenTimestamp).format("MMM Do YY"),
                userCurrencyIndex: 'USD',
                addressOnShowLink: 'Show address',
                identiconSvg: '',
                waitingForFavouriteResponse: false,
                dtOptions: {
                    ajaxUrl: '/addresses/getaddresstxs',
                    showUserFilter: true,
                    filterClass: 'col-sm-6 no-bg p-0',
                    defaultCriteria: {
                        txType: "All",
                        txInputOutputType: null,
                        minAmount: null,
                        maxAmount: null,
                        heightFrom: null,
                        heightTo: null,
                        utcFrom: null,
                        utcTo: null,
                        addressHashes: [options.addressHash],
                        grouped: true,
                        orderBy: 0
                    },
                    filters: [
                        {
                            name: 'Sent / Received',
                            criteria: { txInputOutputType: null }
                        },
                        {
                            name: 'Sent',
                            criteria: { txInputOutputType: '0' }
                        },
                        {
                            name: 'Received',
                            criteria: { txInputOutputType: '1' }
                        }
                    ],
                    customFilterMatch: (queryObject) => {
                        if (queryObject.txInputOutputType === null || queryObject.txInputOutputType === undefined) {
                            return 0;
                        } else if (queryObject.txInputOutputType === '0') {
                            return 1;
                        } else if (queryObject.txInputOutputType === '1') {
                            return 2;
                        }

                        return -1;
                    }
                },
                includeRewards: 0,
                columns: [
                    {
                        key: 'timestamp',
                        header: '<span class="d-none d-lg-inline fa fa-calendar-o"></span>',
                        class: 'col-5 col-lg-1 order-1',
                        render: (data, row) => {
                            var timestamp = Moment(data).format('DD/MMM/YY');
                            return `<span>${timestamp}</span>`;
                        }
                    },
                    {
                        key: 'blockHeight',
                        header: '<span class="d-none d-lg-inline fa fa-cube"></span>',
                        class: 'col-4 col-lg-1 order-2',
                        render: (data, row) => {
                            return `<span class="d-lg-none inline-icon fa fa-cube"></span>
                                    <a href="/blocks/${data}">${data}</a>`;
                        }
                    },
                    {
                        key: 'oppositeItems',
                        header: '<span class="d-none d-lg-inline fa fa-hashtag"></span>',
                        class: 'col-12 col-lg-7 order-12 order-lg-3 address-hashes',
                        render: (data, row, i) => {
                            
                        }
                    },
                    {
                        key: 'transactionInputOutputType',
                        header: '<span class="d-none d-lg-inline fa fa-exchange"></span>',
                        class: 'col-3 col-sm-1 text-right text-center-sm order-4',
                        render: (data, row) => {
                            var icon = '';
                            
                            if (options.txTypes[row.transactionType] === 'Coinstake') {
                                icon = 'fa-bolt stake';
                            } else if (options.txTypes[row.transactionType] === 'CoinbasePrime' || options.txTypes[row.transactionType] === 'CoinbaseHash') {
                                icon = 'fa-cube mining';
                            } else {
                                if (data === 0) {
                                    icon = 'fa-arrow-left red';
                                } else {
                                    icon = 'fa-arrow-right green';
                                }
                            }

                            var txCount = !row.isStakingReward && !row.isMiningReward && data.length > 1 ? `<span>(${data.length})</span>` : ' ';
                            return `<span class="fa ${icon} inline-icon"></span> <span style="font-size: 12px;">${txCount}</span>`;
                        }
                    },
                    {
                        key: 'amount',
                        header: '<span class="d-none d-lg-inline fa fa-paper-plane-o"></span>',
                        class: 'text-right order-12 order-sm-5',
                        render: (data, row) => {
                            var balanceTotal = parseFloat(data.toFixed(4)).toLocaleString();
                            var balanceText = "";

                            switch (row.transactionInputOutputType) {
                                case 0:
                                    balanceText = `-${balanceTotal}`;
                                    break;
                                case 1:
                                    balanceText = `+${balanceTotal}`;
                                    break;
                            }

                            var amounts = `<strong>${balanceText}</strong> <small>NXS</small>`;

                            return amounts;
                        }
                    }
                ]
            },
            components: {
                txTable: dataTableVue,
                collapsableList: collapsableListVue,
                CurrencyHelper: currencyHelper,
                ActivityChart: activityChart
            },
            methods: {
                getHistory(txIoType) {
                    this.filterCriteria.txInputOutputType = txIoType;
                    this.reloadData();
                },
                watchAddress() {
                    if (!this.waitingForFavouriteResponse) {
                        this.waitingForFavouriteResponse = true;

                        $.ajax({
                            url: this.isAddressWatched ? '/favourites/removeaddressfavourite' : '/favourites/createaddressfavourite',
                            type: 'POST',
                            data: {
                                addressId: options.addressId
                            },
                            success: (watchId) => {
                                this.isAddressWatched = !this.isAddressWatched;
                                this.waitingForFavouriteResponse = false;
                            },
                            failure: (err) => {
                                console.log(err);
                                this.waitingForFavouriteResponse = false;
                            }
                        });
                    }
                },
                flipAddress() {
                    if (this.addressOnShow === options.addressAlias) {
                        this.addressOnShow = options.addressHash;
                        this.addressOnShowLink = 'Show alias';
                    } else {
                        this.addressOnShow = options.addressAlias;
                        this.addressOnShowLink = 'Show address';
                    }
                },
                reloadData() {
                    setTimeout(() => {
                        this.$refs.txTable.dataReload();
                    }, 1);
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                },
                formatTimestamp(timestamp) {
                    return Moment(timestamp).format('DD/MMM/YY');
                },
                //getHashText(row, i) {
                //    const data = row.oppositeItems;

                //    if (options.txTypes[row.transactionType] === 'CoinbaseHash') {
                //        return `<span>Coinbase hash reward</span>`;
                //    } if (options.txTypes[row.transactionType] === 'CoinbasePrime') {
                //        return `<span>Coinbase prime reward</span>`;
                //    } else if (options.txTypes[row.transactionType] === 'Coinstake') {
                //        const rewardOrTx = this.dtOptions.defaultCriteria.txInputOutputType === null ? 'reward' : 'transaction';
                //        return `<span>Coinstake ${rewardOrTx}</span>`;
                //    } else if (data.length > 0) {
                //        const id = `addTx${i}`;
                //        const collapse = data.length > 2;

                //        let txAddressHashes = `<table style="width: 100%;"><tr><td><span class="d-lg-none inline-icon fa fa-hashtag"></span></td><td><div class="ml-1 ml-lg-0 read-more">`;

                //        txAddressHashes += collapse ? `<transition name="slide"><ul class="list collapse" id="${id}">` : `<ul class="list">`;
                //        console.log(data);
                //        for (let o = 0; o < data.length; o++) {
                //            const hash = data[o].addressHash;
                //            txAddressHashes += `<li class="d-block d-sm-flex" style="width: 100%; justify-content: space-between;"><a class="d-none d-sm-inline-block" href="/addresses/${hash}">${hash}</a>
                //                                    <a class="d-block d-sm-none" href="/addresses/${hash}">${this.truncateHash(hash, 30)}</a> <span class="text-right" style="font-size: 0.8rem;">(${data[o].amount.toLocaleString()} <span style="font-size: 0.6rem;">NXS</span>)</span></li>`;
                //        }

                //        txAddressHashes += '</ul></transition>';

                //        if (collapse) {
                //            txAddressHashes += `<a class="no-row-link" data-toggle="collapse" href="#${id}" aria-expanded="false" aria-controls="${id}" v-on:click.stop.prevent="showMoreHashes">${data.length - 2}</a>`;
                //        }

                //        txAddressHashes += '</div></td></tr></table>';

                //        return txAddressHashes;
                //    }

                //    return 'Unknown';
                //},
                showMoreHashes() {
                    console.log("moop");
                },
                selectTransaction(tx) {
                    window.location.href = `/transactions/${tx.transactionHash}`;
                }
            },
            mounted() {
                const avatars = new Avatars(SpriteCollection);
                const svg = avatars.create(options.addressHash);

                this.identiconSvg = svg;
            }
        });

        this.qr = new Qrious({
            element: document.getElementById('qrCode'),
            value: options.addressHash,
            level: 'H',
            size: 162
        });
    }
}
