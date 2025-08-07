// CodeRogue Player Data Analytics - Error Handling Middleware
// This file contains custom error types and error handling middleware

import { Request, Response, NextFunction } from 'express';

// Custom Error Types
export class AppError extends Error {
  public readonly statusCode: number;
  public readonly isOperational: boolean;
  public readonly errorCode?: string;
  public readonly details?: any;

  constructor(
    message: string,
    statusCode: number = 500,
    errorCode?: string,
    details?: any,
    isOperational: boolean = true
  ) {
    super(message);
    this.statusCode = statusCode;
    this.isOperational = isOperational;
    this.errorCode = errorCode;
    this.details = details;

    Error.captureStackTrace(this, this.constructor);
  }
}

// Specific Error Classes
export class ValidationError extends AppError {
  constructor(message: string, details?: any) {
    super(message, 400, 'VALIDATION_ERROR', details);
  }
}

export class NotFoundError extends AppError {
  constructor(resource: string = 'Resource') {
    super(`${resource} not found`, 404, 'NOT_FOUND');
  }
}

export class DatabaseError extends AppError {
  constructor(message: string, details?: any) {
    super(message, 500, 'DATABASE_ERROR', details);
  }
}

export class AuthenticationError extends AppError {
  constructor(message: string = 'Authentication failed') {
    super(message, 401, 'AUTHENTICATION_ERROR');
  }
}

export class AuthorizationError extends AppError {
  constructor(message: string = 'Insufficient permissions') {
    super(message, 403, 'AUTHORIZATION_ERROR');
  }
}

export class RateLimitError extends AppError {
  constructor(message: string = 'Too many requests') {
    super(message, 429, 'RATE_LIMIT_ERROR');
  }
}

export class ExternalServiceError extends AppError {
  constructor(service: string, message?: string) {
    super(message || `External service ${service} is unavailable`, 503, 'EXTERNAL_SERVICE_ERROR');
  }
}

// Error Response Interface
interface ErrorResponse {
  success: false;
  error: {
    message: string;
    code?: string;
    statusCode: number;
    details?: any;
    timestamp: string;
    path: string;
    requestId?: string;
  };
}

// Global Error Handler Middleware
export const errorHandler = (
  error: Error,
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  let appError: AppError;

  // Convert known errors to AppError
  if (error instanceof AppError) {
    appError = error;
  } else if (error.name === 'ValidationError') {
    appError = new ValidationError(error.message);
  } else if (error.name === 'CastError') {
    appError = new ValidationError('Invalid data format');
  } else if (error.name === 'MongoError' || error.name === 'MongoServerError') {
    appError = new DatabaseError('Database operation failed');
  } else if (error.message?.includes('ECONNREFUSED')) {
    appError = new DatabaseError('Database connection failed');
  } else {
    // Unknown error - log it and return generic error
    console.error('Unknown error:', error);
    appError = new AppError(
      process.env.NODE_ENV === 'production' 
        ? 'Internal server error' 
        : error.message,
      500,
      'INTERNAL_ERROR',
      process.env.NODE_ENV === 'development' ? error.stack : undefined,
      false
    );
  }

  // Create error response
  const errorResponse: ErrorResponse = {
    success: false,
    error: {
      message: appError.message,
      code: appError.errorCode,
      statusCode: appError.statusCode,
      timestamp: new Date().toISOString(),
      path: req.path,
      requestId: req.headers['x-request-id'] as string
    }
  };

  // Add details in development mode
  if (process.env.NODE_ENV === 'development' && appError.details) {
    errorResponse.error.details = appError.details;
  }

  // Add stack trace in development mode for non-operational errors
  if (process.env.NODE_ENV === 'development' && !appError.isOperational) {
    errorResponse.error.details = {
      ...errorResponse.error.details,
      stack: appError.stack
    };
  }

  // Log error (will be enhanced with proper logging system)
  console.error(`[${new Date().toISOString()}] ${appError.statusCode} - ${appError.message}`, {
    path: req.path,
    method: req.method,
    ip: req.ip,
    userAgent: req.get('User-Agent'),
    requestId: req.headers['x-request-id'],
    stack: appError.stack
  });

  res.status(appError.statusCode).json(errorResponse);
};

// Async Error Wrapper
export const asyncHandler = (
  fn: (req: Request, res: Response, next: NextFunction) => Promise<any>
) => {
  return (req: Request, res: Response, next: NextFunction) => {
    Promise.resolve(fn(req, res, next)).catch(next);
  };
};

// 404 Handler
export const notFoundHandler = (req: Request, res: Response, next: NextFunction) => {
  const error = new NotFoundError(`Route ${req.originalUrl}`);
  next(error);
};

// Request ID Middleware
export const requestIdMiddleware = (req: Request, res: Response, next: NextFunction) => {
  const requestId = req.headers['x-request-id'] || 
    `req_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  
  req.headers['x-request-id'] = requestId as string;
  res.setHeader('X-Request-ID', requestId);
  
  next();
};