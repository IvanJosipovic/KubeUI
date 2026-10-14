# KubeUI Threat Model

## Overview

KubeUI is a desktop Kubernetes user interface built with .NET and Avalonia.

It allows a user to connect to Kubernetes clusters, inspect cluster resources, interact with workloads, view logs, execute supported Kubernetes operations, and use local application features that may communicate with external tools or local integrations.

KubeUI should be treated similarly to a privileged Kubernetes administration client. The effective privileges of KubeUI are determined by the Kubernetes credentials supplied by the user.

## Security Goals

KubeUI should:

1. Never expose Kubernetes credentials, bearer tokens, client certificates, refresh tokens, or other secrets to unauthorized parties.

2. Never allow untrusted cluster content to cause arbitrary code execution on the local workstation.

3. Never allow Kubernetes resource content, logs, names, labels, annotations, YAML, terminal output, or other cluster-controlled data to escape its intended rendering or parsing context.

4. Correctly enforce authentication and authorization on any local API, ACP, MCP, HTTP, WebSocket, or similar service exposed by the application.

5. Prevent untrusted local or remote origins from invoking privileged local application functionality without authorization and explicit user intent where appropriate.

6. Prevent malicious Kubernetes clusters from escaping the trust boundary of the Kubernetes client connection and compromising the user's workstation.

7. Avoid exposing sensitive cluster information through logs, telemetry, crash reports, temporary files, settings, clipboard operations, or other local persistence.

8. Treat credentials stored by KubeUI as sensitive and protect them appropriately at rest and in memory where practical.

## Primary Trust Boundaries

### Kubernetes clusters

Kubernetes API servers are not necessarily trusted.

A user may connect KubeUI to a malicious or compromised Kubernetes cluster. Data returned by the Kubernetes API must therefore be treated as attacker-controlled input.

Examples include:

- resource names
- namespaces
- labels
- annotations
- YAML and JSON resource content
- CustomResourceDefinitions
- Custom Resources
- logs
- events
- pod and container output
- API discovery documents
- OpenAPI schemas
- status fields
- error messages
- URLs contained in resources

A malicious cluster must not be able to execute arbitrary code on the machine running KubeUI.

### Kubernetes credentials

Kubeconfig files, tokens, certificates, client keys, exec credential plugins, and cloud-provider authentication credentials are security-sensitive.

Unauthorized disclosure of credentials that permit cluster access should generally be treated as a high-severity vulnerability.

If credentials provide cluster-admin or equivalent access, practical remote disclosure may warrant critical severity.

### Local services

Any HTTP, WebSocket, ACP, MCP, IPC, or other server exposed by KubeUI is a security boundary.

Do not assume that binding to localhost alone provides authentication.

Consider attacks involving:

- malicious websites
- DNS rebinding
- cross-origin requests
- CSRF-style attacks
- WebSockets
- local unprivileged processes
- malicious MCP or ACP clients
- predictable or reusable authentication tokens

Privileged operations exposed through local services must require appropriate authentication and authorization.

### Local filesystem

Files opened, imported, generated, cached, or written by KubeUI may contain attacker-controlled content.

Relevant attack classes include:

- path traversal
- arbitrary file overwrite
- unsafe temporary files
- symlink attacks
- unsafe archive extraction
- credential disclosure
- loading executable content from attacker-controlled locations

### External processes

Any process launched by KubeUI should be treated as a significant security boundary.

Pay particular attention to:

- command construction
- shell invocation
- argument injection
- environment variables
- executable path resolution
- kubectl or other CLI integration
- Kubernetes exec credential plugins

Cluster-controlled strings must never become executable command text without safe argument handling.

## Untrusted Inputs

The scanner should consider the following attacker-controlled unless proven otherwise:

- all Kubernetes API responses
- resource YAML and JSON
- resource names
- labels and annotations
- CRDs and custom resources
- logs
- event messages
- pod and container output
- URLs provided by Kubernetes resources
- imported kubeconfig files
- configuration files opened by the user
- content received by local HTTP, WebSocket, ACP, or MCP endpoints
- browser-originated requests to local services
- responses from external APIs
- plugin or integration responses

Do not assume Kubernetes API data is trustworthy merely because the user chose to connect to that cluster.

## High Value Assets

The most sensitive assets include:

- Kubernetes bearer tokens
- kubeconfig files
- client certificates and private keys
- cloud authentication credentials
- Kubernetes service account tokens
- OAuth or OIDC tokens
- ACP or MCP authentication tokens
- credentials or secrets retrieved from Kubernetes
- filesystem access available to the KubeUI process
- operating-system user privileges
- Kubernetes operations available through the user's credentials

