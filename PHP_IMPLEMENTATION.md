# Rebuild the Missing PHP Backend for the Existing Unity Project

Status: Proposal only. Only this Markdown document is being added. No PHP, SQL, Unity scripts, scenes, settings, server configuration, or payment accounts have been changed.

## 1. Objective

Reconstruct the backend required by the current SubscriptionSettings asset and the existing LogMasuk/LamanUtama workflows. Keep your existing endpoint filenames, Unity scenes, UI, registration fields, and Stripe subscription purpose.

This is a replacement implementation based on the surviving Unity client, not recovery of the original PHP source. It cannot recreate missing database records, password hashes, customers, or subscription mappings without backups or provider records.

Deliver a complete, documented PHP project covering account registration/login, email verification, session/device handling, subscription lookup, plan selection, plan changes, subscription management, and payment reconciliation. Include a visual, consumer-facing PDF as the primary setup guide, with editable documentation kept alongside it.

## 2. Compatibility and the necessary authentication addition

The surviving client supplies user IDs, email addresses, and Stripe identifiers to private endpoints/pages, but does not send an authentication credential after login. Those identifiers cannot establish ownership.

For a deployable replacement, preserve the endpoint names, form fields, and UI response shapes, and add a small authenticated-session extension to the existing Unity transport. Private endpoints will require a bearer token. Billing pages will require an authenticated browser handoff rather than trusting an email/customer ID from a query string.

This means a secure backend will not be a PHP-only drop-in for the currently unauthenticated private requests. The implementation includes the minimum corresponding C# changes, described in section 10. There will be no replacement scenes or client architecture.

Do not ship an unauthenticated compatibility mode as the default, or expose account/billing management merely to keep an old request working unchanged.

## 3. Proposed technology and deployment

- PHP with a currently supported release compatible with the selected hosting environment; target PHP 8.2+ syntax and verify supported runtime versions before deployment.
- MySQL/MariaDB through PDO, with prepared statements and transactional session/account updates.
- Composer-managed official Stripe PHP SDK.
- PHPMailer with configurable SMTP for email verification.
- Environment configuration outside the public document root.
- Stripe-hosted Checkout and Customer Portal for payment/billing UI.

Use the host's supported database/runtime versions and document the versions actually tested. Resolve and lock Composer dependencies during implementation. No server credentials or Stripe secret keys belong in Unity.

The first release supports multiple configured game identifiers, with accounts, sessions, plans, and subscription ownership scoped consistently to a game. The same email can belong to different games without sharing authentication or access accidentally.

## 4. Proposed backend layout

```text
Backend/SubscriptionServer/
  public/
    insertUser.php
    loginVerify.php
    getinfouser.php
    getinfo.php
    sendVerification.php
    logout.php
    pilihplan.php
    tukarplan.php
    manageSub.php
    verifyEmail.php
    createBillingLink.php
    createCheckout.php
    createPortal.php
    paymentSuccess.php
    paymentCancel.php
    stripeWebhook.php
    health.php
    assets/
  src/
    bootstrap.php
    Config.php
    Database.php
    Request.php
    Response.php
    Auth.php
    Accounts.php
    Verification.php
    Mailer.php
    Billing.php
    SubscriptionSync.php
    RateLimit.php
  config/
    games.example.php
  database/
    migrations/
    seeds/
  bin/
    migrate.php
    seed.php
    processWebhooks.php
    reconcileSubscriptions.php
    cleanup.php
    checkConfig.php
  tests/
  .env.example
  composer.json
  composer.lock
  README.md
```

Only `public` is web-accessible. Shared classes, database files, logs, Composer dependencies, and environment secrets stay outside it. These are proposed file responsibilities; consolidate small helpers if separate classes would add unnecessary complexity.

## 5. All existing endpoint/page responsibilities

### insertUser.php — register an account

