-- Migration script to add TOTP authentication support
-- Run this on your PostgreSQL database to add TOTP fields to the users table

-- Add TOTP secret column (stores the base32-encoded secret)
ALTER TABLE users ADD COLUMN IF NOT EXISTS totp_secret VARCHAR(255);

-- Add TOTP enabled flag (determines if TOTP is required for this user)
ALTER TABLE users ADD COLUMN IF NOT EXISTS totp_enabled BOOLEAN DEFAULT FALSE;

-- Create an index for faster lookups by email (if not already exists)
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- Update existing users to have TOTP disabled by default
UPDATE users SET totp_enabled = FALSE WHERE totp_enabled IS NULL;

-- Example query to check the updated schema:
-- SELECT column_name, data_type, is_nullable, column_default 
-- FROM information_schema.columns 
-- WHERE table_name = 'users' 
-- ORDER BY ordinal_position;