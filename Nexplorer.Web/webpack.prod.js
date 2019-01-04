const merge = require('webpack-merge');
const common = require('./webpack.common.js');

const chunkFileNameJs = 'js/[name].[chunkhash].js';
const chunkFileNameCss = 'css/[name].[chunkhash].css';

module.exports = merge(common, {
    mode: 'production'
});