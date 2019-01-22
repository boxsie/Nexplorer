import $ from 'jquery';
import dt from 'datatables.net-bs4';
import preloaderVue from '../Library/preloaderVue';

export default (defaultCriteria, pagingType) => {
    return {
        template: require('../../Markup/data-table-vue.html'),
        props: ['ajaxUrl', 'columns'],
        data: () => {
            return {
                $dataTable: null,
                dataTable: null,
                filter: null,
                filterCriteria: defaultCriteria,
                showTable: false
            };
        },
        components: {
            Preloader: preloaderVue
        },
        methods: {
            dataReload(filterCriteria, filter) {
                this.filterCriteria = filterCriteria;
                this.filter = filter;
                this.dataTable.ajax.reload();
            }
        },
        mounted() {
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
                    pagingType: pagingType,
                    lengthMenu: [10, 30, 50, 100],
                    ajax: {
                        url: self.ajaxUrl,
                        type: 'POST',
                        data: (data) => {
                            data.filter = self.filter;
                            data.filterCriteria = self.filterCriteria;
                            self.showTable = false;
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

                $('#dataTable tbody').on('click',
                    'tr',
                    function(e) {
                        if (!$(e.target).is('.no-row-link')) {
                            self.$emit('row-click', self.dataTable.row(this).data());
                        }
                    });
            });
        }
    };
};
