# Archived Links



Based on a url saves the html page, its associated images, css and pdf files. The services provides a url that you can use instead of the original resource's url: here the service checks if the original source is available and either redirect to there or returns a previously saved snapshot.

Make sure to serve the static files of the saved snapshots from a different domain (not a subdomain, but a completely different domain) than that of the site using the module: otherwise the snapshots could run harmful scripts.