## Important Vulnerability Classes

The scanner should prioritize:

- arbitrary code execution
- command injection
- authentication bypass
- authorization bypass
- token or credential disclosure
- DNS rebinding attacks against local services
- CSRF-like invocation of localhost APIs
- unsafe WebSocket authentication
- path traversal
- arbitrary file read or write
- insecure deserialization
- unsafe process execution
- unintended secret persistence
- Kubernetes privilege escalation caused by KubeUI itself
- malicious cluster content causing local compromise
- unsafe handling of kubeconfig exec plugins
- injection into terminal or shell functionality
- sensitive information disclosure through logs or telemetry

## Severity Guidance

### Critical

Generally classify as Critical when exploitation can reasonably result in:

- arbitrary code execution on the user's workstation without prior local code execution
- remote compromise through a malicious website or network-accessible endpoint
- authentication bypass that exposes arbitrary Kubernetes operations using the victim's credentials
- theft of highly privileged Kubernetes credentials through a remotely exploitable path
- compromise of arbitrary connected clusters using credentials available to KubeUI

A vulnerability should have a plausible exploitation path rather than merely a theoretical dangerous primitive.

### High

Generally classify as High when exploitation can result in:

- disclosure of Kubernetes credentials or secrets
- arbitrary local file read or write
- significant unauthorized Kubernetes operations
- code execution requiring meaningful user interaction
- compromise requiring connection to a malicious Kubernetes cluster
- authentication bypass where exploitation is limited to the local machine
- command injection requiring attacker-controlled cluster content plus user interaction

### Medium

Generally classify as Medium when exploitation results in:

- limited information disclosure
- lower-impact local API access
- denial of service with meaningful security impact
- unsafe behavior requiring substantial user interaction or unusual configuration
- exposure of non-secret cluster metadata

### Low

Generally classify as Low when:

- impact is minimal
- exploitation requires unrealistic assumptions
- only non-sensitive information is disclosed
- the finding is primarily hardening with no practical security boundary crossed

Do not inflate severity solely because Kubernetes is involved. Severity should reflect the actual privileges obtained and realistic attack path.

## User Interaction

User interaction is not automatically a strong mitigation.

Opening a malicious Kubernetes cluster, viewing a resource, viewing logs, or opening a normal KubeUI screen should generally be considered expected application usage rather than exceptional security consent.

Explicit approval prompts for dangerous actions may meaningfully reduce severity if they clearly explain the operation being authorized.

## Expected Privileges

KubeUI intentionally performs Kubernetes operations permitted by the credentials supplied by the user.

A finding is not a vulnerability merely because KubeUI can:

- create resources
- delete resources
- modify resources
- access pod logs
- execute Kubernetes APIs
- interact with workloads

These actions are expected when authorized by Kubernetes RBAC and initiated through intended application functionality.

The relevant security question is whether an attacker can cause such actions without the user's authorization or beyond the privileges represented by the supplied credentials.

## Out of Scope or Lower Priority

Unless they cross another security boundary, the following are generally lower priority:

- attacks requiring an already compromised local operating-system account
- attacks requiring arbitrary code execution on the workstation before exploiting KubeUI
- purely cosmetic UI issues
- denial of service that only crashes the local application and causes no persistent damage or security impact
- vulnerabilities in Kubernetes itself
- vulnerabilities that exist entirely in an upstream dependency without a KubeUI-specific exploit path

Dependency vulnerabilities are still relevant when KubeUI exposes the vulnerable functionality in a realistically exploitable way.

## False Positive Guidance

Do not report normal Kubernetes administration functionality as privilege escalation.

Do not assume that cluster-supplied strings are trusted.

Do not treat localhost binding as an authentication mechanism.

Do not report theoretical injection merely because a string reaches an API. Demonstrate or explain how attacker-controlled input reaches an unsafe sink.

Do not report vulnerable dependency versions without demonstrating that KubeUI uses an affected code path or that the issue is otherwise materially relevant.

## Findings

Reports should include:

1. The attacker-controlled input.
2. The trust boundary crossed.
3. The vulnerable code path.
4. A realistic attack scenario.
5. The privileges or data gained.
6. Reproduction steps where practical.
7. A proposed remediation.
8. The affected KubeUI version or commit when identifiable.

Proposed patches should preserve existing architecture and avoid unnecessary behavioral changes.
