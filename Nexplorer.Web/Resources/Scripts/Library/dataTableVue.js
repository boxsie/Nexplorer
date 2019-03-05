import $ from 'jquery';
import preloaderVue from '../Library/preloaderVue';

import '../../Style/Components/_data-table-vue.scss';

export default {
    template: require('../../Markup/data-table-vue.html'),
    props: ['ajaxUrl', 'defaultCriteria', 'columns', 'filters', 'ignoreKeys'],
    data: () => {
        return {
            tableData: {},
            tableDataCount: 0,
            filterIndex: 0,
            filter: {},
            currentCriteria: {},
            customCriteria: {},
            page: 1,
            pageLength: 10,
            paginationLength: 7,
            availableLengths: [10, 25, 50, 100],
            showTable: false,
            defaultFilter: {}
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
                criteria: this.customCriteria,
                reload: this.dataReload
            };
        },
        currentFilter() {
            return this.filters[this.filterIndex];
        },
        pageCount() {
            return Math.ceil(this.tableDataCount / this.pageLength);
        },
        pageNumbers() {
            const pages = [];
            
            if (this.pageCount <= this.paginationLength) {
                for (let i = 0; i < this.pageCount; i++) {
                    pages[i] = i + 1;
                }
            } else {
                const halfway = Math.ceil(this.paginationLength / 2);

                for (let i = 0; i < this.paginationLength; i++) {
                    if (i === 0) {
                        pages[i] = 1;
                    } else if (i === 1) {
                        pages[i] = this.page > halfway ? '...' : 2;
                    } else if (i === this.paginationLength - 2) {
                        pages[i] = this.page < this.pageCount - halfway ? '...' : this.pageCount - 1;
                    } else if (i === this.paginationLength - 1) {
                        pages[i] = this.pageCount;
                    } else {
                        if (this.page < halfway) {
                            pages[i] = i + 1;
                        } else if (this.page > this.pageCount - halfway) {
                            pages[i] = this.pageCount - (this.paginationLength - (i + 1));
                        } else {
                            pages[i] = this.page + (i + 1 - halfway);
                        }
                    }
                }
            }

            return pages;
        }
    },
    components: {
        Preloader: preloaderVue
    },
    methods: {
        parseRowClass(rowIndex) {
            const r = (rowIndex + 1) % 2 === 0 ? '' : 'odd-row';
            return `row dt-row ${r}`;

        },
        parseColClass(column, classStr, rowIndex) {
            return column.class.includes('col') ? `${classStr} ${column.class}` : `col ${classStr} ${column.class}`;
        },
        pageNumberClass(page) {
            return isNaN(page) || !page ? 'disabled' : page === this.page ? 'active' : 'enabled';
        },
        changeFilter(filterIndex) {
            if (filterIndex >= this.filters.length)
                return;

            this.filterIndex = filterIndex;
            this.filter = this.filters[this.filterIndex];

            if (!this.filter.isCustom) {
                this.dataReload();
            }
        },
        changePage(page) {
            if (isNaN(page) || !page || page === this.page) {
                return;
            }

            let newPage = this.page;

            if (page > this.pageCount) {
                newPage = this.pageCount;
            } else if (page < 1) {
                newPage = 1;
            } else {
                newPage = page;
            }

            if (newPage === this.page) {
                return;
            }

            this.dataReload(newPage);
        },
        changeLength() {
            this.page = 1;
            this.dataReload();
        },
        dataReload(newPage) {
            this.page = newPage ? newPage : 1;

            this.currentCriteria = Object.assign({}, this.defaultCriteria, this.filter.isCustom ? this.customCriteria : this.filter.criteria);

            const data = {
                filterCriteria: this.currentCriteria,
                start: (this.page - 1) * this.pageLength,
                length: this.pageLength
            };

            this.showTable = false;
            this.setWindowUrl();

            const self = this;

            $.ajax({
                url: this.ajaxUrl,
                type: 'POST',
                data: data,
                success: (result) => {
                    self.dataRefresh(result);
                }
            });
        },
        dataRefresh(result) {
            this.tableData = result.data;
            this.tableDataCount = result.recordsFiltered;
            this.showTable = true;
        },
        createFilterQuery(page, length, filter) {
            const params = {
                page: page,
                length: length
            };
            
            for (let key in filter) {
                if (!filter.hasOwnProperty(key))
                    continue;

                const prop = filter[key];
                const defProp = this.defaultCriteria[key];

                if (prop !== null && !this.isKeyIgnored(key) && prop !== defProp) {
                    params[key] = filter[key];
                }
            }

            return $.param(params);
        },
        createFilterQueryObject(query) {
            const pairs = query.split('&');

            var result = {
                criteria: {}
            };

            pairs.forEach((pair) => {
                pair = pair.split('=');

                var key = pair[0];
                var val = decodeURIComponent(pair[1] || '');

                if (key === 'page' || key === 'length') {
                    result[key] = val;
                } else {
                    result.criteria[key] = val;
                }
            });

            return result;
        },
        getFilterIndexFromQueryObj(filterQueryObj) {
            let filterIndex = -1;
            let customIndex = -1;

            const filterQueryJson = JSON.stringify(filterQueryObj.criteria);

            for (let i = 0; i < this.filters.length; i++) {
                const filter = this.filters[i];

                if (filter.isCustom) {
                    customIndex = i;
                } else if (JSON.stringify(filter.criteria) === filterQueryJson) {
                    filterIndex = i;
                }
            }

            if (filterIndex === -1) {
                if (customIndex > -1) {
                    filterIndex = customIndex;
                } else {
                    filterIndex = 0;
                }
            }

            return filterIndex;
        },
        setWindowUrl() {
            const newUrl = `${this.getBaseUrl()}?${this.createFilterQuery(this.page, this.pageLength, this.currentCriteria)}`;

            if (newUrl !== window.location.href) {
                window.history.pushState('', '', newUrl);
            }
        },
        getBaseUrl() {
            return `${window.location.protocol}//${window.location.host}/${window.location.pathname.split('/').slice(1).join('/')}`;
        },
        isKeyIgnored(key) {
            if (!this.ignoreKeys) {
                return false;
            }

            for (let i = 0; i < this.ignoreKeys.length; i++) {
                if (key === this.ignoreKeys[i]) {
                    return true;
                }
            }

            return false;
        },
        parseUrlCriteria() {
            const getUrl = document.location.href;
            const querySplit = getUrl.split('?');
            const query = querySplit.length > 1 ? querySplit[1] : '';

            if (query) {
                const filterQueryObj = this.createFilterQueryObject(query);
                this.customCriteria = Object.assign({}, this.defaultCriteria, filterQueryObj.criteria);
                this.filterIndex = this.getFilterIndexFromQueryObj(filterQueryObj);
                this.page = parseInt(filterQueryObj.page);
                this.pageLength = parseInt(filterQueryObj.length);
            } else {
                this.filterIndex = 0;
            }

            this.filter = this.filters[this.filterIndex];
            this.dataReload(this.page);
        }
    },
    created() {
        this.currentCriteria = Object.assign({}, this.defaultCriteria);
        this.customCriteria = Object.assign({}, this.defaultCriteria);

        window.addEventListener('popstate', this.parseUrlCriteria);

        this.parseUrlCriteria();
    }
};

