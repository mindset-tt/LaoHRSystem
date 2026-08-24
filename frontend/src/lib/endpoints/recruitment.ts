/**
 * Recruitment + ATS + Onboarding endpoints (Phase 3C5).
 */
import { apiClient } from '../apiClient';

export interface JobRequisition {
    requisitionId: number;
    requisitionNumber: string;
    positionId: number;
    departmentId?: number | null;
    workLocationId?: number | null;
    requestedByEmployeeId: number;
    hiringManagerEmployeeId?: number | null;
    headcount: number;
    reason: string;
    justification?: string | null;
    targetStartDate?: string | null;
    priority: string;
    status: string;
    createdAt: string;
    updatedAt: string;
}

export interface JobOpening {
    openingId: number;
    requisitionId: number;
    title: string;
    titleLao?: string | null;
    summary?: string | null;
    responsibilities?: string | null;
    requirements?: string | null;
    status: string;
    createdAt: string;
    updatedAt: string;
}

export interface Candidate {
    candidateId: number;
    firstName: string;
    lastName: string;
    firstNameLao?: string | null;
    lastNameLao?: string | null;
    email?: string | null;
    phone?: string | null;
    currentLocation?: string | null;
    currentCompany?: string | null;
    currentTitle?: string | null;
    summary?: string | null;
    source?: string | null;
    status: string;
    createdAt: string;
    updatedAt: string;
}

export interface Application {
    applicationId: number;
    candidateId: number;
    openingId: number;
    appliedAt: string;
    source?: string | null;
    currentStage: string;
    status: string;
    rejectionReason?: string | null;
    rejectionComment?: string | null;
    createdAt: string;
    updatedAt: string;
}

export interface ApplicationStageHistory {
    stageHistoryId: number;
    applicationId: number;
    fromStage: string;
    toStage: string;
    actorEmployeeId?: number | null;
    comment?: string | null;
    createdAt: string;
}

export interface Interview {
    interviewId: number;
    applicationId: number;
    interviewType: string;
    scheduledStart?: string | null;
    scheduledEnd?: string | null;
    location?: string | null;
    status: string;
    organizerEmployeeId?: number | null;
    createdAt: string;
    updatedAt: string;
}

export interface InterviewEvaluation {
    evaluationId: number;
    interviewId: number;
    evaluatorEmployeeId: number;
    communicationScore: number;
    experienceScore: number;
    roleFitScore: number;
    recommendation: string;
    comments?: string | null;
    createdAt: string;
}

export interface Offer {
    offerId: number;
    applicationId: number;
    positionId: number;
    proposedStartDate?: string | null;
    salary?: number | null;
    currency?: string | null;
    employmentType?: string | null;
    status: string;
    createdByEmployeeId?: number | null;
    expiresAt?: string | null;
    acceptedAt?: string | null;
    declinedAt?: string | null;
    declineReason?: string | null;
    createdAt: string;
    updatedAt: string;
}

export interface OnboardingProcess {
    onboardingProcessId: number;
    employeeId: number;
    applicationId?: number | null;
    candidateId?: number | null;
    startDate?: string | null;
    ownerEmployeeId?: number | null;
    status: string;
    completedAt?: string | null;
    createdAt: string;
}

export interface OnboardingTask {
    onboardingTaskId: number;
    onboardingProcessId: number;
    title: string;
    description?: string | null;
    ownerEmployeeId?: number | null;
    dueDate?: string | null;
    completedAt?: string | null;
    category: string;
    sortOrder: number;
    createdAt: string;
}

export interface HireResult {
    employeeId: number;
    onboardingProcessId: number;
    alreadyHired: boolean;
}

