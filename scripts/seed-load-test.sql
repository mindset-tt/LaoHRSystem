-- ============================================================
-- LaoHR Phase 4D.1 — LOAD/SOAK SEED (synthetic, DISPOSABLE DB ONLY)
-- INTERNAL_ENGINEERING_PROFILE — never run against production.
--
-- Scale:
--   8 departments / 10 positions / 500 employees
--   5 role users + 20 employee users
--   5,000 attendance rows
--   50 projects + 10 milestones + 500 tasks
--   200 suppliers / 300 POs (+600 lines)
--   3 warehouses / 6 categories / 200 items / 5,000 stock movements
--   300 assets
--   300 supplier invoices (+600 lines)
--   GL: 30 accounts, FY2026 + 12 periods,
--       200 journal entries / ~800 lines
--   150 contracts / 400 employee document metadata rows
--   6 service categories / 200 service requests
--   2 facilities / 20 rooms / 300 room bookings
--   30 vehicles / 300 vehicle bookings / 150 travel requests
-- ============================================================

-- License (signature-verified by API; test tenant license)
INSERT INTO "SystemSettings" ("SettingKey","SettingValue","UpdatedAt") VALUES ('LICENSE_KEY', '@LICENSE@', now())
ON CONFLICT ("SettingKey") DO UPDATE SET "SettingValue" = EXCLUDED."SettingValue";

-- Document-number counters start BEYOND every seeded synthetic number so live
-- writes during load tests never collide (e.g. 'SR-2026-100001' vs seeds).
INSERT INTO "NumberSequences" ("Prefix","Year","LastValue","UpdatedAt")
SELECT p.prefix, extract(year from now())::int, 100000, now()
FROM (VALUES ('AST'),('JE'),('CTR'),('RB'),('VB'),('DOC'),('GRN'),('PAY'),('PO'),('PR'),('SR'),('SUP'),('TRV'),('WO')) AS p(prefix)
ON CONFLICT DO NOTHING;

-- Org structure -------------------------------------------------------------
INSERT INTO "Departments" ("DepartmentName","DepartmentNameEn","DepartmentCode","IsActive","CreatedAt","SortOrder")
SELECT 'Dept ' || g, 'Department ' || g, 'D' || lpad(g::text,2,'0'), true, now(), g FROM generate_series(1,8) g;

INSERT INTO "Positions" ("Title","JobCode","IsActive","CreatedAt")
SELECT 'Position ' || g, 'P' || lpad(g::text,3,'0'), true, now() FROM generate_series(1,10) g;

-- Employees -----------------------------------------------------------------
INSERT INTO "Employees" ("EmployeeCode","LaoName","EnglishName","DependentCount","SalaryCurrency","DepartmentId","PositionId","JobTitle","HireDate","BaseSalary","BankName","BankAccount","IsActive","CreatedAt")
SELECT 'EMP' || lpad(g::text,4,'0'),
       'ພະນັກງານ ' || g, 'Employee ' || g,
       g % 4, 'LAK',
       1 + (g % 8), 1 + (g % 10), 'Specialist ' || (1 + (g % 10)),
       now() - ((g % 3000) || ' days')::interval,
       3000000 + (g * 25000),
       'BCEL', '001' || lpad(g::text,9,'0'),
       true, now()
FROM generate_series(1,500) g;

-- Users ----------------------------------------------------------------------
INSERT INTO "Users" ("Username","PasswordHash","PasswordHashVersion","Role","DisplayName","EmployeeId","IsActive","CreatedAt") VALUES
  ('admin',      '@HASH_admin@',      2, 'Admin',      'Load Admin',      NULL, true, now()),
  ('hradmin',    '@HASH_hradmin@',    2, 'HR',         'Load HR',         NULL, true, now()),
  ('finance',    '@HASH_finance@',    2, 'Finance',    'Load Finance',    NULL, true, now()),
  ('backoffice', '@HASH_backoffice@', 2, 'BackOffice', 'Load BackOffice', NULL, true, now()),
  ('corpadmin',  '@HASH_corpadmin@',  2, 'Corporate',  'Load Corporate',  NULL, true, now());

INSERT INTO "Users" ("Username","PasswordHash","PasswordHashVersion","Role","DisplayName","EmployeeId","IsActive","CreatedAt")
SELECT 'emp' || g, '@HASH_employee1@', 2, 'Employee', 'Emp User ' || g, g, true, now() FROM generate_series(1,20) g;

