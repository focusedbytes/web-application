  3. AuthMethod Entity Design (Minor Concern)

  Current Implementation:
  public class AuthMethod
  {
      public string Identifier { get; private set; }
      public AuthMethodType Type { get; private set; }
      public string? Secret { get; private set; }  // ⚠️ Mixed purpose
      public DateTime CreatedAt { get; private set; }
  }

  Issues:
  1. Secret field serves different purposes:
    - Hashed password for Email
    - OAuth token for Google/Apple
    - null for Phone (will need OTP/SMS verification)
  2. No verification status tracking
  3. No "verified" flag for email/phone

  Better Design:
  public class AuthMethod
  {
      public string Identifier { get; private set; }
      public AuthMethodType Type { get; private set; }
      public string? Secret { get; private set; }
      public bool IsVerified { get; private set; }  // ✨ NEW
      public DateTime? VerifiedAt { get; private set; }  // ✨ NEW
      public DateTime CreatedAt { get; private set; }
      public DateTime? LastUsedAt { get; private set; }  // ✨ NEW

      public void Verify() { ... }
  }

  Benefits:
  - Track which auth methods are verified
  - Know when each method was last used
  - Better security posture

  Priority: � MEDIUM
