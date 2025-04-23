# Security Testing Documentation - AMAP System

| ![ISEP Logo](../Imagens/logoisep.png) | ![DEI Logo](../Imagens/logo_DEI_big.png) |
|:------------------------------------:|:----------------------------------------:|

**Secure Software Development**  
**Master's in Computer Engineering - 2024/2025**  

**Authors:**
Diana Marques 1240445, 
Diogo Costa 1211514, 
Pedro Costa 1211439, 
Pedro Vilarinho 1211149, 
Tiago Tavares 1240494

**Porto, 2025-04-23**

---

## Table of Contents

1. [Introduction](#introduction)
2. [Security Testing Objectives](#security-testing-objectives)
3. [Testing Scope](#testing-scope)
4. [Testing Methodology](#testing-methodology)
5. [Types of Security Tests](#types-of-security-tests)
6. [Testing Environments](#testing-environments)
7. [Testing Tools](#testing-tools)
8. [Security Test Cases](#security-test-cases)
9. [Testing Schedule](#testing-schedule)
10. [Acceptance Criteria](#acceptance-criteria)
11. [Reports and Documentation](#reports-and-documentation)
12. [Incident Response Procedures](#incident-response-procedures)
13. [References](#references)

---

## Introduction

This document establishes the security testing plan for the AMAP system (Association for the Maintenance of Proximity Agriculture). Security testing is essential to ensure that the system is protected against threats and vulnerabilities, maintaining the integrity, confidentiality, and availability of user data and system operations.

The AMAP system is a platform that connects local agricultural producers with consumers (co-producers), enabling the management of subscription periods, product catalogs, offers, subscriptions, deliveries, and payments. Given the sensitive nature of the information managed (personal data, payment details), it is crucial to implement robust security measures.

## Security Testing Objectives

The main goals of security testing for the AMAP system are:

1. **Identify vulnerabilities** in the web application, API, and database
2. **Assess the effectiveness** of implemented security controls
3. **Verify compliance** with defined security requirements
4. **Protect sensitive data** against unauthorized access
5. **Ensure integrity** of transactions and data
6. **Ensure availability** of the system against potential attacks
7. **Validate authentication and authorization** for different user profiles

## Testing Scope

Security testing will cover the following components of the AMAP system:

1. **Backend (RESTful API):**
   - API controllers and endpoints
   - Authentication and authorization mechanisms
   - Input and output data validation
   - Protection against injections and common attacks
   - Session and cookie management

2. **Data Layer:**
   - SQL Server database security
   - Protection against SQL Injection
   - Database access control
   - Encryption of sensitive data

3. **Configurations and Infrastructure:**
   - Security configurations in ASP.NET Core
   - Key and secret management
   - HTTP headers and TLS configurations
   - Cookies and client-side storage

## Testing Methodology

The approach for security testing will follow industry best practices, including:

1. **OWASP Testing Guide**: Using the OWASP Testing Guide as a reference for testing procedures
2. **OWASP ASVS (Application Security Verification Standard)**: Verification of compliance with ASVS requirements
3. **PDCA Cycle (Plan-Do-Check-Act)**: Implementation of a continuous security improvement cycle
4. **Manual and Automated Testing**: Combination of manual techniques and automated tools

## Types of Security Tests

The following types of security tests will be performed:

1. **Authentication and Authorization Tests**
   - Verification of login mechanisms and session management
   - Testing of role-based access control (Manager, Producer, Co-producer)
   - Validation of function segregation

2. **Input Validation Tests**
   - Verification of input data validation
   - Injection tests (SQL, NoSQL, LDAP, OS command)
   - XSS (Cross-Site Scripting) tests
   - CSRF (Cross-Site Request Forgery) tests

3. **Session Management Tests**
   - Analysis of cookie configuration
   - Session fixation tests
   - Session timeout verification

4. **Cryptography Tests**
   - Verification of sensitive data protection
   - Validation of cryptographic implementations
   - Analysis of secure credential storage

5. **Configuration Tests**
   - Analysis of security HTTP headers
   - Verification of TLS configurations
   - Server configuration audits

6. **Business Logic Tests**
   - Validation of critical workflows (payments, subscriptions)
   - Price and quantity manipulation tests
   - Verification of transactional states

7. **Static Code Analysis**
   - Automated code review
   - Verification of known vulnerabilities
   - Analysis of dependencies and libraries

## Testing Environments

Security testing will be conducted in the following environments:

1. **Development Environment**
   - Testing during the development cycle
   - Static code analysis and unit tests

2. **Testing Environment**
   - Isolated environment for penetration testing
   - Attack simulation without impact on production

3. **Pre-Production Environment**
   - Final tests before deployment
   - Vulnerability analysis in conditions similar to production

## Testing Tools

The following tools will be used for security testing:

1. **Static Analysis Tools**
   - SonarQube
   - OWASP Dependency Check
   - Security Code Scan (.NET)

2. **Penetration Testing Tools**
   - OWASP ZAP (Zed Attack Proxy)
   - Burp Suite
   - Postman (for API testing)

3. **Vulnerability Analysis Tools**
   - OWASP Dependency Track
   - Nessus
   - Snyk

4. **Automation Tools**
   - OWASP ASVS Test Scripts
   - OWASP SecurityShepherd
   - Custom scripts for specific cases

## Security Test Cases

### 1. Authentication and Authorization

| # | Test Case | Description | Expected Results |
|---|-----------|-------------|------------------|
| 1.1 | Credential Validation | Login attempts with invalid credentials | Rejection of invalid credentials and blocking after multiple attempts |
| 1.2 | Password Recovery | Testing the password recovery process | Secure process without disclosure of sensitive information |
| 1.3 | Privilege Escalation | Attempt to access higher profile functionalities | Rejection of unauthorized access |
| 1.4 | Resource Access | Verification of access control to protected endpoints | Access only to resources allowed by the user profile |
| 1.5 | Session Timeout | Validation of session expiration | Sessions expire after the configured inactivity period |

### 2. Input Validation

| # | Test Case | Description | Expected Results |
|---|-----------|-------------|------------------|
| 2.1 | SQL Injection | SQL injection attempts in API parameters | Rejection and sanitization of malicious inputs |
| 2.2 | XSS (Cross-Site Scripting) | Testing insertion of malicious scripts | Rejection or escaping of malicious scripts |
| 2.3 | Type Validation | Sending incorrect data types | Proper validation and appropriate error messages |
| 2.4 | File Upload | Testing upload of malicious files | Rejection of dangerous files and validation of MIME types |
| 2.5 | Data Normalization | Verification of input data normalization | Adequate normalization to prevent confusion attacks |

### 3. Sensitive Data Management

| # | Test Case | Description | Expected Results |
|---|-----------|-------------|------------------|
| 3.1 | Transit Encryption | Verification of HTTPS communication | All communication protected with TLS 1.2+ |
| 3.2 | Password Storage | Analysis of credential storage | Passwords stored with secure hash and salt |
| 3.3 | Payment Data | Verification of financial data handling | Adequate protection of payment data |
| 3.4 | Security Logs | Analysis of security logs | Absence of sensitive data in logs |
| 3.5 | Cache and Local Storage | Verification of data in cache and storage | Absence of sensitive data in temporary storage |

### 4. Configuration and Infrastructure

| # | Test Case | Description | Expected Results |
|---|-----------|-------------|------------------|
| 4.1 | HTTP Headers | Analysis of security headers | Presence of headers such as Content-Security-Policy, X-XSS-Protection |
| 4.2 | TLS Configuration | Verification of TLS configurations | Only secure protocols and ciphers enabled |
| 4.3 | Secure Cookies | Analysis of cookie flags | Cookies with appropriate HttpOnly, Secure, and SameSite flags |
| 4.4 | Information Exposure | Verification of exposed technical information | Absence of sensitive information in headers and error messages |

### 5. Business Logic

| # | Test Case | Description | Expected Results |
|---|-----------|-------------|------------------|
| 5.1 | Price Manipulation | Price alteration during the purchase process | Server-side price validation, impossibility of manipulation |
| 5.2 | Quantity Modification | Attempt to alter quantities | Verification of limits and server-side validation |
| 5.3 | Operation Sequence | Testing alteration of operation order | Workflow validation and rejection of invalid sequences |
| 5.4 | Transaction Atomicity | Testing failures during transactions | Atomic transactions without inconsistent states |

## Testing Schedule

| Phase | Test Type | Start Date | End Date | Responsible |
|-------|-----------|------------|----------|-------------|
| 1 | Static Code Analysis | DD/MM/YYYY | DD/MM/YYYY | Development Team |
| 2 | Authentication and Authorization Tests | DD/MM/YYYY | DD/MM/YYYY | Security Team |
| 3 | Input Data Tests | DD/MM/YYYY | DD/MM/YYYY | QA and Security Team |
| 4 | Configuration Tests | DD/MM/YYYY | DD/MM/YYYY | Infrastructure Team |
| 5 | Penetration Tests | DD/MM/YYYY | DD/MM/YYYY | External Consultants |
| 6 | Remediation and Retesting | DD/MM/YYYY | DD/MM/YYYY | Joint Team |

## Acceptance Criteria

The results of security testing will be evaluated according to the following criteria:

1. **Compliance with OWASP ASVS Level 2** (minimum)
2. **Zero critical or high vulnerabilities** not mitigated
3. **Documentation of all medium and low risks** with mitigation plan
4. **Effective protection against the risks identified** in threat modeling
5. **Completed penetration testing** without significant findings
6. **Implementation of all planned mitigations** in the design phase

## Reports and Documentation

The documentation resulting from security testing will include:

1. **Vulnerability Report**
   - Detailed description of vulnerabilities found
   - Severity classification (CVSS)
   - Mitigation recommendations

2. **ASVS Compliance Report**
   - Status of compliance with ASVS requirements
   - Identified gaps
   - Next steps for full compliance

3. **Penetration Testing Results**
   - Methodology used
   - Vulnerabilities exploited
   - Proof of concepts

4. **Remediation Plan**
   - Prioritization of fixes
   - Effort estimates
   - Responsible parties and deadlines

## Incident Response Procedures

If critical vulnerabilities are discovered during testing, the following protocol will be followed:

1. **Immediate Documentation**
   - Detailed record of the vulnerability
   - Preliminary impact assessment

2. **Notification**
   - Immediately inform the security manager
   - Inform relevant stakeholders according to severity

3. **Containment**
   - Implement temporary measures if necessary
   - Limit exposure until definitive resolution

4. **Remediation**
   - Develop and test the fix
   - Deploy in test environment before production

5. **Verification**
   - Retest to confirm resolution
   - Documentation of the implemented solution

## References

1. OWASP Application Security Verification Standard (ASVS) 4.0
2. OWASP Testing Guide v4
3. OWASP Top 10 2021
4. NIST Special Publication 800-53
5. Microsoft SDL (Security Development Lifecycle)
6. ISO/IEC 27001:2013 (Information Security Management) 