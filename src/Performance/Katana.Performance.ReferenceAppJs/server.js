
var connect = require('connect');
var canonicalRequestPatterns = require('./canonicalRequestPatterns.js');

var app = connect()
  .use(connect.logger('dev'))
  .use(connect.static('public'))
  .use(canonicalRequestPatterns)
  .listen(3000);