-- Attendance (5,000): 500 employees x 10 most recent workdays -----------------
INSERT INTO "Attendances" ("EmployeeId","AttendanceDate","ClockIn","ClockOut","WorkHours","Status","IsLate","IsEarlyLeave","ClockInMethod","ClockOutMethod")
SELECT e."EmployeeId",
       d,
       d + interval '8 hour' + (((e."EmployeeId"*7) % 45) || ' minutes')::interval,
       d + interval '17 hour' + (((e."EmployeeId"*3) % 60) || ' minutes')::interval,
       8 + ((e."EmployeeId" % 5) * 0.25),
       CASE WHEN e."EmployeeId" % 37 = 0 THEN 'ABSENT' ELSE 'PRESENT' END,
       (e."EmployeeId" % 11 = 0),
       (e."EmployeeId" % 13 = 0),
       'web','web'
FROM "Employees" e
CROSS JOIN (
  SELECT x AS d FROM generate_series(
    date_trunc('day', now()) - interval '13 days',
    date_trunc('day', now()) - interval '1 day',
    interval '1 day') s(x)
  WHERE extract(isodow from s.x) < 6
) days;

-- Projects / tasks ------------------------------------------------------------
INSERT INTO "Projects" ("Code","Name","Description","Status","Priority","StartDate","DueDate","OwnerId","CreatedAt","UpdatedAt")
SELECT 'PRJ' || lpad(g::text,3,'0'), 'Project ' || g, 'Synthetic project ' || g,
       (ARRAY['ACTIVE','PLANNING','ON_HOLD'])[1 + (g % 3)],
       (ARRAY['LOW','MEDIUM','HIGH'])[1 + (g % 3)],
       now() - interval '90 days', now() + interval '180 days',
       1 + (g % 500), now(), now()
FROM generate_series(1,50) g;

INSERT INTO "Milestones" ("ProjectId","Name","Status","CreatedAt","UpdatedAt")
SELECT p."ProjectId", 'Milestone ' || m, 'IN_PROGRESS', now(), now()
FROM "Projects" p CROSS JOIN generate_series(1,2) m WHERE p."ProjectId" <= 10;

INSERT INTO "ProjectTasks" ("ProjectId","TaskNumber","Title","Status","Priority","ProgressPercent","ReporterId","SortOrder","CreatedAt","UpdatedAt")
SELECT p."ProjectId",
       'T' || lpad((p."ProjectId"*100+g)::text,5,'0'),
       'Task ' || p."ProjectId" || '-' || g,
       (ARRAY['TODO','IN_PROGRESS','REVIEW','DONE'])[1 + (g % 4)],
       (ARRAY['LOW','MEDIUM','HIGH'])[1 + (g % 3)],
       (g*7) % 101, 1 + ((p."ProjectId"+g) % 500), g, now(), now()
FROM "Projects" p CROSS JOIN generate_series(1,10) g;

-- Suppliers / POs --------------------------------------------------------------
INSERT INTO "Suppliers" ("SupplierCode","Name","Email","Phone","Address","Country","PaymentTerms","Status","CreatedAt","UpdatedAt")
SELECT 'SUP' || lpad(g::text,4,'0'), 'Supplier ' || g,
       'sup' || g || '@synthetic.la', '+856-20-' || lpad(((g*7919) % 10000000)::text,8,'0'),
       'Vientiane Synthetic Address ' || g, 'LA',
       (ARRAY['NET15','NET30','NET60'])[1 + (g % 3)], 'ACTIVE', now(), now()
FROM generate_series(1,200) g;

INSERT INTO "PurchaseOrders" ("PONumber","SupplierId","Currency","OrderDate","ExpectedDate","PaymentTerms","Status","Subtotal","Tax","Total","CreatedAt","UpdatedAt")
SELECT 'PO-2026-' || lpad(g::text,5,'0'),
       1 + (g % 200), (ARRAY['LAK','USD','THB'])[1 + (g % 3)],
       ts.order_ts, ts.order_ts + interval '21 days',
       'NET30',
       (ARRAY['DRAFT','SUBMITTED','APPROVED','RECEIVED','CLOSED'])[1 + (g % 5)],
       1000000 + (g * 37000), (1000000 + (g*37000)) * 0.1, (1000000 + (g*37000)) * 1.1,
       ts.order_ts, ts.order_ts
FROM generate_series(1,300) g
CROSS JOIN LATERAL (SELECT now() - ((g % 180) || ' days')::interval AS order_ts) ts;

INSERT INTO "PurchaseOrderItems" ("PurchaseOrderId","Description","Quantity","UnitPrice","TaxRate","LineTotal")
SELECT po."PurchaseOrderId", 'Synthetic PO line ' || g, 1 + (g % 50), 12000 + ((po."PurchaseOrderId"*g) % 900000), 0.1,
       (1 + (g % 50)) * (12000 + ((po."PurchaseOrderId"*g) % 900000))
