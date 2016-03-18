var output = './wwwroot/';

module.exports = {
    devtool: 'source-map',
    entry: {
        'bundle': './js/app.jsx'
    },

    output: {
        path: output,
        filename: '[name].js'
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
