using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceTalentLearning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareerInterests",
                columns: table => new
                {
                    CareerInterestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    Interests = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FutureRoles = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DevelopmentInterests = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerInterests", x => x.CareerInterestId);
                    table.ForeignKey(
                        name: "FK_CareerInterests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Competencies",
                columns: table => new
                {
                    CompetencyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameLao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competencies", x => x.CompetencyId);
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentPlans",
                columns: table => new
                {
                    DevelopmentPlanId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ManagerEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevelopmentPlans", x => x.DevelopmentPlanId);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlans_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlans_Employees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeCertifications",
                columns: table => new
                {
                    CertificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Issuer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCertifications", x => x.CertificationId);
                    table.ForeignKey(
                        name: "FK_EmployeeCertifications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromEmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ToEmployeeId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Employees_FromEmployeeId",
                        column: x => x.FromEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Employees_ToEmployeeId",
                        column: x => x.ToEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    GoalId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ManagerEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    ParentGoalId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GoalType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedByEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.GoalId);
                    table.ForeignKey(
                        name: "FK_Goals_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Goals_Employees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_Goals_Goals_ParentGoalId",
                        column: x => x.ParentGoalId,
                        principalTable: "Goals",
                        principalColumn: "GoalId");
                });

            migrationBuilder.CreateTable(
                name: "LearningCourses",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleLao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeliveryType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningCourses", x => x.CourseId);
                });

            migrationBuilder.CreateTable(
                name: "OneOnOnes",
                columns: table => new
                {
                    OneOnOneId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ManagerEmployeeId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SharedNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ManagerPrivateNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EmployeeNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneOnOnes", x => x.OneOnOneId);
                    table.ForeignKey(
                        name: "FK_OneOnOnes_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OneOnOnes_Employees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceCycles",
                columns: table => new
                {
                    CycleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceCycles", x => x.CycleId);
                });

            migrationBuilder.CreateTable(
                name: "TalentReviews",
                columns: table => new
                {
                    TalentReviewId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    CycleId = table.Column<int>(type: "integer", nullable: true),
                    Potential = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Performance = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Readiness = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedByEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentReviews", x => x.TalentReviewId);
                    table.ForeignKey(
                        name: "FK_TalentReviews_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetencyAssessments",
                columns: table => new
                {
                    AssessmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    CompetencyId = table.Column<int>(type: "integer", nullable: false),
                    AssessorEmployeeId = table.Column<int>(type: "integer", nullable: false),
                    AssessmentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CycleId = table.Column<int>(type: "integer", nullable: true),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetencyAssessments", x => x.AssessmentId);
                    table.ForeignKey(
                        name: "FK_CompetencyAssessments_Competencies_CompetencyId",
                        column: x => x.CompetencyId,
                        principalTable: "Competencies",
                        principalColumn: "CompetencyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetencyAssessments_Employees_AssessorEmployeeId",
                        column: x => x.AssessorEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetencyAssessments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PositionCompetencies",
                columns: table => new
                {
                    PositionCompetencyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PositionId = table.Column<int>(type: "integer", nullable: false),
                    CompetencyId = table.Column<int>(type: "integer", nullable: false),
                    RequiredLevel = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionCompetencies", x => x.PositionCompetencyId);
                    table.ForeignKey(
                        name: "FK_PositionCompetencies_Competencies_CompetencyId",
                        column: x => x.CompetencyId,
                        principalTable: "Competencies",
                        principalColumn: "CompetencyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PositionCompetencies_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentGoals",
                columns: table => new
                {
                    DevelopmentGoalId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DevelopmentPlanId = table.Column<int>(type: "integer", nullable: false),
                    CompetencyId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DesiredOutcome = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TargetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevelopmentGoals", x => x.DevelopmentGoalId);
                    table.ForeignKey(
                        name: "FK_DevelopmentGoals_Competencies_CompetencyId",
                        column: x => x.CompetencyId,
                        principalTable: "Competencies",
                        principalColumn: "CompetencyId");
                    table.ForeignKey(
                        name: "FK_DevelopmentGoals_DevelopmentPlans_DevelopmentPlanId",
                        column: x => x.DevelopmentPlanId,
                        principalTable: "DevelopmentPlans",
                        principalColumn: "DevelopmentPlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoalCheckIns",
                columns: table => new
                {
                    CheckInId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoalId = table.Column<int>(type: "integer", nullable: false),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedByEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalCheckIns", x => x.CheckInId);
                    table.ForeignKey(
                        name: "FK_GoalCheckIns_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingSessions",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Instructor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_TrainingSessions_LearningCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "LearningCourses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CycleId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ManagerEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    PositionId = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OverallRating = table.Column<int>(type: "integer", nullable: true),
                    SelfAchievements = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SelfChallenges = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ManagerComments = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DevelopmentNeeds = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EmployeeSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ManagerSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgementComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_Employees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_PerformanceCycles_CycleId",
                        column: x => x.CycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "CycleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingEnrollments",
                columns: table => new
                {
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedByEmployeeId = table.Column<int>(type: "integer", nullable: true),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingEnrollments", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_TrainingEnrollments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingEnrollments_TrainingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "TrainingSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(7437));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(1572));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(1591));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(1593));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(9153));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 735, DateTimeKind.Utc).AddTicks(2851));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 735, DateTimeKind.Utc).AddTicks(2861));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 735, DateTimeKind.Utc).AddTicks(2864));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 735, DateTimeKind.Utc).AddTicks(2867));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 735, DateTimeKind.Utc).AddTicks(2869));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 735, DateTimeKind.Utc).AddTicks(2875));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 735, DateTimeKind.Utc).AddTicks(2878));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(5345), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(5348) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7452), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7452) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7454), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7455) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7457), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7457) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7459), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7459) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7461), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7461) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7462), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7463) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7464), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7465) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7466), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7466) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7468), new DateTime(2026, 8, 21, 10, 7, 12, 734, DateTimeKind.Utc).AddTicks(7468) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 732, DateTimeKind.Utc).AddTicks(6713));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 732, DateTimeKind.Utc).AddTicks(8439));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 732, DateTimeKind.Utc).AddTicks(8840));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 732, DateTimeKind.Utc).AddTicks(8842));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 732, DateTimeKind.Utc).AddTicks(8844));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 732, DateTimeKind.Utc).AddTicks(8845));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(2611));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(2611));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(1187));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(2564));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(2608));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(2610));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(2609));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 10, 7, 12, 733, DateTimeKind.Utc).AddTicks(2612));

            migrationBuilder.CreateIndex(
                name: "IX_CareerInterests_EmployeeId",
                table: "CareerInterests",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyAssessments_AssessorEmployeeId",
                table: "CompetencyAssessments",
                column: "AssessorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyAssessments_CompetencyId",
                table: "CompetencyAssessments",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyAssessments_EmployeeId_CompetencyId",
                table: "CompetencyAssessments",
                columns: new[] { "EmployeeId", "CompetencyId" });

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentGoals_CompetencyId",
                table: "DevelopmentGoals",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentGoals_DevelopmentPlanId",
                table: "DevelopmentGoals",
                column: "DevelopmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_EmployeeId",
                table: "DevelopmentPlans",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_ManagerEmployeeId",
                table: "DevelopmentPlans",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCertifications_EmployeeId",
                table: "EmployeeCertifications",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCertifications_ExpiryDate",
                table: "EmployeeCertifications",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_FromEmployeeId",
                table: "Feedbacks",
                column: "FromEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ToEmployeeId",
                table: "Feedbacks",
                column: "ToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalCheckIns_GoalId_CreatedAt",
                table: "GoalCheckIns",
                columns: new[] { "GoalId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_EmployeeId",
                table: "Goals",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_ManagerEmployeeId",
                table: "Goals",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_ParentGoalId",
                table: "Goals",
                column: "ParentGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_Status_DueDate",
                table: "Goals",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OneOnOnes_EmployeeId",
                table: "OneOnOnes",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OneOnOnes_ManagerEmployeeId_EmployeeId",
                table: "OneOnOnes",
                columns: new[] { "ManagerEmployeeId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_CycleId_Status",
                table: "PerformanceReviews",
                columns: new[] { "CycleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_EmployeeId",
                table: "PerformanceReviews",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_ManagerEmployeeId",
                table: "PerformanceReviews",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionCompetencies_CompetencyId",
                table: "PositionCompetencies",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionCompetencies_PositionId",
                table: "PositionCompetencies",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TalentReviews_EmployeeId",
                table: "TalentReviews",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingEnrollments_EmployeeId",
                table: "TrainingEnrollments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingEnrollments_SessionId_Status",
                table: "TrainingEnrollments",
                columns: new[] { "SessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_CourseId",
                table: "TrainingSessions",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareerInterests");

            migrationBuilder.DropTable(
                name: "CompetencyAssessments");

            migrationBuilder.DropTable(
                name: "DevelopmentGoals");

            migrationBuilder.DropTable(
                name: "EmployeeCertifications");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "GoalCheckIns");

            migrationBuilder.DropTable(
                name: "OneOnOnes");

            migrationBuilder.DropTable(
                name: "PerformanceReviews");

            migrationBuilder.DropTable(
                name: "PositionCompetencies");

            migrationBuilder.DropTable(
                name: "TalentReviews");

            migrationBuilder.DropTable(
                name: "TrainingEnrollments");

            migrationBuilder.DropTable(
                name: "DevelopmentPlans");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "PerformanceCycles");

            migrationBuilder.DropTable(
                name: "Competencies");

            migrationBuilder.DropTable(
                name: "TrainingSessions");

            migrationBuilder.DropTable(
                name: "LearningCourses");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(4258));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(5753));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(5756));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(5758));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(9849));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 288, DateTimeKind.Utc).AddTicks(3900));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 288, DateTimeKind.Utc).AddTicks(3916));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 288, DateTimeKind.Utc).AddTicks(3921));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 288, DateTimeKind.Utc).AddTicks(3926));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 288, DateTimeKind.Utc).AddTicks(3930));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 288, DateTimeKind.Utc).AddTicks(3934));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 288, DateTimeKind.Utc).AddTicks(3938));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(6633), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(6633) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8544), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8544) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8546), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8547) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8548), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8549) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8550), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8551) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8552), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8553) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8574), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8574) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8576), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8576) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8577), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8578) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8579), new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(8579) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 286, DateTimeKind.Utc).AddTicks(4442));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 286, DateTimeKind.Utc).AddTicks(6577));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 286, DateTimeKind.Utc).AddTicks(7089));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 286, DateTimeKind.Utc).AddTicks(7091));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 286, DateTimeKind.Utc).AddTicks(7093));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 286, DateTimeKind.Utc).AddTicks(7094));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(1946));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(1909));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(637));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(1903));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(1906));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(1908));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(1907));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 21, 9, 40, 35, 287, DateTimeKind.Utc).AddTicks(1951));
        }
    }
}
