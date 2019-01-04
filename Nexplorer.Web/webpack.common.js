const path = require('path');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const AssetsPlugin = require('assets-webpack-plugin');

const webRoot = path.resolve(__dirname, 'wwwroot');
const baseScriptsPath = './Resources/Scripts/';
const controllerScriptsPath = './Resources/Scripts/Controllers/';

const filenameJs = 'js/[name].js';
const filenameCss = 'css/[name].css';
const filenameCssChunk = 'css/[id].css';

module.exports = {
    entry: {
        'vendor': ['babel-polyfill', 'tether', 'bootstrap', 'vue', '@aspnet/signalr', 'moment'],
        'validate': ['jquery-validation', 'jquery-validation-unobtrusive'],
        'layout': baseScriptsPath + 'layout.js',
        'home.index': controllerScriptsPath + 'home.index.js',
        'blocks.index': controllerScriptsPath + 'blocks.index.js',
        'blocks.block': controllerScriptsPath + 'blocks.block.js',
        'transactions.index': controllerScriptsPath + 'transactions.index.js',
        'addresses.index': controllerScriptsPath + 'addresses.index.js',
        'addresses.address': controllerScriptsPath + 'addresses.address.js',
        'network.index': controllerScriptsPath + 'network.index.js',
        'mining.index': controllerScriptsPath + 'mining.index.js',
        'favourites.index': controllerScriptsPath + 'favourites.index.js',
        'admin.index': controllerScriptsPath + 'admin.index.js',
        'manage.enableauthenticator': controllerScriptsPath + 'manage.enableauthenticator.js'
    },
    output: {
        path: webRoot,
        publicPath: '../',
        chunkFilename: filenameJs,
        filename: filenameJs,
        library: 'nexplorer'
    },
    optimization: {
        runtimeChunk: 'single'
    },
    module: {
        rules: [
            {
                test: /\.m?js$/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env'],
                        plugins: ['@babel/plugin-transform-runtime']
                    }
                }
            },
            {
                test: /\.css$/,
                exclude: /(node_modules|bower_components)/,
                use: [
                    { loader: 'css-loader', options: { } },
                    { loader: 'postcss-loader', options: {} },
                    { loader: MiniCssExtractPlugin.loader }
                ]
            },
            {
                test: /\.sass$|\.scss$/,
                use: [
                    { loader: 'css-loader', options: { } },
                    {
                        loader: 'postcss-loader',
                        options: {
                            plugins: function () {
                                return [
                                    require('precss'),
                                    require('autoprefixer')
                                ];
                            }
                        }
                    },
                    { loader: 'sass-loader' },
                    { loader: MiniCssExtractPlugin.loader }
                ]
            },
            {
                test: /\.html$/,
                use: 'html-loader'
            },
            {
                test: /\.woff2?(\?v=[0-9]\.[0-9]\.[0-9])?$/,
                use: 'url-loader'
            },
            {
                test: /\.(jpg|jpeg|gif|png)$/,
                use: 'url-loader?limit=1024&name=img/[name].[ext]'
            },
            {
                test: /\.(woff|woff2|eot|ttf|svg)$/,
                use: 'url-loader?limit=1024&name=font/[name].[ext]'
            },
            {
                test: /bootstrap\/dist\/js\/umd\//,
                use: 'imports-loader?jQuery=jquery'
            },
            {
                test: /font-awesome\.config\.js/,
                use: [
                    { loader: 'style-loader' },
                    { loader: 'font-awesome-loader' }
                ]
            }
        ]
    },
    resolve: {
        alias: {
            'vue': 'vue/dist/vue.js',
            'jquery.validation': 'jquery-validation/dist/jquery.validate.js'
        }
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: filenameCss,
            chunkFilename: filenameCssChunk
        }),
        new AssetsPlugin({
            filename: 'App_Data/webpack.assets.json',
            path: __dirname,
            prettyPrint: true
        })
    ]
};