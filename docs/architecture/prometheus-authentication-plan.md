# Prometheus Authentication — Plan

Status: Phase 1 complete (2026-11-08). Phases 2 and 3 remain planned.

Progress:

- [x] Agreed on file-backed persistence with a plaintext-secret notice and HTTPS enforcement enabled by default.
- [x] Add persisted custom headers and Basic Auth settings.
- [x] Apply request authentication safely and enforce HTTPS policy.
- [x] Add settings UI, localization, and regression tests.
- [x] Build and run focused plus full test suites.

Verification: `dotnet build KubeUI.slnx --tl:off -clp:ErrorsOnly` succeeded with 0 errors. Full `dotnet test --no-restore --minimum-expected-tests 1` passed: 1,865 passed, 12 skipped, 0 failed. Two unrelated flaky failures on the first full run passed when isolated and on the subsequent full run.

## Goals

- Connect to Prometheus-compatible endpoints using authentication methods commonly used by self-hosted, enterprise, and managed deployments.
- Make credential behavior explicit, including persistence, token renewal, and transport security.
- Keep provider-specific signing and identity flows out of the generic static-header mechanism.

## Authentication options

### 1. Custom headers — first phase

- Allow users to configure one or more HTTP header name/value pairs for the selected cluster's metrics endpoint.
- Support static bearer tokens (`Authorization: Bearer …`), API keys, and gateway-specific headers such as tenant identifiers.
- Header values may contain secrets. Mask values in the UI and never include them in logs, diagnostics, or exports.
- Validate header names and reject headers controlled by the HTTP transport. Handle duplicate names deliberately; HTTP header names are case-insensitive.
- Custom headers are static: KubeUI does not acquire or refresh their values.

### 2. Basic Auth — first phase

- Provide username and password fields; KubeUI constructs the `Authorization: Basic …` header.
- Mask the password in the UI. Do not require users to Base64-encode credentials themselves.
- Basic Auth and custom `Authorization` headers are mutually exclusive for a request; surface conflicting configuration clearly.

### 3. OIDC/OAuth — later phase

- Provide an interactive login flow, access-token acquisition, and refresh/re-authentication behavior.
- Send acquired access tokens as bearer authorization; do not treat this as static custom-header configuration.
- Before implementation, decide how access/refresh tokens are persisted. Do not silently serialize them as ordinary settings without an explicit security decision.

### 4. Credential providers — provider-by-provider

- Add integrations based on demand; do not promise arbitrary credential-provider or plugin support.
- Provider integration must support request signing as well as token acquisition. AWS Managed Prometheus requires SigV4 signing, not a static bearer token.
- Azure Managed Prometheus already has a separate Azure credential flow; preserve it.
- Evaluate Google Cloud identity and other provider-specific flows independently.

## Persistence and user notice

- Persist custom-header values and Basic Auth credentials in the existing settings file for the initial implementation.
- Display a clear notice wherever credentials are configured: settings contain secrets in plaintext, and anyone who can read the file can read them.
- Mask secrets in the UI. Exclude them from logs, diagnostics, exports, and error messages.
- Existing settings persistence restricts the settings directory/file to the current user on Unix. Windows relies on the user-profile directory ACL; do not imply that file permissions encrypt credentials.
- OS credential-store integration may be considered later; it is not a prerequisite for the initial feature.

## HTTPS behavior

- Add a user-selectable **Require HTTPS for authentication** option, checked by default.
- When enabled, do not send configured credentials to an HTTP endpoint; explain the required correction.
- When disabled, allow the request but show a clear warning that credentials and traffic are unencrypted in transit.
- Apply consistently to custom headers, Basic Auth, and future OAuth flows. Reuse existing endpoint HTTPS configuration where possible; avoid duplicate scheme controls.
- Never silently downgrade an HTTPS endpoint to HTTP.

## Deployment context

- Prometheus Operator does not define one inbound authentication scheme. Its scrape authentication settings protect Prometheus-to-target requests, not KubeUI-to-Prometheus queries. Inbound access is typically controlled by network exposure, Prometheus web configuration, or a proxy.
- OpenShift monitoring commonly uses OAuth-protected routes and bearer tokens with monitoring RBAC. Tokens may expire, so static header support requires users to replace expired tokens manually; OAuth integration can improve this later.
- Prometheus-compatible gateways may require both authentication and tenant headers. Keep those separate configuration concerns.
- AWS Managed Service for Prometheus uses SigV4; a bearer header is insufficient.

## Delivery order

1. Add custom headers and Basic Auth, settings persistence, masking, conflict validation, HTTPS enforcement/override, and tests.
2. Add OIDC/OAuth after deciding token lifecycle and persistence behavior.
3. Add provider integrations individually, prioritized by user demand; retain Azure's existing flow and assess AWS SigV4 and Google Cloud identity.

## Test coverage

- Request tests verify custom headers and Basic Auth are applied only to the intended endpoint and never leak into logs.
- Settings tests verify intended persistence and round-trip behavior for header values and Basic Auth credentials.
- HTTPS tests cover default enforcement, explicit HTTP opt-out, warnings/state, and no credential transmission when HTTPS is required.
- Validation tests cover malformed/duplicate headers, protected header names, and conflicts with Basic Auth or OAuth.
- OAuth/provider tests must cover expiry, refresh/re-authentication, cancellation, and one-time retry behavior where applicable.
- Keep provider-specific signing tests separate from generic header tests.
