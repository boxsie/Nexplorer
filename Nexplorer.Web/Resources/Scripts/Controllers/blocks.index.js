import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import BlockchainTable from '../Library/blockchainTable.js';

export class BlockViewModel {
    constructor(options) {
        this.blocksVue = new Vue({
            el: '#blockPages',
            data: {
            },
            components: {
                BlockchainTable: BlockchainTable('blocks')
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/blockhub').build();

                this.connection.on('newBlockPubSub', (block) => {
                    this.$refs.blockTable.refreshPage();
                });

                this.connection.start();
            }
        });
    }
}
