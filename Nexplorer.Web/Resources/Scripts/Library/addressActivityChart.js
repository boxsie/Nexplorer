import $ from 'jquery';
import Moment from 'moment';
import LineChart from './lineChartVue';
import preloaderVue from '../Library/preloaderVue';

export default {
    template: require('../../Markup/address-activity-chart.html'),
    props: ['width', 'height', 'addressHash'],
    components: {
        LineChart,
        Preloader: preloaderVue
    },
    data() {
        return {
            datacollection: null,
            currentDays: 7,
            showChart: false,
            chartOptions: {
                maintainAspectRatio: false,
                scales: {
                    yAxes: [
                        {
                            ticks: {
                                maxTicksLimit: 5,
                                callback: function(value, index, values) {
                                    return value.toLocaleString();
                                }
                            }
                        }
                    ]
                },
                tooltips: {
                    callbacks: {
                        label: function(tooltipItem, data) {
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
                }
            }
        };
    },
    mounted() {
        this.getBalance();
    },
    methods: {
        getBalance() {
            this.showChart = false;

            $.ajax({
                type: 'GET',
                url: '/addresses/getaddressbalance',
                data: {
                    addressHash: this.addressHash,
                    days: this.currentDays
                },
                success: (result) => {
                    this.datacollection = {
                        labels: result.map(function(x) {
                            return Moment(x.date).format("MMM Do");
                        }),
                        datasets: [
                            {
                                label: 'Balance',
                                backgroundColor: 'rgba(99, 98, 187, 0.3)',
                                borderColor: '#6362bb',
                                fillOpacity: .3,
                                data: result.map(function(x) {
                                    if (x.balance <= 0)
                                        x.balance = 0;

                                    return x.balance;
                                }),
                                lineTension: 0
                            }
                        ]
                    };
                    
                    this.showChart = true;
                }
            });
        },
        changeDays(days) {
            if (this.currentDays !== days) {
                this.currentDays = days;
                this.getBalance();
            }
        }
    }
};
