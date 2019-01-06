const config = require('./webpack.common.config');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

const chunkFileNameJs = 'js/[name].js';
const chunkFileNameCss = 'css/[name].css';

config.mode = 'development';

config.plugins.push(new ExtractTextPlugin({
    filename: chunkFileNameCss
}));

config.resolve.alias['vue'] = 'vue/dist/vue.js';

module.exports = config;