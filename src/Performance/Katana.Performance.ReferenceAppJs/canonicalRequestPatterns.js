var url = require('url');

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
		res.write('2kb ascii []');
		res.end();
	});

