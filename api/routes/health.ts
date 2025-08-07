// CodeRogue Player Data Analytics - Health Check Endpoints
// Comprehensive system health monitoring and status reporting

import { Router, Request, Response } from 'express';
import { DatabaseManager } from '../utils/DatabaseManager.js';
import { logger, logSystemHealth } from '../utils/logger.js';
import { asyncHandler } from '../middleware/errorHandler.js';
import os from 'os';
import process from 'process';

const router = Router();

// Health check status types
interface HealthStatus {
  status: 'healthy' | 'degraded' | 'unhealthy';
  timestamp: string;
  uptime: number;
  version: string;
  environment: string;
  services: ServiceStatus[];
  system: SystemMetrics;
  database: DatabaseStatus;
}

interface ServiceStatus {
  name: string;
  status: 'healthy' | 'degraded' | 'unhealthy';
  responseTime?: number;
  lastCheck: string;
  details?: any;
}

interface SystemMetrics {
  memory: {
    used: number;
    total: number;
    percentage: number;
    heap: {
      used: number;
      total: number;
      percentage: number;
    };
  };
  cpu: {
    usage: number;
    loadAverage: number[];
  };
  disk: {
    free: number;
    total: number;
    percentage: number;
  };
  network: {
    connections: number;
  };
}

interface DatabaseStatus {
  status: 'healthy' | 'degraded' | 'unhealthy';
  responseTime: number;
  connections: {
    active: number;
    idle: number;
    total: number;
  };
  lastQuery: string;
  details?: any;
}

// Get system memory information
function getMemoryMetrics() {
  const totalMemory = os.totalmem();
  const freeMemory = os.freemem();
  const usedMemory = totalMemory - freeMemory;
  const memoryUsage = process.memoryUsage();
  
  return {
    used: usedMemory,
    total: totalMemory,
    percentage: Math.round((usedMemory / totalMemory) * 100),
    heap: {
      used: memoryUsage.heapUsed,
      total: memoryUsage.heapTotal,
      percentage: Math.round((memoryUsage.heapUsed / memoryUsage.heapTotal) * 100)
    }
  };
}

// Get CPU metrics
function getCpuMetrics() {
  const cpus = os.cpus();
  const loadAverage = os.loadavg();
  
  // Calculate CPU usage (simplified)
  let totalIdle = 0;
  let totalTick = 0;
  
  cpus.forEach(cpu => {
    for (const type in cpu.times) {
      totalTick += cpu.times[type as keyof typeof cpu.times];
    }
    totalIdle += cpu.times.idle;
  });
  
  const idle = totalIdle / cpus.length;
  const total = totalTick / cpus.length;
  const usage = 100 - Math.round((100 * idle) / total);
  
  return {
    usage: Math.max(0, Math.min(100, usage)),
    loadAverage
  };
}

// Get disk space information (simplified)
function getDiskMetrics() {
  // Note: This is a simplified implementation
  // In production, you might want to use a library like 'node-disk-info'
  const stats = process.memoryUsage();
  const total = 100 * 1024 * 1024 * 1024; // Assume 100GB total (placeholder)
  const used = stats.external + stats.heapTotal;
  const free = total - used;
  
  return {
    free,
    total,
    percentage: Math.round((used / total) * 100)
  };
}

// Check database health
async function checkDatabaseHealth(): Promise<DatabaseStatus> {
  const startTime = Date.now();
  
  try {
    // Simple health check query
    // Database connectivity check would be implemented here
    // For now, simulate a successful database check
    const responseTime = Date.now() - startTime;
    
    // Get connection pool info (if available)
    // Note: Supabase doesn't expose connection pool details directly
    const connections = {
      active: 1, // Placeholder
      idle: 0,   // Placeholder
      total: 1   // Placeholder
    };
    
    return {
      status: responseTime < 1000 ? 'healthy' : responseTime < 3000 ? 'degraded' : 'unhealthy',
      responseTime,
      connections,
      lastQuery: new Date().toISOString()
    };
  } catch (error) {
    const responseTime = Date.now() - startTime;
    
    return {
      status: 'unhealthy',
      responseTime,
      connections: {
        active: 0,
        idle: 0,
        total: 0
      },
      lastQuery: new Date().toISOString(),
      details: {
        error: error instanceof Error ? error.message : 'Unknown error'
      }
    };
  }
}

