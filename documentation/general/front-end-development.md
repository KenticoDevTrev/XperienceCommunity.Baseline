# Front End Development

Pretty much every site leverages Javascript, Css, and Images.  These are simple to add and use.

However, not every site uses higher performing tools and systems such as bundling, minification, gzip compression, webp, Typescript, React, Sass, etc.  Not to mention source mapping and watch handling for development.

The Baseline starting site comes with our [FrontEndDev](../starting-site/xbyk/FrontEndDev/) system.  This takes all these advanced operations and allows you to leverage them simply by defining your [taskconfigs.js](../starting-site/xbyk/FrontEndDev/taskconfigs.js).


# Node.js + Gulp + WebPack
The tools to accomplish this are Node.js as the framework (must install on your computer if you don't have it), Webpack for the Typescript/React compilation, and GULP to handle LESS/SASS, combine, minify, and handle the rendered assets.  While some of these systems are older (Gulp) and may not be the fastest thing around (WebPack), this library still performs these operations well.

## Gulpfile.js and Tasks
The system operates on a group of [tasks](../starting-site/xbyk/FrontEndDev/tasks/), which all:

1. Read the configuration of the taskconfig.js
2. Perform their action

Each also contains a "Stream" option so when a watched file is updated, it can re-execute

The `gulpfile.js` uses these tasks and hooks up the proper chains (when executed through the `npm run` commands or when a watched file is touched).

The beauty of this system is it handles setting up watches on each of your configurations automatically, and executes the proper operations and dependent operations.

## Tasks and Execution Order

Here's the given tasks and what they do, and the order they execute in.  Note that while these operate in order, individually they perform parallel operations on the different items within themselves (ex if you have 2 configurations for build:scss, they will both run in parallel)

- **build:precopy**
  - Copies resources (either node_modules or even from other areas of your project)
  - If your theme has it's own build pipeline and it's not easily compatible with the FrontEndDev system, you can always use this to copy the compiled resources into the FrontEndDev for bundling and such
- **CSS Handling** 
  - **build:scss** - Renders Sass (easy to adjust to for Less) to individual CSS files
  - **build:cssRaw** - Copies individual CSS (handles minification, gzip, and map)
  - **build.cssBundle** - Bundles css files together (handles minification, gzip, and map)
- **Javascript Handling**
  - **build:typescript** - Renders Typescript files into Javascript (handles minification and map)
  - **build:react** - Renders React files into Javascript (handles minification and map)
  - **build.jsRaw** - Copies individual Js (handles minification, gzip, and map)
  - **build.jsBundle** - Bundles js files together (handles minification, gzip, and map)
- **Image Handling**
  - **build:images** - Creates optimized and webp versions of images
- **build:copy** - Copies files, usually used to copy them to the actual MVC site wwwroot

## Watch

There is an `npm run watch` command which will scan through all of your configurations, use the `paths` plus any additional `watchPaths`, and any changes on those will automatically execute the operations (which will most likely trigger any other dependent operations due to their watch paths)

``` javascript

typescript: [
        { 
            environments: ['dev', 'production'],
            // manualWebpackConfig: "../typescript/Helper/webpack.config.js",
            entry: "./typescript/Helper/index.ts",
            dest: "./js/bundles/footer-bundle/generated",
            fileName: "helpers.js",
            includeMapOnProduction: true,
            minify: false,
			watchPaths: ['./typescript/Helpers/**/*.ts']
        }
    ],
    jsbundles: [
        {
			environments: ['dev', 'production'],
			paths: [
				// can also do map files of any raw of these, which copy over only on dev
				'./js/bundles/footer-bundle/**/*.js'
			],
			base: { base: "./js/bundles/footer-bundle" },
			dest: "../MVC/wwwroot/js/bundles/footer-bundle",
			bundleName: "footer-bundle.js",
			bundleOnDev: true,
			minify: true, 
			gzip: true,
			obfuscateOnProduction: false, 
			includeMapOnProduction: true
		}

```

If any change happens to the entry or watch paths, it will automatically run:

1. **build:typescript** (adding the file to `/js/bundles/footer-bundle/generated`)
2. **build:jsbundles** (bundling and adding the footer-bundle.js to the MVC folder)

## Environments
There are 2 environments you can configure all the configs with, `dev` and `production`

`dev` operations are run with any `npm run build` or `npm run build:____`, or `npm run watch`

`production` is only ran with `npm run production`. 

You can use this if you wish to optimize and limit un-needed operations during developing.

## Build Deployments
If using Azure DevOps Pipelines, add this to your MVC Build pipeline to install and run the 

command:
```
- task: Npm@1
  displayName: NPM Install
  inputs:
    command: 'install'
    workingDir: '$(System.DefaultWorkingDirectory)/FrontEndDev'
- task: Npm@1
  displayName: NPM run Gulp build
  inputs:
    command: 'custom'
    workingDir: '$(System.DefaultWorkingDirectory)/FrontEndDev'
    customCommand: 'run production'
```


## WebPack Configs and tsconfig.json
For configuring Typescript / React, we leveraged WebPack.  Typescript usually requires a tsconfig.json file to tell your IDE (such as VSCode or Visual Studio) where to look for Typescript definitions and such.  The Baseline version is set to look for any Typescript files in the FrontEndDev/typescript and FrontEndDev/react folders.

The Webpack compilation normally takes a webpack.config.js file that tells Webpack how to package up the Typescript/React into a single javascript file.  The `taskconfigs.js`'s react and typescript array of options handles ths largely for you.  However, you still can use the `manualWebpackConfig` property to point to a webpack file if you want full control (such as combining multiple entry points).

# Tailwind or Other Operations
Keep in mind that not everything is compatible with this by default.  If you use things like Tailwind or PostCss, you often have to do these separately prior to running the bundling and other operations.

THis can be easy enough to do, for example, I enabled Tailwind on my operation simply by modifying my package.json scripts:

```javascript
"scripts": {
    "production": "npm run build:tailwindcss && cross-env NODE_ENV=production gulp --color --gulpfile \"Gulpfile.js\" build",
    "build": "npm run build:tailwindcss && cross-env NODE_ENV=development gulp --color --gulpfile \"Gulpfile.js\" build",
    "watch": "cross-env NODE_ENV=development gulp --color --gulpfile \"Gulpfile.js\" watch",
    ...
    "build:tailwindcss": "npx tailwindcss -i ./tailwind/site.css -o ./css/bundles/main-bundle/generated/styles.css",
    "build:tailwindcss:watch": "npx tailwindcss -i ./tailwind/site.css -o ./css/bundles/main-bundle/generated/styles.css --watch"
  }
```

This may require for Watches to have two terminals running watches.

Additionally, I ran into this myself where my Theme had their own React / CSS processing, so I kept those seperate and use the preCopy to pull them in, you can do similar.

## Typescript Build Error
Some users have reported seeing typescript errors on build.  If you see this, please install the [`Microsoft.TypeScript.MSBuild`](https://www.nuget.org/packages/Microsoft.TypeScript.MSBuild/) nuget package on the MVC Project.
 