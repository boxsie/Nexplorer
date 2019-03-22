
export default {
    template: require('../../Markup/collapsable-list-vue.html'),
    props: ['items', 'minItems'],
    data: () => {
        return {
            expanded: false
        };
    }
};
