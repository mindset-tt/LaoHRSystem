/**
 * Project workspace endpoints (Phase 2).
 * Lightweight PM slice: project list / detail, task list / detail, comments, activity.
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