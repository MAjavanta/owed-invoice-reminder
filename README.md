# Owed Invoice Reminder

---

## 1. Landing page

The user is a UK freelancer. They've been chasing a late invoice manually — emails ignored, awkward phone calls. They find the app via Reddit, a search result, or a TikTok.

**What they see:**
- Headline: "Stop chasing late invoices. Let UK law do it for you."
- Sub-headline: "Automated payment reminders that escalate legally — from gentle nudge to statutory interest calculation."
- Three-step explainer: Add invoice → We send reminders on your behalf → Get paid
- Pricing: 14-day free trial, then £11/month
- Two CTAs: "Start free trial" and "See how it works"

**What they do:** Click "Start free trial."

---

## 2. Registration

**What they see:** A simple form — Name/business name, email address, password.

**What they do:** Fill it in, click "Create account."

**What happens:**
- Frontend: POST `/api/auth/register` with `{ name, email, password }`
- Backend: validates the request (FluentValidation), checks email isn't already taken, hashes the password with BCrypt, creates a `User` record in the database, generates a JWT containing the user's ID and email, returns `{ token, user }`
- Frontend: stores the JWT in localStorage, stores the user's name in an auth context, redirects to `/dashboard`

---

## 3. First-time dashboard

**What they see:** An empty state. A friendly message: "You haven't added any invoices yet." One prominent button: "Add your first invoice." A subtle secondary prompt: "Set up your account first → Settings."

**Why settings first matters:** The name and email signature in their Settings is what appears in every reminder email sent to their clients. If they skip this, reminders go out saying "A user" instead of their business name. The app should gently nudge them here before they add anything.

---

## 4. Settings (first visit)

