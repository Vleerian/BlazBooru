## Quickstart

Clone this repository and run `dotnet run` in BlazBooruAPI to start up the API service, then `dotnet run` in BlazBooru to start up the clientside service

## Summary

BlazBooruCommon is the shared classlib contianing the data models the site uses.
BlazBooruAPI is the backend portion of BlazBooru which manages the BlazBooru database, and serves post data and files.
BlazBooru is the frontend portion which serves the website itself.

Once BlazBooru has reached feature completion, the two projects will be merged into a single program.

## Installation

BlazBooruAPI will run on any system with support for dotnet 5 or newer. Ensure that the targeted framework in `BlazBooruAPI.csproj`, `BlazBooru.csproj`, and `BlazBooruCommon` match the framework you intend to use.

Currently, BlazBooru only supports SQLite3 as a database engine - the performance of the API will depend on the read/write of the drive the database (and consequently BlazBooruAPI itself) is stored on.

## License

See License.md
