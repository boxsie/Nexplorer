import Qrious from 'qrious';

export class EnableAuthenticaorViewModel {
    constructor(options) {

        console.log(options);
        this.qr = new Qrious({
            element: document.getElementById('qrCode'),
            value: options.authUri,
            level: 'H',
            size: 250
        });
        console.log(this.qr);
    }
}
