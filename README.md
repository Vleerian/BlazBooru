## Quickstart

Clone this repository and run `dotnet run` to start a basic BlazBooruAPI instance.

## Motivation

BlazBooruAPI is the backend portion of BlazBooru which manages the BlazBooru database, and serves post data and files.

## Installation

BlazBooruAPI will run on any system with support for dotnet 5 or newer. Ensure that the targeted framework in `BlazBooruAPI.csproj` matches the framework you intend to use.

Currently, BlazBooruAPI only supports SQLite3 as a database engine - the performance of the API will depend on the read/write of the drive the database (and consequently BlazBooruAPI itself) is stored on.

## API Reference

`/api/posts/` will return the **json data only** of most recent 42 posts
`/api/posts/<postID>` will return the **json data only** of the specified post
`/api/file/<fileID>` will return the file stored in the database

## License

See License.md