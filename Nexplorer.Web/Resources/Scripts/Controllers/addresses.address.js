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
        const defaultFilter = 'both';

        const defaultCriteria = {
            minAmount: null,
            maxAmount: null,
            heightFrom: null,
            heightTo: null,
            utcFrom: null,
            utcTo: null,
            isStakeReward: null,
            isMiningReward: null,
            addressHashes: [options.addressHash],
            orderBy: 0
        };

        this.vm = new Vue({
            el: '#main',
            data: {
                addressHash: options.addressHash,
                nxsBalance: options.nxsBalance.toLocaleString(),
                usdValue: options.usdValue.toLocaleString(),
                totalTxCount: options.totalTxCount.toLocaleString(),
                isAddressWatched: options.isAddressWatched,
                addressOnShow: options.addressAlias,
                lastBlockSeenTimestamp: Moment(options.lastBlockSeenTimestamp).format("MMM Do YY"),
                userCurrencyIndex: 'USD',
                addressOnShowLink: 'Show address',
                identiconSvg: '',
                waitingForFavouriteResponse: false,
                currentFilter: defaultFilter,
                filterCriteria: defaultCriteria,
                includeRewards: 0,
                transactionTableAjaxUrl: '/addresses/getaddresstxs',
                transactionTableColumns: [
                    {
                        title: '<span class="fa fa-calendar-o"></span>',
                        data: 'timestamp',
                        width: '5%',
                        render: (data, type, row) => {
                            var timestamp = Moment(data).format('DD/MMM/YY');
                            return `<span>${timestamp}</span>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-cube"></span>',
                        className: 'block-col hidden-xs',
                        data: 'blockHeight',
                        width: '7%',
                        render: (data, type, row) => {
                            return `<a href="/blocks/${data}">#${data}</a>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-hashtag"></span>',
                        className: '',
                        data: 'oppositeAddresses',
                        width: '55%',
                        render: (data, type, row) => {
                            if (options.txTypes[row.transactionType] === 'CoinbaseHash') {
                                return `<span>Coinbase hash reward</span>`;
                            } if (options.txTypes[row.transactionType] === 'CoinbasePrime') {
                                return `<span>Coinbase prime reward</span>`;
                            } else if (options.txTypes[row.transactionType] === 'Coinstake') {
                                const rewardOrTx = this.vm.currentFilter === 'both' ? 'reward' : 'transaction';
                                return `<span>Coinstake ${rewardOrTx}</span>`;
                            } else if (data.length > 0) {
                                const id = `addTx${row.i}`;
                                const collapse = data.length > 2;

                                let txAddressHashes = `<div class="read-more">`;

                                txAddressHashes += collapse ? `<ul class="list collapse" id="${id}">` : `<ul class="list">`;

                                for (let i = 0; i < data.length; i++) {
                                    const hash = data[i].addressHash;
                                    txAddressHashes += `<li><a class="hidden-xs hidden-sm" href="/addresses/${hash}">${hash}</a>
                                                    <a class="visible-sm" href="/addresses/${hash}">${this.vm.truncateHash(hash, 32)}</a>
                                                    <a class="visible-xs" href="/addresses/${hash}">${this.vm.truncateHash(hash, 8)}</a></li>`;
                                }

                                txAddressHashes += '</ul>';

                                if (collapse) {
                                    txAddressHashes += `<a class="collapsed expand-link no-row-link" data-toggle="collapse" href="#${id}" aria-expanded="false" aria-controls="${id}">${data.length - 2}</a>`;
                                }

                                txAddressHashes += '</div>';

                                return txAddressHashes;
                            }

                            return 'Unknown';
                        }
                    },
                    {
                        title: '<span class="fa fa-exchange"></span>',
                        className: 'in-out-col',
                        data: 'inputOutputs',
                        width: '5%',
                        render: (data, type, row) => {
                            var dataFirst = data[0];
                            var icon = '';
                            
                            if (options.txTypes[row.transactionType] === 'Coinstake') {
                                icon = 'fa-bolt stake';
                            } else if (options.txTypes[row.transactionType] === 'CoinbasePrime' || options.txTypes[row.transactionType] === 'CoinbaseHash') {
                                icon = 'fa-cube mining';
                            } else {
                                if (dataFirst.transactionIoType === 0) {
                                    icon = 'fa-arrow-left red';
                                } else {
                                    icon = 'fa-arrow-right green';
                                }
                            }

                            var txCount = !row.isStakingReward && !row.isMiningReward && data.length > 1 ? `<span>(${data.length})</span>` : ' ';
                            return `<span class="fa ${icon} tx-type-icon"></span> ${txCount}`;
                        }
                    },
                    {
                        title: '<span class="fa fa-paper-plane-o"></span>',
                        className: 'balance-col',
                        data: 'inputOutputs',
                        width: '28%',
                        render: (data, type, row) => {
                            var amounts = '<ul class="list">';
                            var dFirst = data[0];

                            if (options.txTypes[row.transactionType] === 'Coinstake') {
                                const stakeAmount = data[1] ? parseFloat(data[1].amount) - parseFloat(dFirst.amount) : parseFloat(dFirst.amount);
                                const addOrSub = this.vm.currentFilter === 'input' ? '-' : '+';
                                amounts += `<li><strong>${addOrSub}${stakeAmount.toLocaleString()}</strong> <small>NXS</small></li>`;
                            } else if (options.txTypes[row.transactionType] === 'CoinbasePrime' || options.txTypes[row.transactionType] === 'CoinbaseHash') {
                                amounts += `<li><strong>${parseFloat(dFirst.amount.toFixed(4)).toLocaleString()}</strong> <small>NXS</small></li>`;
                            } else {
                                const sub = dFirst.transactionIoType === 0 ? '-' : '+';
                                amounts += `<li><strong>${sub}${parseFloat(data.reduce((a, b) => +a + +b.amount.toFixed(4), 0)).toLocaleString()}</strong> <small>NXS</small></li>`;
                            }

                            amounts += '</ul>';

                            return amounts;
                        }
                    }
                ]
            },
            components: {
                TransactionTable: dataTableVue(defaultFilter, defaultCriteria, 'first_last_numbers'),
                CurrencyHelper: currencyHelper,
                ActivityChart: activityChart
            },
            methods: {
                getHistory(txType) {
                    this.currentFilter = txType;
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
                    switch (this.includeRewards) {
                        case '0':
                            this.filterCriteria.isStakeReward = null;
                            this.filterCriteria.isMiningReward = null;
                            break;
                        case '1':
                            this.filterCriteria.isStakeReward = true;
                            this.filterCriteria.isMiningReward = true;
                            break;
                        case '2':
                            this.filterCriteria.isStakeReward = false;
                            this.filterCriteria.isMiningReward = false;
                            break;
                    }

                    this.$refs.txTable.dataReload(this.currentFilter, this.filterCriteria);
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                },
                selectTransaction(tx) {
                    window.location.href = `/transactions/${tx.hash}`;
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