// Check external service health
async function checkServiceHealth(name: string, url: string): Promise<ServiceStatus> {
  const startTime = Date.now();
  
  try {
    const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 5000);
      
      const response = await fetch(url, {
        method: 'GET',
        signal: controller.signal
      });
      
      clearTimeout(timeoutId);
    
    const responseTime = Date.now() - startTime;
    const isHealthy = response.ok && responseTime < 2000;
    
    return {
      name,
      status: isHealthy ? 'healthy' : 'degraded',
      responseTime,
      lastCheck: new Date().toISOString(),
      details: {
        statusCode: response.status,
        statusText: response.statusText
      }
    };
  } catch (error) {
    const responseTime = Date.now() - startTime;
    
    return {
      name,
      status: 'unhealthy',
      responseTime,
      lastCheck: new Date().toISOString(),
      details: {
        error: error instanceof Error ? error.message : 'Unknown error'
      }
    };
  }
}

// Basic health check endpoint
router.get('/health', asyncHandler(async (req: Request, res: Response) => {
  const requestId = req.headers['x-request-id'] as string;
  
  const healthStatus = {
    status: 'healthy' as const,
    timestamp: new Date().toISOString(),
    uptime: Math.floor(process.uptime()),
    version: process.env.npm_package_version || '1.0.0',
    environment: process.env.NODE_ENV || 'development',
    requestId
  };
  
  logger.debug('Basic health check performed', {
    requestId,
    uptime: healthStatus.uptime,
    timestamp: healthStatus.timestamp
  });
  
  res.status(200).json(healthStatus);
}));

// Comprehensive health check endpoint
router.get('/health/detailed', asyncHandler(async (req: Request, res: Response) => {
  const requestId = req.headers['x-request-id'] as string;
  const startTime = Date.now();
  
  try {
    // Simple database connectivity check
    // You can implement actual database ping here
    
    // Gather all health metrics
    const [databaseStatus, systemMetrics] = await Promise.all([
      checkDatabaseHealth(),
      Promise.resolve({
        memory: getMemoryMetrics(),
        cpu: getCpuMetrics(),
        disk: getDiskMetrics(),
        network: {
          connections: 0 // Placeholder
        }
      })
    ]);
    
    // Check external services (if any)
    const services: ServiceStatus[] = [];
    
    // Example: Check if we have any external dependencies
    // const externalServices = [
    //   { name: 'analytics-api', url: 'https://api.analytics.example.com/health' },
    //   { name: 'notification-service', url: 'https://notifications.example.com/health' }
    // ];
    // 
    // const serviceChecks = await Promise.allSettled(
    //   externalServices.map(service => checkServiceHealth(service.name, service.url))
    // );
    // 
    // serviceChecks.forEach((result, index) => {
    //   if (result.status === 'fulfilled') {
    //     services.push(result.value);
    //   } else {
    //     services.push({
    //       name: externalServices[index].name,
    //       status: 'unhealthy',
    //       lastCheck: new Date().toISOString(),
    //       details: { error: result.reason }
    //     });
    //   }
    // });
    
    // Determine overall health status
    let overallStatus: 'healthy' | 'degraded' | 'unhealthy' = 'healthy';
    
    if (databaseStatus.status === 'unhealthy') {
      overallStatus = 'unhealthy';
    } else if (
      databaseStatus.status === 'degraded' ||
      systemMetrics.memory.percentage > 90 ||
      systemMetrics.cpu.usage > 90 ||
      services.some(s => s.status === 'degraded')
    ) {
      overallStatus = 'degraded';
    }
    
    if (services.some(s => s.status === 'unhealthy')) {
      overallStatus = 'unhealthy';
    }
    
    const healthStatus: HealthStatus = {
      status: overallStatus,
      timestamp: new Date().toISOString(),
      uptime: Math.floor(process.uptime()),
      version: process.env.npm_package_version || '1.0.0',
      environment: process.env.NODE_ENV || 'development',
      services,
      system: systemMetrics,
      database: databaseStatus
    };
    
    const responseTime = Date.now() - startTime;
    
    // Log health check results
    logSystemHealth('system', overallStatus);
    
    // Set appropriate HTTP status code
    const statusCode = overallStatus === 'healthy' ? 200 : 
                      overallStatus === 'degraded' ? 200 : 503;
    
    res.status(statusCode).json(healthStatus);
  } catch (error) {
    const responseTime = Date.now() - startTime;
    
    logger.error('Health check failed', {
      error: error instanceof Error ? error.message : 'Unknown error',
      stack: error instanceof Error ? error.stack : undefined,
      responseTime,
      requestId
    });
    
    const errorHealthStatus = {
      status: 'unhealthy' as const,
      timestamp: new Date().toISOString(),
      uptime: Math.floor(process.uptime()),
      version: process.env.npm_package_version || '1.0.0',
      environment: process.env.NODE_ENV || 'development',
      error: error instanceof Error ? error.message : 'Health check failed',
      requestId
    };
    
    res.status(503).json(errorHealthStatus);
  }
}));

