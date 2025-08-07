// CodeRogue Player Data Analytics - Error Handling Middleware
// Unified error handling with proper error types and responses

import { Request, Response, NextFunction } from 'express';
import { logger } from '../utils/logger';

// Custom Error Classes
export class AppError extends Error {
  public readonly statusCode: number;
  public readonly isOperational: boolean;
  public readonly errorCode?: string;

  constructor(
    message: string,
    statusCode: number = 500,
    errorCode?: string,
    isOperational: boolean = true
  ) {
    super(message);
    this.statusCode = statusCode;
    this.isOperational = isOperational;
    this.errorCode = errorCode;

    Error.captureStackTrace(this, this.constructor);
  }
}

export class ValidationError extends AppError {
  constructor(message: string, field?: string) {
    super(message, 400, 'VALIDATION_ERROR');
    this.name = 'ValidationError';
  }
}

export class DatabaseError extends AppError {
  constructor(message: string, originalError?: Error) {
    super(message, 500, 'DATABASE_ERROR');
    this.name = 'DatabaseError';
    if (originalError) {
      this.stack = originalError.stack;
    }
  }
}

export class NotFoundError extends AppError {
  constructor(resource: string) {
    super(`${resource} not found`, 404, 'NOT_FOUND');
    this.name = 'NotFoundError';
  }
}

export class UnauthorizedError extends AppError {
  constructor(message: string = 'Unauthorized access') {
    super(message, 401, 'UNAUTHORIZED');
    this.name = 'UnauthorizedError';
  }
}

export class RateLimitError extends AppError {
  constructor(message: string = 'Too many requests') {
    super(message, 429, 'RATE_LIMIT_EXCEEDED');
    this.name = 'RateLimitError';
  }
}

// Error Response Interface
interface ErrorResponse {
  success: false;
  error: {
    message: string;
    code?: string;
    statusCode: number;
    timestamp: string;
    path: string;
    details?: any;
  };
}

// Development Error Response (includes stack trace)
interface DevErrorResponse extends ErrorResponse {
  error: ErrorResponse['error'] & {
    stack?: string;
  };
}

// Error Handler Middleware
export const errorHandler = (
  err: Error,
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  let error = err;

  // Convert non-AppError instances to AppError
  if (!(error instanceof AppError)) {
    // Handle specific error types
    if (error.name === 'ValidationError') {
      error = new ValidationError(error.message);
    } else if (error.name === 'CastError') {
      error = new ValidationError('Invalid data format');
    } else if (error.name === 'MongoError' || error.name === 'PostgresError') {
      error = new DatabaseError('Database operation failed', error);
    } else {
      error = new AppError(
        'Something went wrong',
        500,
        'INTERNAL_SERVER_ERROR',
        false
      );
    }
  }

  const appError = error as AppError;

  // Log error
  logger.error('Error occurred:', {
    message: appError.message,
    statusCode: appError.statusCode,
    errorCode: appError.errorCode,
    stack: appError.stack,
    url: req.originalUrl,
    method: req.method,
    ip: req.ip,
    userAgent: req.get('User-Agent'),
    timestamp: new Date().toISOString()
  });

  // Prepare error response
  const errorResponse: ErrorResponse = {
    success: false,
    error: {
      message: appError.message,
      code: appError.errorCode,
      statusCode: appError.statusCode,
      timestamp: new Date().toISOString(),
      path: req.originalUrl
    }
  };

  // Add stack trace in development
  if (process.env.NODE_ENV === 'development') {
    (errorResponse as DevErrorResponse).error.stack = appError.stack;
  }

  // Send error response
  res.status(appError.statusCode).json(errorResponse);
};

// Async Error Handler Wrapper
export const asyncHandler = (
  fn: (req: Request, res: Response, next: NextFunction) => Promise<any>
) => {
  return (req: Request, res: Response, next: NextFunction) => {
    Promise.resolve(fn(req, res, next)).catch(next);
  };
};

// 404 Handler
export const notFoundHandler = (
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  const error = new NotFoundError(`Route ${req.originalUrl}`);
  next(error);
};

// Unhandled Promise Rejection Handler
export const handleUnhandledRejection = () => {
  process.on('unhandledRejection', (reason: any, promise: Promise<any>) => {
    logger.error('Unhandled Promise Rejection:', {
      reason: reason?.message || reason,
      stack: reason?.stack,
      promise: promise.toString()
    });
    
    // Graceful shutdown
    process.exit(1);
  });
};

// Uncaught Exception Handler
export const handleUncaughtException = () => {
  process.on('uncaughtException', (error: Error) => {
    logger.error('Uncaught Exception:', {
      message: error.message,
      stack: error.stack
    });
    
    // Graceful shutdown
    process.exit(1);
  });
};