const config = require('./webpack.common.config');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

const chunkFileNameJs = 'js/[name].[chunkhash].js';
const chunkFileNameCss = 'css/[name].[chunkhash].css';

config.output.chunkFilename = chunkFileNameJs;
config.output.filename = chunkFileNameJs;

config.module.rules.push({
    test: /\.js$/,
    loader: 'babel-loader',
    query: {
        presets: ['es2015']
    }
});

config.module.resolve = {
    alias: {
        'vue': 'vue/dist/vue.min',
        'jquery.validation': 'jquery-validation/dist/jquery.validate.js'
    }
};

config.plugins.push(new webpack.DefinePlugin({
    'process.env': {
        NODE_ENV: '"production"'
    }
}));

config.plugins.push(new webpack.optimize.UglifyJsPlugin({
    compress: {
        warnings: false
    },
    output: {
        comments: false
    }
}));

config.plugins.push(new ExtractTextPlugin({
    filename: chunkFileNameCss
}));

config.plugins.push(new webpack.optimize.CommonsChunkPlugin({
    names: ['vendor'],
    filename: chunkFileNameJs,
    minChunks: Infinity
}));

module.exports = config;