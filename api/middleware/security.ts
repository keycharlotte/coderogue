// CodeRogue Player Data Analytics - Security Middleware
// Comprehensive security middleware including rate limiting, CORS, and security headers

import rateLimit from 'express-rate-limit';
import helmet from 'helmet';
import { Request, Response, NextFunction } from 'express';
import { logger, logSecurityEvent } from '../utils/logger';

// Rate limiting configurations
export const createRateLimit = (options: {
  windowMs: number;
  max: number;
  message?: string;
  skipSuccessfulRequests?: boolean;
  skipFailedRequests?: boolean;
}) => {
  return rateLimit({
    windowMs: options.windowMs,
    max: options.max,
    message: {
      error: 'Too many requests',
      message: options.message || 'Too many requests from this IP, please try again later.',
      retryAfter: Math.ceil(options.windowMs / 1000)
    },
    standardHeaders: true,
    legacyHeaders: false,
    skipSuccessfulRequests: options.skipSuccessfulRequests || false,
    skipFailedRequests: options.skipFailedRequests || false,
    handler: (req: Request, res: Response) => {
      const clientIp = req.ip || req.connection.remoteAddress || 'unknown';
      
      logSecurityEvent('rate_limit_exceeded', {
        ip: clientIp,
        userAgent: req.get('User-Agent'),
        path: req.path,
        method: req.method,
        limit: options.max,
        windowMs: options.windowMs
      });
      
      res.status(429).json({
        error: 'Too many requests',
        message: options.message || 'Too many requests from this IP, please try again later.',
        retryAfter: Math.ceil(options.windowMs / 1000)
      });
    },
    skip: (req: Request) => {
      // Skip rate limiting for health checks
      return req.path.startsWith('/api/health');
    }
  });
};

// General API rate limiting
export const generalRateLimit = createRateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 1000, // Limit each IP to 1000 requests per windowMs
  message: 'Too many API requests from this IP, please try again later.'
});

// Strict rate limiting for authentication endpoints
export const authRateLimit = createRateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 10, // Limit each IP to 10 auth requests per windowMs
  message: 'Too many authentication attempts from this IP, please try again later.',
  skipSuccessfulRequests: true
});

// Analytics endpoints rate limiting
export const analyticsRateLimit = createRateLimit({
  windowMs: 5 * 60 * 1000, // 5 minutes
  max: 100, // Limit each IP to 100 analytics requests per windowMs
  message: 'Too many analytics requests from this IP, please try again later.'
});

// Events endpoints rate limiting (more permissive for game events)
export const eventsRateLimit = createRateLimit({
  windowMs: 1 * 60 * 1000, // 1 minute
  max: 200, // Limit each IP to 200 event requests per minute
  message: 'Too many event requests from this IP, please try again later.'
});

// Helmet security configuration
export const securityHeaders = helmet({
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      styleSrc: ["'self'", "'unsafe-inline'"],
      scriptSrc: ["'self'"],
      imgSrc: ["'self'", "data:", "https:"],
      connectSrc: ["'self'"],
      fontSrc: ["'self'"],
      objectSrc: ["'none'"],
      mediaSrc: ["'self'"],
      frameSrc: ["'none'"]
    }
  },
  crossOriginEmbedderPolicy: false, // Disable for API
  hsts: {
    maxAge: 31536000,
    includeSubDomains: true,
    preload: true
  },
  noSniff: true,
  frameguard: { action: 'deny' },
  xssFilter: true
});

// IP whitelist middleware
export const createIPWhitelist = (allowedIPs: string[]) => {
  return (req: Request, res: Response, next: NextFunction) => {
    const clientIp = req.ip || req.connection.remoteAddress || '';
    
    // Allow localhost and development IPs
    const developmentIPs = ['127.0.0.1', '::1', 'localhost'];
    const allAllowedIPs = [...allowedIPs, ...developmentIPs];
    
    if (process.env.NODE_ENV === 'development' || allAllowedIPs.some(ip => clientIp.includes(ip))) {
      return next();
    }
    
    logSecurityEvent('ip_blocked', {
      ip: clientIp,
      userAgent: req.get('User-Agent'),
      path: req.path,
      method: req.method,
      allowedIPs: allAllowedIPs
    });
    
    res.status(403).json({
      error: 'Forbidden',
      message: 'Access denied from this IP address'
    });
  };
};

