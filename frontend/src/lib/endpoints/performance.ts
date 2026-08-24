/**
 * Performance + Talent + Learning endpoints (Phase 3C6).
 */
import { apiClient } from '../apiClient';

export interface Goal {
    goalId: number;
    employeeId: number;
    managerEmployeeId?: number | null;
    parentGoalId?: number | null;
    title: string;
    description?: string | null;
    goalType: string;
    startDate?: string | null;
    dueDate?: string | null;
    progressPercent: number;
    status: string;
    createdByEmployeeId?: number | null;
    createdAt: string;
    updatedAt: string;
}

export interface GoalCheckIn {
    checkInId: number;
    goalId: number;
    progressPercent: number;
    status?: string | null;
    comment?: string | null;
    createdByEmployeeId?: number | null;
    createdAt: string;
}

export interface PerformanceCycle {
    cycleId: number;
    name: string;
    startDate?: string | null;
    endDate?: string | null;
    reviewDueDate?: string | null;
    status: string;
    createdAt: string;
}

export interface PerformanceReview {
    reviewId: number;
    cycleId: number;
    employeeId: number;
    managerEmployeeId?: number | null;
    positionId?: number | null;
    departmentId?: number | null;
    status: string;
    overallRating?: number | null;
    selfAchievements?: string | null;
    selfChallenges?: string | null;
    managerComments?: string | null;
    developmentNeeds?: string | null;
    employeeSubmittedAt?: string | null;
    managerSubmittedAt?: string | null;
    finalizedAt?: string | null;
    acknowledgedAt?: string | null;
    acknowledgementComment?: string | null;
    createdAt: string;
    updatedAt: string;
}

export interface Feedback {
    feedbackId: number;
    fromEmployeeId: number;
    toEmployeeId: number;
    type: string;
    message: string;
    visibility: string;
    createdAt: string;
}

export interface OneOnOne {
    oneOnOneId: number;
    managerEmployeeId: number;
    employeeId: number;
    scheduledAt?: string | null;
    status: string;
    sharedNotes?: string | null;
    managerPrivateNotes?: string | null;
    employeeNotes?: string | null;
    createdAt: string;
    updatedAt: string;
}

export interface Competency {
    competencyId: number;
    code: string;
    name: string;
    nameLao?: string | null;
    description?: string | null;
    category: string;
    isActive: boolean;
    createdAt: string;
}

export interface PositionCompetency {
    positionCompetencyId: number;
    positionId: number;
    competencyId: number;
    requiredLevel: number;
    isRequired: boolean;
}

export interface CompetencyAssessment {
    assessmentId: number;
    employeeId: number;
    competencyId: number;
    assessorEmployeeId: number;
    assessmentType: string;
    level: number;
    cycleId?: number | null;
    assessedAt: string;
}

export interface DevelopmentPlan {
    developmentPlanId: number;
    employeeId: number;
    managerEmployeeId?: number | null;
    periodStart?: string | null;
    periodEnd?: string | null;
    status: string;
    createdAt: string;
    updatedAt: string;
}

export interface LearningCourse {
    courseId: number;
    code: string;
    title: string;
    titleLao?: string | null;
    description?: string | null;
    category?: string | null;
    deliveryType: string;
    isActive: boolean;
    createdAt: string;
}

export interface TrainingEnrollment {
    enrollmentId: number;
    sessionId: number;
    employeeId: number;
    status: string;
    assignedByEmployeeId?: number | null;
    enrolledAt?: string | null;
    completedAt?: string | null;
    result?: string | null;
}

export interface EmployeeCertification {
    certificationId: number;
    employeeId: number;
    name: string;
    issuer?: string | null;
    issuedDate?: string | null;
    expiryDate?: string | null;
    status: string;
}

export interface CareerInterest {
    careerInterestId: number;
    employeeId: number;
    interests?: string | null;
    futureRoles?: string | null;
    developmentInterests?: string | null;
    createdAt: string;
    updatedAt: string;
}

export interface TalentReview {
    talentReviewId: number;
    employeeId: number;
    cycleId?: number | null;
    potential?: string | null;
    performance?: string | null;
    readiness?: string | null;
    notes?: string | null;
    reviewedByEmployeeId?: number | null;
    createdAt: string;
}

