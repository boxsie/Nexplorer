import $ from 'jquery';
import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import moment from 'moment';
import dateAge from '../Library/dateAge';

import '../../Images/nxs-icon.png';

import '../../Style/home.index.scss';

export class HomeViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                blocks: [],
                txs: []
            },
            computed: {
                timestampUtc() {
                    return this.$layoutHub.utcMoment.format("DD/MM HH:mm:ss");
                },
                lastPrice() {
                    return (this.$layoutHub.latestPrice.last ? this.$layoutHub.latestPrice.last : 0).toFixed(8);
                },
                spread() {
                    return Math.trunc((this.$layoutHub.latestPrice.ask ? this.$layoutHub.latestPrice.ask - this.$layoutHub.latestPrice.bid : 0) * Math.pow(10, 8)).toLocaleString();
                },
                volume() {
                    return (this.$layoutHub.latestPrice.baseVolume ? this.$layoutHub.latestPrice.baseVolume : 0).toFixed(4);
                }
            },
            methods: {
                formatTimeStamp(timestamp) {
                    return timestamp.format("DD/MM HH:mm:ss");
                },
                updateBlocks() {
                    $.ajax({
                        url: 'blocks/getrecentblocks',
                        data: {
                        },
                        success: (result) => {
                            this.blocks = result;
                            this.txs = [];

                            for (let i = 0; i < this.blocks.length; i++) {
                                for (let j = 0; j < this.blocks[i].transactions.length; j++) {
                                    this.txs.push(this.blocks[i].transactions[j]);
                                }
                            }

                            this.txs = this.txs.slice(0, 10);
                            console.log(this.txs);
                        }
                    });
                },
                selectRow(href) {
                    window.location.href = href;
                }
            },
            components: {
                DateAge: dateAge
            },
            mounted() {
                this.updateBlocks();

                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/homehub').build();

                this.connection.on('newBlockPubSub',
                    (blockData) => {
                        var b = JSON.parse(blockData);
                        
                        this.blocks.splice(0, 0, b);
                        this.blocks.pop();

                        document.title = `#${b.height} | Nexplorer - A Nexus Block Explorer`;
                    });

                this.connection.on('newTxPubSub',
                    (tx) => {
                        this.txs.splice(0, 0, JSON.parse(tx));
                        this.txs.pop();
                    });
                
                this.connection.start()
                    .then(() => {
                    });
            }
        });
    }
}
