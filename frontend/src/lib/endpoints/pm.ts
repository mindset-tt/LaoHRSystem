/**
 * PM planning endpoints (Phase 3C4) — kanban board, dependencies, gantt,
 * capacity, health, portfolio, RAID.
 */
import { apiClient } from '../apiClient';

export interface KanbanCard {
    taskId: number;
    title: string;
    status: string;
    priority: string;
    dueDate?: string | null;
    progressPercent: number;
    milestoneId?: number | null;
    parentTaskId?: number | null;
    isOverdue: boolean;
    assigneeNames: (string | null)[];
}

export interface KanbanColumn {
    status: string;
    cards: KanbanCard[];
}

export interface KanbanBoard {
    columns: KanbanColumn[];
}

export interface DependencyDto {
    taskDependencyId: number;
    predecessorTaskId: number;
    successorTaskId: number;
    type: string;
}

export interface GanttTask {
    taskId: number;
    title: string;
    parentTaskId?: number | null;
    milestoneId?: number | null;
    startDate?: string | null;
    dueDate?: string | null;
    progressPercent: number;
    status: string;
}

export interface GanttMilestone {
    milestoneId: number;
    name: string;
    dueDate?: string | null;
    status: string;
}

export interface GanttData {
    tasks: GanttTask[];
    milestones: GanttMilestone[];
    dependencies: DependencyDto[];
}

export interface ResourceWorkload {
    employeeId: number;
    employeeName: string;
    totalAllocationPercent: number;
    isOverallocated: boolean;
    activeProjectCount: number;
}

export interface ProjectHealth {
    projectId: number;
    scheduleHealth: string;
    riskHealth: string;
    overallHealth: string;
    overdueTaskCount: number;
    openCriticalRiskCount: number;
    openIssueCount: number;
    progressPercent: number;
}

export interface PortfolioItem {
    projectId: number;
    code: string;
    name: string;
    status: string;
    priority: string;
    ownerName?: string | null;
    startDate?: string | null;
    dueDate?: string | null;
    progressPercent: number;
    health: string;
    openRiskCount: number;
    overdueTaskCount: number;
    openIssueCount: number;
    nextMilestoneDate?: string | null;
}

export const pmApi = {
    getBoard: (projectId: number) => apiClient.get<KanbanBoard>(`/api/pm/projects/${projectId}/board`),
    moveTask: (projectId: number, input: { taskId: number; status: string; sortOrder?: number }) =>
        apiClient.post<void>(`/api/pm/projects/${projectId}/board/move`, input),
    getDependencies: (projectId: number) => apiClient.get<DependencyDto[]>(`/api/pm/projects/${projectId}/dependencies`),
    addDependency: (projectId: number, input: { predecessorTaskId: number; successorTaskId: number }) =>
        apiClient.post<DependencyDto>(`/api/pm/projects/${projectId}/dependencies`, input),
    removeDependency: (projectId: number, dependencyId: number) =>
        apiClient.delete<void>(`/api/pm/projects/${projectId}/dependencies/${dependencyId}`),
    getTimeline: (projectId: number) => apiClient.get<GanttData>(`/api/pm/projects/${projectId}/timeline`),
    getCapacity: () => apiClient.get<ResourceWorkload[]>('/api/pm/capacity'),
    getHealth: (projectId: number) => apiClient.get<ProjectHealth>(`/api/pm/projects/${projectId}/health`),
    getPortfolio: () => apiClient.get<PortfolioItem[]>('/api/pm/portfolio'),
    getAssumptions: (projectId: number) => apiClient.get<unknown[]>(`/api/pm/projects/${projectId}/assumptions`),
    addAssumption: (projectId: number, input: { description: string; ownerId?: number }) =>
        apiClient.post<unknown>(`/api/pm/projects/${projectId}/assumptions`, input),
    getDecisions: (projectId: number) => apiClient.get<unknown[]>(`/api/pm/projects/${projectId}/decisions`),
    addDecision: (projectId: number, input: { decision: string; context?: string; ownerId?: number }) =>
        apiClient.post<unknown>(`/api/pm/projects/${projectId}/decisions`, input),
};

export default pmApi;
