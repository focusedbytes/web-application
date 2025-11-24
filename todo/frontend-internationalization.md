  7. Frontend Internationalization (As Per Your CLAUDE.md)

  Current State: All text changed from Ukrainian to English (hardcoded)

  Your Goal (from CLAUDE.md):
  UI currently in Ukrainian (temporary)
  Will implement i18n for multilingual support

  Recommendation:
  // When implementing i18n:
  import { t } from '$lib/i18n';

  // Instead of:
  <button>Create User</button>

  // Use:
  <button>{t('users.create')}</button>

  Priority: � LOW (future feature, not blocking)