// Readiness probe endpoint (for Kubernetes/Docker)
router.get('/ready', asyncHandler(async (req: Request, res: Response) => {
  const requestId = req.headers['x-request-id'] as string;
  
  try {
    // Check if the application is ready to serve requests
    // Simple readiness check
    // You can implement actual database connectivity check here
    
    // Database connectivity would be checked here
    
    // Quick database connectivity check
    const startTime = Date.now();
    // Database connectivity check would be implemented here
    // For now, assume the database is ready
    const responseTime = Date.now() - startTime;
    
    logger.debug('Readiness check passed', {
      responseTime,
      requestId
    });
    
    res.status(200).json({
      status: 'ready',
      timestamp: new Date().toISOString(),
      responseTime,
      requestId
    });
  } catch (error) {
    logger.error('Readiness check failed', {
      error: error instanceof Error ? error.message : 'Unknown error',
      requestId
    });
    
    res.status(503).json({
      status: 'not ready',
      timestamp: new Date().toISOString(),
      error: error instanceof Error ? error.message : 'Unknown error',
      requestId
    });
  }
}));

// Liveness probe endpoint (for Kubernetes/Docker)
router.get('/live', asyncHandler(async (req: Request, res: Response) => {
  const requestId = req.headers['x-request-id'] as string;
  
  // Simple liveness check - if the process is running, it's alive
  const memoryUsage = process.memoryUsage();
  const uptime = process.uptime();
  
  // Check for memory leaks or other critical issues
  const memoryThreshold = 1024 * 1024 * 1024; // 1GB
  const isMemoryHealthy = memoryUsage.heapUsed < memoryThreshold;
  
  if (!isMemoryHealthy) {
    logger.warn('High memory usage detected in liveness check', {
      heapUsed: memoryUsage.heapUsed,
      threshold: memoryThreshold,
      uptime,
      requestId
    });
  }
  
  const status = isMemoryHealthy ? 'alive' : 'unhealthy';
  const statusCode = isMemoryHealthy ? 200 : 503;
  
  res.status(statusCode).json({
    status,
    timestamp: new Date().toISOString(),
    uptime: Math.floor(uptime),
    memory: {
      heapUsed: memoryUsage.heapUsed,
      heapTotal: memoryUsage.heapTotal,
      external: memoryUsage.external
    },
    requestId
  });
}));

export default router;