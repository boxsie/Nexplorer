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
                includeRewards: 0
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
                selectTransaction(tx) {
                    window.location.href = `/transactions/${tx.transactionHash}`;
                },
                getRowTxItems(row) {
                    return  row.transactionItems.filter((item) => {
                        return item.transactionInputOutputType !== row.transactionInputOutputType;
                    });
                },
                calculateRowReward(row) {
                    let diff = 0;

                    for (let i = 0; i < row.transactionItems.length; i++) {
                        const ti = row.transactionItems[i];

                        if (row.addressHash !== ti.addressHash) {
                            diff += ti.amount;
                        }
                    }

                    return row.amount - diff;
                },
                checkForCoinstakeTopupTx(row) {
                    const rowItems = this.getRowTxItems(row);
                    return rowItems.length < 2 && rowItems[0].addressHash === row.addressHash;
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
