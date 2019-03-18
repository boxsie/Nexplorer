import $ from 'jquery';
import preloaderVue from '../Library/preloaderVue';

import '../../Style/Components/_data-table-vue.scss';

export default {
    template: require('../../Markup/data-table-vue.html'),
    props: ['options', 'columns'],
    data: () => {
        return {
            dtOptions: {
                defaultCriteria: {},
                ajaxUrl: '',
                localData: [],
                filters: [],
                showUserFilter: false,
                availableLengths: [10, 25, 50, 100],
                showRowIndex: false,
                paginationLength: 7
            },
            tableData: {
                pageItems: [],
                totalItems: 0
            },
            defaultCriteria: {
                page: 1,
                length: 10
            },
            criteria: {},
            filter: {
                name: '',
                isUserFilter: false
            },
            isLoading: false,
            baseUrl: `${window.location.protocol}//${window.location.host}/${window.location.pathname.split('/')
                .slice(1).join('/')}`
        };
    },
    computed: {
        dt() {
            return {
                tableData: this.tableData,
                reload: this.dataReload
            };
        },
        dtCriteria() {
            return {
                criteria: this.criteria,
                reload: this.reloadData
            };
        },
        pageCount() {
            return Math.ceil(this.tableData.totalItems / this.criteria.length);
        },
        pageNumbers() {
            const pages = [];

            if (this.pageCount <= this.dtOptions.paginationLength) {
                for (let i = 0; i < this.pageCount; i++) {
                    pages[i] = i + 1;
                }
            } else {
                const halfway = Math.ceil(this.dtOptions.paginationLength / 2);

                for (let i = 0; i < this.dtOptions.paginationLength; i++) {
                    if (i === 0) {
                        pages[i] = 1;
                    } else if (i === 1) {
                        pages[i] = this.criteria.page > halfway ? '...' : 2;
                    } else if (i === this.dtOptions.paginationLength - 2) {
                        pages[i] = this.criteria.page < this.pageCount - halfway ? '...' : this.pageCount - 1;
                    } else if (i === this.dtOptions.paginationLength - 1) {
                        pages[i] = this.pageCount;
                    } else {
                        if (this.criteria.page < halfway) {
                            pages[i] = i + 1;
                        } else if (this.criteria.page > this.pageCount - halfway) {
                            pages[i] = this.pageCount - (this.dtOptions.paginationLength - (i + 1));
                        } else {
                            pages[i] = this.criteria.page + (i + 1 - halfway);
                        }
                    }
                }
            }

            return pages;
        },
        headerRowClass() {
            const ic = this.dtOptions.showIndex ? 'row-index' : '';
            return `row dt-row ${ic}`;
        },
        filterClass() {
            const col = this.options.filterClass ? this.options.filterClass : 'col-12';
            return `custom-filter ${col}`;
        },
        hasLocalData() {
            return this.dtOptions.localData && this.dtOptions.localData.length > 0;
        },
        hasFilters() {
            return this.dtOptions.filters && this.dtOptions.filters.length > 0;
        },
        showUserFilter() {
            return this.dtOptions.showUserFilter || this.filter.isUserFilter;
        }
    },
    components: {
        Preloader: preloaderVue
    },
    methods: {
        parseRowClass(rowIndex) {
            const r = (rowIndex + 1) % 2 === 0 ? '' : 'odd-row';
            const ic = this.dtOptions.showRowIndex ? 'row-index' : '';
            return `row dt-row ${r} ${ic}`;
        },
        parseColClass(column, classStr, rowIndex) {
            if (!column.class) {
                return 'col';
            }

            return column.class.includes('col') ? `${classStr} ${column.class}` : `col ${classStr} ${column.class}`;
        },
        pageNumberClass(page) {
            return isNaN(page) || !page ? 'disabled' : page === this.criteria.page ? 'active' : 'enabled';
        },
        getFilterClass(i) {
            return this.dtOptions.filters[i].name === this.filter.name ? 'active-link' : '';
        },
        changeFilter(filterIndex) {
            if (!this.hasFilters || this.dtOptions.filters[filterIndex].name === this.filter.name || filterIndex >= this.dtOptions.filters.length) {
                return;
            }

            this.filter = this.dtOptions.filters[filterIndex];

            if (!this.filter.isUserFilter) {
                this.criteria = Object.assign({}, this.defaultCriteria, this.filter.criteria);
                this.reloadData();
            }
        },
        changePage(page) {
            if (isNaN(page) || !page || page === this.criteria.page) {
                return;
            }

            let newPage = this.criteria.page;

            if (page > this.pageCount) {
                newPage = this.pageCount;
            } else if (page < 1) {
                newPage = 1;
            } else {
                newPage = page;
            }

            if (newPage === this.criteria.page) {
                return;
            }

            this.criteria.page = newPage;
            this.reloadData();
        },
        changeLength() {
            this.criteria.page = 1;
            this.reloadData();
        },
        reloadData() {
            this.isLoading = true;

            const criteria = {
                filterCriteria: this.criteria,
                start: (this.criteria.page - 1) * this.criteria.length,
                length: this.criteria.length
            };

            if (this.dtOptions.ajaxUrl) {
                const self = this;
                $.ajax({
                    url: this.dtOptions.ajaxUrl,
                    type: 'POST',
                    data: criteria,
                    success: (result) => {
                        self.tableData.pageItems = result.data;
                        self.tableData.totalItems = result.recordsFiltered;
                    }
                }).always(() => { self.isLoading = false; });
            } else if (this.hasLocalData) {
                this.tableData.pageItems = this.dtOptions.localData.slice(criteria.start, criteria.start + criteria.length);
                this.tableData.totalItems = this.dtOptions.localData.length;
                this.isLoading = false;
            }

            this.setUrlQuery();
        },
        setUrlQuery() {
            const params = {};

            for (let key in this.criteria) {
                if (!this.criteria.hasOwnProperty(key))
                    continue;

                const prop = this.criteria[key];
                const defProp = this.defaultCriteria[key];
                
                if (prop && prop !== defProp) {
                    params[key] = this.criteria[key];
                }
            }
            
            const p = Object.keys(params).length > 0 ? `?${$.param(params)}` : '';
            const newUrl = `${this.baseUrl}${p}`;

            if (newUrl !== window.location.href) {
                window.history.pushState('', '', newUrl);
            }
        },
        getUrlQuery() {
            const getUrl = document.location.href;
            const querySplit = getUrl.split('?');
            const query = querySplit.length > 1 ? querySplit[1] : '';
            const result = {};

            if (query) {
                const pairs = query.split('&');

                pairs.forEach((pair) => {
                    var kvp = pair.split('=');

                    var key = kvp[0];
                    var val = decodeURIComponent(kvp[1] || '');

                    result[key] = key === 'page' || key === 'length' ? parseInt(val) : val;
                });
            }

            return result;
        },
        matchQueryToFilter(queryObj) {
            let matchedFilterIndex = -1;

            if (!this.hasFilters) {
                return matchedFilterIndex;
            }

            let userIndex = -1;

            const qo = Object.assign({}, queryObj);
            delete qo.page;
            delete qo.length;

            this.dtOptions.filters.forEach((filter, i) => {
                if (JSON.stringify(filter.criteria) === JSON.stringify(qo)) {
                    matchedFilterIndex = i;
                } else if (filter.isUserFilter) {
                    userIndex = i;
                }
            });

            return matchedFilterIndex >= 0 ? matchedFilterIndex : userIndex;
        },
        onPageLoad(isPopState) {
            const qo = this.getUrlQuery();

            if (this.hasFilters) {
                this.changeFilter(this.matchQueryToFilter(qo));
            }

            const newCriteria = Object.assign({}, this.defaultCriteria, qo);

            if (JSON.stringify(newCriteria) !== JSON.stringify(this.criteria)) {
                this.criteria = newCriteria;
                this.reloadData();
            }
        }
    },
    mounted() {
        this.dtOptions = Object.assign({}, this.dtOptions, this.options);
        this.defaultCriteria = Object.assign({}, this.defaultCriteria, this.dtOptions.defaultCriteria);
        this.criteria = Object.assign({}, this.criteria, this.defaultCriteria);
        
        window.addEventListener('popstate', () => this.onPageLoad(true));

        this.onPageLoad();
    }
};

