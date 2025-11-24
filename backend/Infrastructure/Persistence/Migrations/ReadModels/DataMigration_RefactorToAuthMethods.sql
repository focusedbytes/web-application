-- Data Migration Script for RefactorToAuthMethods Migration
-- This script should be run BEFORE applying the EF Core migration
-- It migrates data from the old Accounts table structure to the new AuthMethods structure

-- Step 1: Add new columns to Users table (if migration hasn't run yet)
-- These columns will be created by EF migration, but we do it here first for data migration
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'Users' AND column_name = 'Username') THEN
        ALTER TABLE "Users" ADD COLUMN "Username" text NOT NULL DEFAULT '';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'Users' AND column_name = 'DisplayName') THEN
        ALTER TABLE "Users" ADD COLUMN "DisplayName" text NULL;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'Users' AND column_name = 'LastLoginAt') THEN
        ALTER TABLE "Users" ADD COLUMN "LastLoginAt" timestamp with time zone NULL;
    END IF;
END $$;

-- Step 2: Generate usernames from email addresses (before @ symbol) or fallback to user_<id>
UPDATE "Users" u
SET
    "Username" = CASE
        WHEN a."Email" IS NOT NULL AND a."Email" != '' THEN
            LOWER(REGEXP_REPLACE(SPLIT_PART(a."Email", '@', 1), '[^a-zA-Z0-9_-]', '_', 'g'))
        ELSE
            'user_' || REPLACE(CAST(u."Id" AS text), '-', '')
    END,
    "LastLoginAt" = a."LastLoginAt"
FROM "Accounts" a
WHERE u."Id" = a."UserId";

-- Step 3: Handle duplicate usernames by appending numbers
WITH duplicates AS (
    SELECT "Id", "Username",
           ROW_NUMBER() OVER (PARTITION BY "Username" ORDER BY "CreatedAt") as rn
    FROM "Users"
    WHERE "Username" IN (
        SELECT "Username"
        FROM "Users"
        GROUP BY "Username"
        HAVING COUNT(*) > 1
    )
)
UPDATE "Users" u
SET "Username" = d."Username" || '_' || d.rn
FROM duplicates d
WHERE u."Id" = d."Id" AND d.rn > 1;

-- Step 4: Create unique index on Username (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Users_Username') THEN
        CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");
    END IF;
END $$;

-- Step 5: Create AuthMethods table (if not exists)
CREATE TABLE IF NOT EXISTS "AuthMethods" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "Identifier" text NOT NULL,
    "Type" text NOT NULL,
    "Secret" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_AuthMethods_Users_UserId" FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- Step 6: Migrate existing account data to AuthMethods
-- Convert email-based accounts to Email auth methods
INSERT INTO "AuthMethods" ("Id", "UserId", "Identifier", "Type", "Secret", "CreatedAt")
SELECT
    gen_random_uuid() as "Id",
    a."UserId",
    a."Email" as "Identifier",
    'Email' as "Type",
    a."HashedPassword" as "Secret",
    a."CreatedAt"
FROM "Accounts" a
WHERE a."Email" IS NOT NULL AND a."Email" != '';

-- Step 7: Create indexes on AuthMethods (if not exist)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_AuthMethods_UserId') THEN
        CREATE INDEX "IX_AuthMethods_UserId" ON "AuthMethods" ("UserId");
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_AuthMethods_Identifier_Type') THEN
        CREATE UNIQUE INDEX "IX_AuthMethods_Identifier_Type" ON "AuthMethods" ("Identifier", "Type");
    END IF;
END $$;

-- Step 8: Verification - Show counts before dropping Accounts table
DO $$
DECLARE
    accounts_count INTEGER;
    authmethods_count INTEGER;
    users_with_username INTEGER;
BEGIN
    SELECT COUNT(*) INTO accounts_count FROM "Accounts";
    SELECT COUNT(*) INTO authmethods_count FROM "AuthMethods";
    SELECT COUNT(*) INTO users_with_username FROM "Users" WHERE "Username" != '';

    RAISE NOTICE 'Migration Summary:';
    RAISE NOTICE '  Original Accounts: %', accounts_count;
    RAISE NOTICE '  Migrated AuthMethods: %', authmethods_count;
    RAISE NOTICE '  Users with Username: %', users_with_username;

    IF accounts_count != authmethods_count THEN
        RAISE WARNING 'Data mismatch detected! Accounts count does not match AuthMethods count.';
    END IF;
END $$;

-- Step 9: Now the Accounts table can be safely dropped by the EF Core migration
-- DROP TABLE "Accounts"; -- This will be done by EF Core migration
RAISE NOTICE 'Data migration completed successfully. You can now run the EF Core migration.';
