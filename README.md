# DemoGo

### Requirments
##### DNVM
`@powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"`

##### DNX 1.0.0-rc1-update1
`dnvm install 1.0.0-rc1-update1`

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
