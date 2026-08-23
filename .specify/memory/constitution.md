<!--
Sync Impact Report
- Version change: unversioned scaffold -> 1.0.0
- Modified principles: template placeholders -> Security-First, Validated Inputs,
  Service Boundaries, Documented Code, and Verifiable Delivery
- Added sections: Security Requirements, Architecture and Documentation,
  Development Workflow
- Removed sections: none
- Follow-up TODOs: Confirm the project's original ratification date.
-->

# Taskify Constitution

## Core Principles

### I. Security-First
Security is a release-blocking requirement. Every change MUST identify its trust
boundaries, protect secrets, apply least privilege, and prevent unauthorized access.
Security-sensitive behavior MUST have automated tests or documented verification evidence.
The rationale is to make protection of users and systems a design constraint rather than
a post-release correction.

### II. Validated Inputs
All user-controlled and inter-service inputs MUST be validated at the receiving boundary
against an explicit schema, type, size, format, and authorization context. Invalid input
MUST produce a safe, observable error without partial mutation or sensitive data disclosure.
Validation rules MUST be covered by tests for accepted, rejected, and boundary values.

### III. Service Boundaries
Taskify MUST preserve clear microservice ownership. Each service MUST own its data and
business rules, expose an explicit versioned contract, and communicate through documented
interfaces. Cross-service dependencies MUST include timeout, retry, failure, and backward-
compatibility behavior. Direct access to another service's private storage is prohibited.

### IV. Documented Code
Production code MUST document public interfaces, security assumptions, non-obvious
invariants, operational behavior, and failure modes. Documentation MUST be updated in the
same change as the behavior it describes. Comments MUST explain intent or constraints,
not restate implementation. The rationale is to keep distributed ownership and review
possible as the system grows.

### V. Verifiable Delivery
Every behavior change MUST include proportionate automated tests and MUST pass the
applicable quality gates before release. Tests MUST cover service contracts, validation,
authorization, and failure behavior where those concerns are affected. Changes MUST be
small enough to review, and exceptions MUST record their risk, owner, and expiry.

## Security Requirements

Secrets MUST NOT be committed to source control, logs, fixtures, or documentation.
Authentication and authorization MUST be enforced server-side at every protected
boundary. Logs and telemetry MUST exclude credentials, tokens, and unnecessary personal
data. Dependencies and container images MUST be reviewed for known vulnerabilities, and
critical findings MUST block release until resolved or formally accepted.

## Architecture and Documentation

The microservices architecture MUST be represented in project documentation, including
service ownership, communication paths, data ownership, external dependencies, and
operational assumptions. The project README MUST be updated in the same change for every
architecture change and MUST include current setup, run steps, required configuration,
service startup order or orchestration, and verification commands. Documentation MUST
identify any environment-specific prerequisites.

## Development Workflow

Pull requests MUST describe affected services, trust boundaries, input schemas, data
ownership, tests, and documentation updates. Reviewers MUST verify constitution compliance
and MUST reject missing security validation, undocumented contract changes, or stale run
instructions. Releases MUST retain enough structured logs, metrics, and traces to diagnose
service failures without exposing protected data.

## Governance

This constitution supersedes conflicting project practices. Amendments MUST be proposed
with their motivation, affected principles, compatibility impact, migration plan when
needed, and updated compliance checks. Approval MUST come from the project maintainers
responsible for security and architecture. Every pull request and release review MUST
verify compliance, and exceptions MUST be time-bound and recorded with an owner.

The constitution uses semantic versioning. A MAJOR version removes or redefines a
governance requirement incompatibly; a MINOR version adds a principle or materially
expands requirements; a PATCH version clarifies wording without changing obligations.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): confirm original adoption date | **Last Amended**: 2026-08-23
