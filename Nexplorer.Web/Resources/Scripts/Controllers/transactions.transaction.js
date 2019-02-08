import $ from 'jquery';
import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import moment from 'moment';

import txTable from '../Library/transactionTable';
import '../../Style/transactions.transaction.scss';

export class TransactionViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                confirmations: options.confirmations,
                inputs: options.inputs,
                outputs: options.outputs,
                columns: [
                    {
                        title: '<span class="fa fa-hashtag"></span>',
                        className: '',
                        data: 'addressHash',
                        width: '70%',
                        render: (data, type, row) => {
                            const hash = data;

                            return `<a class="d-none d-sm-block" href="/addresses/${hash}">${hash}</a>
                                    <a class="d-sm-none" href="/addresses/${hash}">${this.vm.truncateHash(hash, 24)}</a>`;
                        }
                    },
                    {
                        title: '<span class="fa fa-paper-plane-o"></span>',
                        className: 'text-right',
                        data: 'amount',
                        width: '30%',
                        render: (data, type, row) => {
                            var balanceTotal = parseFloat(data.toFixed(4)).toLocaleString();
                            var amounts = `<strong>${balanceTotal}</strong> <small>NXS</small>`;
                            return amounts;
                        }
                    }
                ]
            },
            computed: {
                confirmationText() {
                    return this.confirmations.toLocaleString();
                }
            },
            components: {
                txTableInputs: txTable('Inputs'),
                txTableOutputs: txTable('Outputs')
            },
            methods: {
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                }
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/blockhub').build();

                this.connection.on('newBlockPubSub', (block) => {
                    this.confirmations++;
                });

                this.connection.start();
            }
        });
    }
}
