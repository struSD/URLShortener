# URLShortener

## HTTP API
- PUT(api/url BODY = {origin_url}) => save the record to database
- GET(api/url/{Id}) => redirect to the origin URL by ID
- GET(api/url/{ShortenedUrl}) => redirect to the origin URL by Shortened URL

## Models

### URL :
Id
- Origin URL
- Shortened URL