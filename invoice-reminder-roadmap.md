# Invoice Reminder — Build Roadmap

---

## Phase 1 — API with persistence
*You can create, read, update, and delete invoice records via API calls. Nothing else.*

- Create ASP.NET Core Web API project (.NET 8)
- Install EF Core + Npgsql provider
- Install Docker Desktop, run PostgreSQL locally via Docker
- Create `Invoice` model: `Id, ClientName, ClientEmail, InvoiceRef, AmountPence, DueDate, Status, CreatedAt`
- Create `AppDbContext`, register with DI in `Program.cs`
- Run first EF migration, verify table created
- Build `InvoicesController`: POST, GET all, GET by id, PATCH (edit), PATCH mark-paid, DELETE
- Enable Swagger (already included by default), test all endpoints via Swagger UI

**Done when:** you can create an invoice via Swagger and see it in the database.

---

## Phase 2 — Background worker (core product logic)
*A job runs on a schedule and identifies which invoices need a reminder. No emails yet — just console output.*

- Install Quartz.NET
- Register Quartz in `Program.cs` with a daily trigger (every minute while developing)
- Create `ReminderCheckJob`: queries all invoices where status != Paid and due date has passed
- Calculate days overdue for each
- Create `InvoiceReminder` table: `Id, InvoiceId, ReminderNumber, SentAt`
- Job determines which reminder number is next (1, 2, 3, 4) based on schedule (day 1, 7, 14, 30)
- For now: log to console "Would send reminder #X for invoice {ref} to {email}"
- Add statutory interest calculation method: `(AmountPence × (BoERate + 0.08)) / 365 × daysOverdue`
- Hardcode BoE base rate as a config value for now

**Done when:** console output shows the correct reminder numbers and interest figures for overdue test invoices.

---

## Phase 3 — React frontend (no auth yet)
*A UI where you can see invoices, add new ones, and mark them paid. Talks to local API.*

- Create React + TypeScript + Vite project in a separate folder (e.g. `/client`)
- Install Axios, TanStack Query, React Router, Tailwind CSS, shadcn/ui
- Configure Axios instance with `baseURL: http://localhost:5000`
- Configure CORS in the API to allow localhost:5173
- Invoice list page: fetch and display all invoices in a table with status badges
- Add invoice form: controlled form inputs, POST on submit, invalidate query on success
- Mark as paid button: PATCH call, table refreshes
- Basic navigation: Home (list) / Add Invoice
- Install `date-fns`, use it to show "X days overdue" in the table

**Done when:** you can open the browser, add an invoice via the form, see it in the list, and mark it paid.

---

## Phase 4 — Authentication
*Users must register and log in. Each user only sees their own invoices.*

- Add `User` model: `Id, Name, Email, PasswordHash, BusinessName, EmailSignature, CreatedAt`
- Add `UserId` FK to `Invoice`, run migration
- Install `BCrypt.Net-Next` and `Microsoft.AspNetCore.Authentication.JwtBearer`
- Build `AuthController`: POST `/register`, POST `/login` — both return a JWT
- Add JWT validation middleware in `Program.cs`
- Add `[Authorize]` to `InvoicesController`, scope all queries to `UserId` from JWT claims
- React: create `AuthContext` storing the JWT in localStorage
- Axios interceptor: attach `Authorization: Bearer {token}` to every request
- Add Login page, Register page
- Protected route wrapper: redirect to /login if no token
- Add logout button (clears localStorage, redirects)
- Settings page: GET/PATCH user name, business name, email signature

**Done when:** two separate accounts each only see their own invoices.

---

## Phase 5 — Real emails via Resend
*The background worker actually sends emails instead of logging.*

- Create a Resend account, get API key
- Install Resend .NET SDK (or use `HttpClient` with the REST API)
- Add `RESEND_API_KEY` to `appsettings.Development.json` (never commit this)
- Build `EmailService`: a C# class with a `SendReminderAsync(invoice, reminderNumber, daysOverdue)` method
- Write 4 HTML email templates as C# string interpolation (one per reminder tier)
- Email 4 includes the calculated statutory interest amount and the fixed compensation (£40/£70/£100)
- Wire `ReminderCheckJob` to call `EmailService` instead of `Console.WriteLine`
- After sending, insert a row into `InvoiceReminders`
- Add reply-to header set to the user's email address
- Test with a real email address in dev

