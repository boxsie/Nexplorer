export default {
    template: '<span>{{ currencyOutput }}<span v-bind:class="currencyClass"></span></span>',
    props: ['currencyIndex', 'currencyValue'],
    data: () => {
        return {
            currencySymbol: '$',
            currencyClass: ''
        };
    },
    computed: {
        currencyOutput() {
            return this.currencySymbol + this.currencyValue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        }
    },
    methods: {
        setCurrency() {
            switch (this.currencyIndex) {
                case 'USD':
                    this.currencySymbol = '$';
                    this.currencyClass = '';
                    break;
                case 'GBP':
                    this.currencySymbol = '£';
                    this.currencyClass = '';
                    break;
                case 'EUR':
                    this.currencySymbol = '€';
                    this.currencyClass = '';
                    break;
                case 'AUD':
                    this.currencySymbol = '$';
                    this.currencyClass = '';
                    break;
                case 'BTC':
                    this.currencySymbol = '';
                    this.currencyClass = 'fa fa-btc';
                    break;
                default:
                    this.currencySymbol = '$';
                    this.currencyClass = '';
                    break;
            }
        }
    },
    mounted() {
        this.setCurrency();
    }
};

