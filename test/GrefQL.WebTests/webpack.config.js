var output = './wwwroot/';

module.exports = {
  entry: {
      'bundle': './wwwroot/app.jsx'
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
  },

};
