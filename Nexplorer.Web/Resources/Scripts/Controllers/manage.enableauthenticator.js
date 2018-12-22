import Qrious from 'qrious';

export class EnableAuthenticaorViewModel {
    constructor(options) {

        this.qr = new Qrious({
            element: document.getElementById('qrCode'),
            value: options.authUri,
            level: 'H',
            size: 250
        });
    }
}