- Accept `_name`, `_email`, `_notel`, `_password`, and `_game_name` as the existing client sends them.
- Validate the configured game, field lengths, normalized email, phone policy, and password policy server-side.
- Enforce unique email/phone rules within that game, including concurrent registration attempts.
- Store a password hash, never the plaintext password.
- Create an unverified account and queue its initial verification email after committing the account.
- Return plain text beginning with `Successful` for successful registration.
- Preserve `email already registered`, `phone number already registered`, and `error` where needed by the existing form.
- Do not undo a completed registration solely because SMTP is temporarily unavailable; allow verification resend and make delivery status clear.

### loginVerify.php — authenticate and establish a device session

- Accept `_email`, `_password`, `_device_name`, and `_game_name`.
- Verify the password and account/game identity.
- Issue a cryptographically random session token on accepted login; store its hash server-side.
- Add `session_token` and `session_expires_at` to the existing user JSON.
- Preserve the legacy distinction: accepted login responds with `login_logs: 0`; later authenticated profile responses return `login_logs: 1` while that session is valid.
- Use a transaction/lock so simultaneous logins cannot both acquire the single active session.
- Treat device name as a display label, not a secret or proof of device identity.
- Preserve the existing invalid-email/password literals where the Unity UI depends on them; rate-limit attempts and return controlled errors.

For the existing other-device panel, a correct-password login that encounters another active session returns `login_logs: 1`, the existing device label, and a short-lived `session_action_token`. That token can authorize revocation of the conflicting session only; it must not authorize profile reads, billing, or premium access. No full session token is issued until login succeeds.

### getinfouser.php — refresh the current user

- Require the session bearer token.
- Continue accepting `id_user` and `_game_name`; cross-check them against the authenticated account rather than trusting them.
- Return the existing user fields, with integer `id_user`/`login_logs` and string `confirmation`.
- Update last-seen state under a documented session-expiry policy.
- Return 401 for an invalid/revoked/expired credential; never return another user's profile based on a supplied ID.

### getinfo.php — return the current subscription

- Require a valid account session and verify the supplied `_email`/`_game_name` match it.
- Return the current subscription summary using the existing field names.
- Return `no result` only for a confirmed absence of a subscription.
- Keep provider/network/database failures separate from a confirmed free account.
- Return server-derived `status`, plan details, provider identifiers, and display dates.

The existing `CanAccessPremium()` accepts `active`. The initial server policy must match that behavior. If trials or grace periods are introduced, update the access contract explicitly rather than mapping every provider state to active.

### sendVerification.php — resend verification email

- Require account authentication and use its stored name/email.
- Continue accepting `_email` and `_name` for compatibility, but do not let them redirect another account's verification email.
- Rate-limit sends, expire/revoke old tokens according to policy, and return `success` when the configured delivery path accepts the request.
- Provide a development mail-log transport that is unavailable in production.

### logout.php — revoke a session

- Require either the current bearer token or the restricted, password-verified session-action token from the other-device flow.
- Continue accepting `id_user` only as a consistency check.
- Revoke only the authorized session/account scope and return `logout`.
- Never permit logging out another player merely by guessing their numeric ID.

### pilihplan.php — existing plan-selection page

- Enter through a short-lived billing handoff created for an authenticated, verified account.
- Show server-configured plans and their prices/currency/renewal terms.
- Ignore an email query parameter as an authorization source.
- Use a CSRF-protected POST to create Checkout and redirect to its hosted payment page.
- Never accept an arbitrary Stripe price/customer ID from the browser as the account's entitlement mapping.

### tukarplan.php — existing change-plan page

- Verify browser/account/game ownership.
- Show current plan and available configured alternatives.
- Use the configured Stripe Customer Portal for supported changes, with an explanation of the applicable effective date/proration policy.
- Keep price mappings and portal configuration aligned.
- Do not change subscriptions through an unprotected GET request.

### manageSub.php — existing subscription-management page

- Verify browser/account ownership before creating a short-lived Customer Portal session.
- Allow only the operations enabled in the server/Stripe portal configuration.
- Use the stored customer mapping, not a caller-selected `stripe_id`.
- Provide a return page telling the player to return to Unity and refresh.

