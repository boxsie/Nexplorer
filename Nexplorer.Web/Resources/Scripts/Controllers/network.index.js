import '../../Style/network.index.scss';

export class NetworkIndexViewModel {
    constructor(options) {
        this.peerGeolocations = options.peerGeolocations;

        this.peerInfoVm = new Vue({
            el: '#peerInfo',
            data: {
                peerInfo: {
                    address: '',
                    chainHeight: '',
                    location: ''
                }
            },
            methods: {
                peerInfoBack: () => {
                }
            }
        });

    }

    initMap() {
        const map = new google.maps.Map(document.getElementById('map'));
        const bounds = new google.maps.LatLngBounds();

        for (var i = 0; i < this.peerGeolocations.length; i++) {
            const peerGeo = this.peerGeolocations[i];

            const marker = new google.maps.Marker({
                position: peerGeo.position,
                title: peerGeo.address + '\n' + peerGeo.location,
                map: map
            });

            marker.addListener('click', () => {
                this.peerInfoVm.peerInfo = peerGeo;
            });

            const loc = new google.maps.LatLng(marker.position.lat(), marker.position.lng());

            bounds.extend(loc);
        }

        map.fitBounds(bounds);
        map.panToBounds(bounds);
    }
}
