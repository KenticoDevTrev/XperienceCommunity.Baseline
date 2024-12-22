const configs = require('../taskconfigs').configs;
const env = require('../taskconfigs').environment;
const webpack = require('webpack');
const path = require('path');
const { getEnvironmentConfigs, removeExtension } = require('./gulpTaskHelpers');

function typescript() {
    var tsConfigs = configs.typescript;
    var envConfigs = getEnvironmentConfigs(tsConfigs);
    return Promise.all(envConfigs.map(tsConfig => webpackResolve(tsConfig)));
}

function react() {
    var reactConfigs = configs.react;
    var envConfigs = getEnvironmentConfigs(reactConfigs);
    return Promise.all(envConfigs.map(tsConfig => webpackResolve(tsConfig)));
}

// Could not get webpack to work as a readable stream for merge, so have to send an array of configs
function webpackResolve(tsConfig){
    return new Promise(function(resolve, reject) {
        let config = getWebpackConfig(tsConfig);    
        webpack(config, (err, stats) => {
            if (err) {
                return reject(err);
            }
            if (stats.hasErrors()) {
                return reject(new Error(stats.compilation.errors.join('\n')))
            }
            resolve();
        });
    });
}

function getWebpackConfig(config) {
    // Link to webpack.config already given.
    if(config.manualWebpackConfig){
        return require(config.manualWebpackConfig);
    }
    // Generate webpack.config from configuration
    var webpackConfig = {};
    webpackConfig.entry = path.resolve(config.entry);
    webpackConfig.mode = env.isDev ? "development" : "production";
    webpackConfig.output = {
        filename: (config.minify ? removeExtension(config.fileName, ".js")+".min.js" : config.fileName),
        path: path.resolve(config.dest)
    };
    var sourceMap = (env.isDev || config.includeMapOnProduction);
    if(sourceMap) {
        webpackConfig.devtool = "source-map";
        webpackConfig.module = {
            rules: [
                {
                    test: /\.ts(x?)$/,
                    exclude: /node_modules/,
                    use: [
                        {
                            loader: "ts-loader"
                        }
                    ]
                },
    
                // All output '.js' files will have any sourcemaps re-processed by 'source-map-loader'. 
                {
                    enforce: "pre",
                    test: /\.js$/,
                    loader: "source-map-loader"
                }
            ]
        }
    }
    webpackConfig.resolve = { extensions: [".ts", ".tsx", ".js"]};
    
    webpackConfig.optimization = {
        minimize: config.minify
    }
    
    return webpackConfig;
}


module.exports = {
    typescript, react, webpackResolve
}
