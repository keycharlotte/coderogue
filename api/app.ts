import express from 'express';
import cors from 'cors';
import compression from 'compression';
import path from 'path';
import { initializeDatabase } from './config/database';
import eventsRoutes from './routes/events';
import analyticsRoutes from './routes/analytics';
import authRoutes from './routes/auth';
import healthRoutes from './routes/health';
import {
  generalRateLimit,
  authRateLimit,
  analyticsRateLimit,
  eventsRateLimit,
  securityHeaders,
  corsOptions,
  securityLogger,
  requestSizeLimit,
  validateUserAgent
} from './middleware/security';
import { requestLogger } from './utils/logger';
import { errorHandler } from './middleware/errorHandler';

async function createApp() {
  const app = express();

  // Initialize database
  await initializeDatabase();

  // Trust proxy for accurate IP addresses
  app.set('trust proxy', 1);

  // Security middleware (applied first)
  app.use(securityHeaders);
  app.use(cors(corsOptions));
  app.use(compression());
  app.use(validateUserAgent);
  app.use(securityLogger);
  app.use(requestSizeLimit('50mb'));

  // General rate limiting
  app.use(generalRateLimit);

  // Body parsing middleware
  app.use(express.json({ limit: '50mb' }));
  app.use(express.urlencoded({ extended: true, limit: '50mb' }));

  // Request logging middleware
  app.use(requestLogger);

  // Serve static files from the frontend build
  app.use(express.static(path.join(process.cwd(), 'dist')));

  // Routes with specific rate limiting
  app.use('/api/v1/auth', authRateLimit, authRoutes);
  app.use('/api/v1/events', eventsRateLimit, eventsRoutes);
  app.use('/api/v1/analytics', analyticsRateLimit, analyticsRoutes);
  app.use('/api/health', healthRoutes);

  // Basic health check (legacy endpoint)
  app.get('/health', (req, res) => {
    res.json({ 
      status: 'OK', 
      timestamp: new Date().toISOString(),
      version: '1.0.0'
    });
  });

  // API documentation endpoint
  app.get('/api/docs', (req, res) => {
    res.json({
      title: 'CodeRogue Player Data Analytics API',
      version: '1.0.0',
      description: 'API for collecting and analyzing player data from CodeRogue game',
      endpoints: {
        events: {
          'POST /api/v1/events': 'Report single game event',
          'POST /api/v1/events/batch': 'Report multiple game events',
          'GET /api/v1/events/session/:sessionId': 'Get events for a session'
        },
        analytics: {
          'GET /api/v1/analytics/cards': 'Get card statistics and analysis',
          'GET /api/v1/analytics/cards/:cardId': 'Get specific card analysis',
          'GET /api/v1/analytics/players/behavior': 'Get player behavior analysis',
          'GET /api/v1/analytics/players/:playerId': 'Get specific player analysis',
          'GET /api/v1/analytics/balance': 'Get game balance analysis',
          'GET /api/v1/analytics/summary': 'Get overall analytics summary'
        },
        system: {
          'GET /health': 'System health check',
          'GET /api/docs': 'API documentation'
        }
      },
      timestamp: new Date().toISOString()
    });
  });

  // Catch-all handler for frontend routes (SPA support)
  app.get('*', (req, res, next) => {
    // If the request is for an API route, continue to 404 handler
    if (req.originalUrl.startsWith('/api/')) {
      return next();
    }
    // For all other routes, serve the frontend app
    res.sendFile('index.html', { root: path.join(process.cwd(), 'dist') });
  });

  // 404 handler for API routes only
  app.use('/api/*', (req, res) => {
    res.status(404).json({
      error: 'Not Found',
      message: `Route ${req.method} ${req.originalUrl} not found`,
      statusCode: 404,
      timestamp: new Date().toISOString()
    });
  });

  // Global error handler
  app.use(errorHandler);

  return app;
}

export default createApp;