export const recruitmentApi = {
    // Requisitions
    getRequisitions: (status?: string) => apiClient.get<JobRequisition[]>(`/api/recruitment/requisitions${status ? `?status=${status}` : ''}`),
    getRequisition: (id: number) => apiClient.get<JobRequisition>(`/api/recruitment/requisitions/${id}`),
    createRequisition: (input: Record<string, unknown>) => apiClient.post<JobRequisition>('/api/recruitment/requisitions', input),
    submitRequisition: (id: number) => apiClient.post<void>(`/api/recruitment/requisitions/${id}/submit`),
    approveRequisition: (id: number) => apiClient.post<void>(`/api/recruitment/requisitions/${id}/approve`),

    // Openings
    getOpenings: (status?: string) => apiClient.get<JobOpening[]>(`/api/recruitment/openings${status ? `?status=${status}` : ''}`),
    createOpening: (input: Record<string, unknown>) => apiClient.post<JobOpening>('/api/recruitment/openings', input),

    // Candidates
    getCandidates: (search?: string) => apiClient.get<Candidate[]>(`/api/recruitment/candidates${search ? `?search=${search}` : ''}`),
    getCandidate: (id: number) => apiClient.get<Candidate>(`/api/recruitment/candidates/${id}`),
    createCandidate: (input: Record<string, unknown>) => apiClient.post<Candidate>('/api/recruitment/candidates', input),

    // Applications
    getApplications: (params: { openingId?: number; stage?: string } = {}) => {
        const sp = new URLSearchParams();
        if (params.openingId) sp.set('openingId', params.openingId.toString());
        if (params.stage) sp.set('stage', params.stage);
        const q = sp.toString();
        return apiClient.get<Application[]>(`/api/recruitment/applications${q ? `?${q}` : ''}`);
    },
    getApplication: (id: number) => apiClient.get<Application>(`/api/recruitment/applications/${id}`),
    createApplication: (input: { candidateId: number; openingId: number; source?: string }) => apiClient.post<Application>('/api/recruitment/applications', input),
    moveApplication: (id: number, stage: string, comment?: string) => apiClient.post<void>(`/api/recruitment/applications/${id}/move`, { stage, comment }),
    getApplicationHistory: (id: number) => apiClient.get<ApplicationStageHistory[]>(`/api/recruitment/applications/${id}/history`),

    // Interviews
    getInterviews: (applicationId?: number) => apiClient.get<Interview[]>(`/api/recruitment/interviews${applicationId ? `?applicationId=${applicationId}` : ''}`),
    createInterview: (input: Record<string, unknown>) => apiClient.post<Interview>('/api/recruitment/interviews', input),
    submitEvaluation: (interviewId: number, input: Record<string, unknown>) => apiClient.post<InterviewEvaluation>(`/api/recruitment/interviews/${interviewId}/evaluations`, input),

    // Offers
    getOffers: (status?: string) => apiClient.get<Offer[]>(`/api/recruitment/offers${status ? `?status=${status}` : ''}`),
    getOffer: (id: number) => apiClient.get<Offer>(`/api/recruitment/offers/${id}`),
    createOffer: (input: Record<string, unknown>) => apiClient.post<Offer>('/api/recruitment/offers', input),
    acceptOffer: (id: number) => apiClient.post<void>(`/api/recruitment/offers/${id}/accept`),
    declineOffer: (id: number, reason?: string) => apiClient.post<void>(`/api/recruitment/offers/${id}/decline`, { reason }),

    // Hire
    hire: (applicationId: number) => apiClient.post<HireResult>(`/api/recruitment/applications/${applicationId}/hire`),

    // Onboarding
    getOnboarding: (status?: string) => apiClient.get<OnboardingProcess[]>(`/api/recruitment/onboarding${status ? `?status=${status}` : ''}`),
    getOnboardingTasks: (id: number) => apiClient.get<OnboardingTask[]>(`/api/recruitment/onboarding/${id}/tasks`),
    addOnboardingTask: (id: number, input: Record<string, unknown>) => apiClient.post<OnboardingTask>(`/api/recruitment/onboarding/${id}/tasks`, input),
    completeOnboardingTask: (taskId: number) => apiClient.post<void>(`/api/recruitment/onboarding/tasks/${taskId}/complete`),
};

export default recruitmentApi;