FROM "PurchaseOrders" po CROSS JOIN generate_series(1,2) g;

-- Inventory --------------------------------------------------------------------
INSERT INTO "Warehouses" ("Code","Name","Status","CreatedAt")
SELECT 'WH' || g, 'Warehouse ' || g, 'ACTIVE', now() FROM generate_series(1,3) g;

INSERT INTO "InventoryCategories" ("Code","Name","SortOrder","IsActive")
SELECT 'IC' || g, 'Inventory Category ' || g, g, true FROM generate_series(1,6) g;

INSERT INTO "InventoryItems" ("SKU","Name","CategoryId","UnitOfMeasure","ItemType","TrackInventory","ReorderLevel","IsActive","CreatedAt","UpdatedAt")
SELECT 'SKU-' || lpad(g::text,6,'0'), 'Item ' || g, 1 + (g % 6), 'pcs', 'STOCK', true, 10 + (g % 40), true, now(), now()
FROM generate_series(1,200) g;

INSERT INTO "StockMovements" ("ItemId","WarehouseId","MovementType","Quantity","ReferenceType","OccurredAt","PerformedByEmployeeId","Notes")
SELECT 1 + (g % 200), 1 + (g % 3),
       (ARRAY['IN','OUT','TRANSFER','ADJUSTMENT'])[1 + (g % 4)],
       CASE g % 4 WHEN 1 THEN -5 ELSE 25 END,
       'SYNTHETIC', now() - ((g % 120) || ' days')::interval,
       1 + (g % 500), 'load-test synthetic movement'
FROM generate_series(1,5000) g;

-- Assets -------------------------------------------------------------------------
INSERT INTO "Assets" ("AssetCode","Name","SerialNumber","PurchaseDate","AcquisitionCost","Currency","CustodianEmployeeId","Status","CreatedAt","UpdatedAt")
SELECT 'AST-' || lpad(g::text,5,'0'), 'Asset ' || g, 'SN' || lpad(g::text,8,'0'),
       now() - ((g % 700) || ' days')::interval, 1500000 + (g * 31000), 'LAK',
       1 + (g % 500), (ARRAY['ACTIVE','MAINTENANCE','DISPOSED'])[1 + (g % 3)], now(), now()
FROM generate_series(1,300) g;

-- Supplier invoices ---------------------------------------------------------------
INSERT INTO "SupplierInvoices" ("InvoiceNumber","SupplierId","PurchaseOrderId","InvoiceDate","ReceivedDate","DueDate","Currency","Subtotal","TaxAmount","TotalAmount","PaidAmount","RemainingAmount","Status","MatchStatus","CreatedByEmployeeId","CreatedAt","UpdatedAt")
SELECT 'INV-S-' || lpad(g::text,6,'0'),
       po."SupplierId", po."PurchaseOrderId",
       inv.ts, inv.ts + interval '2 days', inv.ts + interval '32 days',
       po."Currency", po."Subtotal", po."Tax", po."Total",
       CASE WHEN g % 3 = 0 THEN po."Total" ELSE 0 END,
       CASE WHEN g % 3 = 0 THEN 0 ELSE po."Total" END,
       (ARRAY['OPEN','PARTIALLY_PAID','PAID'])[1 + (g % 3)],
       (ARRAY['MATCHED','UNMATCHED'])[1 + (g % 2)],
       1 + (g % 500), inv.ts, inv.ts
FROM generate_series(1,300) g
JOIN "PurchaseOrders" po ON po."PurchaseOrderId" = g
CROSS JOIN LATERAL (SELECT now() - ((g % 150) || ' days')::interval AS ts) inv;

INSERT INTO "SupplierInvoiceLines" ("SupplierInvoiceId","Description","Quantity","UnitPrice","Subtotal","TaxAmount")
SELECT si."SupplierInvoiceId", 'Synthetic line ' || g, 1 + (g % 10),
       50000 + ((si."SupplierInvoiceId" * g) % 800000),
       (1 + (g % 10)) * (50000 + ((si."SupplierInvoiceId" * g) % 800000)),
       ((1 + (g % 10)) * (50000 + ((si."SupplierInvoiceId" * g) % 800000))) * 0.1
FROM "SupplierInvoices" si CROSS JOIN generate_series(1,2) g;

