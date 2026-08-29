# Spotify Recommendation App

Prototype recommendation algorithm that generates Spotify recommendations based on a user’s past listens.

Status: 2026-08-28

## Goals
- Build a recommendation engine that uses historical listen data.
- Integrate with Spotify authentication and scopes required to read user profile and playback data.
- Provide a simple frontend to exercise the auth code flow and display recommendations.

## TODO
- Add unit tests for `AuthService`.
- Decide whether `AuthService` needs its own controller.
- Implement a minimal frontend and test the auth code flow.
- Running into an invalid uri issue when testing redirect uri : FIX
- Consider adding additional user scopes later.

## Potential user scopes to consider
- `user-read-email`
  - Description: Read access to user’s email address.
  - Visible to users: Get your real email address.
  - Endpoints that require this scope: Get Current User's Profile

- `user-read-private`
  - Description: Read access to user’s subscription details (type of user account).
  - Visible to users: Access your subscription details.
  - Endpoints that require this scope: Search for an Item, Get Current User's Profile

## Notes
- Keep scope usage minimal for initial development; add broader scopes only when needed.
- Avoid logging sensitive values; log summaries (presence, length) when debugging auth flows.