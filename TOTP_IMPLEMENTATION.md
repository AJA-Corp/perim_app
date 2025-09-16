# TOTP Authentication Implementation Guide

## Overview
This implementation adds Time-based One-Time Password (TOTP) authentication to the PerimApp application, compatible with Google Authenticator, Microsoft Authenticator, and other TOTP applications.

## Database Schema Changes
Before using the TOTP features, you need to update your PostgreSQL database schema by running the provided migration script:

```sql
-- Run the database_migration_totp.sql script
-- This adds totp_secret and totp_enabled columns to the users table
```

## How It Works

### For New Users (Registration Flow)
1. User fills in email, password, and confirmation password
2. User clicks "Suivant" (Next)
3. System generates a unique TOTP secret for the user
4. User is redirected to TOTP setup page (`TotpSetupPage`)
5. Page displays:
   - QR code for scanning with authenticator app
   - Manual secret key entry option
   - Instructions for setting up authenticator app
6. User scans QR code or manually enters secret in their authenticator app
7. User enters the 6-digit TOTP code to verify setup
8. Once verified, user is logged in and redirected to main page

### For Existing Users (Login Flow)
1. User enters email and password
2. System validates credentials
3. If TOTP is enabled for the user:
   - User is redirected to TOTP verification page (`TotpVerificationPage`)
   - User enters 6-digit code from their authenticator app
   - Upon successful verification, user is logged in
4. If TOTP is not enabled, user is logged in directly (backward compatibility)

### For Home Code Authentication
- Home code authentication continues to work as before
- TOTP is not required for home code login

## Technical Implementation

### New Components
- **TotpService**: Core service for TOTP operations (secret generation, QR codes, validation)
- **TotpSetupPage**: UI for setting up TOTP during registration
- **TotpVerificationPage**: UI for entering TOTP codes during login
- **Database migration**: Adds TOTP fields to users table

### Dependencies Added
- **OtpNet**: Core TOTP library for generating and validating codes
- **QRCoder**: QR code generation for authenticator setup

### Authentication Flow Changes
- `AuthenticateUserAsync` now returns `-100` when password is correct but TOTP is required
- New method `AuthenticateUserWithTotpAsync` for TOTP code validation
- Registration process now includes TOTP secret generation

### Database Schema
```sql
ALTER TABLE users ADD COLUMN totp_secret VARCHAR(255);
ALTER TABLE users ADD COLUMN totp_enabled BOOLEAN DEFAULT FALSE;
```

## Security Features
- TOTP secrets are stored securely in the database
- QR codes are generated on-demand and not stored
- TOTP validation includes time window tolerance for clock drift
- Backward compatibility ensures existing users aren't locked out

## Configuration
- TOTP codes are 6 digits (standard)
- Time step is 30 seconds (standard)
- Validation window allows ±30 seconds for clock drift
- QR codes include proper issuer identification

## Testing
A basic test suite is included in `Testing/TotpTest.cs` that validates:
- Secret generation
- Code generation
- Code validation
- QR code generation

## User Experience
1. **Clear instructions**: Step-by-step setup process
2. **Multiple setup options**: QR code scanning or manual entry
3. **Error handling**: Clear error messages for invalid codes
4. **Intuitive UI**: Consistent with existing app design
5. **Backward compatibility**: Existing users can continue using the app

## Troubleshooting
- Ensure database migration is applied before using TOTP features
- Check that OtpNet and QRCoder packages are properly installed
- Verify TOTP secrets are properly generated during registration
- Test with multiple authenticator apps for compatibility