/**
 * Project workspace endpoints (Phase 2 + Phase 3).
 * Lightweight PM slice: project list / detail, task list / detail, comments, activity.
 * Phase 3 adds risk, issue (with comments), and resource endpoints.
 */
import { apiClient } from '../apiClient';
import type { PaginatedResponse } from '../types/pagination';

export interface ProjectListItem {
    projectId: number;
    code: string;
    name: string;
    status: string;
    priority: string;
    color?: string | null;
    startDate?: string | null;
    dueDate?: string | null;
    completedAt?: string | null;
    ownerId: number;
    ownerName?: string | null;
    memberCount: number;
    openTaskCount: number;
    doneTaskCount: number;
    createdAt: string;
}

export interface ProjectMemberDetail {
    projectMemberId: number;
    employeeId: number;
    employeeName: string;
    role: string;
    joinedAt: string;
}

export interface MilestoneDetail {
    milestoneId: number;
    name: string;
    description?: string | null;
    dueDate?: string | null;
    completedAt?: string | null;
    status: string;
}

export interface ProjectDetail {
    projectId: number;
    code: string;
    name: string;
    description?: string | null;
    status: string;
    priority: string;
    color?: string | null;
    startDate?: string | null;
    dueDate?: string | null;
    completedAt?: string | null;
    ownerId: number;
    ownerName?: string | null;
    createdAt: string;
    updatedAt: string;
    members: ProjectMemberDetail[];
    milestones: MilestoneDetail[];
    taskCountByStatus: Record<string, number>;
}

export interface TaskAssigneeSummary {
    employeeId: number;
    employeeName: string;
    role: string;
}

export interface TaskListItem {
    taskId: number;
    projectId: number;
    projectName: string;
    projectCode: string;
    milestoneId?: number | null;
    milestoneName?: string | null;
    taskNumber?: string | null;
    title: string;
    status: string;
    priority: string;
    startDate?: string | null;
    dueDate?: string | null;
    completedAt?: string | null;
    progressPercent: number;
    reporterId: number;
    reporterName?: string | null;
    assignees: TaskAssigneeSummary[];
    commentCount: number;
    isOverdue: boolean;
    createdAt: string;
    updatedAt: string;
}

export interface TaskCommentDetail {
    commentId: number;
    authorId: number;
    authorName: string;
    body: string;
    parentCommentId?: number | null;
    createdAt: string;
}

export interface TaskDetail {
    taskId: number;
    projectId: number;
    milestoneId?: number | null;
    taskNumber?: string | null;
    title: string;
    description?: string | null;
    status: string;
    priority: string;
    startDate?: string | null;
    dueDate?: string | null;
    completedAt?: string | null;
    progressPercent: number;
    estimatedHours?: number | null;
    actualHours?: number | null;
    reporterId: number;
    assignees: TaskAssigneeSummary[];
    comments: TaskCommentDetail[];
    createdAt: string;
    updatedAt: string;
}

export interface ActivityListItem {
    activityId: number;
    projectId: number;
    taskId?: number | null;
    actorId: number;
    actorName?: string | null;
    action: string;
    payloadJson?: string | null;
    createdAt: string;
}

export interface CreateProjectInput {
    code: string;
    name: string;
    description?: string;
    status?: string;
    priority?: string;
    color?: string;
    startDate?: string;
    dueDate?: string;
    ownerId?: number;
}

export interface UpdateProjectInput {
    name?: string;
    description?: string;
    status?: string;
    priority?: string;
    color?: string;
    startDate?: string;
    dueDate?: string;
    ownerId?: number;
}

export interface AddMemberInput {
    employeeId: number;
    role: string;
}

export interface CreateTaskInput {
    title: string;
    description?: string;
    status?: string;
    priority?: string;
    milestoneId?: number;
    startDate?: string;
    dueDate?: string;
    estimatedHours?: number;
    assigneeIds?: number[];
}

export interface UpdateTaskInput {
    title?: string;
    description?: string;
    status?: string;
    priority?: string;
    milestoneId?: number;
    startDate?: string;
    dueDate?: string;
    progressPercent?: number;
    estimatedHours?: number;
    actualHours?: number;
}

function toQuery(params: Record<string, unknown>): string {
    const sp = new URLSearchParams();
    for (const [k, v] of Object.entries(params)) {
        if (v === undefined || v === null || v === '') continue;
        sp.set(k, String(v));
    }
    const q = sp.toString();
    return q ? `?${q}` : '';
}

