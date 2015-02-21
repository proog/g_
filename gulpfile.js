var gulp = require('gulp');
var uglify = require('gulp-uglify');
var concat = require('gulp-concat');

var input = './public/js/**/!(*.min).js';
var output = './public/js/g.min.js';

gulp.task('js', function() {
    gulp.src(input)
        .pipe(uglify())
        .pipe(concat(output))
        .pipe(gulp.dest('./'));
});

gulp.task('default', function() {
    gulp.run('js');
    gulp.watch(input, ['js']);
});