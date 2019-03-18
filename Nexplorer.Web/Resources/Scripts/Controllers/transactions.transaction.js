import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import moment from 'moment';

import dataTableVue from '../Library/dataTableVue';

import '../../Style/transactions.transaction.scss';

export class TransactionViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                inputDtOptions: {},
                outputDtOptions: {},
                confirmations: options.confirmations,
                columns: []
            },
            computed: {
                confirmationText() {
                    return this.confirmations.toLocaleString();
                }
            },
            components: {
                txTableInputs: dataTableVue,
                txTableOutputs: dataTableVue
            },
            methods: {
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    return start + '...';
                }
            },
            created() {
                this.columns = [
                    {
                        key: 'addressHash',
                        header: '<span class="fa fa-hashtag"></span>',
                        class: 'col-8 col-sm-9',
                        render: (data, row) => {
                            return `<a class="d-none d-sm-inline" href="/addresses/${data}">${data}</a>
                                    <a class="d-sm-none" href="/addresses/${data}">${this.truncateHash(data, 30)}</a>`;
                        }
                    },
                    {
                        key: 'amount',
                        header: '<span class="fa fa-paper-plane-o"></span>',
                        class: 'col-4 col-sm-3 text-right',
                        render: (data, row) => {
                            const balanceTotal = parseFloat(data.toFixed(4)).toLocaleString();
                            const amounts = `<strong>${balanceTotal}</strong> <small>NXS</small>`;
                            return amounts;
                        }
                    }
                ];

                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/blockhub').build();

                this.connection.on('newBlockPubSub', (block) => {
                    this.confirmations++;
                });

                this.connection.start();

                this.inputDtOptions = {
                    localData: options.inputs,
                    useQueryString: false
                };

                this.outputDtOptions = {
                    localData: options.outputs,
                    useQueryString: false
                };
            }
        });
    }
}