export const performanceApi = {
    // Goals
    getGoals: (employeeId?: number) => apiClient.get<Goal[]>(`/api/performance/goals${employeeId ? `?employeeId=${employeeId}` : ''}`),
    getGoal: (id: number) => apiClient.get<Goal>(`/api/performance/goals/${id}`),
    createGoal: (input: Record<string, unknown>) => apiClient.post<Goal>('/api/performance/goals', input),
    addCheckIn: (goalId: number, input: Record<string, unknown>) => apiClient.post<GoalCheckIn>(`/api/performance/goals/${goalId}/checkins`, input),
    getCheckIns: (goalId: number) => apiClient.get<GoalCheckIn[]>(`/api/performance/goals/${goalId}/checkins`),

    // Cycles
    getCycles: () => apiClient.get<PerformanceCycle[]>('/api/performance/cycles'),
    createCycle: (input: Record<string, unknown>) => apiClient.post<PerformanceCycle>('/api/performance/cycles', input),

    // Reviews
    getReviews: (cycleId?: number) => apiClient.get<PerformanceReview[]>(`/api/performance/reviews${cycleId ? `?cycleId=${cycleId}` : ''}`),
    getReview: (id: number) => apiClient.get<PerformanceReview>(`/api/performance/reviews/${id}`),
    createReview: (input: Record<string, unknown>) => apiClient.post<PerformanceReview>('/api/performance/reviews', input),
    submitSelfReview: (id: number, input: Record<string, unknown>) => apiClient.post<void>(`/api/performance/reviews/${id}/self`, input),
    submitManagerReview: (id: number, input: Record<string, unknown>) => apiClient.post<void>(`/api/performance/reviews/${id}/manager`, input),
    finalizeReview: (id: number) => apiClient.post<void>(`/api/performance/reviews/${id}/finalize`),
    acknowledgeReview: (id: number, comment?: string) => apiClient.post<void>(`/api/performance/reviews/${id}/acknowledge`, { comment }),

    // Feedback
    getFeedback: (toEmployeeId?: number) => apiClient.get<Feedback[]>(`/api/performance/feedback${toEmployeeId ? `?toEmployeeId=${toEmployeeId}` : ''}`),
    createFeedback: (input: Record<string, unknown>) => apiClient.post<Feedback>('/api/performance/feedback', input),

    // 1:1s
    getOneOnOnes: () => apiClient.get<OneOnOne[]>('/api/performance/one-on-ones'),
    createOneOnOne: (input: Record<string, unknown>) => apiClient.post<OneOnOne>('/api/performance/one-on-ones', input),

    // Competencies
    getCompetencies: () => apiClient.get<Competency[]>('/api/performance/competencies'),
    createCompetency: (input: Record<string, unknown>) => apiClient.post<Competency>('/api/performance/competencies', input),
    getPositionCompetencies: (positionId: number) => apiClient.get<PositionCompetency[]>(`/api/performance/positions/${positionId}/competencies`),
    addPositionCompetency: (positionId: number, input: Record<string, unknown>) => apiClient.post<PositionCompetency>(`/api/performance/positions/${positionId}/competencies`, input),
    getAssessments: (employeeId?: number) => apiClient.get<CompetencyAssessment[]>(`/api/performance/assessments${employeeId ? `?employeeId=${employeeId}` : ''}`),
    createAssessment: (input: Record<string, unknown>) => apiClient.post<CompetencyAssessment>('/api/performance/assessments', input),

    // Development plans
    getDevelopmentPlans: (employeeId?: number) => apiClient.get<DevelopmentPlan[]>(`/api/performance/development-plans${employeeId ? `?employeeId=${employeeId}` : ''}`),
    createDevelopmentPlan: (input: Record<string, unknown>) => apiClient.post<DevelopmentPlan>('/api/performance/development-plans', input),

    // Learning
    getCourses: () => apiClient.get<LearningCourse[]>('/api/performance/courses'),
    createCourse: (input: Record<string, unknown>) => apiClient.post<LearningCourse>('/api/performance/courses', input),
    getEnrollments: (employeeId?: number) => apiClient.get<TrainingEnrollment[]>(`/api/performance/enrollments${employeeId ? `?employeeId=${employeeId}` : ''}`),
    createEnrollment: (input: Record<string, unknown>) => apiClient.post<TrainingEnrollment>('/api/performance/enrollments', input),
    completeEnrollment: (id: number) => apiClient.post<void>(`/api/performance/enrollments/${id}/complete`),

    // Certifications
    getCertifications: (employeeId?: number) => apiClient.get<EmployeeCertification[]>(`/api/performance/certifications${employeeId ? `?employeeId=${employeeId}` : ''}`),
    createCertification: (input: Record<string, unknown>) => apiClient.post<EmployeeCertification>('/api/performance/certifications', input),

    // Career
    getCareerInterest: () => apiClient.get<CareerInterest>('/api/performance/career'),
    saveCareerInterest: (input: Record<string, unknown>) => apiClient.post<CareerInterest>('/api/performance/career', input),

    // Talent
    getTalentReviews: () => apiClient.get<TalentReview[]>('/api/performance/talent'),
    createTalentReview: (input: Record<string, unknown>) => apiClient.post<TalentReview>('/api/performance/talent', input),
};

export default performanceApi;
