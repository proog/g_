var gulp = require('gulp');
var uglify = require('gulp-uglify');
var concat = require('gulp-concat');

var input = './public/js/**/!(*.min).js';
var output = './public/js/g.min.js';

function js() {
    gulp.src(input)
        .pipe(uglify())
        .on('error', function(error) {
            console.log(error);
            this.emit('end');
        })
        .pipe(concat(output))
        .pipe(gulp.dest('./'));
}
gulp.task('js', js);

gulp.task('default', function() {
    js();
    gulp.watch(input, ['js']);
});