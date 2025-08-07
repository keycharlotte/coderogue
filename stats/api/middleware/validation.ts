import Joi from 'joi';
import { Request, Response, NextFunction } from 'express';
import { ValidationError } from './errors.js';
import { logger } from '../utils/logger.js';

// Common validation schemas
export const commonSchemas = {
  // UUID validation
  uuid: Joi.string().uuid().required(),
  
  // Pagination
  pagination: {
    page: Joi.number().integer().min(1).default(1),
    limit: Joi.number().integer().min(1).max(100).default(10),
    offset: Joi.number().integer().min(0).default(0)
  },
  
  // Date range
  dateRange: {
    startDate: Joi.date().iso(),
    endDate: Joi.date().iso().min(Joi.ref('startDate'))
  },
  
  // Common string validations
  nonEmptyString: Joi.string().trim().min(1).required(),
  optionalString: Joi.string().trim().allow('', null),
  
  // Numeric validations
  positiveNumber: Joi.number().positive().required(),
  nonNegativeNumber: Joi.number().min(0).required()
};

// Player data validation schemas
export const playerSchemas = {
  // Player registration/update
  playerData: Joi.object({
    playerId: commonSchemas.uuid,
    playerName: Joi.string().trim().min(2).max(50).pattern(/^[a-zA-Z0-9_-]+$/).required(),
    email: Joi.string().email().optional(),
    level: Joi.number().integer().min(1).max(1000).default(1),
    experience: commonSchemas.nonNegativeNumber.default(0),
    coins: commonSchemas.nonNegativeNumber.default(0),
    gems: commonSchemas.nonNegativeNumber.default(0)
  }),
  
  // Player query parameters
  playerQuery: Joi.object({
    playerId: commonSchemas.uuid.optional(),
    playerName: Joi.string().trim().min(2).max(50).optional(),
    ...commonSchemas.pagination
  })
};

// Game event validation schemas
export const eventSchemas = {
  // Single game event
  gameEvent: Joi.object({
    eventId: commonSchemas.uuid.optional(),
    playerId: commonSchemas.uuid,
    eventType: Joi.string().valid(
      'level_start', 'level_complete', 'level_fail',
      'item_collected', 'skill_used', 'enemy_defeated',
      'achievement_unlocked', 'purchase_made', 'login', 'logout'
    ).required(),
    eventData: Joi.object().optional(),
    timestamp: Joi.date().iso().default(() => new Date()),
    sessionId: commonSchemas.uuid.optional(),
    metadata: Joi.object({
      platform: Joi.string().valid('web', 'mobile', 'desktop').optional(),
      version: Joi.string().pattern(/^\d+\.\d+\.\d+$/).optional(),
      userAgent: Joi.string().max(500).optional()
    }).optional()
  }),
  
  // Batch events
  batchEvents: Joi.object({
    events: Joi.array().items(Joi.object({
      eventId: commonSchemas.uuid.optional(),
      playerId: commonSchemas.uuid,
      eventType: Joi.string().valid(
        'level_start', 'level_complete', 'level_fail',
        'item_collected', 'skill_used', 'enemy_defeated',
        'achievement_unlocked', 'purchase_made', 'login', 'logout'
      ).required(),
      eventData: Joi.object().optional(),
      timestamp: Joi.date().iso().default(() => new Date()),
      sessionId: commonSchemas.uuid.optional()
    })).min(1).max(100).required()
  }),
  
  // Event query parameters
  eventQuery: Joi.object({
    playerId: commonSchemas.uuid.optional(),
    eventType: Joi.string().optional(),
    sessionId: commonSchemas.uuid.optional(),
    ...commonSchemas.dateRange,
    ...commonSchemas.pagination
  })
};

// Analytics validation schemas
export const analyticsSchemas = {
  // Analytics query parameters
  analyticsQuery: Joi.object({
    playerId: commonSchemas.uuid.optional(),
    metric: Joi.string().valid(
      'player_retention', 'level_completion', 'item_usage',
      'revenue', 'engagement', 'progression'
    ).optional(),
    granularity: Joi.string().valid('hour', 'day', 'week', 'month').default('day'),
    ...commonSchemas.dateRange,
    groupBy: Joi.array().items(Joi.string().valid(
      'playerId', 'eventType', 'level', 'platform', 'date'
    )).optional()
  }),
  
  // Summary query parameters
  summaryQuery: Joi.object({
    playerId: commonSchemas.uuid.optional(),
    ...commonSchemas.dateRange
  })
};

