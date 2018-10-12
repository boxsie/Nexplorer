import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import BlockchainTable from '../Library/blockchainTable.js';

export class TransactionViewModel {
    constructor(options) {
        this.txsVue = new Vue({
            el: '#transactionPages',
            data: {
                currentPageTxs: [],
                currentPage: 0
            },
            components: {
                BlockchainTable: BlockchainTable('transactions')
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/transactionhub').build();

                this.connection.on('newTxPubSub', (tx) => {
                    this.$refs.txTable.refreshPage();
                });

                this.connection.start();
            }
        });
    }
}
