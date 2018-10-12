export default (pagerType) => {

    var template;

    switch (pagerType) {
        case 'custom':
            template = require('../../Markup/Pagination/custom-pager.html');
            break;
        default:
            template = require('../../Markup/Pagination/bootstrap-pager.html');
            break;
    }

    return {
        template: template,
        props: ['pageCount', 'currentPage', 'disabled'],
        data: () => {
            return {
            };
        },
        methods: {
            goToFirstPage() {
                if (this.disabled)
                    return;

                this.setPage(0);
            },
            goToLastPage() {
                if (this.disabled)
                    return;

                this.setPage(this.pageCount);
            },
            nextPage() {
                if (this.disabled)
                    return;

                this.setPage(this.currentPage + 1);
            },
            previousPage() {
                if (this.disabled)
                    return;

                this.setPage(this.currentPage - 1);
            },
            handlePageInput(event) {
                if (this.disabled)
                    return;

                this.setPage(Number(event.target.value));
            },
            setPage(requestPage) {
                let newPage;

                if (requestPage <= 0) {
                    newPage = 1;
                } else if (requestPage > this.pageCount) {
                    newPage = this.pageCount;
                } else {
                    newPage = requestPage;
                }

                if (newPage !== this.currentPage) {
                    this.$emit('page-change', newPage);
                }
            }
        }
    }
};