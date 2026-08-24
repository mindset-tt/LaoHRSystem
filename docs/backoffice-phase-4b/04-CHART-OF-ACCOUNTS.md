# 04 — Chart of Accounts

## Entity
`Account`: AccountCode (unique), Name, NameLao, AccountType, ParentAccountId,
IsPostingAccount, IsActive, Currency.

## Account types
`ASSET`, `LIABILITY`, `EQUITY`, `REVENUE`, `EXPENSE` (configurable; no statutory
numbering generated).

## Hierarchy
Parent/child via `ParentAccountId`. Cycle prevention at the service/controller
level (parent must not be self or a descendant).

## Posting account
`IsPostingAccount == false` = header/summary account; journal lines must target
posting accounts.

## Deactivation
Accounts with journal history cannot be deleted — deactivate instead.

## Access
`CanManageCoa` (Admin/Finance only).