## 6. Additional PHP files the existing flow needs

| File | Why it is needed |
| --- | --- |
| `verifyEmail.php` | Consume an expiring verification link and mark the account verified |
| `createBillingLink.php` | Convert an authenticated Unity request into a short-lived, restricted browser handoff |
| `createCheckout.php` | Create a server-controlled Stripe subscription Checkout session |
| `createPortal.php` | Create an ownership-checked Stripe billing management session |
| `paymentSuccess.php` | Show payment processing/completion feedback without granting access from the URL |
| `paymentCancel.php` | Return gracefully when checkout is canceled |
| `stripeWebhook.php` | Validate and durably receive provider events |
| `health.php` | Check API version/readiness without exposing secrets or configuration details |
| CLI migration/seed scripts | Create the database and configured game/plan catalog reproducibly |
| CLI webhook/reconciliation scripts | Process received events and repair missed/out-of-order subscription state |
| CLI cleanup/configuration checks | Remove expired operational records and detect deployment mistakes |

Payment mutations use authenticated POST requests with browser CSRF protection. Verification links are expiring, single-purpose tokens and must not expose user passwords or session credentials.

## 7. Database design

Proposed tables:

| Table | Main data |
| --- | --- |
| `games` | Stable game key, display name, active flag, allowed plan mappings |
| `users` | Game ID, user ID, name, normalized email/phone, password hash, verification state |
| `sessions` | User/game binding, hashed token, device label, expiry, last seen, revoked state |
| `session_actions` | Hashed, short-lived token scoped to conflicting-session revocation |
| `verification_tokens` | Hashed one-time email token, account, expiry, consumption/revocation |
| `billing_handoffs` | Hashed browser ticket, account/game, allowed action, expiry, one-time exchange state |
| `billing_customers` | Unique account/game to Stripe customer mapping |
| `plans` | Public plan key, Stripe price/product mapping, display text, active flag |
| `subscriptions` | Provider subscription/item mapping, billing status, period boundaries, cancellation flags |
| `checkout_attempts` | Durable idempotency/reuse state to prevent accidental duplicate purchases |
| `webhook_events` | Unique provider event ID, payload/metadata, processing state, retry information |
| `rate_limits` / mail outbox | Bounded request attempts and reliable verification-email delivery |

Use foreign keys, unique constraints, UTC timestamps, and transactional updates. Do not store card numbers/CVC. Retain only necessary provider references and account data.

Keep cancellation time and paid-through period end separate internally. Map `start_date` and `end_date` to documented meanings in the Unity response; if the existing end-date display becomes paid-through expiry, change its existing label to make that explicit.

## 8. Session and browser lifecycle

### Unity API session

1. Validate credentials and acquire the active session atomically.
2. Return a session token only on accepted login.
3. Send it through the Authorization header on private requests.
4. Reject mismatched user/game fields even when a valid credential is present.
5. Apply configurable idle/absolute expiry; a profile poll must not revive a revoked session.
6. Clear the credential with account data on logout/401.

For the first implementation, keep the token in memory. After restarting the app, cached PlayerPrefs alone must not restore authentication. This changes the old cached-login behavior deliberately; document it and handle it cleanly in the same login scene. Persistent sign-in requires a platform credential-vault adapter and a verified restore flow, and is separate follow-up work rather than plaintext token storage in PlayerPrefs.

### Browser handoff

1. Unity requests a billing link using its bearer credential and the desired allowed action.
2. The server checks verification, ownership, game, and plan availability.
3. It returns an HTTPS URL to the existing named billing page with an expiring one-time ticket.
4. The browser exchanges that ticket for a restricted browser session, with Secure/HttpOnly cookie attributes and an appropriate SameSite policy.
5. Redirect to a clean URL after exchange. Use no-referrer/no-store headers and redact tickets from logs.
6. Actual checkout/portal creation requires the restricted browser session and a CSRF-protected user action. Merely loading a ticket URL must not perform a billing mutation.

