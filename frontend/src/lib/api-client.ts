export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

export interface ApiProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  [key: string]: unknown;
}

export class ApiError extends Error {
  public readonly status: number;
  public readonly problem?: ApiProblemDetails;

  constructor(status: number, message: string, problem?: ApiProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }
}

export async function apiClient<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${API_BASE_URL.replace(/\/$/, '')}/${endpoint.replace(/^\//, '')}`;

  const studentId =
    localStorage.getItem('student_id') ||
    sessionStorage.getItem('student_id') ||
    '';

  const headers = new Headers(options.headers || {});
  if (!headers.has('Content-Type') && options.body) {
    headers.set('Content-Type', 'application/json');
  }

  if (studentId && !headers.has('X-Student-Id')) {
    headers.set('X-Student-Id', studentId.trim());
  }

  const response = await fetch(url, {
    ...options,
    headers,
  });

  if (response.status === 204) {
    return undefined as unknown as T;
  }

  const contentType = response.headers.get('content-type') || '';
  const isJson = contentType.includes('application/json') || contentType.includes('application/problem+json');

  if (!response.ok) {
    let errorDetail = `HTTP ${response.status} ${response.statusText}`;
    let problem: ApiProblemDetails | undefined;

    if (isJson) {
      try {
        problem = await response.json();
        errorDetail = problem?.detail || problem?.title || errorDetail;
      } catch {
        // Fallback to generic message
      }
    } else {
      const text = await response.text();
      if (text) errorDetail = text;
    }

    throw new ApiError(response.status, errorDetail, problem);
  }

  if (isJson) {
    return (await response.json()) as T;
  }

  return (await response.text()) as unknown as T;
}