// Authentication validation schemas
export const authSchemas = {
  // User registration
  register: Joi.object({
    username: Joi.string().trim().min(3).max(30).pattern(/^[a-zA-Z0-9_-]+$/).required(),
    email: Joi.string().email().required(),
    password: Joi.string().min(8).max(128).pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/).required()
  }),
  
  // User login
  login: Joi.object({
    username: Joi.string().trim().required(),
    password: Joi.string().required()
  })
};

// Generic validation middleware factory
export const validate = (schema: Joi.ObjectSchema, source: 'body' | 'query' | 'params' = 'body') => {
  return (req: Request, res: Response, next: NextFunction) => {
    const data = source === 'body' ? req.body : source === 'query' ? req.query : req.params;
    
    const { error, value } = schema.validate(data, {
      abortEarly: false,
      stripUnknown: true,
      convert: true
    });
    
    if (error) {
      const validationErrors = error.details.map(detail => ({
        field: detail.path.join('.'),
        message: detail.message,
        value: detail.context?.value
      }));
      
      logger.warn('Validation failed', {
        source,
        errors: validationErrors,
        originalData: data,
        requestId: req.headers['x-request-id']
      });
      
      throw new ValidationError('Validation failed', validationErrors);
    }
    
    // Replace the original data with validated and sanitized data
    if (source === 'body') {
      req.body = value;
    } else if (source === 'query') {
      req.query = value;
    } else {
      req.params = value;
    }
    
    next();
  };
};

// Specific validation middleware for common use cases
export const validatePlayerData = validate(playerSchemas.playerData, 'body');
export const validatePlayerQuery = validate(playerSchemas.playerQuery, 'query');
export const validateGameEvent = validate(eventSchemas.gameEvent, 'body');
export const validateBatchEvents = validate(eventSchemas.batchEvents, 'body');
export const validateEventQuery = validate(eventSchemas.eventQuery, 'query');
export const validateAnalyticsQuery = validate(analyticsSchemas.analyticsQuery, 'query');
export const validateSummaryQuery = validate(analyticsSchemas.summaryQuery, 'query');
export const validateRegister = validate(authSchemas.register, 'body');
export const validateLogin = validate(authSchemas.login, 'body');
export const validateUuidParam = validate(Joi.object({ id: commonSchemas.uuid }), 'params');

// Input sanitization middleware
export const sanitizeInput = (req: Request, res: Response, next: NextFunction) => {
  // Sanitize string inputs to prevent XSS
  const sanitizeObject = (obj: any): any => {
    if (typeof obj === 'string') {
      return obj
        .trim()
        .replace(/[<>"'&]/g, (match) => {
          const entities: { [key: string]: string } = {
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#x27;',
            '&': '&amp;'
          };
          return entities[match] || match;
        });
    }
    
    if (Array.isArray(obj)) {
      return obj.map(sanitizeObject);
    }
    
    if (obj && typeof obj === 'object') {
      const sanitized: any = {};
      for (const [key, value] of Object.entries(obj)) {
        sanitized[key] = sanitizeObject(value);
      }
      return sanitized;
    }
    
    return obj;
  };
  
  // Sanitize request body
  if (req.body) {
    req.body = sanitizeObject(req.body);
  }
  
  // Sanitize query parameters
  if (req.query) {
    req.query = sanitizeObject(req.query);
  }
  
  next();
};

// Rate limiting validation
export const validateRateLimit = (req: Request, res: Response, next: NextFunction) => {
  const rateLimitInfo = {
    ip: req.ip,
    userAgent: req.get('User-Agent'),
    endpoint: req.originalUrl,
    method: req.method,
    timestamp: new Date().toISOString()
  };
  
  logger.debug('Rate limit check', rateLimitInfo);
  next();
};

// Content type validation middleware
export const validateContentType = (expectedTypes: string[] = ['application/json']) => {
  return (req: Request, res: Response, next: NextFunction) => {
    const contentType = req.get('Content-Type');
    
    if (req.method === 'GET' || req.method === 'DELETE') {
      return next();
    }
    
    if (!contentType || !expectedTypes.some(type => contentType.includes(type))) {
      logger.warn('Invalid content type', {
        expected: expectedTypes,
        received: contentType,
        method: req.method,
        path: req.path
      });
      
      return res.status(415).json({
        error: 'Unsupported Media Type',
        message: `Expected content type: ${expectedTypes.join(' or ')}`
      });
    }
    
    next();
  };
};

// Alias for sanitizeInput to match import expectations
export const sanitizeRequest = sanitizeInput;

export default {
  validate,
  sanitizeInput,
  validateRateLimit,
  // Export all specific validators
  validatePlayerData,
  validatePlayerQuery,
  validateGameEvent,
  validateBatchEvents,
  validateEventQuery,
  validateAnalyticsQuery,
  validateSummaryQuery,
  validateRegister,
  validateLogin,
  validateUuidParam
};