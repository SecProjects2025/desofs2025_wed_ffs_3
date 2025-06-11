# MFA Testing Guide

This guide explains how to test the Multi-Factor Authentication (MFA) flow for managers using Postman or a similar API client.

## 1. MFA Setup (Generate QR Code/Secret)

**Request:**
```
POST /api/auth/mfa/setup
Headers:
  Authorization: Bearer <manager_token>
Body (JSON):
  {
    "userId": "<manager_user_id>"
  }
```
**Response:**
- Returns a URL to a QR code (open in browser) or a secret key for use in an authenticator app (e.g., Google Authenticator).

## 2. Scan QR Code or Enter Secret
- Use the QR code URL or secret key in your authenticator app to add the account.

## 3. MFA Enable (Verify and Enable MFA)

**Request:**
```
POST /api/auth/mfa/enable
Headers:
  Authorization: Bearer <manager_token>
Body (JSON):
  {
    "userId": "<manager_user_id>",
    "code": "<6-digit code from app>"
  }
```
**Response:**
- Success if the code is valid and MFA is enabled for the user.

## 4. MFA Verification During Login

**Request:**
```
POST /api/auth/mfa/verify
Body (JSON):
  {
    "userId": "<manager_user_id>",
    "code": "<6-digit code from app>"
  }
```
**Response:**
- Success if the code is valid and login is completed.

## Notes
- Replace `<manager_token>` and `<manager_user_id>` with actual values from your environment.
- Codes are time-based and change every 30 seconds in the authenticator app.
- If you encounter issues, ensure the system time is correct on both the server and your device. 