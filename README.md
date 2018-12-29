# CSharp API Server

A minimal, but non trivial, HTTP web server intended for API development.

## Backstory

For a recent project, I needed to expose some logic libraries to HTTP actions while in a Mac environment. I found the
framework options to be overly cumbersome for my purposes. The alternative that seems promoted by the community is to
provide the HTTP listening functionality, then handle various details within the route definitions.

I find spreading interfacing logic in a code base to be a poor design choice, and thus threw this project together.

## What is it?

This provides a standardized way to expose routes in a single location and associate those routes with corresponding
handlers. Each handler receives an interface to the request and response object that allow the route handler to remain
slim and focused.

## What it isn't

finished.

The work motivating this project is complete, thus development on this is stopping until there is another reason to pick
 it up.

- There is currently no provision for hosting static files, which was intended to be a route level definition
interpreted after route handlers
- Standard responses should be drafted for common response codes. This should likely read an ENV variable to control the
 detail provided in the standard responses so that development and QA environments can operate in a less "black box"
 than edge and production environments.
- Request and Response should allow for headers to be configured and set
- Route pre-handler policies added such that request can be evaluated and rejected based on specific criteria prior to
the request handler being invoked. This is to maintain concentration of concerns and keep each portion slim.

I would love to continue this work, but there are more pressing concerns.


-
