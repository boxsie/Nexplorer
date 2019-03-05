import $ from 'jquery';

export default (id) => {
    return {
        template: require('../../Markup/transaction-table.html'),
        props: ['txs', 'columns'],
        data: () => {
            return {
                $dataTableTxs: null,
                dataTableTxs: null,
                grouped: false,
                txTableId: 'txTable' + id
        };
        },
        methods: {
            reloadData() {

            },
            groupData(data) {
                const g = data.reduce((groups, item) => {
                        var val = item['addressHash'];
                        groups[val] = groups[val] || [];
                        groups[val].push(item);
                        return groups;
                    },
                    {});

                const grouped = [];

                for (let i = 0; i < g.length; i++) {
                    const item = g[i];

                    grouped.push({
                        addressHash: item.key,
                        amount: item.reduce((a, b) => a.amount + b.amount, 0)
                    });
                }

                return grouped;
            }
        },
        mounted() {
            var self = this;

            $(() => {
                self.$dataTableTxs = $(`#${self.txTableId}`);

                self.dataTableTxs = self.$dataTableTxs.DataTable({
                    dom: '<"top"f>rt<"bottom"lp><"clear">',
                    responsive: true,
                    info: true,
                    pagingType: 'first_last_numbers',
                    lengthMenu: [10, 30, 50, 100],
                    columns: self.columns,
                    data: self.txs
                });
            });
        }
    }
};
