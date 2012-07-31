using System;
using System.Collections.Generic;
using System.Linq;
using Owin;

namespace Gate.Middleware
{
    internal static partial class ShowExceptions
    {
        static void ErrorPage(CallParameters call, Exception ex, Action<string> write)
        {
            // XXX test this more thoroughly on mono, it shouldn't throw NullRef,
            // but rather, branch gracefully if something comes up null
            try
            {

                var request = new Request(call);
                var path = request.PathBase + request.Path;
                var frames = StackFrames(ex);
                var first = frames.FirstOrDefault();
                var location = "";
                if (ex.TargetSite != null && ex.TargetSite.DeclaringType != null)
                {
                    location = ex.TargetSite.DeclaringType.FullName + "." + ex.TargetSite.Name;
                }
                else if (first != null)
                {
                    location = first.Function;
                }


                // adapted from Django <djangoproject.com>
                // Copyright (c) 2005, the Lawrence Journal-World
                // Used under the modified BSD license:
                // http://www.xfree86.org/3.3.6/COPYRIGHT2.html#5
                write(@"
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
<html lang=""en"">
<head>
  <meta http-equiv=""content-type"" content=""text/html; charset=utf-8"" />
  <meta name=""robots"" content=""NONE,NOARCHIVE"" />
  <title>");
                write(h(ex.GetType().Name));
                write(@" at ");
                write(h(path));
                write(@"</title>
  <style type=""text/css"">
    html * { padding:0; margin:0; }
    body * { padding:10px 20px; }
    body * * { padding:0; }
    body { font:small sans-serif; }
    body>div { border-bottom:1px solid #ddd; }
    h1 { font-weight:normal; }
    h2 { margin-bottom:.8em; }
    h2 span { font-size:80%; color:#666; font-weight:normal; }
    h3 { margin:1em 0 .5em 0; }
    h4 { margin:0 0 .5em 0; font-weight: normal; }
    table {
        border:1px solid #ccc; border-collapse: collapse; background:white; }
    tbody td, tbody th { vertical-align:top; padding:2px 3px; }
    thead th {
        padding:1px 6px 1px 3px; background:#fefefe; text-align:left;
        font-weight:normal; font-size:11px; border:1px solid #ddd; }
    tbody th { text-align:right; color:#666; padding-right:.5em; }
    table.vars { margin:5px 0 2px 40px; }
    table.vars td, table.req td { font-family:monospace; }
    table td.code { width:100%;}
    table td.code div { overflow:hidden; }
    table.source th { color:#666; }
    table.source td {
        font-family:monospace; white-space:pre; border-bottom:1px solid #eee; }
    ul.traceback { list-style-type:none; }
    ul.traceback li.frame { margin-bottom:1em; }
    div.context { margin: 10px 0; }
    div.context ol {
        padding-left:30px; margin:0 10px; list-style-position: inside; }
    div.context ol li {
        font-family:monospace; white-space:pre; color:#666; cursor:pointer; }
    div.context ol.context-line li { color:black; background-color:#ccc; }
    div.context ol.context-line li span { float: right; }
    div.commands { margin-left: 40px; }
    div.commands a { color:black; text-decoration:none; }
    #summary { background: #ffc; }
    #summary h2 { font-weight: normal; color: #666; }
    #summary ul#quicklinks { list-style-type: none; margin-bottom: 2em; }
    #summary ul#quicklinks li { float: left; padding: 0 1em; }
    #summary ul#quicklinks>li+li { border-left: 1px #666 solid; }
    #explanation { background:#eee; }
    #template, #template-not-exist { background:#f6f6f6; }
    #template-not-exist ul { margin: 0 0 0 20px; }
    #traceback { background:#eee; }
    #requestinfo { background:#f6f6f6; padding-left:120px; }
    #summary table { border:none; background:transparent; }
    #requestinfo h2, #requestinfo h3 { position:relative; margin-left:-100px; }
    #requestinfo h3 { margin-bottom:-1em; }
    .error { background: #ffc; }
    .specific { color:#cc3300; font-weight:bold; }
  </style>
  <script type=""text/javascript"">
  //<!--
    function getElementsByClassName(oElm, strTagName, strClassName){
        // Written by Jonathan Snook, http://www.snook.ca/jon;
        // Add-ons by Robert Nyman, http://www.robertnyman.com
        var arrElements = (strTagName == ""*"" && document.all)? document.all :
        oElm.getElementsByTagName(strTagName);
        var arrReturnElements = new Array();
        strClassName = strClassName.replace(/\-/g, ""\\-"");
        var oRegExp = new RegExp(""(^|\\s)"" + strClassName + ""(\\s|$$)"");
        var oElement;
        for(var i=0; i<arrElements.length; i++){
            oElement = arrElements[i];
            if(oRegExp.test(oElement.className)){
                arrReturnElements.push(oElement);
            }
        }
        return (arrReturnElements)
    }
    function hideAll(elems) {
      for (var e = 0; e < elems.length; e++) {
        elems[e].style.display = 'none';
      }
    }
    window.onload = function() {
      hideAll(getElementsByClassName(document, 'table', 'vars'));
      hideAll(getElementsByClassName(document, 'ol', 'pre-context'));
      hideAll(getElementsByClassName(document, 'ol', 'post-context'));
    }
    function toggle() {
      for (var i = 0; i < arguments.length; i++) {
        var e = document.getElementById(arguments[i]);
        if (e) {
          e.style.display = e.style.display == 'none' ? 'block' : 'none';
        }
      }
      return false;
    }
    function varToggle(link, id) {
      toggle('v' + id);
      var s = link.getElementsByTagName('span')[0];
      var uarr = String.fromCharCode(0x25b6);
      var darr = String.fromCharCode(0x25bc);
      s.innerHTML = s.innerHTML == uarr ? darr : uarr;
      return false;
    }
    //-->
  </script>
</head>
<body>

<div id=""summary"">
  <h1>");
                write(h(ex.GetType().Name));
                write(@" at ");
                write(h(path));
                write(@"</h1>
  <h2>");
                write(h(ex.Message));
                write(@"</h2>
  <table><tr>
    <th>.NET</th>
    <td>
");
                if (!string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(first.File))
                {
                    write(@"
      <code>");
                    write(h(location));
                    write(@"</code>: in <code>");
                    write(h(first.File));
                    write(@"</code>, line ");
                    write(h(first.Line));
                    write(@"
");
                }
                else if (!string.IsNullOrEmpty(location))
                {
                    write(@"
      <code>");
                    write(h(location));
                    write(@"</code>
");
                }
                else
                {
                    write(@"
      unknown location
");
                }
                write(@"
    </td>
  </tr><tr>
    <th>Web</th>
    <td><code>");
                write(h(request.Method));
                write(@" ");
                write(h(request.Host + path));
                write(@" </code></td>
  </tr></table>

  <h3>Jump to:</h3>
  <ul id=""quicklinks"">
    <li><a href=""#get-info"">GET</a></li>
    <li><a href=""#post-info"">POST</a></li>
    <li><a href=""#cookie-info"">Cookies</a></li>
    <li><a href=""#header-info"">Headers</a></li>
    <li><a href=""#env-info"">ENV</a></li>
  </ul>
</div>

<div id=""traceback"">
  <h2>Traceback <span>(innermost first)</span></h2>
  <ul class=""traceback"">
");
                foreach (var frameIndex in frames.Select((frame, index) => Tuple.Create(frame, index)))
                {
                    var frame = frameIndex.Item1;
                    var index = frameIndex.Item2;

                    write(@"
      <li class=""frame"">
        <code>");
                    write(h(frame.File));
                    write(@"</code>: in <code>");
                    write(h(frame.Function));
                    write(@"</code>

          ");
                    if (frame.ContextCode != null)
                    {
                        write(@"
          <div class=""context"" id=""c{%=h frame.object_id %}"">
              ");
                        if (frame.PreContextCode != null)
                        {
                            write(@"
              <ol start=""");
                            write(h(frame.PreContextLine + 1));
                            write(@""" class=""pre-context"" id=""pre");
                            write(h(index));
                            write(@""">
                ");
                            foreach (var line in frame.PreContextCode)
                            {
                                write(@"
                <li onclick=""toggle('pre");
                                write(h(index));
                                write(@"', 'post");
                                write(h(index));
                                write(@"')"">");
                                write(h(line));
                                write(@"</li>
                ");
                            }
                            write(@"
              </ol>
              ");
                        }
                        write(@"

            <ol start=""");
                        write(h(frame.Line));
                        write(@""" class=""context-line"">
              <li onclick=""toggle('pre");
                        write(h(index));
                        write(@"', 'post");
                        write(h(index));
                        write(@"')"">");
                        write(h(frame.ContextCode));
                        write(@"<span>...</span></li></ol>

              ");
                        if (frame.PostContextCode != null)
                        {
                            write(@"
              <ol start='");
                            write(h(frame.Line + 1));
                            write(@"' class=""post-context"" id=""post");
                            write(h(index));
                            write(@""">
                ");
                            foreach (var line in frame.PostContextCode)
                            {
                                write(@"
                <li onclick=""toggle('pre");
                                write(h(index));
                                write(@"', 'post");
                                write(h(index));
                                write(@"')"">");
                                write(h(line));
                                write(@"</li>
                ");
                            }
                            write(@"
              </ol>
              ");
                        }
                        write(@"
          </div>
          ");
                    }
                    write(@"
      </li>
");
                }
                write(@"
  </ul>
</div>

<div id=""requestinfo"">
  <h2>Request information</h2>

  <h3 id=""get-info"">GET</h3>
  ");
                if (request.Query.Any())
                {
                    write(@"
    <table class=""req"">
      <thead>
        <tr>
          <th>Variable</th>
          <th>Value</th>
        </tr>
      </thead>
      <tbody>
          ");
                    foreach (var kv in request.Query.OrderBy(kv => kv.Key))
                    {
                        write(@"
          <tr>
            <td>");
                        write(h(kv.Key));
                        write(@"</td>
            <td class=""code""><div>");
                        write(h(kv.Value));
                        write(@"</div></td>
          </tr>
          ");
                    }
                    write(@"
      </tbody>
    </table>
  ");
                }
                else
                {
                    write(@"
    <p>No GET data.</p>
  ");
                }
                write(@"

  <h3 id=""post-info"">POST</h3>
  ");

                var form = request.ReadForm();
                if (form.Any())
                {
                    write(@"
    <table class=""req"">
      <thead>
        <tr>
          <th>Variable</th>
          <th>Value</th>
        </tr>
      </thead>
      <tbody>
          ");
                    foreach (var kv in form.OrderBy(kv => kv.Key))
                    {
                        write(@"
          <tr>
            <td>");
                        write(h(kv.Key));
                        write(@"</td>
            <td class=""code""><div>");
                        write(h(kv.Value));
                        write(@"</div></td>
          </tr>
          ");
                    }
                    write(@"
      </tbody>
    </table>
  ");
                }
                else
                {
                    write(@"
    <p>No POST data.</p>
  ");
                }
                write(@"


  <h3 id=""cookie-info"">COOKIES</h3>
  ");
                if (request.Cookies.Any())
                {
                    write(@"
    <table class=""req"">
      <thead>
        <tr>
          <th>Variable</th>
          <th>Value</th>
        </tr>
      </thead>
      <tbody>
        ");
                    foreach (var kv in request.Cookies.OrderBy(kv => kv.Key))
                    {
                        write(@"
          <tr>
            <td>");
                        write(h(kv.Key));
                        write(@"</td>
            <td class=""code""><div>");
                        write(h(kv.Value));
                        write(@"</div></td>
          </tr>
        ");
                    }
                    write(@"
      </tbody>
    </table>
  ");
                }
                else
                {
                    write(@"
    <p>No cookie data.</p>
  ");
                }
                write(@"

  <h3 id=""cookie-info"">HEADERS</h3>
  ");
                if (request.Headers.Any())
                {
                    write(@"
    <table class=""req"">
      <thead>
        <tr>
          <th>Variable</th>
          <th>Value</th>
        </tr>
      </thead>
      <tbody>
        ");
                    foreach (var kv in request.Headers.OrderBy(kv => kv.Key))
                    {
                        write(@"
          <tr>
            <td nowrap=""nowrap"">");
                        write(h(kv.Key));
                        write(@"</td>
            <td class=""code""><div>");
                        foreach (var v in kv.Value)
                        {
                            write(h(v));
                            write(@"<br/>");
                        }
                        write(@"</div></td>
          </tr>
        ");
                    }
                    write(@"
      </tbody>
    </table>
  ");
                }
                else
                {
                    write(@"
    <p>No header data.</p>
  ");
                }
                write(@"

  <h3 id=""env-info"">OWIN ENV</h3>
    <table class=""req"">
      <thead>
        <tr>
          <th>Variable</th>
          <th>Value</th>
        </tr>
      </thead>
      <tbody>
        ");
                foreach (var kv in call.Environment.OrderBy(kv => kv.Key))
                {
                    write(@"
          <tr>
            <td>");
                    write(h(kv.Key));
                    write(@"</td>
            <td class=""code""><div>");
                    write(h(kv.Value));
                    write(@"</div></td>
          </tr>
          ");
                }
                write(@"
      </tbody>
    </table>

</div>

<div id=""explanation"">
  <p>
    You're seeing this error because you use <code>Gate.Helpers.ShowExceptions</code>.
  </p>
</div>

</body>
</html>
            ");
            }
            catch
            {
                return;
            }
        }
    }
}