export const projectsApi = {
    getAll: (params?: {
        status?: string;
        priority?: string;
        search?: string;
        mineOnly?: boolean;
        page?: number;
        pageSize?: number;
    }) =>
        apiClient.get<PaginatedResponse<ProjectListItem>>(
            `/api/projects${toQuery(params ?? {})}`
        ),
    getById: (id: number) =>
        apiClient.get<ProjectDetail>(`/api/projects/${id}`),
    create: (input: CreateProjectInput) =>
        apiClient.post<ProjectDetail>('/api/projects', input),
    update: (id: number, input: UpdateProjectInput) =>
        apiClient.put<void>(`/api/projects/${id}`, input),
    addMember: (id: number, input: AddMemberInput) =>
        apiClient.post<void>(`/api/projects/${id}/members`, input),
    removeMember: (id: number, employeeId: number) =>
        apiClient.delete<void>(`/api/projects/${id}/members/${employeeId}`),
    getActivities: (id: number, page = 1, pageSize = 50) =>
        apiClient.get<PaginatedResponse<ActivityListItem>>(
            `/api/projects/${id}/activities${toQuery({ page, pageSize })}`
        ),
};

export const projectTasksApi = {
    getAll: (
        projectId: number,
        params?: {
            status?: string;
            priority?: string;
            milestoneId?: number;
            assignedToMe?: boolean;
            search?: string;
            page?: number;
            pageSize?: number;
        }
    ) =>
        apiClient.get<PaginatedResponse<TaskListItem>>(
            `/api/projects/${projectId}/projecttasks${toQuery(params ?? {})}`
        ),
    getById: (projectId: number, taskId: number) =>
        apiClient.get<TaskDetail>(`/api/projects/${projectId}/projecttasks/${taskId}`),
    create: (projectId: number, input: CreateTaskInput) =>
        apiClient.post<TaskDetail>(`/api/projects/${projectId}/projecttasks`, input),
    update: (projectId: number, taskId: number, input: UpdateTaskInput) =>
        apiClient.put<void>(`/api/projects/${projectId}/projecttasks/${taskId}`, input),
    addComment: (projectId: number, taskId: number, body: string, parentCommentId?: number) =>
        apiClient.post<TaskCommentDetail>(
            `/api/projects/${projectId}/projecttasks/${taskId}/comments`,
            { body, parentCommentId }
        ),
    setAssignees: (projectId: number, taskId: number, employeeIds: number[]) =>
        apiClient.put<void>(
            `/api/projects/${projectId}/projecttasks/${taskId}/assignees`,
            { employeeIds }
        ),
};

export const myTasksApi = {
    getAll: (params?: { status?: string; page?: number; pageSize?: number }) =>
        apiClient.get<PaginatedResponse<TaskListItem>>(
            `/api/my-tasks${toQuery(params ?? {})}`
        ),
};

// =============================================================================
// Phase 3 — Risk / Issue / Resource
// =============================================================================

export interface RiskListItem {
    riskId: number;
    projectId: number;
    title: string;
    priority: string;
    likelihood: number;
    impact: number;
    score: number;
    status: string;
    ownerId?: number | null;
    ownerName?: string | null;
    dueDate?: string | null;
    updatedAt: string;
}

export interface RiskDetail extends Omit<RiskListItem, 'score'> {
    description?: string | null;
    mitigation?: string | null;
    createdAt: string;
}

export interface CreateRiskInput {
    title: string;
    description?: string;
    priority?: string;
    likelihood?: number;
    impact?: number;
    status?: string;
    mitigation?: string;
    ownerId?: number;
    dueDate?: string;
}

export interface UpdateRiskInput {
    title?: string;
    description?: string;
    priority?: string;
    likelihood?: number;
    impact?: number;
    status?: string;
    mitigation?: string;
    ownerId?: number;
    dueDate?: string;
}

export const risksApi = {
    getAll: (
        projectId: number,
        params?: { status?: string; priority?: string; search?: string; page?: number; pageSize?: number }
    ) =>
        apiClient.get<PaginatedResponse<RiskListItem>>(
            `/api/projects/${projectId}/risks${toQuery(params ?? {})}`
        ),
    getById: (projectId: number, riskId: number) =>
        apiClient.get<RiskDetail>(`/api/projects/${projectId}/risks/${riskId}`),
    create: (projectId: number, input: CreateRiskInput) =>
        apiClient.post<RiskDetail>(`/api/projects/${projectId}/risks`, input),
    update: (projectId: number, riskId: number, input: UpdateRiskInput) =>
        apiClient.put<void>(`/api/projects/${projectId}/risks/${riskId}`, input),
    delete: (projectId: number, riskId: number) =>
        apiClient.delete<void>(`/api/projects/${projectId}/risks/${riskId}`),
};