// Request size limiting middleware
export const requestSizeLimit = (maxSize: string = '10mb') => {
  return (req: Request, res: Response, next: NextFunction) => {
    const contentLength = req.get('Content-Length');
    
    if (contentLength) {
      const sizeInBytes = parseInt(contentLength);
      const maxSizeInBytes = parseSize(maxSize);
      
      if (sizeInBytes > maxSizeInBytes) {
        logSecurityEvent('request_size_exceeded', {
          ip: req.ip,
          contentLength: sizeInBytes,
          maxSize: maxSizeInBytes,
          path: req.path,
          method: req.method
        });
        
        return res.status(413).json({
          error: 'Payload Too Large',
          message: `Request size exceeds maximum allowed size of ${maxSize}`
        });
      }
    }
    
    next();
  };
};

// User-Agent validation middleware
export const validateUserAgent = (req: Request, res: Response, next: NextFunction) => {
  const userAgent = req.get('User-Agent');
  
  if (!userAgent || userAgent.length < 3) {
    logSecurityEvent('invalid_user_agent', {
      ip: req.ip,
      userAgent: userAgent || 'missing',
      path: req.path,
      method: req.method
    });
    
    return res.status(400).json({
      error: 'Bad Request',
      message: 'Valid User-Agent header is required'
    });
  }
  
  // Block known malicious user agents
  const maliciousPatterns = [
    /bot/i,
    /crawler/i,
    /spider/i,
    /scraper/i,
    /curl/i,
    /wget/i
  ];
  
  const isMalicious = maliciousPatterns.some(pattern => pattern.test(userAgent));
  
  if (isMalicious && process.env.NODE_ENV === 'production') {
    logSecurityEvent('malicious_user_agent', {
      ip: req.ip,
      userAgent,
      path: req.path,
      method: req.method
    });
    
    return res.status(403).json({
      error: 'Forbidden',
      message: 'Access denied'
    });
  }
  
  next();
};

// CORS configuration
export const corsOptions = {
  origin: (origin: string | undefined, callback: (err: Error | null, allow?: boolean) => void) => {
    // Allow requests with no origin (mobile apps, etc.)
    if (!origin) return callback(null, true);
    
    const allowedOrigins = [
      'http://localhost:3000',
      'http://localhost:5173',
      'http://127.0.0.1:3000',
      'http://127.0.0.1:5173',
      ...(process.env.ALLOWED_ORIGINS?.split(',') || [])
    ];
    
    if (allowedOrigins.includes(origin) || process.env.NODE_ENV === 'development') {
      callback(null, true);
    } else {
      logSecurityEvent('cors_blocked', {
        origin,
        allowedOrigins
      });
      
      callback(new Error('Not allowed by CORS'));
    }
  },
  credentials: true,
  optionsSuccessStatus: 200,
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization', 'X-Requested-With', 'X-Request-ID']
};

// Security event logging middleware
export const securityLogger = (req: Request, res: Response, next: NextFunction) => {
  const startTime = Date.now();
  
  res.on('finish', () => {
    const duration = Date.now() - startTime;
    const statusCode = res.statusCode;
    
    // Log suspicious activities
    if (statusCode >= 400) {
      logSecurityEvent('suspicious_request', {
        ip: req.ip,
        userAgent: req.get('User-Agent'),
        path: req.path,
        method: req.method,
        statusCode,
        duration,
        body: req.method !== 'GET' ? req.body : undefined
      });
    }
  });
  
  next();
};

// Helper function to parse size strings
function parseSize(size: string): number {
  const units: { [key: string]: number } = {
    b: 1,
    kb: 1024,
    mb: 1024 * 1024,
    gb: 1024 * 1024 * 1024
  };
  
  const match = size.toLowerCase().match(/^(\d+(?:\.\d+)?)\s*(b|kb|mb|gb)?$/);
  
  if (!match) {
    throw new Error(`Invalid size format: ${size}`);
  }
  
  const value = parseFloat(match[1]);
  const unit = match[2] || 'b';
  
  return Math.floor(value * units[unit]);
}

export default {
  generalRateLimit,
  authRateLimit,
  analyticsRateLimit,
  eventsRateLimit,
  securityHeaders,
  createIPWhitelist,
  requestSizeLimit,
  validateUserAgent,
  corsOptions,
  securityLogger
};