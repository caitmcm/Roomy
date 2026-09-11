# Roomy

## About

### How I built this

This is a a hotel room booking API built with ASP.NET Core (.NET 10), FastEndpoints and EF Core on SQLite.

### Workflow

> Claude Code was used in this project for its speed and design feedback. I have iterated and ensured quality.

**Functional Requirements**

I first took notes on the data model outlined in the brief, such as "Hotels having rooms" and the room types. I made some assumptions which I've listed at the end of this document.

I wrote down a RESTful API url structure following the data model, such as `GET /hotel/{name}/room/{id}/`

**Url Design**

I used the hotel's name as a url field to avoid surfacing the database id the controller response. For the purpose of the assignment, the user is expected to copy and paste the hotel's name into related endpoints.

I used `/booking` as a root object as GET `hotel/{name}/room/{id}/booking/{reference}` would require more than the booking reference.

**Non-functional Requirements**

I lastly took notes on the non-functional requirements and which packages I could use to implement them. I started with Fast Endpoints for its Swagger and FluentValidation integration and the REPR (Request EndPoint, Response) pattern which I'm fond of. I identified I'd later need to create a CI/CD pipeline to deploy from Github to Azure.

**Claude**

I used Claude Code to critique my initial review and identify anything I've missed. It raised a couple of concerns I was okay to accept as trade-offs for the scope of the assessment, for example a concurrency risk on two writes for the same room at the same time.

I used Claude's Plan mode to build an implementation plan. I workshopped the plan and preferred it didn't write verbose code comments. I had it build the amended plan.

**3-Layer Domain Model**

I wrote a 3-layer domain model to avoid returning the Entity Framework entities above the Repository layer.

Entity Framework entities in the `Data.Entities` namespace are used by the Repository classes only. The repository maps these to `Domain.` namespace entities and returns them to the Endpoint.

The Endpoint maps the `Domain.` namespace entities to a `Response` DTO, and returns the DTO to the calling client.

**Unit and Integration Testing Projects**

The EndpointTests project tests the endpoint classes and validators. This project uses FastEndpoint's unit testing packages and NSubstitute to mock the repositories.

The RepositoryTests project uses an in-memory database to test the repositories.

**CI/CD**

I created a GitHub Action pipeline to deploy the code to Azure using Azure Web App Deployment Center.

See the deployed app here:

https://roomy-api-c9a0a4hwf8cgh7ex.ukwest-01.azurewebsites.net/swagger/index.html

## Using the Swagger Page

### Seeding

**Reset and seed test data**

```
POST /debug
```

**Delete all data**

```
DELETE /debug
```

### Using the API

**List all hotels, or query by name.**

```

curl -X 'GET' \
  'https://localhost:7136/hotel' \
  -H 'accept: application/json'

curl -X 'GET' \
  'https://localhost:7136/hotel?name=Riverside' \
  -H 'accept: application/json'

curl -X 'GET' \
  'https://localhost:7136/hotel?name=Riverside%20Lodge' \
  -H 'accept: application/json'

```

**Find an available room in your chosen hotel for your party size.**

From and To dates are expressed in `YYYY-MM-DD` format.

```

curl -X 'GET' \
  'https://localhost:7136/hotel/Riverside%20Lodge/room?from=2026-09-10&to=2026-09-12&guests=2' \
  -H 'accept: application/json'

```

**Book a room.**

```
curl -X 'POST' \
  'https://localhost:7136/booking' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "hotelName": "Riverside Lodge",
  "roomNumber": 3,
  "startDate": "2026-09-10",
  "endDate": "2026-09-12",
  "numberOfGuests": 2,
  "leadGuestName": "Cait M"
}'

// Sample Error:

curl -X 'POST' \
  'https://localhost:7136/booking' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "hotelName": "The Kelvin Arms",
  "roomNumber": 3,
  "startDate": "2026-09-21",
  "endDate": "2026-09-23",
  "numberOfGuests": 2,
  "leadGuestName": "Cait M"
}'

{
  "statusCode": 409,
  "message": "One or more errors occurred!",
  "errors": {
    "startDate": [
      "Room 3 is already booked for part of that range."
    ]
  }
}

```

**Retrieve your booking.**

```
curl -X 'GET' \
  'https://localhost:7136/booking/BK-N2JJ8U' \
  -H 'accept: application/json'
```

## Assumptions

### Room Availability

Check-in at the hotel is after 3pm and check-out is before 12pm. This means a room is occupied for that night.

A room can be "turned over" if a guest checks out before 12pm and a new guest checks in after 3pm that same day.

```plaintext
Guest 1 checks into room 1 after 3pm on day 1, sleeps one night, Checks out before 12pm on day 2; room 1 is available.

Guest 2 checks into room 1 after 3pm on day 2, sleep one night, checks out before 12pm on day 3.
```

### Room Types

The single, double, and deluxe room types sleep the following number of people:

- A single room sleeps 1 person.
- A double room sleeps 2 people.
- A deluxe room sleeps 3 people.

I've configured these in the `RoomType.Capacity` database property.
