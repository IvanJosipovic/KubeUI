# Security Policy

Security is important to the KubeUI project. If you believe you have found a security vulnerability, please report it privately so it can be investigated and addressed before public disclosure.

## Supported Versions

Security fixes are provided for the latest released version of KubeUI.

| Version | Supported |
| --- | --- |
| Latest release | Yes |
| Older releases | No |

Users are encouraged to update to the latest available version before reporting an issue that may already have been resolved.

## Reporting a Vulnerability

Do not report security vulnerabilities through public GitHub issues, discussions, pull requests, or other public channels.

Please report vulnerabilities using either of these methods:

* Email **security@kubeui.com**
* GitHub's private vulnerability reporting feature

To report through GitHub:

1. Open the KubeUI repository on GitHub.
2. Select **Security**.
3. Select **Advisories**.
4. Select **Report a vulnerability**.
5. Include enough information to reproduce and evaluate the issue.

A useful report should include:

* A description of the vulnerability.
* The affected KubeUI version or commit.
* Steps required to reproduce the issue.
* The expected and observed behavior.
* The security impact.
* Any prerequisites required for exploitation.
* A proof of concept, if available.
* Suggested mitigations or fixes, if known.

Please avoid accessing, modifying, or disclosing data that does not belong to you while investigating a vulnerability.

## What to Expect

Security reports will be reviewed and validated as soon as reasonably possible.

If the issue is confirmed, the project may:

* Develop and test a fix.
* Coordinate disclosure with the reporter.
* Publish a GitHub Security Advisory.
* Request a CVE when appropriate.
* Release a patched version.
* Credit the reporter, unless anonymity is requested.

The severity assigned by the project may differ from the reporter's initial assessment. Severity will be based on the demonstrated attack requirements, affected trust boundaries, realistic impact, and available mitigations.

## Coordinated Disclosure

Please allow the project reasonable time to investigate and address a confirmed vulnerability before publishing technical details.

Once a fix is available, disclosure may include:

* A description of the vulnerability.
* Affected versions.
* Fixed versions.
* Mitigation guidance.
* CVE information, when applicable.
* Reporter attribution, when requested.

## Scope

KubeUI interacts with Kubernetes clusters and may handle sensitive information such as:

* Kubernetes API credentials and authentication tokens.
* Cluster configuration.
* Kubernetes resources and secrets.
* Local application configuration.
* Credentials used by integrations or external clients.
* Data returned by Kubernetes APIs or KubeUI integrations.

Reports involving unintended disclosure, authentication or authorization bypasses, credential exposure, unsafe local interfaces, privilege escalation, or unintended access to Kubernetes resources are considered security relevant.

Issues that depend entirely on an already compromised local user account, administrator controlled configuration, or behavior explicitly documented as trusted may have limited or no security impact.

## Security Considerations

KubeUI is a desktop application that operates with the permissions granted to the user and credentials provided to it.

Users should:

* Grant KubeUI only the Kubernetes permissions required for their workflows.
* Protect kubeconfig files and authentication credentials.
* Keep KubeUI and its dependencies updated.
* Avoid exposing local KubeUI services to untrusted networks.
* Review permissions before allowing external clients or integrations to communicate with KubeUI.

KubeUI cannot provide stronger isolation than the operating system, Kubernetes credentials, and cluster permissions under which it is running.

## Security Research

Good faith security research intended to identify and responsibly report vulnerabilities is welcome.

The project does not authorize:

* Destructive testing.
* Denial of service attacks.
* Access to data belonging to other users.
* Social engineering.
* Testing against infrastructure you do not own or have permission to assess.

Please keep testing limited to systems and data you are authorized to use.
