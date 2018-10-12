import moment from 'moment';
import LineChart from './lineChartVue.js';
import 'chartjs-plugin-streaming';


export default {
    template: require('../../Markup/streaming-line-chart.html'),
    props: ['initialData', 'width', 'height', 'latest', 'delay', 'exp', 'showY', 'hidePoints'],
    components: {
        LineChart
    },
    data() {
        return {
            datacollection: {
                datasets: [
                    {
                        backgroundColor: 'rgba(99, 98, 187, 0.3)',
                        borderColor: '#6362bb',
                        fillOpacity: .3,
                        lineTension: 0.2,
                        pointRadius: this.hidePoints ? 0 : 2,
                        data: this.initialData ? this.initialData.map((x) => {
                            return {
                                x: x.date,
                                y: x.val
                            }
                        }) : []
                    }]
            },
            currentDays: 7,
            chartOptions: {
                maintainAspectRatio: false,
                scales: {
                    yAxes: [{
                        ticks: {
                            display: this.showY,
                            maxTicksLimit: 3,
                            callback: (value, index, values) => {
                                return this.exp ? value.toExponential() : value.toLocaleString();
                            }
                        },
                        gridLines: {
                            display: false,
                            drawBorder: false
                        }
                    }],
                    xAxes: [{
                        type: 'realtime',
                        ticks: {
                            fontSize: 8
                        },
                        time: {
                            displayFormats: {
                                millisecond: 'H:mm:ss',
                                second: 'H:mm:ss',
                                minute: 'H:mm',
                                hour: 'H:mm',
                                day: 'H:mm',
                                week: 'H:mm',
                                month: 'H:mm',
                                quarter: 'H:mm',
                                year: 'H:mm'
                            },
                            parser: (utcTime) => {
                                return moment.utc(utcTime);
                            }  
                        },
                        gridLines: {
                            display: false
                        }
                    }]
                },
                tooltips: {
                    callbacks: {
                        label: (tooltipItem, data) => {
                            var label = data.datasets[tooltipItem.datasetIndex].label || '';

                            if (label) {
                                label += ': ';
                            }

                            label += tooltipItem.yLabel.toLocaleString();

                            return label;
                        }
                    }
                },
                legend: {
                    display: false
                },
                plugins: {
                    streaming: {
                        duration: 300000,
                        delay: this.delay,
                        refresh: this.delay,
                        onRefresh: (chart) => {
                            chart.data.datasets.forEach((dataset) => {
                                dataset.data.push({
                                    x: moment.utc(),
                                    y: this.latest
                                });
                            });
                        }
                    }
                }
            }
        };
    },
    mounted() {

    }
};
