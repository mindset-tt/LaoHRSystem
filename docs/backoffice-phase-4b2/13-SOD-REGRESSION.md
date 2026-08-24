# 13 — SOD REGRESSION

Segregation of duties regression.

## Policies

`SegregationOfDutiesService` enforces three configurable policies (conservative
defaults enabled):

| Policy | Setting key | Default |
|---|---|---|
| PreventInvoiceSelfApproval | `SOD_PREVENT_INVOICE_SELF_APPROVAL` | enabled |
| PreventPaymentSelfApproval | `SOD_PREVENT_PAYMENT_SELF_APPROVAL` | enabled |
| PreventJournalSelfPost | `SOD_PREVENT_JOURNAL_SELF_POST` | enabled |

## Tests

Enabled → rejected:
- `EnforceInvoiceApproval_SameCreator_Throws`
- `EnforcePaymentApproval_SameCreator_Throws`
- `EnforceJournalPost_SameCreator_Throws`
- `EnforceInvoiceApproval_DifferentApprover_Succeeds`

Disabled → permitted (if otherwise authorized):
- `EnforceInvoiceApproval_WhenPolicyDisabled_SameCreatorPermitted`
- `EnforcePaymentApproval_WhenPolicyDisabled_SameCreatorPermitted`
- `EnforceJournalPost_WhenPolicyDisabled_SameCreatorPermitted`

## Status

PASS — server-enforced, both enabled and disabled paths covered.
