import Vue from 'vue';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

import '../../Style/admin.index.scss';

export class AdminIndexViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                syncOutputs: []
            },
            components: {
            },
            methods: {
                addSyncOutput(syncOutput) {
                    const split = syncOutput.split("|");

                    this.syncOutputs.splice(0, 0, {
                        timestamp: split[0],
                        level: split[1],
                        message: split[2]
                    });
                }
            },
            created() {
                const self = this;

                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/adminhub').build();

                this.connection.on('syncOutput', (syncOutput) => {
                    self.addSyncOutput(syncOutput);
                });

                this.connection.start()
                    .then(() => {
                        this.connection.invoke("getLatestSyncOutputs").then((latestOutputs) => {
                            for (let i = 0; i < latestOutputs.length; i++) {
                                self.addSyncOutput(latestOutputs[i]);
                            }
                        });
                    });
            }
        });
    }
}
