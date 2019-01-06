const config = require('./webpack.common.config');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

const chunkFileNameJs = 'js/[name].[chunkhash].js';
const chunkFileNameCss = 'css/[name].[chunkhash].css';

config.output.chunkFilename = chunkFileNameJs;
config.output.filename = chunkFileNameJs;

config.mode = 'production';

config.module.rules.push({
    test: /\.js$/,
    exclude: /node_modules/,
    use: {
      loader: 'babel-loader'
    }
});

config.plugins.push(new webpack.DefinePlugin({
    'process.env': {
        NODE_ENV: '"production"'
    }
}));

config.plugins.push(new ExtractTextPlugin({
    filename: chunkFileNameCss
}));

config.resolve.alias['vue'] = 'vue/dist/vue.min.js';

module.exports = config;