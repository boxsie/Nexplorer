import $ from 'jquery';

export default {
    template: require('../../Markup/address-alias-edit.html'),
    props: ['currentItem'],
    data: () => {
        return {
            bufferItem: {}
        };
    },
    methods: {
        editFavourite(favourite) {
            this.currentItem = favourite;
            this.bufferItem = {
                hash: favourite.addressDto.hash,
                alias: favourite.alias
            };

            $('#editFavouriteModal').modal();
        }
    }
};

