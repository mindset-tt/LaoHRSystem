/**
 * Standard pagination envelope — matches the backend's PaginatedResponse<T>.
 */
export interface PaginatedResponse<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
}

export interface PaginatedQuery {
    page?: number;
    pageSize?: number;
    search?: string;
    sort?: string;
    direction?: 'asc' | 'desc';
}
