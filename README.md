# Lao HR System

A comprehensive Human Resources Management System built for organizations in Laos. This system handles employee management, payroll, attendance tracking, leave management, and comprehensive reporting.

## 📋 Project Overview

**Lao HR System** is an enterprise-grade HR management solution that provides:

- **Employee Management**: Complete employee records with personal information, documents, and profiles
- **Attendance Tracking**: Biometric integration with ZK Teco devices for accurate attendance logging
- **Payroll Management**: Automated salary calculations, deductions, adjustments, and tax handling
- **Leave Management**: Leave requests, approval workflows, and leave balance tracking
- **Holiday Management**: Company holidays and special leave types
- **Reporting & Analytics**: Comprehensive HR reports and dashboard analytics
- **Multi-Currency Support**: Handle salaries in LAK, USD, THB
- **Audit Logging**: Complete audit trail for compliance and monitoring
- **Bank Transfer Integration**: Automated salary disbursement to employee bank accounts
- **License Management**: System license validation and enforcement

## 🏗️ Architecture

The project is divided into three main components:

```
LaoHRSystem/
├── Backend/              # .NET Core REST API
│   ├── LaoHR.API/       # Main API application
│   ├── LaoHR.Shared/    # Shared models and entities
│   ├── LaoHR.Tests/     # Unit and integration tests
│   ├── LaoHR.Bridge.Service/  # Windows Service for scheduled jobs
│   ├── LaoHR.Bridge.Config/   # Configuration desktop app (WPF)
│   └── LaoHR.LicenseGen/      # License generation utility
└── frontend/             # Next.js React frontend
```

## 🛠️ Tech Stack

### Backend
- **.NET 10.0** - Latest .NET runtime
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with SQL Server support
- **JWT Authentication** - Secure token-based authentication
- **FluentValidation** - Input validation
- **QuestPDF** - PDF generation for payroll slips
- **iText7** - Advanced PDF manipulation
- **ClosedXML** - Excel file handling
- **Swashbuckle** - Swagger/OpenAPI documentation
- **Windows Services** - Background job processing

### Frontend
- **Next.js 16** - React framework with TypeScript
- **React 19** - UI library
- **Tailwind CSS** - Utility-first CSS framework
- **TypeScript** - Type-safe JavaScript

### Database
- **SQL Server** - Enterprise database with full-text search support
- **Entity Framework Core Migrations** - Schema versioning

## 📦 Core Components

### LaoHR.API
Main ASP.NET Core REST API with the following controllers:

- **AuthController** - Authentication and JWT token generation
- **EmployeesController** - Employee CRUD operations
- **AttendanceController** - Attendance tracking and biometric integration
- **PayrollController** - Salary and payroll management
- **LeaveController** - Leave requests and approvals
- **HolidaysController** - Holiday management
- **DashboardController** - Analytics and metrics
- **ReportsController** - HR reports generation
- **DocumentsController** - Employee document management
- **AuditLogsController** - Audit trail logging
- **BankTransferController** - Salary disbursement
- **LicenseController** - System license management
- **WorkScheduleController** - Work schedule management
- **CompanySettingsController** - System configuration

### LaoHR.Shared
Shared library containing:
- **Entities** - Database models (Employee, Department, Attendance, PayrollData, etc.)
- **Models** - DTOs and request/response models
- **Services** - Common business logic

### LaoHR.Bridge.Service
Windows Service that runs scheduled jobs:
- Leave accrual calculations
- Payroll processing
- Attendance synchronization
- Report generation

### LaoHR.Bridge.Config
WPF desktop application for:
- System configuration
- License management
- ZK Teco device setup
- Database connection settings

### LaoHR.LicenseGen
Command-line utility for generating system licenses.

### LaoHR.Tests
Comprehensive test suite with:
- Unit tests
- Integration tests
- Code coverage reporting (Cobertura format)

### frontend (Next.js)
Modern React application with:
- Responsive UI with Tailwind CSS
- API integration with backend
- Employee dashboards
- Payroll management interface
- Leave request workflows
- Reporting dashboards

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- Node.js 18+ with npm
- SQL Server 2019 or later
- Git

### Backend Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd LaoHRSystem/Backend
```

2. **Configure database connection**
   - Edit `LaoHR.API/appsettings.json`
   - Update the `ConnectionStrings:DefaultConnection` with your SQL Server details

3. **Install dependencies and run migrations**
```bash
cd LaoHR.API
dotnet restore
dotnet ef database update
```

4. **Run the API**
```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or configured port)

**API Documentation**: Visit `https://localhost:5001/swagger` for interactive API documentation.

### Frontend Setup

1. **Install dependencies**
```bash
cd frontend
npm install
```

2. **Configure API connection**
   - Create/update `frontend/src/proxy.ts` with your backend API URL

3. **Run development server**
```bash
npm run dev
```

The frontend will be available at `http://localhost:3000`

4. **Build for production**
```bash
npm run build
npm start
```

### Windows Service Setup (LaoHR.Bridge.Service)

1. **Build the service**
```bash
cd LaoHR.Bridge.Service
dotnet publish -c Release
```

2. **Install as Windows Service**
```powershell
sc.exe create LaoHRService binPath= "C:\path\to\LaoHR.Bridge.Service.exe"
```

