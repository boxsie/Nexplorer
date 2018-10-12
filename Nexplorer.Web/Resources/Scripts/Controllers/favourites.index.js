import $ from 'jquery';
import BootstrapPager from '../Library/bootstrapPager.js';

export class FavouritesIndexViewModel {
    constructor(options) {
        this.vm = new Vue({
            el: '#main',
            data: {
                currentPageItems: [],
                currentPage: 1,
                localItems: options.favourites,
                currentItem: {},
                itemsPerPage: 10,
                pagingDisabled: false,
                errorMessage: '',
                bufferItem: {
                    hash: '',
                    alias: ''
                }
            },
            components: {
                BootstrapPager: BootstrapPager()
            },
            methods: {
                editFavourite(favourite) {
                    this.currentItem = favourite;
                    this.bufferItem = {
                        hash: favourite.addressDto.hash,
                        alias: favourite.alias
                    };

                    $('#editFavouriteModal').modal();
                },
                updateFavourite() {
                    this.currentItem.alias = this.bufferItem.alias;

                    $.ajax({
                        url: '/favourites/setaddressalias',
                        type: 'POST',
                        data: {
                            favouriteAddressId: this.currentItem.favouriteAddressId,
                            alias: this.currentItem.alias
                        },
                        success: () => {
                            $('#editFavouriteModal').modal('hide');
                        },
                        failure: (error) => {
                            this.errorMessage = error;
                        }
                    });
                },
                removeFavourite(favourite) {
                    $.ajax({
                        url: '/favourites/removeaddressfavourite',
                        type: 'POST',
                        data: {
                            addressId: favourite.addressDto.addressId
                        },
                        success: () => {
                            this.localItems.splice(this.localItems.indexOf(favourite), 1);
                            this.changePage(this.currentPage);
                        },
                        failure: (error) => {
                            this.errorMessage = error;
                        }
                    });
                },
                truncateHash(hash, len) {
                    const start = hash.substring(0, len);
                    const end = hash.substring(hash.length - len, hash.length);

                    return start + '...' + end;
                },
                changePage(newPage) {
                    if (newPage < 1 || newPage > Math.ceil(this.localItems.length / this.itemsPerPage))
                        return;

                    const start = (newPage - 1) * this.itemsPerPage;

                    this.currentPageItems = this.localItems.slice(start, start + this.itemsPerPage);

                    this.currentPage = newPage;
                },
                addItem(item) {
                    this.currentPageItems.splice(0, 0, item);
                    this.currentPageItems.pop();
                }
            },
            mounted() {
                this.changePage(1);
            }
        });
    }
}
