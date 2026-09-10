# Roomy

**Code**

A hotel room booking API built with ASP.NET Core (.NET 10), FastEndpoints and EF Core on SQLite.

🤖 - Claude Code was used in this project for its speed and design feedback. I have iterated and ensured quality.

**CI/CD**

GitHub Action created through Azure Web App Deployment Center.

See the deployed app here:
https://roomy-api-c9a0a4hwf8cgh7ex.ukwest-01.azurewebsites.net/swagger/index.html

## Using the Swagger Page

### Seeding

**Re-seed test data (if the database is blank)**

```
POST /debug
```

Teardown later.

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

### Room availability assumptions

Check-in at the hotel is after 3pm and check-out is before 12pm. This means a room is occupied for that night.

A room can be "turned over" if a guest checks out before 12pm and a new guest checks in after 3pm that same day.

```plaintext
Guest 1 checks into room 1 after 3pm on day 1, sleeps one night, Checks out before 12pm on day 2; room 1 is available.

Guest 2 checks into room 1 after 3pm on day 2, sleep one night, checks out before 12pm on day 3.
```
