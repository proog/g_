var gulp = require('gulp');
var uglify = require('gulp-uglify');
var concat = require('gulp-concat');

var input = './public/js/**/!(*.min).js';
var output = './public/js/g.min.js';

function js(productionMode) {
    var src = gulp.src(input);

    if(productionMode)
        src = src.pipe(uglify());

    src.on('error', function(error) {
        console.log(error);
        this.emit('end');
    })
        .pipe(concat(output))
        .pipe(gulp.dest('./'));
}
gulp.task('js', function() {
    js(true);
});

gulp.task('default', function() {
    js(true);
    gulp.watch(input, ['js']);
});