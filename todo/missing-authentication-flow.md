1. Missing Authentication Flow (Critical)

  Current State: We have user management, but no actual authentication/login
   endpoints.

  What's Missing:
  // Backend needs:
  POST /api/auth/login
    - Verify credentials (identifier + password)
    - Generate JWT token
    - Update LastLoginAt

  POST /api/auth/register
    - Public user registration endpoint

  POST /api/auth/social/google
  POST /api/auth/social/apple
    - OAuth flow handlers

  Recommendation: This should be your next priority. Without this, you can't
   actually log users in.

  Priority: � HIGH
