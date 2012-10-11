
var connect = require('connect');
var canonicalRequestPatterns = require('./canonicalRequestPatterns.js');

var app = connect()
  .use(connect.logger('dev'))
  .use(canonicalRequestPatterns)
  .use(connect.static('public'))
  .listen(3000);

