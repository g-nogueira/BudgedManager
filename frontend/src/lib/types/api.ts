export interface ApiError {
  error: string;
  type: 'ValidationError' | 'NotFound' | 'Conflict' | 'Forbidden';
}