**What they see:**
- Business name (pre-filled from registration)
- Your email address (shown, not editable here — for reference, clients see this as the reply-to)
- Email signature (optional — free text, e.g. "Kind regards, Jamie — jamie@freelance.co.uk | 07700 900000")
- Reminder schedule (the default shown with descriptions — they can change timing but most won't)

**The default reminder schedule shown:**
| Reminder | Timing | Tone |
|---|---|---|
| 1st | 1 day after due date | Polite — "just checking in" |
| 2nd | 7 days overdue | Firmer follow-up |
| 3rd | 14 days overdue | References the Late Payment Act |
| 4th | 30 days overdue | Calculates statutory interest, formal |

**What they do:** Update their business name, maybe add a signature, click Save.

**What happens:** PATCH `/api/users/settings` — backend updates the user record.

---

## 5. Adding an invoice

**What they see:** A form with the following fields:
- Client / company name (required)
- Client email address (required — this is where all reminders go)
- Invoice reference number (optional — their own ref like "INV-042")
- Invoice amount (£, required)
- Invoice date (when it was issued — optional, for their records)
- Due date (required — this is what the reminder schedule is based on)

Below the form, a live preview updates as they fill it in:
> "First reminder will send on **14 Feb 2026** if unpaid."

**What they do:** Fill in the form, click "Add invoice."

**What happens:**
- Frontend: POST `/api/invoices` with the form data
- Backend: validates, creates an `Invoice` record with `status = "Outstanding"`, returns the created invoice
- Frontend: React Query invalidates the invoice list query, the dashboard re-fetches and shows the new invoice

---

## 6. The invoice dashboard

**What they see:** A list/table of their invoices. Each row shows:
- Client name
- Invoice ref (or "—" if none)
- Amount (£)
- Due date
- Status badge — colour-coded:
  - Grey: **Outstanding** (not yet due)
  - Yellow: **Overdue** (1–6 days)
  - Amber: **Overdue** (7–29 days)
  - Red: **Seriously overdue** (30+ days)
  - Green: **Paid**
- Next action text — e.g. "1st reminder sends tomorrow" or "2nd reminder sent 3 days ago" or "Next: 3rd reminder in 4 days"

Summary cards at the top:
- Total outstanding: £X
- Total overdue: £X
- Invoices paid this month: X

**What they can do:**
- Click "Add invoice" for a new one
- Click any invoice row to open the detail view
- Mark an invoice as paid inline

---

## 7. Invoice detail page

**What they see:** All the invoice info, plus a timeline of reminder activity:

```
Invoice: INV-042
Client: Acme Ltd (billing@acme.co.uk)
Amount: £1,200.00
Due date: 1 Jan 2026
Status: Overdue — 14 days

Reminder timeline:
  ✓  2 Jan 2026 — 1st reminder sent (gentle)
  ✓  8 Jan 2026 — 2nd reminder sent (follow-up)
  →  15 Jan 2026 — 3rd reminder scheduled (Late Payment Act)
     31 Jan 2026 — 4th reminder scheduled (statutory interest)
```

If the invoice is 30+ days overdue, a callout shows:
> "Statutory interest is now accruing at 13.5% per annum (Bank of England base rate + 8%). Interest so far: **£13.42**. This amount is included in your next reminder."

**Actions available:**
- "Mark as paid" — stops all future reminders
- "Send a reminder now" — manually triggers the next reminder immediately (useful if client promised payment and didn't follow through)
- "Edit invoice" — update amount, due date, client email
- "Delete invoice" — with confirmation dialog

---

## 8. The automated reminder job (background — invisible to user)

This is the core of the product. The user doesn't see this happening, but it's what they're paying for.

**When it runs:** Daily at 8:00am UK time (Europe/London timezone, handles BST/GMT automatically), triggered by Quartz.NET.

**What it does, step by step:**
1. Queries all invoices where `status != "Paid"` and `dueDate < today`
2. For each overdue invoice:
   a. Calculates `daysOverdue = today - dueDate`
   b. Fetches the list of reminders already sent for this invoice
   c. Determines which reminder number is next based on the user's schedule
   d. Checks whether that reminder is due today (e.g. if schedule says day 7, and it's been exactly 7 days)
   e. If due: sends the email via Resend, inserts an `InvoiceReminder` record into the database (invoiceId, reminderNumber, sentAt)
3. Logs the result for each invoice: sent, skipped (not due yet), or errored
4. If an email send fails: logs the error, marks the reminder as failed, Sentry captures the exception

**Calculating statutory interest:**
```
Annual rate = Bank of England base rate + 8%
Daily rate = Annual rate / 365
Interest = Invoice amount × daily rate × days overdue
```
This is calculated fresh on each run so the 4th reminder email always shows the current accrued amount.

---

## 9. What the client receives (the reminder emails)

The emails are sent from your app's sending domain (e.g. `reminders@yourdomain.com`) with the reply-to set to the freelancer's email address. The client hits reply and it goes straight to the freelancer.

**Email 1 — Day 1, polite:**
> Subject: Payment reminder — Invoice INV-042 (£1,200.00)
>
> Hi Acme Ltd,
>
> This is a friendly reminder that invoice INV-042 for £1,200.00 was due on 1 January 2026 and remains outstanding.
>
> If payment has already been made, please disregard this message. If you have any questions, please reply to this email.
>
> Kind regards,
> Jamie
> jamie@freelance.co.uk

**Email 3 — Day 14, references legislation:**
> Subject: Overdue payment — Invoice INV-042 (£1,200.00) — 14 days outstanding
>
> Hi Acme Ltd,
>
> Invoice INV-042 for £1,200.00 remains unpaid after 14 days. I would appreciate prompt payment to avoid further action.
>
> Please be aware that under the **Late Payment of Commercial Debts (Interest) Act 1998**, I am entitled to claim statutory interest on overdue payments from businesses.
>
> If you are experiencing difficulties, please reply to discuss payment terms.
>
> Kind regards, Jamie

**Email 4 — Day 30+, statutory interest:**
> Subject: Final notice — Invoice INV-042 — Statutory interest now accruing
>
> Hi Acme Ltd,
>
> Invoice INV-042 for £1,200.00 is now 30 days overdue. Under the Late Payment of Commercial Debts (Interest) Act 1998, statutory interest is accruing at 13.5% per annum.
>
> Interest accrued to date: **£13.32**
> Total now owed: **£1,213.32**
>
> Please arrange payment immediately. If payment is not received, I reserve the right to pursue the outstanding debt plus interest through the appropriate legal channels.
>
> Kind regards, Jamie

---

## 10. Marking an invoice as paid

**What happens when the client pays:**
- Money arrives in the freelancer's bank account (outside the app — bank transfer, Wise, whatever)
- The freelancer opens the app and sees the invoice still showing as overdue
- Clicks "Mark as paid" (either from the list or the detail page)
- PATCH `/api/invoices/{id}` with `{ status: "Paid", paidAt: today }`
- Backend updates the invoice record
- Background job will find `status = "Paid"` and skip this invoice forever
- Dashboard stats update — paid total goes up, overdue total goes down

The app does not process the actual payment. It only tracks whether you've marked it paid.

---

## 11. Subscription — end of free trial

After 14 days, if the user hasn't subscribed, a banner appears at the top of every page:
> "Your free trial has ended. Subscribe for £11/month to keep your reminders running."

Any invoices they added are still visible but reminders have paused.

**When they click Subscribe:**
1. Frontend: POST `/api/payments/create-checkout-session`
2. Backend: creates a Stripe Checkout Session with the monthly price, passes the user's email, sets success and cancel URLs, returns the session URL
3. Frontend: `window.location.href = checkoutUrl` — user is sent to Stripe's hosted checkout page
4. User enters card details on Stripe's page (your app never sees the card number)
5. Stripe redirects to `/dashboard?subscribed=true` on success
6. **Simultaneously:** Stripe fires a `checkout.session.completed` webhook to your API endpoint `/api/webhooks/stripe`
7. Backend webhook handler: validates the Stripe signature (important — don't skip this), updates the user's `subscriptionStatus = "Active"` and `stripeCustomerId` in the database
8. Reminders resume immediately

**Important:** Don't rely on the success redirect to activate the subscription. The webhook is the source of truth. The redirect can fail (user closes the tab); the webhook always fires.

---

## 12. Managing the subscription

**What the user sees:** In Settings, a "Manage subscription" button.

**What happens:**
1. Frontend: POST `/api/payments/create-portal-session`
2. Backend: creates a Stripe Customer Portal session for this user's Stripe customer ID, returns the URL
3. Frontend: redirects to Stripe's hosted portal
4. User can update their card, see billing history, or cancel
5. Stripe's portal handles everything — no UI to build

**If they cancel:**
- Stripe fires `customer.subscription.deleted` webhook
- Backend handler: sets user's `subscriptionStatus = "Cancelled"`
- Background job checks subscription status before sending any reminders — pauses them
- User's data and invoices remain (they can resubscribe later)

---

## 13. Data model summary

These are the five tables your database needs at MVP:

**Users**
`id, name, email, passwordHash, businessName, emailSignature, stripeCustomerId, subscriptionStatus, createdAt`

**Invoices**
`id, userId, clientName, clientEmail, invoiceRef, amountPence, invoiceDate, dueDate, status, paidAt, createdAt`

**InvoiceReminders**
`id, invoiceId, reminderNumber, sentAt, success, errorMessage`

**ReminderSchedules**
`id, userId, reminderNumber, daysAfterDue`
_(seeded with defaults: 1, 7, 14, 30 — user can customise)_

**WebhookEvents**
`id, stripeEventId, eventType, processedAt`
_(prevents processing the same webhook twice if Stripe retries)_

---

## 14. What a typical week looks like for the user

Monday: logs in, sees Acme Ltd invoice is now 7 days overdue. Dashboard shows "2nd reminder sent this morning."

Wednesday: gets an email notification (optional feature) that the 2nd reminder bounced — client email was wrong. Updates the email address.

Friday: client pays. Logs in, marks the invoice as paid. Overdue count drops by one.

Next Monday: adds two new invoices from last week's work. Sets due dates 30 days out. App confirms when first reminders will fire.

Total time in app that week: maybe 8 minutes.