Do not put the long-lived Unity bearer token into a query string. Invalid/reused/expired handoffs display a clear return-to-game message.

## 9. Stripe subscription synchronization

- Create/reuse the account's customer server-side.
- Select prices from the game's server-owned plan catalog.
- Persist checkout intent/idempotency state and reuse or expire pending sessions to prevent duplicate purchases.
- Confirm eligibility before opening another checkout for an already subscribed account.
- Verify webhook signatures against the unmodified raw request body.
- Persist event receipt before acknowledging it, deduplicate provider event IDs, and process events with retry/error tracking.
- Reconcile against current provider state when events are delayed or out of order. Serialize reconciliation per subscription/account so an older concurrent result cannot overwrite newer state.
- Handle subscription creation/update/deletion and invoice/payment events needed by the chosen access policy.
- Keep pending checkout, failed payment, canceled-at-period-end, and expired access distinct.
- Provide a scheduled reconciliation job and monitoring for failed webhook processing.

Success/cancel page navigation does not activate a plan. Unity refreshes through `getinfo.php` after browser return; that endpoint serves reconciled server state and reports pending/unavailable state honestly.

Default policy: active subscriptions grant premium access; cancellation at period end retains active access until the paid period ends; incomplete, unpaid, expired, and canceled subscriptions do not grant access. Explicitly define past-due handling and any future trials before implementing them.

## 10. Minimal changes to existing Unity code

These are proposed alongside the PHP work because the current transport lacks private-request authentication:

| Existing file | Required adjustment |
| --- | --- |
| `Player.cs` | Add session token/expiry and restricted action-token response fields; attach Authorization to private requests; report HTTP status codes |
| `LoginController.cs` | Store accepted session credentials in manager memory; preserve the existing other-device panel using its restricted action token |
| `PlayerManager.cs` | Clear credentials on logout/401; reject cached-login restoration without a credential; retain current refresh coordination and gameplay navigation |
| `LoadPaymentSite.cs` | Request an authenticated billing handoff, then open the returned trusted HTTPS URL through the existing WebView/browser flow |
| `SubscriptionSettings.cs` | Add the billing-link endpoint; retain the existing six API and three page filenames |
| Editor regression checks | Update transport fixtures and test expiry, revocation, handoff errors, and ownership mismatches |

The existing forms, button entry points, and scene hierarchy stay in place. The backend base URL and game identifier remain configured through the same settings asset shown in the screenshot.

Registration phone/email fields and existing response keys remain compatible. Browser-supplied customer/email/price values become hints at most, never authorization.

## 11. Configuration to document

`.env.example` and setup guidance will cover:

- Environment name, HTTPS public base URL including any hosting subfolder, and application secret where needed.
- Database host/port/name/user/password.
- SMTP host/port/encryption/user/password/from address and development-only mail transport.
- Stripe test/live secret keys, webhook signing secret, API version, and portal configuration.
- Session, action-token, verification-token, and handoff lifetimes.
- Rate-limit settings, trusted proxy configuration, log locations, and worker/cron commands.

The plan catalog/game seed configuration holds game keys and allowed price mappings. Check consistency with Unity's Game Identifier.

Test and live deployments use separate credentials, databases, and webhook endpoints. Configuration checks must detect missing secrets, invalid return URLs, public secret-file exposure, and environment mismatches without printing credentials.

## 12. Implementation phases

### Phase 1 — Finalize the client/server contract

Document every existing request/response, the additive credential fields, status-code behavior, device-conflict flow, browser handoff, date meanings, and subscription access policy. Establish the database schema and hosting/subfolder layout.

Acceptance: each Unity screen action maps to an explicitly defined backend operation; no private operation relies on a supplied ID/email alone.

### Phase 2 — Account, session, and verification endpoints

Implement configuration/bootstrap, migrations, account validation, password hashing, login/session locking, profile reads, logout/action-token revocation, verification links, SMTP delivery, and rate limiting.