-- General ledger --------------------------------------------------------------------
INSERT INTO "Accounts" ("AccountCode","Name","AccountType","IsPostingAccount","IsActive","Currency","CreatedAt")
SELECT lpad(g::text,4,'0'), 'Account ' || g,
       (ARRAY['ASSET','LIABILITY','EQUITY','REVENUE','EXPENSE'])[1 + (g % 5)],
       true, true, 'LAK', now()
FROM generate_series(1,30) g;

INSERT INTO "FiscalYears" ("Name","StartDate","EndDate","Status","CreatedAt")
VALUES ('FY2026', make_timestamptz(2026,1,1,0,0,0), make_timestamptz(2026,12,31,23,59,59), 'OPEN', now());

INSERT INTO "FiscalPeriods" ("FiscalYearId","PeriodNumber","StartDate","EndDate","Status")
SELECT fy."FiscalYearId", m,
       make_timestamptz(2026,m,1,0,0,0),
       (date_trunc('month', make_timestamptz(2026,m,28,0,0,0)) + interval '1 month - 1 second'),
       CASE WHEN m < 8 THEN 'CLOSED' ELSE 'OPEN' END
FROM "FiscalYears" fy CROSS JOIN generate_series(1,12) m;

INSERT INTO "JournalEntries" ("JournalNumber","PostingDate","FiscalPeriodId","SourceType","Description","Status","Currency","CreatedByEmployeeId","CreatedAt","PostedAt")
SELECT 'JV-2026-' || lpad(g::text,6,'0'),
       je.ts,
       1 + (extract(month from je.ts))::int,
       'SYNTHETIC', 'Synthetic journal entry ' || g,
       'POSTED', 'LAK', 1 + (g % 500), je.ts, je.ts
FROM generate_series(1,200) g
CROSS JOIN LATERAL (SELECT now() - ((g % 210) || ' days')::interval AS ts) je;

INSERT INTO "JournalLines" ("JournalEntryId","AccountId","Debit","Credit","Currency","Reference")
SELECT j."JournalEntryId", 1 + (g % 30),
       CASE WHEN g % 2 = 0 THEN amt.amount ELSE 0 END,
       CASE WHEN g % 2 = 1 THEN amt.amount ELSE 0 END,
       'LAK', 'SYNTH'
FROM "JournalEntries" j
CROSS JOIN generate_series(1,4) g
CROSS JOIN LATERAL (SELECT 100000 + ((j."JournalEntryId" * 13000 + g * 7000) % 9000000) AS amount) amt;

-- Contracts / documents -----------------------------------------------------------
INSERT INTO "Contracts" ("ContractNumber","Title","SupplierId","ContractType","OwnerEmployeeId","DepartmentId","StartDate","EndDate","NoticeDate","Amount","Currency","Status","RenewalType","AutoRenew","CreatedAt","UpdatedAt")
SELECT 'CTR-2026-' || lpad(g::text,5,'0'), 'Synthetic Contract ' || g,
       1 + (g % 200), 'SERVICE', 1 + (g % 500), 1 + (g % 8),
       c.start_ts, c.start_ts + interval '365 days', c.start_ts + interval '335 days',
       2000000 + (g * 45000), 'LAK',
       (ARRAY['DRAFT','ACTIVE','EXPIRING_SOON','EXPIRED','TERMINATED'])[1 + (g % 5)],
       'MANUAL', false, c.start_ts, now()
FROM generate_series(1,150) g
CROSS JOIN LATERAL (SELECT now() - ((g % 400) || ' days')::interval + interval '30 days' AS start_ts) c;

INSERT INTO "EmployeeDocuments" ("EmployeeId","DocumentType","FileName","FilePath","UploadedAt")
SELECT e."EmployeeId",
       (ARRAY['CONTRACT','ID_CARD','RESUME','CERTIFICATE'])[1 + (g % 4)],
       'doc-' || g || '.pdf', 'documents/' || e."EmployeeId" || '/synthetic_' || g || '.pdf', now()
FROM "Employees" e
CROSS JOIN LATERAL (SELECT (e."EmployeeId" - 1) * 1 + 1 AS g) x
WHERE e."EmployeeId" <= 400;

-- Service desk ----------------------------------------------------------------------
INSERT INTO "ServiceRequestCategories" ("Code","Name","IsActive")
SELECT 'SRC' || g, 'Service Category ' || g, true FROM generate_series(1,6) g;

