import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import '../../Style/blocks.block.scss';

export class BlockViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                confirmations: options.confirmations,
                showNext: options.showNext
            },
            computed: {
                confirmationText() {
                    return this.confirmations.toLocaleString();
                }
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/blockhub').build();

                this.connection.on('newBlockPubSub', (block) => {
                    this.confirmations++;
                    this.showNext = true;
                });

                this.connection.start();
            }
        });
    }
}
