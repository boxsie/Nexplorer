import $ from 'jquery';
import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import moment from 'moment';

import swiperVue from '../Library/swiperVue.js';
import swiperVueSlide from '../Library/swiperVueSlide.js';
import blockchainTable from '../Library/blockchainTable.js';

import '../../Images/nxs-icon.png';

import '../../Style/home.index.scss';

export class HomeViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                chainHeight: options.latestBlock.height.toLocaleString(),
                hashDiff: options.hashDiff.toFixed(6).toLocaleString(),
                primeDiff: options.primeDiff.toFixed(6).toLocaleString(),
                posDiff: options.posDiff.toFixed(6).toLocaleString(),
                blockCountLastDay: 0,
                txCountLastDay: 0,
                bittrexSummary: {}
            },
            computed: {
                timestampUtc() {
                    return this.$layoutHub.utcMoment.format("DD/MM HH:mm:ss");
                }
            },
            components: {
                Swiper: swiperVue,
                SwiperSlide: swiperVueSlide,
                BlockTable: blockchainTable('blocks'),
                TxTable: blockchainTable('transactions')
            },
            mounted() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/homehub').build();

                this.connection.on('newBlockPubSub',
                    (blockData) => {
                        var jsonBlockData = JSON.parse(blockData);
                        this.setSlideBlockData(jsonBlockData);
                        this.$refs.blockTable.addItem(jsonBlockData.block);

                        this.connection.invoke('getBlockCountLastDay')
                                .then((count) => this.blockCountLastDay = count)
                            .then(() => this.connection.invoke('getTransactionCountLastDay'))
                                .then((count) => this.txCountLastDay = count);

                        document.title = `#${jsonBlockData.block.height} | Nexplorer - A Nexus Block Explorer`;
                    });

                this.connection.on('newTxPubSub',
                    (tx) => {
                        this.$refs.txTable.addItem(JSON.parse(tx));
                    });

                this.connection.on('newBittrexSummaryPubSub',
                    (summary) => {
                        this.setLatestBittrexSummary(summary);
                    });
                
                this.connection.start()
                    .then(() => {
                        this.connection.invoke('getLatestBittrexSummary').then((summary) => this.setLatestBittrexSummary(summary));
                        this.connection.invoke('getBlockCountLastDay').then((count) => this.blockCountLastDay = count);
                        this.connection.invoke('getTransactionCountLastDay').then((count) => this.txCountLastDay = count);
                    });
            },
            methods: {
                setSlideBlockData(blockData) {
                    this.chainHeight = blockData.block.height.toLocaleString();

                    this.hashDiff = blockData.difficulty.hash.toFixed(6).toLocaleString();
                    this.posDiff = blockData.difficulty.pos.toFixed(6).toLocaleString();
                    this.primeDiff = blockData.difficulty.prime.toFixed(6).toLocaleString();
                },
                setLatestBittrexSummary(summary) {
                    const obj = JSON.parse(summary);

                    const bittrexSummary = {
                        last: obj.last.toFixed(8),
                        volume: obj.baseVolume.toLocaleString(),
                        bid: obj.bid.toFixed(8),
                        openBuyOrders: obj.openBuyOrders.toLocaleString(),
                        ask: obj.ask.toFixed(8),
                        openSellOrders: obj.openSellOrders.toLocaleString(),
                        timeStamp: moment(obj.timeStamp).format("dddd, MMMM Do YYYY, h:mm:ss a")
                    };

                    this.bittrexSummary = bittrexSummary;

                    const $summaryLast = $("#summaryLast");
                    const $summaryBid = $("#summaryBid");
                    const $summaryAsk = $("#summaryAsk");
                    const $summaryVolume = $("#summaryVolume");

                    if (this.previousSummary) {

                        if (obj.last > this.previousSummary.last) {
                            this.animateBittrexSummaryItem($summaryLast, true);
                        } else if (obj.last < this.previousSummary.last) {
                            this.animateBittrexSummaryItem($summaryLast, false);
                        }

                        if (obj.volume > this.previousSummary.volume) {
                            this.animateBittrexSummaryItem($summaryVolume, true);
                        } else if (obj.volume < this.previousSummary.volume) {
                            this.animateBittrexSummaryItem($summaryVolume, false);
                        }

                        if (obj.bid > this.previousSummary.bid) {
                            this.animateBittrexSummaryItem($summaryBid, true);
                        } else if (obj.bid < this.previousSummary.bid) {
                            this.animateBittrexSummaryItem($summaryBid, false);
                        }

                        if (obj.ask > this.previousSummary.ask) {
                            this.animateBittrexSummaryItem($summaryAsk, true);
                        } else if (obj.ask < this.previousSummary.ask) {
                            this.animateBittrexSummaryItem($summaryAsk, false);
                        }
                    }

                    this.previousSummary = obj;
                },
                animateBittrexSummaryItem($elem, isUp) {
                    $elem.removeClass('summary-up').removeClass('summary-down');
                    $elem.addClass(isUp ? 'summary-up' : 'summary-down');
                },
                formatTimeStamp(timestamp) {
                    return timestamp.format("DD/MM HH:mm:ss");
                }
            }
        });
    }
}
