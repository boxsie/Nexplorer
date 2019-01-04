const merge = require('webpack-merge');
const common = require('./webpack.common.js');

const chunkFileNameJs = 'js/[name].js';
const chunkFileNameCss = 'css/[name].css';

module.exports = merge(common, {
    mode: 'development'
});