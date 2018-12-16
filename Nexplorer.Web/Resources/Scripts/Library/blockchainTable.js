import Moment from 'moment';

import BootstrapPager from '../Library/bootstrapPager';
import dateAge from '../Library/dateAge';

export default (tableType) => {

    var template;
    var url;

    switch (tableType) {
        case 'blocks':
            template = require('../../Markup/block-table.html');
            url = 'blocks/getrecentblocks';
            break;
        case 'transactions':
            template = require('../../Markup/tx-table.html');
            url = 'transactions/getrecenttransactions';
            break;
        case 'addresses':
            template = require('../../Markup/address-table.html');
            url = 'addresses/getaddresstxs';
            break;
        default:
            return {};
    }

    return {
        template: template,
        props: ['localItems', 'itemsPerPage', 'pagingDisabled'],
        data: () => {
            return {
                currentPageItems: [],
                currentPage: 1,
                tableType: tableType
            };
        },
        components: {
            BootstrapPager: BootstrapPager(),
            DateAge: dateAge
        },
        methods: {
            changePage(newPage) {
                if (this.localItems) {
                    if (newPage < 1 || newPage > Math.ceil(this.localItems.length / this.itemsPerPage))
                        return;

                    const start = (newPage - 1) * this.itemsPerPage;

                    this.currentPageItems = this.localItems.slice(start, start + this.itemsPerPage);
                } else {
                    $.ajax({
                        url: url,
                        data: {
                            start: (newPage - 1) * this.itemsPerPage,
                            count: this.itemsPerPage
                        },
                        success: (items) => {
                            this.currentPageItems = items;
                        }
                    });
                }

                this.currentPage = newPage;
            },
            refreshPage() {
                this.changePage(this.currentPage);
            },
            addItem(item) {
                this.currentPageItems.splice(0, 0, item);
                this.currentPageItems.pop();
            },
            truncateHash(hash, len) {
                const start = hash.substring(0, len);
                const end = hash.substring(hash.length - len, hash.length);

                return start + '...' + end;
            }
        },
        created() {
            this.changePage(1);
        }
    };
};
