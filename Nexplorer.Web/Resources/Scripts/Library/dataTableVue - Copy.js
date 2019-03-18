import $ from 'jquery';
import preloaderVue from '../Library/preloaderVue';

import '../../Style/Components/_data-table-vue.scss';

export default {
    template: require('../../Markup/data-table-vue.html'),
    props: ['options', 'defaultCriteria', 'columns'],
    data: () => {
        return {
            dtOptions: {
                ajaxUrl: '',
                localData: [],
                filters: [],
                showUserFilter: false,
                availableLengths: [10, 25, 50, 100]
            },
            tableData: {
                pageItems: [],
                totalItems: 0
            },
            criteria: {
                page: 1,
                length: 10
            },
            filter: {
                name: '',
                isUserFilter: false
            },
            isLoading: false,
            baseUrl: `${window.location.protocol}//${window.location.host}/${window.location.pathname.split('/').slice(1).join('/')}`
        };
    },
    computed: {
        hasFilters() {
            return this.dtOptions.filters && this.dtOptions.filters.length > 0;
        },
        hasLocalData() {
            return this.dtOptions.localData && this.dtOptions.localData.length > 0;
        },
        showUserFilter() {
            return this.localOptions.showUserFilter || this.filter.isUserFilter;
        }
    },
    watch: {
        
    },
    components: {
        Preloader: preloaderVue
    },
    methods: {
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
                    url: this.options.ajaxUrl,
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
                    
                    result[key] = val;
                });
            }

            return result;
        },
        matchQueryToFilter(queryObj) {
            var matchedFilter = {};

            if (!this.hasFilters) {
                return matchedFilter;
            }

            this.filters.forEach((filter) => {
                if (JSON.stringify(filter.criteria) === JSON.stringify(queryObj)) {
                    matchedFilter = filter;
                }
            });

            return matchedFilter;
        },
        onPageLoad(isPopState) {
            const qo = this.getUrlQuery();

            if (this.hasFilters) {
                this.filter = this.matchQueryToFilter(qo);
            }

            const newCriteria = Object.assign({}, this.defaultCriteria, this.criteria, qo);

            if (JSON.stringify(newCriteria) !== JSON.stringify(this.criteria)) {
                this.criteria = newCriteria;
            }
        }
    },
    mounted() {
        this.dtOptions = Object.assign({}, this.dtOptions, this.options);
        this.criteria = Object.assign({}, this.criteria, this.defaultCriteria);

        window.addEventListener('popstate', () => this.onPageLoad(true));

        this.onPageLoad();
    }
};