Acceptance: registration/login/verification/logout work with a fresh database and authenticated ownership checks, including concurrent logins and expired tokens.

### Phase 3 — Subscription and billing pages

Implement the game/plan catalog, customer mappings, authenticated billing links, existing named pages, Checkout creation, Portal redirects, and success/cancel pages.

Acceptance: a verified test account can enter its own plan and management flow; changing customer IDs or plan fields cannot access another account or arbitrary prices.

### Phase 4 — Webhooks and reconciliation

Implement signature verification, durable receipt, idempotent processing, retries, current-state reconciliation, and cleanup/monitoring commands.

Acceptance: test subscription state converges correctly despite duplicate, missing, delayed, or out-of-order events; return URLs alone never grant access.

### Phase 5 — Minimal Unity integration

Update the existing transport/manager/payment methods for credentials and handoffs, without creating scenes or replacing UI. Update settings defaults, editor checks, and setup documentation.

Acceptance: the same LogMasuk/LamanUtama workflows operate with the reconstructed backend; invalid credentials and network failures recover through the existing notices.

### Phase 6 — Deployment guide and end-to-end checks

Provide server installation, database creation, Composer install, environment setup, game/plan seeding, SMTP configuration, webhook registration, cron setup, Unity configuration, and a test-mode purchase walkthrough. Produce the illustrated PDF described in section 14 from the completed implementation and verify its page layout before delivery.

Acceptance: a consumer can follow the PDF to deploy a clean backend and configure the existing Unity project without editing ordinary deployment values into source code. Screenshots, example configuration, commands, and expected results match the delivered version.

## 13. Verification matrix

- Registration validation, duplicate email/phone, concurrent registration, and password hashing.
- Correct/incorrect credentials, simultaneous login attempts, stale device sessions, revocation, and expiry.
- Cross-game and cross-account ID/email/token substitution.
- Restricted action tokens cannot read profiles or create billing links.
- Verification resend limits, expired/reused links, SMTP failure, and production rejection of development mail logging.
- Empty subscription versus provider/database outage.
- Handoff expiry/replay, CSRF failures, and manipulated billing/customer/price parameters.
- Duplicate checkout clicks, interrupted requests, and pending checkout reuse.
- Correct and incorrect webhook signatures, stale signed events, duplicates, retry after worker failure, and out-of-order reconciliation.
- Activation, renewal failure, plan changes, immediate cancellation, cancellation at period end, and expiry.
- Existing Unity notices, credential cleanup, browser return, and premium access updates.

Use isolated databases, a fake provider adapter for repeatable backend tests, and authorized Stripe test-mode integration checks. No real payment is needed for development verification.

## 14. Setup walkthrough to deliver

1. Provision an HTTPS PHP/database host or local HTTPS development environment.
2. Upload/install the backend with only `public` exposed to the web.
3. Install locked Composer dependencies.
4. Create private environment configuration and database credentials.
5. Run migrations and seed the game/plan catalog.
6. Configure SMTP and verify a test mailbox.
7. Configure Stripe test-mode prices, Customer Portal, and webhooks.
8. Configure webhook processing/reconciliation/cleanup jobs and check server health.
9. In Unity, enter the backend base folder and matching Game Identifier in the current SubscriptionSettings asset.
10. Test registration, verification, login/device conflict, subscription, plan changes, management, and logout.
11. Validate desktop/browser and mobile WebView return behavior.
12. Complete the production checklist using separate live configuration when ready.

The deployment guide must cover both an origin-root installation and a hosting subfolder, since the current Unity base URL supports a folder path.

### 14.1 Primary deliverable: visual PDF setup guide

Create `output/pdf/Subscription_System_Setup_Guide.pdf` for purchasers/game owners setting up this template, including readers with limited backend experience. Use plain English, numbered actions, and the exact labels visible in Unity and the delivered server setup. Preserve Malay UI labels in screenshots and explain their meaning where helpful.

