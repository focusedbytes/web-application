  4. DisplayName vs Username Confusion (UX Concern)

  Current State:
  - Username - unique, immutable, @username style
  - DisplayName - optional, can be anything

  Potential Issues:
  1. What should the UI show? Username or DisplayName?
  2. If DisplayName is null, do we fall back to Username?
  3. Should DisplayName be unique too?

  Recommendation:
  // Add property to User aggregate:
  public string EffectiveDisplayName => DisplayName ?? Username;

  // Frontend uses this everywhere for display

  Frontend Components Should:
  - Always use EffectiveDisplayName for display
  - Show @username only in profiles/settings
  - Make it clear that username is permanent

  Priority: � LOW (but improves UX)