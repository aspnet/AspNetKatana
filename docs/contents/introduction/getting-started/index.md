---
title: Getting started
order: 1
template: section.jade
---

- resp.write("hello world")
  - from webapp (learn class Startup, Request, Response)
    - via microsoft.owin.host.systemweb
  - from exe (learn WebApplication.Start)
    - with microsoft.owin.hosting
    - via microsoft.owin.host.httplistener
