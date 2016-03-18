var path = require('path');
var context =  path.resolve(__dirname, 'wwwroot');
module.exports = {
    devtool: 'source-map',
    context: path.resolve(__dirname, 'wwwroot'),
    entry: {
        'bundle': context + '/js/app.jsx'
    },
    output: {
        path: context,
        filename: '[name].js',
        publicPath: '/'
    },

    module: {
        loaders: [
            {
                test: /\.jsx?$/,
                loader: 'babel',
                exclude: /node_modules/,
                query: {
                    presets: ['react', 'es2015']
                }
            }
        ]
    }
};
