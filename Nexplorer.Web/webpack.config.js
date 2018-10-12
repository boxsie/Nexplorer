const config = require('./webpack.common.config');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

const chunkFileNameJs = 'js/[name].js';
const chunkFileNameCss = 'css/[name].css';

config.plugins.push(new ExtractTextPlugin({
    filename: chunkFileNameCss
}));

config.plugins.push(new webpack.optimize.CommonsChunkPlugin({
    names: ['vendor'],
    filename: chunkFileNameJs,
    minChunks: Infinity
}));

module.exports = config;