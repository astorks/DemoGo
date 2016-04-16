# DemoGo

### Requirments
[.Net CLI](https://github.com/dotnet/cli)

### Setup
- Install DNX Core
- Run `dnu restore` on package
- Run `dnx run --server.url=http://localhost:8805`

### Api calls
##### On-demand parsing
`/ondemand?demoUrl=https://www.dropbox.com/s/1oixsz22cq8b9hv/003124717232588849309_0984015001.dem?dl=1`
##### Scheduled parsing
`/schedule?demoUrl=https://www.dropbox.com/s/1oixsz22cq8b9hv/003124717232588849309_0984015001.dem?dl=1&callbackUrl=http://localhost/demo-finished`
##### Fetch completed scheduled parsing
`/demo/00000000-0000-0000-0000-000000000000`
