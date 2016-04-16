# DemoGo

### Requirments
[.Net CLI](https://github.com/dotnet/cli)

### Setup
- Install DNX Core
- Run `dnu restore` on package
- Run `dnx run --server.url=http://localhost:8805`

### Api calls
##### On-demand parsing
`/ondemand?demoUrl={demoUrl}`
##### Scheduled parsing
`/schedule?demoUrl={demoUrl}&callbackUrl=http://localhost/demo-finished`
##### Fetch scheduled parsing progress
`/demo/{demoId}/progress`
##### Fetch complete scheduled parsing
`/demo/{demoId}`
