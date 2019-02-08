import $ from 'jquery';
import dt from 'datatables.net-bs4';
import preloaderVue from '../Library/preloaderVue';

export default (pagingType) => {
    return {
        template: require('../../Markup/data-table-vue.html'),
        props: ['filter', 'filterCriteria', 'ajaxUrl', 'columns', 'ignoreKeys'],
        data: () => {
            return {
                $dataTable: null,
                dataTable: null,
                showTable: false,
                defaultCriteria: {},
                defaultFilter: {},
                justArrived: true
            };
        },
        components: {
            Preloader: preloaderVue
        },
        methods: {
            dataReload() {
                setTimeout(() => {
                    this.dataTable.ajax.reload();
                }, 1);
            },
            createFilterQuery(page, length) {
                const params = {
                    page: page,
                    length: length
                };

                if (this.filter) {
                    params.filter = this.filter;
                }

                for (let key in this.filterCriteria) {
                    if (this.filterCriteria.hasOwnProperty(key) && this.filterCriteria[key] != null && !this.isKeyIgnored(key)) {
                        params[key] = this.filterCriteria[key];
                    }
                }

                return $.param(params);
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
            createFilterQueryObject() {
                const getUrl = window.location.href;
                const query = getUrl.split('?');

                if (query.length < 2) {
                    return '';
                }

                const pairs = query[1].split('&');

                var result = {};
                
                pairs.forEach((pair) => {
                    pair = pair.split('=');
                    result[pair[0]] = decodeURIComponent(pair[1] || '');
                });

                return JSON.parse(JSON.stringify(result));
            },
            getDefaultCriteria(filterQueryObject) {
                const crit = JSON.parse(JSON.stringify(this.filterCriteria));

                if (!filterQueryObject) {
                    return crit;
                }

                for (let key in crit) {
                    if (crit.hasOwnProperty(key)) {
                        if (filterQueryObject[key]) {
                            crit[key] = filterQueryObject[key];
                        }
                    }
                }

                return crit;
            },
            getDefaultFilter(filterQueryObject) {
                return filterQueryObject && filterQueryObject.filter ? filterQueryObject.filter : null;
            },
            getDisplayStart(filterQueryObject) {
                return filterQueryObject && filterQueryObject.page && filterQueryObject.length ? filterQueryObject.page * filterQueryObject.length : 0;
            },
            getLegthStart(filterQueryObject) {
                return filterQueryObject && filterQueryObject.length ? filterQueryObject.length : 10;
            },
            setWindowUrl(page, length) {
                if (this.justArrived) {
                    window.history.pushState('', '', `${this.getBaseUrl()}?${this.createFilterQuery(page, length)}`);
                    this.justArrived = false;
                } else {
                    window.history.replaceState('', '', `${this.getBaseUrl()}?${this.createFilterQuery(page, length)}`);
                }
            },
            resetWindowUrl() {
                window.history.replaceState('', '', this.getBaseUrl());
            },
            getBaseUrl() {
                const getUrl = window.location;
                const pathnames = getUrl.pathname.split('/');
                return getUrl.protocol + '//' + getUrl.host + '/' + pathnames.slice(1).join('/');
            }
        },
        mounted() {
            this.defaultCriteria = JSON.stringify(this.filterCriteria);
            this.defaultFilter = JSON.stringify(this.filter);

            const filterQueryObject = this.createFilterQueryObject();

            this.$emit('filter-update', this.getDefaultFilter(filterQueryObject));
            this.$emit('filter-criteria-update', this.getDefaultCriteria(filterQueryObject));
            
            var self = this;

            $(() => {
                self.$dataTable = $('#dataTable');

                self.dataTable = self.$dataTable.DataTable({
                    dom: '<"top"f>rt<"bottom"lp><"clear">',
                    searching: false,
                    ordering: false,
                    responsive: true,
                    serverSide: true,
                    info: true,
                    displayStart: self.getDisplayStart(filterQueryObject),
                    pagingType: pagingType,
                    lengthMenu: [10, 30, 50, 100],
                    pageLength: self.getLegthStart(filterQueryObject),
                    ajax: {
                        url: self.ajaxUrl,
                        type: 'POST',
                        data: (data) => {
                            data.filter = self.filter;
                            data.filterCriteria = self.filterCriteria;

                            console.log(self.filterCriteria);
                            self.showTable = false;

                            var page = self.$dataTable.DataTable().page();
                            var len = self.$dataTable.DataTable().page.len();

                            if (page > 0 || len > 10 || JSON.stringify(data.filter) !== self.defaultFilter || JSON.stringify(data.filterCriteria) !== self.defaultCriteria) {
                                self.setWindowUrl(page, len);
                            } else {
                                self.resetWindowUrl();
                            }

                            return data;
                        },
                        dataFilter: (data) => {
                            const obj = JSON.parse(data);
                            const pageInfo = self.dataTable.page.info();

                            if (!obj.data) {
                                return data;
                            }

                            self.showTable = true;

                            for (let i = 0; i < obj.data.length; i++) {
                                obj.data[i].i = i + 1 + pageInfo.page * pageInfo.length;
                            }

                            return JSON.stringify(obj);
                        },
                        dataType: 'json'
                    },
                    order: [[2, 'dsc']],
                    columns: self.columns
                });

                $('#dataTable tbody').on('click', 'tr', function(e) {
                    if (!$(e.target).is('.no-row-link')) {
                        self.$emit('row-click', self.dataTable.row(this).data());
                    }
                });
            });
        }
    };
};