export interface IssueListItem {
    issueId: number;
    projectId: number;
    taskId?: number | null;
    taskNumber?: string | null;
    title: string;
    status: string;
    priority: string;
    reporterId: number;
    reporterName?: string | null;
    assigneeId?: number | null;
    assigneeName?: string | null;
    dueDate?: string | null;
    resolvedAt?: string | null;
    isOverdue: boolean;
    commentCount: number;
    updatedAt: string;
}

export interface IssueCommentDetail {
    issueCommentId: number;
    issueId: number;
    authorId: number;
    authorName: string;
    body: string;
    createdAt: string;
}

export interface IssueDetail {
    issueId: number;
    projectId: number;
    taskId?: number | null;
    taskNumber?: string | null;
    title: string;
    description?: string | null;
    status: string;
    priority: string;
    reporterId: number;
    reporterName?: string | null;
    assigneeId?: number | null;
    assigneeName?: string | null;
    dueDate?: string | null;
    resolvedAt?: string | null;
    createdAt: string;
    updatedAt: string;
    comments: IssueCommentDetail[];
}

export interface CreateIssueInput {
    title: string;
    description?: string;
    status?: string;
    priority?: string;
    taskId?: number;
    assigneeId?: number;
    dueDate?: string;
}

export interface UpdateIssueInput {
    title?: string;
    description?: string;
    status?: string;
    priority?: string;
    taskId?: number;
    assigneeId?: number;
    dueDate?: string;
}

export const issuesApi = {
    getAll: (
        projectId: number,
        params?: {
            status?: string;
            priority?: string;
            assigneeId?: number;
            mineOnly?: boolean;
            search?: string;
            page?: number;
            pageSize?: number;
        }
    ) =>
        apiClient.get<PaginatedResponse<IssueListItem>>(
            `/api/projects/${projectId}/issues${toQuery(params ?? {})}`
        ),
    getById: (projectId: number, issueId: number) =>
        apiClient.get<IssueDetail>(`/api/projects/${projectId}/issues/${issueId}`),
    create: (projectId: number, input: CreateIssueInput) =>
        apiClient.post<IssueDetail>(`/api/projects/${projectId}/issues`, input),
    update: (projectId: number, issueId: number, input: UpdateIssueInput) =>
        apiClient.put<void>(`/api/projects/${projectId}/issues/${issueId}`, input),
    delete: (projectId: number, issueId: number) =>
        apiClient.delete<void>(`/api/projects/${projectId}/issues/${issueId}`),
    addComment: (projectId: number, issueId: number, body: string) =>
        apiClient.post<IssueCommentDetail>(
            `/api/projects/${projectId}/issues/${issueId}/comments`,
            { body }
        ),
};

export interface ResourceListItem {
    resourceId: number;
    projectId: number;
    projectCode: string;
    projectName: string;
    employeeId: number;
    employeeName: string;
    role: string;
    allocationPercent: number;
    startDate?: string | null;
    endDate?: string | null;
}

export interface ResourceDetail extends ResourceListItem {
    notes?: string | null;
    createdAt: string;
}

export interface CreateResourceInput {
    employeeId: number;
    role?: string;
    allocationPercent?: number;
    startDate?: string;
    endDate?: string;
    notes?: string;
}

export interface UpdateResourceInput {
    role?: string;
    allocationPercent?: number;
    startDate?: string;
    endDate?: string;
    notes?: string;
}

export const resourcesApi = {
    getAll: (
        projectId: number,
        params?: { role?: string; activeOn?: boolean; page?: number; pageSize?: number }
    ) =>
        apiClient.get<PaginatedResponse<ResourceListItem>>(
            `/api/projects/${projectId}/resources${toQuery(params ?? {})}`
        ),
    getCrossProject: (params?: {
        employeeId?: number;
        role?: string;
        activeOn?: boolean;
        page?: number;
        pageSize?: number;
    }) =>
        apiClient.get<PaginatedResponse<ResourceListItem>>(
            `/api/resources${toQuery(params ?? {})}`
        ),
    create: (projectId: number, input: CreateResourceInput) =>
        apiClient.post<ResourceDetail>(`/api/projects/${projectId}/resources`, input),
    update: (projectId: number, resourceId: number, input: UpdateResourceInput) =>
        apiClient.put<void>(`/api/projects/${projectId}/resources/${resourceId}`, input),
    delete: (projectId: number, resourceId: number) =>
        apiClient.delete<void>(`/api/projects/${projectId}/resources/${resourceId}`),
};