var path = require('path');

module.exports = {
    devtool: 'source-map',
    entry: path.resolve(__dirname, 'js', 'app.js'),
    output: {
        path: path.resolve(__dirname, 'wwwroot', 'js'),
        filename: 'app.js'
    },

    module: {
        loaders: [{
            test: /\.jsx?$/,
            loader: 'babel',
            exclude: /node_modules/
        }]
    }
};