3. **Start the service**
```powershell
net start LaoHRService
```

## 🔐 Authentication & Security

- **JWT Token-based Authentication**: Secure API endpoints
- **Token Configuration**: Set in `appsettings.json`
  - `Jwt:Key` - Secret key for signing tokens
  - `Jwt:Issuer` - Token issuer
  - `Jwt:Audience` - Token audience
  - `Jwt:DurationInMinutes` - Token expiration time
- **HTTPS** - All production endpoints use SSL/TLS
- **Audit Logging** - All sensitive operations are logged

## 📊 Database Schema

Key entities include:
- **Employee** - Core employee data
- **Department** - Organization structure
- **Attendance** - Daily attendance records
- **SalarySlip** - Monthly payroll slips
- **LeaveRequest** - Leave applications
- **Holiday** - Company holidays
- **AuditLog** - System audit trail
- **ConversionRate** - Currency exchange rates
- **License** - System license information

## 🔧 Configuration

### appsettings.json Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=LaoHR_DB;..."
  },
  "Jwt": {
    "Key": "YourSecretKey",
    "Issuer": "LaoHRServer",
    "Audience": "LaoHRClient",
    "DurationInMinutes": 60
  },
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "User": "your-email@example.com",
    "FromEmail": "noreply@laohr.com",
    "HrEmail": "hr@laohr.com"
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Set to `Development` or `Production`
- Database connection string can be overridden via environment variables

## 📝 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration (if enabled)
- `POST /api/auth/refresh` - Refresh JWT token

### Employees
- `GET /api/employees` - List all employees
- `GET /api/employees/{id}` - Get employee details
- `POST /api/employees` - Create new employee
- `PUT /api/employees/{id}` - Update employee
- `DELETE /api/employees/{id}` - Delete employee

### Attendance
- `GET /api/attendance` - Get attendance records
- `POST /api/attendance` - Log attendance
- `GET /api/attendance/{employeeId}` - Get employee attendance history

### Payroll
- `GET /api/payroll` - List salary slips
- `POST /api/payroll/calculate` - Calculate payroll
- `GET /api/payroll/{id}` - Get salary slip details
- `POST /api/payroll/adjust` - Apply salary adjustments

### Leave
- `GET /api/leave` - List leave requests
- `POST /api/leave` - Submit leave request
- `PUT /api/leave/{id}/approve` - Approve leave request
- `PUT /api/leave/{id}/reject` - Reject leave request

### Reports
- `GET /api/reports/attendance` - Attendance report
- `GET /api/reports/payroll` - Payroll report
- `GET /api/reports/leave` - Leave report
- `GET /api/reports/dashboard` - Dashboard metrics

See Swagger documentation for complete API reference.

## 🧪 Testing

Run tests with:

```bash
cd LaoHR.Tests
dotnet test
```

Generate coverage report:
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

## 📋 Database Migrations

View migrations:
```bash
dotnet ef migrations list
```

Create new migration:
```bash
dotnet ef migrations add <MigrationName>
```

Apply migrations:
```bash
dotnet ef database update
```

## 🌐 Multi-Language Support

The system supports multiple languages with entities containing both Lao and English fields:
- `LaoName` / `EnglishName` - Employee names
- `DepartmentName` / `DepartmentNameEn` - Department names

## 💱 Multi-Currency Support

Employees' salaries can be configured in:
- **LAK** (Laotian Kip) - Default
- **USD** (US Dollar)
- **THB** (Thai Baht)

Currency conversion rates are managed in the `ConversionRate` entity.

## 📧 Email Notifications

The system can send automated emails for:
- Leave request approvals/rejections
- Payroll notifications
- System alerts

Configure SMTP settings in `appsettings.json`.

## 📄 PDF Generation

- **QuestPDF** - Professional salary slip generation
- **iText7** - Advanced PDF manipulation and watermarking

## 📈 Audit Logging

All critical operations are logged including:
- Employee data modifications
- Payroll calculations
- Leave request changes
- Login/logout events
- System configuration changes

View logs in the `AuditLog` table.

## 🔗 Biometric Integration

The system integrates with **ZK Teco** biometric devices for:
- Real-time attendance tracking
- Employee fingerprint recognition
- Seamless data synchronization

## 🚨 Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists and user has proper permissions

### API Not Starting
- Check port availability (default: 5001)
- Review logs for dependency issues
- Verify all NuGet packages are installed

### Frontend Connection Issues
- Verify backend API is running
- Check proxy configuration in `frontend/src/proxy.ts`
- Ensure CORS is properly configured

## 📚 Documentation

- API Documentation: Available at `/swagger` endpoint
- Database Schema: See `Backend/LaoHR.API/Database/LaoHR_Schema.sql`
- Migration History: Located in `Backend/LaoHR.API/Migrations/`

## 🤝 Contributing

1. Create a feature branch (`git checkout -b feature/AmazingFeature`)
2. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
3. Push to the branch (`git push origin feature/AmazingFeature`)
4. Open a Pull Request

## 📄 License

This project is proprietary software. All rights reserved.

## 📞 Support

For issues, questions, or feature requests, please contact the development team.

---

**Last Updated**: February 2026
**Version**: 1.0
**Status**: Production Ready
