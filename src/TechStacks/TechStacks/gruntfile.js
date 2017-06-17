/// <reference path="~/node_modules/grunt/lib/grunt.js" />
/// <reference path="~/node_modules/grunt/lib/util/task.js"/>
/// <reference path="~/node_modules/gulp/index.js"/>
/// <reference path="~/node_modules/requirejs/require.js"/>

'use strict';

module.exports = function (grunt) {

    var fs = require('fs');
    var path = require('path');
    // include gulp
    var gulp = require('gulp');
    // include plug-ins
    var rimraf = require('gulp-rimraf');
    var uglify = require('gulp-uglify');
    var newer = require('gulp-newer');
    var useref = require('gulp-useref');
    var gulpif = require('gulp-if');
    var minifyCss = require('gulp-minify-css');
    var gulpReplace = require('gulp-replace');
    var webRoot = 'wwwroot/';
    var webBuildDir = grunt.option('serviceStackSettingsDir') || './wwwroot_build/';

    var configFile = 'config.json';
    var configDir = webBuildDir + 'publish/';
    var configPath = configDir + configFile;
    var appSettingsFile = 'appsettings.txt';
    var appSettingsDir = webBuildDir + 'deploy/';
    var appSettingsPath = appSettingsDir + appSettingsFile;

    function createConfigsIfMissing() {
        if (!fs.existsSync(configPath)) {
            if (!fs.existsSync(configDir)) {
                fs.mkdirSync(configDir);
            }
            fs.writeFileSync(configPath, JSON.stringify({
                "iisApp": "TechStacks",
                "serverAddress": "deploy-server.example.com",
                "userName": "{WebDeployUserName}",
                "password": "{WebDeployPassword}"
            }, null, 4));
        }
        if (!fs.existsSync(appSettingsPath)) {
            if (!fs.existsSync(appSettingsDir)) {
                fs.mkdirSync(appSettingsDir);
            }
            fs.writeFileSync(appSettingsPath,
                '# Release App Settings\r\nDebugMode false');
        }
    }

    // Deployment config
    createConfigsIfMissing();
    var config = require(configPath);

    // Project configuration.
    grunt.initConfig({
        karma: {
            unit: {
                configFile: 'tests/karma.conf.js',
                singleRun: true,
                browsers: ['PhantomJS'],
                logLevel: 'ERROR'
            }
        },
        msbuild: {
            release: {
                src: ['TechStacks.csproj'],
                options: {
                    projectConfiguration: 'Release',
                    targets: ['Clean', 'Rebuild'],
                    stdout: true,
                    version: 15,
                    maxCpuCount: 4,
                    buildParameters: {
                        WarningLevel: 2
                    },
                    verbosity: 'quiet'
                }
            }
        },
        nugetrestore: {
            restore: {
                src: 'packages.config',
                dest: '../../packages/'
            }
        },
        msdeploy: {
            pack: {
                options: {
                    verb: 'sync',
                    source: {
                        iisApp: path.resolve('./wwwroot')
                    },
                    dest: {
                        'package': path.resolve('./webdeploy.zip')
                    },
                    skip: {
                        Directory:'\\App_Data'
                    }
                }
            },
            push: {
                options: {
                    verb: 'sync',
                    allowUntrusted: 'true',
                    source: {
                        'package': path.resolve('./webdeploy.zip')
                    },
                    dest: {
                        iisApp: config.iisApp,
                        wmsvc: config.serverAddress,
                        UserName: config.userName,
                        Password: config.password
                    }
                }
            }
        },
        gulp: {
            'wwwroot-clean-dlls': function () {
                return gulp.src(webRoot + '/bin/**/*.*', { read: false })
                    .pipe(rimraf());
            },
            'wwwroot-copy-bin': function () {
                var binDest = webRoot + 'bin/';
                var dest = gulp.dest(binDest).on('end', function () {
                    grunt.log.ok('wwwroot-copy-bin finished.');
                });
                return gulp.src('./bin/**/*')
                    .pipe(newer(binDest))
                    .pipe(dest);
            },
            'wwwroot-copy-webconfig': function () {
                return gulp.src('./web.config')
                    .pipe(newer(webRoot))
                    .pipe(gulpReplace('<compilation debug="true" targetFramework="4.5">', '<compilation targetFramework="4.5">'))
                    .pipe(gulp.dest(webRoot));
            },
            'wwwroot-copy-asax': function () {
                return gulp.src('./Global.asax')
                    .pipe(newer(webRoot))
                    .pipe(gulp.dest(webRoot));
            },
            'wwwroot-clean-client-assets': function () {
                return gulp.src([
                    webRoot + '**/*.*',
                    '!./wwwroot/bin/**/*.*', //Don't delete dlls
                    '!./wwwroot/**/*.asax', //Don't delete asax
                    '!./wwwroot/**/*.config', //Don't delete config
                    '!./wwwroot/lib/**/*' //Don't delete existing 3rd party client libs
                ], { read: false })
                        .pipe(rimraf());
            },
            'wwwroot-copy-partials': function () {
                var partialsDest = webRoot + 'partials';
                return gulp.src('partials/**/*.html')
                    .pipe(newer(partialsDest))
                    .pipe(gulp.dest(partialsDest));
            },
            'wwwroot-copy-bootstrap-fonts': function () {
                return gulp.src('./bower_components/bootstrap/dist/fonts/*.*')
                    .pipe(newer(webRoot + 'lib/fonts/'))
                    .pipe(gulp.dest(webRoot + 'lib/fonts/'));
            },
            'wwwroot-copy-roboto-fonts': function () {
                return gulp.src('./bower_components/roboto-fontface/fonts/*.*')
                    .pipe(newer(webRoot + 'lib/css/fonts/'))
                    .pipe(gulp.dest(webRoot + 'lib/css/fonts/'));
            },
            'wwwroot-copy-chosen-resources': function () {
                return gulp.src('./bower_components/chosen/*.png')
                    .pipe(newer(webRoot + 'lib/css/'))
                    .pipe(gulp.dest(webRoot + 'lib/css/'));
            },
            'wwwroot-copy-images': function () {
                return gulp.src('./img/**/*.*')
                    .pipe(newer(webRoot + 'img/'))
                    .pipe(gulp.dest(webRoot + 'img/'));
            },
            'wwwroot-bundle': function () {
                var assets = useref.assets({ searchPath: './' });

                return gulp.src('./**/*.cshtml')
                    .pipe(assets)
                    .pipe(gulpif('*.js', uglify()))
                    .pipe(gulpif('*.css', minifyCss()))
                    .pipe(assets.restore())
                    .pipe(useref())
                    .pipe(gulp.dest(webRoot));
            },
            'wwwroot-copy-deploy-files': function () {
                return gulp.src(appSettingsDir + '*.*')
                    .pipe(newer(webRoot))
                    .pipe(gulp.dest(webRoot));
            }
        }
    });

    grunt.loadNpmTasks('grunt-karma');
    grunt.loadNpmTasks('ssvs-utils');
    grunt.loadNpmTasks('grunt-gulp');
    grunt.loadNpmTasks('grunt-msbuild');
    grunt.loadNpmTasks('grunt-nuget');

    grunt.registerTask('01-run-tests', ['karma']);
    grunt.registerTask('02-package-server', [
        'nugetrestore',
        'msbuild:release',
        'gulp:wwwroot-clean-dlls',
        'gulp:wwwroot-copy-bin',
        'gulp:wwwroot-copy-webconfig',
        'gulp:wwwroot-copy-asax',
        'gulp:wwwroot-copy-deploy-files'
    ]);
    grunt.registerTask('03-package-client', [
        'gulp:wwwroot-clean-client-assets',
        'gulp:wwwroot-copy-partials',
        'gulp:wwwroot-copy-bootstrap-fonts',
        'gulp:wwwroot-copy-roboto-fonts',
        'gulp:wwwroot-copy-chosen-resources',
        'gulp:wwwroot-copy-images',
        'gulp:wwwroot-copy-deploy-files',
        'gulp:wwwroot-bundle'
    ]);

    grunt.registerTask('build', ['02-package-server', '03-package-client']);

    grunt.registerTask('04-deploy-app', ['msdeploy:pack', 'msdeploy:push']);

    grunt.registerTask('package-and-deploy', ['02-package-server', '03-package-client', '04-deploy-app']);

    grunt.registerTask('default', ['karma', 'build']);
};