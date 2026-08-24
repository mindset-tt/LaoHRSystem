-- Phase 4C.1 — Representative corporate DR dataset (coherent synthetic chain).
-- Seeded before pg_dump; verified after pg_restore by exact value + relationship.

-- Employee (host/requester/owner)
INSERT INTO "Employees" ("EmployeeCode","LaoName","EnglishName","IsActive","DependentCount","SalaryCurrency","BaseSalary","CreatedAt","UpdatedAt")
VALUES ('DR-EMP-001','ພະນັກງານ DR','DR Employee',true,0,'LAK',0,now(),now());

-- Supplier
INSERT INTO "Suppliers" ("SupplierCode","Name","Status","CreatedAt","UpdatedAt")
VALUES ('DR-SUP-001','DR Supplier','ACTIVE',now(),now());

-- CorporateDocument + DocumentVersion
INSERT INTO "CorporateDocuments" ("DocumentNumber","Title","DocumentType","OwnerEntityType","OwnerEntityId","Confidentiality","Status","CurrentVersion","CreatedAt","UpdatedAt")
VALUES ('DR-DOC-001','DR Policy','POLICY','GENERAL',1,'CONFIDENTIAL','ACTIVE',2,now(),now());
INSERT INTO "DocumentVersions" ("DocumentId","VersionNumber","FileName","MimeType","FileSize","StorageReference","UploadedAt")
VALUES (1,1,'policy-v1.pdf','application/pdf',100,'dr/ref/v1',now());
INSERT INTO "DocumentVersions" ("DocumentId","VersionNumber","FileName","MimeType","FileSize","StorageReference","UploadedAt")
VALUES (1,2,'policy-v2.pdf','application/pdf',120,'dr/ref/v2',now());

-- Contract + ContractHistory
INSERT INTO "Contracts" ("ContractNumber","Title","OwnerEmployeeId","StartDate","EndDate","Amount","Currency","Status","CreatedAt","UpdatedAt")
VALUES ('DR-CTR-001','DR Service Agreement',1,'2026-01-01','2026-12-31',1000,'LAK','ACTIVE',now(),now());
INSERT INTO "ContractHistories" ("ContractId","ChangeType","PreviousStartDate","PreviousEndDate","PreviousAmount","PreviousStatus","NewStartDate","NewEndDate","NewAmount","NewStatus","ChangedAt")
VALUES (1,'RENEWAL','2025-01-01','2025-12-31',900,'ACTIVE','2026-01-01','2026-12-31',1000,'ACTIVE',now());

-- ServiceRequest + ServiceRequestHistory
INSERT INTO "ServiceRequests" ("RequestNumber","RequesterEmployeeId","CategoryId","Subject","Priority","Status","CreatedAt")
VALUES ('DR-SR-001',1,1,'DR IT Request','HIGH','IN_PROGRESS',now());
INSERT INTO "ServiceRequestHistories" ("ServiceRequestId","ChangeType","FromValue","ToValue","ChangedAt")
VALUES (1,'STATUS_CHANGE','OPEN','IN_PROGRESS',now());

-- Facility + Room + RoomBooking
INSERT INTO "Facilities" ("FacilityCode","Name","FacilityType","Status","CreatedAt","UpdatedAt")
VALUES ('DR-FAC-001','DR Head Office','OFFICE','ACTIVE',now(),now());
INSERT INTO "Rooms" ("FacilityId","Code","Name","RoomType","Capacity","Bookable","IsActive")
VALUES (1,'DR-R-001','DR Meeting Room','MEETING_ROOM',10,true,true);
INSERT INTO "RoomBookings" ("BookingNumber","RoomId","BookedByEmployeeId","StartAt","EndAt","Title","Status","CreatedAt")
VALUES ('DR-RB-001',1,1,'2026-01-01 09:00:00','2026-01-01 10:00:00','DR Meeting','CONFIRMED',now());

-- WorkOrder
INSERT INTO "WorkOrders" ("WorkOrderNumber","SourceType","SourceId","Title","Priority","Status","CreatedAt")
VALUES ('DR-WO-001','FACILITY',1,'DR Facility Repair','HIGH','OPEN',now());

-- Asset + Vehicle + VehicleBooking + VehicleTrip
INSERT INTO "Assets" ("AssetCode","Name","Status","CreatedAt","UpdatedAt")
VALUES ('DR-AST-001','DR Vehicle Asset','AVAILABLE',now(),now());
INSERT INTO "Vehicles" ("VehicleCode","RegistrationNumber","AssetId","Make","Model","Status","CurrentOdometer","CreatedAt","UpdatedAt")
VALUES ('DR-V-001','DR-REG-001',1,'Toyota','Hilux','AVAILABLE',1000,now(),now());
INSERT INTO "VehicleBookings" ("BookingNumber","VehicleId","RequesterEmployeeId","StartAt","EndAt","Purpose","Status","CreatedAt")
VALUES ('DR-VB-001',1,1,'2026-01-01 09:00:00','2026-01-01 10:00:00','DR Trip','CONFIRMED',now());
INSERT INTO "VehicleTrips" ("VehicleId","DriverEmployeeId","StartedAt","CompletedAt","StartOdometer","EndOdometer","Destination","Purpose")
VALUES (1,1,'2026-01-01 09:00:00','2026-01-01 10:00:00',1000,1050,'Vientiane','DR Delivery');

-- TravelRequest + linked Expense
INSERT INTO "TravelRequests" ("TravelNumber","EmployeeId","Purpose","Destination","DepartureDate","ReturnDate","Status","CreatedAt","UpdatedAt")
VALUES ('DR-TRV-001',1,'DR Conference','Bangkok','2026-02-01','2026-02-03','APPROVED',now(),now());
INSERT INTO "Expenses" ("ExpenseNumber","EmployeeId","CategoryId","Title","ExpenseDate","Currency","Amount","ExchangeRateUsed","AmountLak","Status","TravelRequestId","CreatedAt","UpdatedAt")
VALUES ('DR-EXP-001',1,1,'DR Travel Expense','2026-02-01','LAK',500,1,500,'APPROVED',1,now(),now());

-- Visitor + Visit
INSERT INTO "Visitors" ("FullName","Company","CreatedAt")
VALUES ('DR Visitor','DR Company',now());
INSERT INTO "Visits" ("VisitorId","HostEmployeeId","Purpose","ExpectedAt","Status","CreatedAt")
VALUES (1,1,'DR Meeting','2026-01-01 09:00:00','EXPECTED',now());