The PDF is the primary consumer walkthrough. Keep an editable source under `Docs/SetupGuide/`, including the guide text, illustration/screenshot assets, and a reproducible PDF-generation script. Keep the server README and technical API contract available as references; the consumer should not need to read source code to complete the normal setup.

The guide will be written against the implemented backend, rather than presenting proposed filenames or untested commands as finished instructions.

### 14.2 Guide structure

| Part | What the consumer learns | Visual/support material |
| --- | --- | --- |
| Start here | What is included, who needs hosting, and what must already be available | One-page checklist and setup roadmap |
| How the system works | Unity talks to the PHP server; the server uses the database, SMTP, and Stripe | Clearly labeled architecture diagram |
| Prepare your accounts | Hosting/domain/HTTPS, database access, SMTP account, Stripe test environment, and matching Unity version | Prerequisite checklist and a worksheet for non-secret setup values |
| Upload the backend | Which folder becomes the web document root and which files remain private | Annotated folder tree and correct/incorrect upload examples |
| Create the database | Create a database/user, grant required access, enter connection settings, and run migrations | Hosting/database screenshots where available and an example successful migration result |
| Configure the server | Copy the environment example and fill in the required values | Redacted, annotated configuration example and a field-to-value table |
| Configure email | Enter SMTP settings and test verification delivery | SMTP field mapping and verification-email/link examples |
| Configure Stripe | Create a recurring product/price, configure the Portal, register the webhook, and copy the correct test values | Numbered callouts, plan-mapping diagram, and test/live environment indicators |
| Run background jobs | Configure webhook processing, reconciliation, and cleanup using the hosting scheduler | Example scheduler entries with an explanation of each job |
| Connect Unity | Open the existing settings asset and enter the backend URL, game identifier, and paths | Annotated SubscriptionSettings Inspector screenshot based on the current project |
| Test the whole flow | Register, verify, sign in, subscribe in test mode, return to the game, change/cancel a plan, and log out | Screenshots of the existing LogMasuk/LamanUtama screens and expected status transitions |
| Fix common problems | Identify the failed checkpoint and take a specific corrective action | Symptom/cause/action tables and example error messages |
| Prepare for release | Separate test/live settings, verify the target-platform billing approach, and back up configuration/data | Go-live checklist and test/live comparison table |
| Reference | Find file responsibilities, endpoint names, commands, and relevant official documentation | Clickable links, glossary, and concise appendices |

Document one recommended hosting path end to end, and clearly separate any supported alternative path so beginners do not mix instructions. Include a hosting-subfolder example because the Unity Backend Base URL can point to a folder. Describe what to do when a shared host lacks Composer/SSH only if that deployment route is supported and tested.

### 14.3 Step format and checkpoints

Each setup step should contain:

1. **Where to go:** the exact application, page, menu, or file to open.
2. **What to do:** a short numbered instruction or a copyable command.
3. **What to enter:** a concrete example using harmless placeholder values.
4. **What success looks like:** a screenshot or a clearly labeled expected result.
5. **If it fails:** a direct troubleshooting reference or recovery instruction.

Keep one main task per step and place the corresponding visual next to it. Add completion checkpoints after database setup, email delivery, backend health, Unity connection, and test subscription activation. Explain optional steps explicitly; do not make the consumer infer which commands apply to their environment.

Use searchable/selectable text for commands, paths, and configuration examples. Screenshots must not be the only source of values that a consumer needs to copy.

### 14.4 Visual requirements

