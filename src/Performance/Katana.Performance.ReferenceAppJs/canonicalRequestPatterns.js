var url = require('url');
var fs = require('fs');

var paths = {};

module.exports = function(req, res, next) {

  var r = url.parse(req.url);

  var item = paths[r.pathname];
  if (item)
  {
  	item.action(req, res, next);
  	return;
  }

  next();
};

paths.add = function(path, description, action) {
	var item = this[path] = {};
    return {
    	description: function(description) {
	    	item.description = description;
	    	return this;
	    },
	    action: function(action) {
	    	item.action = action;
	    	return this;
	    }
    };
};

function alphabetCRLF(length)
{
	var alphabet = 'abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ\r\n';
	var result = alphabet;
	while (result.length < length)
	{
		result = result + alphabet;
	}
	return result.substring(0, length);
}

var _2KAlphabet = alphabetCRLF(2 << 10);

paths.add('/')
	.action(function(req,res) {
		res.setHeader('Content-Type', 'text/html');
		res.write('<ul>');
		for (var path in paths) {
			var item = paths[path];
			if (item.description) {
				res.write('<li><a href="');
				res.write(path);
				res.write('"/>');
				res.write(path);
				res.write("</a> ");
				res.write(item.description);
				res.write('</li>');
			}
		}
		res.write('</ul>');
		res.end();
	});

paths.add('/small-immediate-syncwrite')
	.description('Return 2kb ascii byte[] in a sync Write')
	.action(function(req, res, next) {
		res.setHeader('Content-Type', 'text/plain');
		res.write(_2KAlphabet);
		res.end();
	});

paths.add('/large-immediate-syncwrite')
	.description('Return 1mb ascii byte[] in 2kb sync Write')
	.action(function(req, res, next) {
		res.setHeader('Content-Type', 'text/plain');
        for (var loop = 0; loop != (1 << 20)/(2 << 10); ++loop)
        {
			res.write(_2KAlphabet);
        }
		res.end();
	});

paths.add('/large-immediate-asyncwrite')
	.description('Return 1mb ascii byte[] in 2kb await WriteAsync')
	.action(function(req, res, next) {
		res.setHeader('Content-Type', 'text/plain');

		var loop = 0;
		
		var go = function() {
			while (loop != (1 << 20)/(2 << 10)) {
				++loop;
				if (!res.write(_2KAlphabet)) {
					return;
				}
			}
	        res.end();
		};

		res.on('drain', go);
		go();
	});

paths.add('/small-longpolling-syncwrite')
	.description('Return 2kb sync Write after 12sec await delay')
	.action(function(req, res, next) {
		setTimeout(function(){
			res.setHeader('Content-Type', 'text/plain');
			res.write(_2KAlphabet);
			res.end();
		}, 12000)
	});


paths.add('/small-staticfile')
	.description('Sending 2k static file with server accelleration extension')
	.action(function(req, res, next) {
		res.setHeader('Content-Type', 'text/plain');
		var file = fs.createReadStream('public/small.txt');
		file.pipe(res);
	});

paths.add('/large-staticfile')
	.description('Sending 1m static file with server accelleration extension')
	.action(function(req, res, next) {
		res.setHeader('Content-Type', 'text/plain');
		var file = fs.createReadStream('public/large.txt');
		file.pipe(res);
	});

