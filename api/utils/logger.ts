// CodeRogue Player Data Analytics - Logging System
// Structured logging with Winston for requests, responses, and errors

import winston from 'winston';
import path from 'path';
import { Request, Response, NextFunction } from 'express';

// Log levels
const logLevels = {
  error: 0,
  warn: 1,
  info: 2,
  http: 3,
  debug: 4
};

// Log colors
const logColors = {
  error: 'red',
  warn: 'yellow',
  info: 'green',
  http: 'magenta',
  debug: 'white'
};

winston.addColors(logColors);

// Custom format for console output
const consoleFormat = winston.format.combine(
  winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss:ms' }),
  winston.format.colorize({ all: true }),
  winston.format.printf((info) => {
    const { timestamp, level, message, ...meta } = info;
    const metaStr = Object.keys(meta).length ? JSON.stringify(meta, null, 2) : '';
    return `${timestamp} [${level}]: ${message} ${metaStr}`;
  })
);

// Custom format for file output
const fileFormat = winston.format.combine(
  winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss:ms' }),
  winston.format.errors({ stack: true }),
  winston.format.json()
);

// Create transports
const transports: winston.transport[] = [
  // Console transport
  new winston.transports.Console({
    level: process.env.NODE_ENV === 'production' ? 'info' : 'debug',
    format: consoleFormat
  }),

  // Error log file
  new winston.transports.File({
    filename: path.join(process.cwd(), 'logs', 'error.log'),
    level: 'error',
    format: fileFormat,
    maxsize: 5242880, // 5MB
    maxFiles: 5
  }),

  // Combined log file
  new winston.transports.File({
    filename: path.join(process.cwd(), 'logs', 'combined.log'),
    format: fileFormat,
    maxsize: 5242880, // 5MB
    maxFiles: 5
  })
];

// Add daily rotate file transport for production
if (process.env.NODE_ENV === 'production') {
  const DailyRotateFile = require('winston-daily-rotate-file');
  
  transports.push(
    new DailyRotateFile({
      filename: path.join(process.cwd(), 'logs', 'application-%DATE%.log'),
      datePattern: 'YYYY-MM-DD',
      zippedArchive: true,
      maxSize: '20m',
      maxFiles: '14d',
      format: fileFormat
    })
  );
}

// Create logger instance
export const logger = winston.createLogger({
  level: process.env.NODE_ENV === 'production' ? 'info' : 'debug',
  levels: logLevels,
  format: fileFormat,
  transports,
  exitOnError: false
});

// Request logging middleware
export const requestLogger = (req: Request, res: Response, next: NextFunction) => {
  const startTime = Date.now();
  const requestId = req.headers['x-request-id'] || 'unknown';
  
  // Log incoming request
  logger.http('Incoming request', {
    requestId,
    method: req.method,
    url: req.originalUrl,
    ip: req.ip,
    userAgent: req.get('User-Agent'),
    contentType: req.get('Content-Type'),
    contentLength: req.get('Content-Length'),
    timestamp: new Date().toISOString()
  });

  // Override res.end to log response
  const originalEnd = res.end;
  res.end = function(chunk?: any, encoding?: any): Response {
    const duration = Date.now() - startTime;
    
    // Log response
    logger.http('Request completed', {
      requestId,
      method: req.method,
      url: req.originalUrl,
      statusCode: res.statusCode,
      duration: `${duration}ms`,
      contentLength: res.get('Content-Length'),
      timestamp: new Date().toISOString()
    });

    // Call original end method
    return originalEnd.call(this, chunk, encoding);
  };

  next();
};

// Database operation logger
export const logDatabaseOperation = (
  operation: string,
  table: string,
  duration?: number,
  error?: Error
) => {
  const logData = {
    operation,
    table,
    duration: duration ? `${duration}ms` : undefined,
    timestamp: new Date().toISOString()
  };

  if (error) {
    logger.error('Database operation failed', {
      ...logData,
      error: error.message,
      stack: error.stack
    });
  } else {
    logger.debug('Database operation completed', logData);
  }
};

// Database query logger (alias for logDatabaseOperation)
export const logDatabaseQuery = logDatabaseOperation;

// API endpoint performance logger
export const logApiPerformance = (
  endpoint: string,
  method: string,
  duration: number,
  statusCode: number,
  requestId?: string
) => {
  const level = duration > 1000 ? 'warn' : 'info';
  
  logger.log(level, 'API performance', {
    endpoint,
    method,
    duration: `${duration}ms`,
    statusCode,
    requestId,
    timestamp: new Date().toISOString()
  });
};

// Security event logger
export const logSecurityEvent = (
  event: string,
  details: any,
  severity: 'low' | 'medium' | 'high' = 'medium'
) => {
  logger.warn('Security event', {
    event,
    severity,
    details,
    timestamp: new Date().toISOString()
  });
};

// Business logic logger
export const logBusinessEvent = (
  event: string,
  details: any,
  userId?: string
) => {
  logger.info('Business event', {
    event,
    userId,
    details,
    timestamp: new Date().toISOString()
  });
};

// Error logger with context
export const logError = (
  error: Error,
  context?: {
    requestId?: string;
    userId?: string;
    operation?: string;
    additionalInfo?: any;
  }
) => {
  logger.error('Application error', {
    message: error.message,
    stack: error.stack,
    name: error.name,
    ...context,
    timestamp: new Date().toISOString()
  });
};

// System health logger
export const logSystemHealth = (
  component: string,
  status: 'healthy' | 'degraded' | 'unhealthy',
  metrics?: any
) => {
  const level = status === 'healthy' ? 'info' : status === 'degraded' ? 'warn' : 'error';
  
  logger.log(level, 'System health check', {
    component,
    status,
    metrics,
    timestamp: new Date().toISOString()
  });
};

// Create logs directory if it doesn't exist
import fs from 'fs';
const logsDir = path.join(process.cwd(), 'logs');
if (!fs.existsSync(logsDir)) {
  fs.mkdirSync(logsDir, { recursive: true });
}

export default logger;