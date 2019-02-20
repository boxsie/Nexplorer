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
            availableLengths: [10, 30, 50, 100],
            showTable: false,
            defaultFilter: {}
        };
    },
    computed: {
        currentFilter() {
            return this.filters[this.filterIndex];
        }
    },
    components: {
        Preloader: preloaderVue
    },
    methods: {
        parseClass(column, classStr) {
            return column.class.includes('col') ? `${classStr} ${column.class}` : `col ${classStr} ${column.class}`;
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
            const lastPage = Math.ceil(this.tableDataCount / this.pageLength);

            if (page === null || page > lastPage) {
                this.page = lastPage;
            } else if (page < 1) {
                this.page = 1;
            } else {
                this.page = page;
            }

            this.dataReload();
        },
        changeLength() {
            this.page = 1;
            this.dataReload();
        },
        dataReload() {
            this.currentCriteria = Object.assign({}, this.defaultCriteria, this.filter.isCustom ? this.customCriteria : this.filter.criteria);

            const data = {
                filterCriteria: this.currentCriteria,
                start: this.page - 1,
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
            } else {
                this.filterIndex = 0;
            }

            this.filter = this.filters[this.filterIndex];
            this.dataReload();
        }
    },
    created() {
        this.currentCriteria = Object.assign({}, this.defaultCriteria);
        this.customCriteria = Object.assign({}, this.defaultCriteria);

        window.addEventListener('popstate', this.parseUrlCriteria);

        this.parseUrlCriteria();
    }
};