**Done when:** adding an invoice with a past due date and waiting for the job to run results in an actual email arriving.

---

## Phase 6 — Deploy to Railway + Netlify
*The app is live on the internet.*

- Create Railway account, new project
- Add a PostgreSQL service in Railway, copy the connection string
- Add the API as a second Railway service from your GitHub repo
- Set all environment variables in Railway dashboard (DB connection string, JWT secret, Resend key)
- Update CORS in API to allow your Netlify domain
- Create Netlify account, import the `/client` repo (or subfolder)
- Set `VITE_API_URL` environment variable in Netlify to your Railway API URL
- Add `_redirects` file to `/client/public`: `/* /index.html 200`
- Test the full flow on the live URLs

**Done when:** you can register, log in, add an invoice, and receive a reminder email — all via the live production URLs.

---

## Phase 7 — GitHub workflow + CI
*Tests run automatically. main = production.*

- Ensure repo is on GitHub with `main` and `develop` branches
- Enable branch protection on `main` (require PR to merge)
- Write 3–5 unit tests: statutory interest calculation, reminder number logic, overdue day counting
- Create `.github/workflows/ci.yml`: on push, run `dotnet test`
- Add Railway deploy trigger on merge to `main` (Railway has a GitHub integration)
- Add Netlify auto-deploy (already set up by default when you connect GitHub)

**Done when:** pushing to `develop` runs tests in GitHub Actions; merging to `main` auto-deploys both API and frontend.

---

## Phase 8 — Stripe payments
*Users pay £11/month after a 14-day trial. App gates usage behind active subscription.*

- Create Stripe account, create a Product + recurring Price (£11/month)
- Install `Stripe.NET`
- Add `StripeCustomerId`, `SubscriptionStatus`, `TrialEndsAt` to `User`, run migration
- On register: set `TrialEndsAt = DateTime.UtcNow.AddDays(14)`
- Build `POST /payments/create-checkout-session`: creates Stripe Checkout Session, returns URL
- Build `POST /payments/create-portal-session`: creates Stripe Customer Portal session
- Build `POST /webhooks/stripe`: handles `checkout.session.completed` and `customer.subscription.deleted`
  - Store Stripe event IDs to prevent duplicate processing
- In `ReminderCheckJob`: skip users whose subscription is inactive and trial has ended
- In API: return 402 on invoice endpoints if user has no active subscription or trial
- React: show trial countdown banner, "Subscribe" button, "Manage subscription" link
- Add Stripe keys to Railway environment variables

**Done when:** a test card payment activates a subscription and reminders start flowing.

---

## Phase 9 — Production tooling
*Errors are caught. You can see what users are doing.*

- Install `Sentry.AspNetCore`, add DSN to environment variables, register in `Program.cs`
- Install `@sentry/react` in the frontend, wrap app in `Sentry.init`
- Install `Serilog`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`
- Replace default .NET logging with Serilog in `Program.cs`
- Add structured log properties to key operations (reminder sent, payment activated, invoice created)
- Install PostHog JS in React, add `posthog.capture()` on: invoice created, subscription started, trial started
- Identify user on login: `posthog.identify(userId, { email })`

**Done when:** a runtime error in production shows up in Sentry with a stack trace; PostHog shows an event for each invoice created.

---

## Phase 10 — Pre-launch polish
*The app is ready to show people.*

- Landing page: replace the React homepage with a proper marketing page (who it's for, how it works, CTA)
- Snooze feature: "client promised payment, pause reminders for X days" — adds a `SnoozedUntil` field to Invoice, job skips snoozed invoices
- Client grouping: invoices list can be grouped or filtered by client name
- Reminder history visible on invoice detail (list of InvoiceReminder rows with dates)
- Add Resend domain verification (your own domain so emails come from yourname@yourdomain.com)
- Settings: let users change their reminder schedule (which days reminders fire)
- "Send now" button: manually trigger the next reminder for a specific invoice outside the schedule
