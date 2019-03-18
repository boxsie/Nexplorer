import $ from 'jquery';
import Qrious from 'qrious';
import Moment from 'moment';
import Avatars from '@dicebear/avatars';
import SpriteCollection from '@dicebear/avatars-identicon-sprites';

import dataTableVue from '../Library/dataTableVue';
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
                    alwaysCustom: true,
                    filterClass: 'col-sm-6 no-bg p-0'
                },
                filterCriteria: {
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
                        criteria: {}
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
                ignoreKeys: ['addressHashes'],
                includeRewards: 0,
                columns: [
                    {
                        key: 'timestamp',
                        header: '<span class="d-none d-lg-inline fa fa-calendar-o"></span>',
                        class: 'col-3 col-lg-1 order-1',
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
                            if (options.txTypes[row.transactionType] === 'CoinbaseHash') {
                                return `<span>Coinbase hash reward</span>`;
                            } if (options.txTypes[row.transactionType] === 'CoinbasePrime') {
                                return `<span>Coinbase prime reward</span>`;
                            } else if (options.txTypes[row.transactionType] === 'Coinstake') {
                                const rewardOrTx = this.vm.filterCriteria.txInputOutputType === null ? 'reward' : 'transaction';
                                return `<span>Coinstake ${rewardOrTx}</span>`;
                            } else if (data.length > 0) {
                                const id = `addTx${i}`;
                                const collapse = data.length > 2;

                                let txAddressHashes = `<table><tr><td><span class="d-lg-none inline-icon fa fa-hashtag"></span></td><td><div class="ml-1 ml-lg-0 read-more">`;

                                txAddressHashes += collapse ? `<ul class="list collapse" id="${id}">` : `<ul class="list">`;

                                for (let i = 0; i < data.length; i++) {
                                    const hash = data[i].addressHash;
                                    txAddressHashes += `<li><a class="d-none d-sm-block" href="/addresses/${hash}">${hash}</a>
                                                    <a class="d-sm-none" href="/addresses/${hash}">${this.vm.truncateHash(hash, 40)}</a></li>`;
                                }

                                txAddressHashes += '</ul>';

                                if (collapse) {
                                    txAddressHashes += `<a class="collapsed expand-link no-row-link" data-toggle="collapse" href="#${id}" aria-expanded="false" aria-controls="${id}">${data.length - 2}</a>`;
                                }

                                txAddressHashes += '</div></td></tr></table>';

                                return txAddressHashes;
                            }

                            return 'Unknown';
                        }
                    },
                    {
                        key: 'transactionInputOutputType',
                        header: '<span class="d-none d-lg-inline fa fa-exchange"></span>',
                        class: 'col-1 text-center order-4',
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
                        class: 'text-right order-5',
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