INSERT INTO "ServiceRequests" ("RequestNumber","RequesterEmployeeId","CategoryId","Subject","Priority","Status","AssignedEmployeeId","DepartmentId","DueDate","CreatedAt","ResolvedAt")
SELECT 'SR-2026-' || lpad(g::text,6,'0'),
       1 + (g % 500), 1 + (g % 6), 'Synthetic request ' || g,
       (ARRAY['LOW','MEDIUM','HIGH','URGENT'])[1 + (g % 4)],
       (ARRAY['OPEN','IN_PROGRESS','RESOLVED','CLOSED'])[1 + (g % 4)],
       1 + ((g*3) % 500), 1 + (g % 8),
       now() + ((g % 14) || ' days')::interval,
       now() - ((g % 60) || ' days')::interval,
       CASE WHEN g % 3 = 0 THEN now() - ((g % 30) || ' days')::interval ELSE NULL END
FROM generate_series(1,200) g;

-- Facilities: rooms + bookings --------------------------------------------------------
INSERT INTO "Facilities" ("FacilityCode","Name","FacilityType","Status","CreatedAt","UpdatedAt")
SELECT 'FAC' || g, 'Facility ' || g, 'OFFICE', 'ACTIVE', now(), now() FROM generate_series(1,2) g;

INSERT INTO "Rooms" ("FacilityId","Code","Name","RoomType","Capacity","Floor","Bookable","IsActive")
SELECT f."FacilityId", 'R' || lpad((f."FacilityId"*100+g)::text,4,'0'), 'Room ' || f."FacilityId" || '-' || g,
       'MEETING', 4 + (g % 16), (1 + (g % 5))::text, true, true
FROM "Facilities" f CROSS JOIN generate_series(1,10) g;

INSERT INTO "RoomBookings" ("BookingNumber","RoomId","BookedByEmployeeId","StartAt","EndAt","Title","ParticipantCount","Status","CreatedAt")
SELECT 'RB-2026-' || lpad(g::text,6,'0'),
       1 + (g % 20), 1 + (g % 500),
       date_trunc('day', now()) - interval '7 days' + (((g % 10) * interval '1 day') + (((g % 9) + 8) * interval '1 hour')),
       date_trunc('day', now()) - interval '7 days' + (((g % 10) * interval '1 day') + ((((g % 9) + 8) + 1) * interval '1 hour')),
       'Synthetic meeting ' || g, 2 + (g % 12), 'CONFIRMED', now()
FROM generate_series(1,300) g;

-- Fleet ---------------------------------------------------------------------------------
INSERT INTO "Vehicles" ("VehicleCode","RegistrationNumber","Make","Model","Year","VehicleType","FuelType","Status","CurrentOdometer","CreatedAt","UpdatedAt")
SELECT 'VEH-' || lpad(g::text,4,'0'), 'NBK-' || lpad(g::text,4,'0'),
       (ARRAY['Toyota','Ford','Hyundai','Isuzu'])[1 + (g % 4)],
       'Model ' || (g % 7), 2018 + (g % 8), 'SEDAN', 'PETROL', 'ACTIVE', g * 1379, now(), now()
FROM generate_series(1,30) g;

INSERT INTO "VehicleBookings" ("BookingNumber","VehicleId","RequesterEmployeeId","DriverEmployeeId","StartAt","EndAt","Purpose","Destination","Status","CreatedAt")
SELECT 'VB-2026-' || lpad(g::text,6,'0'),
       1 + (g % 30), 1 + (g % 500), 1 + ((g*7) % 500),
       date_trunc('day', now()) - interval '7 days' + (((g % 10) * interval '1 day') + (((g % 9) + 7) * interval '1 hour')),
       date_trunc('day', now()) - interval '7 days' + (((g % 10) * interval '1 day') + (((g % 9) + 11) * interval '1 hour')),
       'Synthetic trip', 'Destination ' || (g % 18), 'CONFIRMED', now()
FROM generate_series(1,300) g;

-- Travel register --------------------------------------------------------------------------
INSERT INTO "TravelRequests" ("TravelNumber","EmployeeId","DepartmentId","Purpose","Destination","DepartureDate","ReturnDate","EstimatedCost","Currency","Status","CreatedAt","UpdatedAt")
SELECT 'TR-2026-' || lpad(g::text,6,'0'), 1 + (g % 500), 1 + (g % 8),
       'Synthetic travel purpose', (ARRAY['Luang Prabang','Champasak','Savannakhet','Hanoi','Bangkok'])[1 + (g % 5)],
       tr.dep, tr.dep + interval '3 days', 1500000 + (g * 21000), 'LAK',
       (ARRAY['DRAFT','SUBMITTED','APPROVED','COMPLETED'])[1 + (g % 4)], tr.dep, tr.dep
FROM generate_series(1,150) g
CROSS JOIN LATERAL (SELECT now() + (((g % 40) - 20) || ' days')::interval AS dep) tr;