- Use actual screenshots of the completed Unity settings and the existing application screens, with sample/test accounts.
- Use numbered callouts, arrows, highlights, captions, and a small legend to show exactly which field/button the instruction refers to.
- Include a configuration mapping visual connecting the server public URL and game key to Unity's Backend Base URL and Game Identifier.
- Include a flow diagram showing email verification and a separate diagram showing payment confirmation through the server/provider, rather than through a return-page click.
- Include a folder-layout diagram explaining `public`, private configuration, dependencies, and database/maintenance files.
- Keep screenshot crops focused and labels legible at normal PDF viewing size.
- Do not expose passwords, bearer tokens, handoff tickets, SMTP credentials, or payment secrets in screenshots. Use obviously fictitious examples and identify where real private values belong.
- If access to a hosting or Stripe dashboard is unavailable, use a clearly labeled illustrative diagram and a link to the relevant official instructions. Do not fabricate an actual screenshot or claim that an inaccessible account/setup was tested.
- Treat the supplied Inspector screenshot as a useful reference, but capture the final implemented settings again if fields change. Do not rely on a temporary clipboard image as the only retained source asset.

### 14.5 Consumer-friendly presentation

- Use a consistent printable page size, readable body text, clear headings, page numbers, and a clickable contents page/bookmarks.
- Organize the PDF into short sections with clear progress markers.
- Use consistent callout styles for required values, optional steps, expected results, and troubleshooting.
- Distinguish public configuration values from private server secrets in plain language.
- Keep detailed API internals and uncommon hosting variations in appendices.
- Include the guide's version/date and the tested Unity, PHP, database, and dependency versions.
- Keep instructions and diagrams synchronized with the delivered backend and Unity configuration; update the guide when their setup changes.

### 14.6 PDF production and quality checks

Generate the PDF from the editable guide source using a standard PDF authoring workflow. Render every final page to images and visually inspect the complete document before delivery. Check for clipped text, overlapping callouts, unreadable screenshots, broken tables, missing glyphs, and awkward page breaks; correct and re-render as needed.

Also verify extracted text, copyable commands, page numbering, bookmarks/contents, and external/internal links. Walk through the instructions in order on a clean test installation, record expected results, and disclose any hosting, dashboard, device, or live-payment step that was not actually verified.

The final delivery should include the PDF and its editable/reproducible source. PDF generation happens during backend implementation after the relevant setup screens and commands exist; updating this proposal does not start that implementation.

## 15. Lost data and existing Stripe accounts

New PHP files do not recover a lost database. If old users/subscriptions still exist in Stripe, preserve those subscriptions and build a separate reviewed reconciliation/import process using provider records and verified account ownership. Do not create replacement paid subscriptions or associate customers solely by an unverified email match.

If no database backup exists, document account re-registration/password-recovery implications. Automatic recovery/import of historical users is outside the initial clean-server implementation.

## 16. Completion criteria

- All six existing API filenames and three existing billing pages are implemented.
- Required verification, browser handoff, provider, maintenance, and database files are included.
- The current Unity UI works with the documented authentication extension.
- Secrets remain server-side; account/game ownership is enforced consistently.
- Subscription access is derived from reconciled provider state.
- A fresh installation is reproducible from migrations, configuration examples, and setup instructions.
- A consumer-facing PDF setup guide is included with annotated visuals, step-by-step checkpoints, troubleshooting, and clickable navigation.
- The PDF matches the delivered implementation, has been rendered and visually checked page by page, and includes editable/reproducible source.
- Tests and actual hosting/device/payment verification limits are reported accurately.
- No live data migration, deployment, payment-account change, or charge occurs without a separate explicit instruction.

## References used for this proposal

- [Stripe Checkout integration](https://docs.stripe.com/checkout/quickstart?client=html&lang=php)
- [Stripe webhook handling](https://docs.stripe.com/webhooks)
- [Stripe Customer Portal integration](https://docs.stripe.com/customer-management/integrate-customer-portal)
- [PHP password hashing](https://www.php.net/manual/en/function.password-hash.php)
- [PHPMailer project and SMTP guidance](https://github.com/PHPMailer/PHPMailer)

Implementation completed following approval. See Backend/SubscriptionServer/README.md for installation and API details, Docs/VERIFICATION.md for validation results and limits, and output/pdf/Subscription_System_Setup_Guide.pdf for the illustrated consumer guide. Existing scenes and UI are retained. The proposal above records the approved scope.
