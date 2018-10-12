import Moment from 'moment';
import BootstrapPager from '../Library/bootstrapPager.js';

export default {
    template: require('../../Markup/address-tx-table.html'),
    props: ['addressHash', 'txType', 'pageCount', 'txPerPage', 'onScreenTxType'],
    data: () => {
        return {
            currentPageTxs: [],
            currentPage: 1
        };
    },
    components: {
        CustomPagerTop: BootstrapPager('custom'),
        CustomPagerBottom: BootstrapPager('custom')
    },
    methods: {
        changePage(newPage) {
            this.currentPageTxs = [];

            $.ajax({
                url: '/addresses/getaddresstxs',
                data: {
                    start: (newPage - 1) * this.txPerPage,
                    count: this.txPerPage,
                    txType: this.txType,
                    addressHash: this.addressHash
                },
                success: (txs) => {
                    this.currentPageTxs = txs;
                    this.currentPage = newPage;
                }
            });
        },
        getDate(date) {
            return Moment(date).format("MMM Do 'YY");
        }
    },
    created() {
        this.changePage(1);
    }
};
