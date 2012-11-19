
var connect = require('connect');
var cluster = require('cluster');
var numCPUs = require('os').cpus().length;
var canonicalRequestPatterns = require('./canonicalRequestPatterns.js');


if (cluster.isMaster) {
  // Fork workers.
  for (var i = 0; i < numCPUs; i++) {
    cluster.fork();
  }

  cluster.on('exit', function(worker, code, signal) {
    console.log('worker ' + worker.process.pid + ' died');
  });
} else {
  // Workers can share any TCP connection
  // In this case its a HTTP server
  var app = connect()
    .use(connect.logger('dev'))
    .use(canonicalRequestPatterns)
    .use(connect.static('public'))
    .listen(3000);
}


