import Vue from 'vue';
import moment from 'moment';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

export default {
    install(Vue, options) {
        Vue.prototype.$layoutHub = new Vue({
            data: {
                utcMoment: moment()
            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/layouthub').build();

                this.connection.start()
                    .then(() => {
                        this.connection.on('updateTimestampUtc',
                            (timestamp) => {
                                this.utcMoment = moment(timestamp);
                            });

                        this.connection.invoke('getLatestTimestampUtc').then((timestamp) => {
                            this.utcMoment = moment(timestamp);
                        });
                    });

                setInterval(() => {
                    this.utcMoment = moment(this.utcMoment.add(1, 's'));
                }, 1000);
            }
        });
    }
};