// CodeRogue Player Data Analytics - Database Configuration
// This file handles PostgreSQL database connection and configuration

// Load environment variables first
import dotenv from 'dotenv';
dotenv.config();

import { Pool, PoolClient, QueryResult } from 'pg';
import { logger } from '../utils/logger.js';

// Database configuration interface
interface DatabaseConfig {
  host: string;
  port: number;
  database: string;
  user: string;
  password: string;
  ssl: boolean;
  max: number;
  idleTimeoutMillis: number;
  connectionTimeoutMillis: number;
  acquireTimeoutMillis: number;
  createTimeoutMillis: number;
  destroyTimeoutMillis: number;
  reapIntervalMillis: number;
  createRetryIntervalMillis: number;
  propagateCreateError: boolean;
}

// Retry configuration
interface RetryConfig {
  maxRetries: number;
  initialDelay: number;
  maxDelay: number;
  backoffMultiplier: number;
  retryableErrors: string[];
}

// Database health status
interface DatabaseHealth {
  isHealthy: boolean;
  lastCheck: Date;
  consecutiveFailures: number;
  totalChecks: number;
  averageResponseTime: number;
  lastError?: string;
}

// Server configuration
export interface ServerConfig {
  port: number;
  cors: {
    origin: string[];
    credentials: boolean;
  };
  logLevel: 'debug' | 'info' | 'warn' | 'error';
  analytics: {
    batchSize: number;
    flushInterval: number;
    retentionDays: number;
  };
}

// Development mode flag
const isDevelopmentMode = process.env.NODE_ENV === 'development' && process.env.DB_SKIP === 'true';

// Debug logging for development mode
console.log('Development mode check:', {
  NODE_ENV: process.env.NODE_ENV,
  DB_SKIP: process.env.DB_SKIP,
  isDevelopmentMode
});

// Database configuration from environment variables
const dbConfig: DatabaseConfig = {
  host: process.env.DB_HOST || 'localhost',
  port: parseInt(process.env.DB_PORT || '5432'),
  database: process.env.DB_NAME || 'coderogue_analytics',
  user: process.env.DB_USER || 'postgres',
  password: process.env.DB_PASSWORD || '',
  ssl: process.env.DB_SSL === 'true',
  max: parseInt(process.env.DB_MAX_CONNECTIONS || '20'),
  idleTimeoutMillis: parseInt(process.env.DB_IDLE_TIMEOUT || '30000'),
  connectionTimeoutMillis: parseInt(process.env.DB_CONNECTION_TIMEOUT || '10000'),
  acquireTimeoutMillis: parseInt(process.env.DB_ACQUIRE_TIMEOUT || '60000'),
  createTimeoutMillis: parseInt(process.env.DB_CREATE_TIMEOUT || '30000'),
  destroyTimeoutMillis: parseInt(process.env.DB_DESTROY_TIMEOUT || '5000'),
  reapIntervalMillis: parseInt(process.env.DB_REAP_INTERVAL || '1000'),
  createRetryIntervalMillis: parseInt(process.env.DB_CREATE_RETRY_INTERVAL || '200'),
  propagateCreateError: process.env.DB_PROPAGATE_CREATE_ERROR !== 'false'
};

// Retry configuration
const retryConfig: RetryConfig = {
  maxRetries: parseInt(process.env.DB_MAX_RETRIES || '3'),
  initialDelay: parseInt(process.env.DB_INITIAL_RETRY_DELAY || '1000'),
  maxDelay: parseInt(process.env.DB_MAX_RETRY_DELAY || '10000'),
  backoffMultiplier: parseFloat(process.env.DB_BACKOFF_MULTIPLIER || '2'),
  retryableErrors: [
    'ECONNRESET',
    'ECONNREFUSED',
    'ETIMEDOUT',
    'ENOTFOUND',
    'ENETUNREACH',
    'connection terminated unexpectedly',
    'server closed the connection unexpectedly',
    'Connection terminated',
    'Client has encountered a connection error'
  ]
};

// Database health tracking
const databaseHealth: DatabaseHealth = {
  isHealthy: true,
  lastCheck: new Date(),
  consecutiveFailures: 0,
  totalChecks: 0,
  averageResponseTime: 0
};

let healthCheckInterval: NodeJS.Timeout | null = null;

// Retry function with exponential backoff
async function withRetry<T>(
  operation: () => Promise<T>,
  operationName: string
): Promise<T> {
  let lastError: Error;
  
  for (let attempt = 1; attempt <= retryConfig.maxRetries; attempt++) {
    try {
      return await operation();
    } catch (error) {
      lastError = error as Error;
      
      // Check if error is retryable
      const isRetryable = retryConfig.retryableErrors.some(retryableError => 
        lastError.message.includes(retryableError)
      );
      
      if (!isRetryable || attempt === retryConfig.maxRetries) {
        logger.error(`${operationName} failed after ${attempt} attempts`, {
          error: lastError.message,
          attempt,
          maxRetries: retryConfig.maxRetries
        });
        throw lastError;
      }
      
      const delay = Math.min(
        retryConfig.initialDelay * Math.pow(retryConfig.backoffMultiplier, attempt - 1),
        retryConfig.maxDelay
      );
      
      logger.warn(`${operationName} failed, retrying in ${delay}ms`, {
        error: lastError.message,
        attempt,
        maxRetries: retryConfig.maxRetries,
        delay
      });
      
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }
  
  throw lastError!;
}

// Start health check monitoring
function startHealthChecks(): void {
  if (healthCheckInterval) {
    clearInterval(healthCheckInterval);
  }
  
  healthCheckInterval = setInterval(async () => {
    const startTime = Date.now();
    databaseHealth.totalChecks++;
    
    try {
      if (pool) {
        const client = await pool.connect();
        try {
          await client.query('SELECT 1');
          client.release();
          
          const responseTime = Date.now() - startTime;
          databaseHealth.averageResponseTime = 
            (databaseHealth.averageResponseTime * (databaseHealth.totalChecks - 1) + responseTime) / databaseHealth.totalChecks;
          
          if (!databaseHealth.isHealthy) {
            logger.info('Database health restored');
          }
          
          databaseHealth.isHealthy = true;
          databaseHealth.consecutiveFailures = 0;
          databaseHealth.lastCheck = new Date();
          delete databaseHealth.lastError;
        } catch (error) {
          client.release();
          throw error;
        }
      } else {
        throw new Error('Database pool not initialized');
      }
    } catch (error) {
      databaseHealth.isHealthy = false;
      databaseHealth.consecutiveFailures++;
      databaseHealth.lastCheck = new Date();
      databaseHealth.lastError = error instanceof Error ? error.message : String(error);
      
      logger.error('Database health check failed', {
        error: databaseHealth.lastError,
        consecutiveFailures: databaseHealth.consecutiveFailures
      });
    }
  }, 30000); // Check every 30 seconds
}

// Environment configuration
const config = {
  database: dbConfig,
  
  server: {
    port: parseInt(process.env.PORT || '3001'),
    cors: {
      origin: process.env.CORS_ORIGIN?.split(',') || ['http://localhost:3000'],
      credentials: true
    },
    logLevel: (process.env.LOG_LEVEL as 'error' | 'warn' | 'info' | 'debug' | 'verbose') || 'info',
    analytics: {
      batchSize: parseInt(process.env.ANALYTICS_BATCH_SIZE || '100'),
      flushInterval: parseInt(process.env.ANALYTICS_FLUSH_INTERVAL || '5000'),
      retentionDays: parseInt(process.env.ANALYTICS_RETENTION_DAYS || '90')
    }
  } as ServerConfig
};

// Database connection pool
let pool: Pool | null = null;







// Initialize database connection
export async function initializeDatabase(): Promise<Pool> {
  if (pool) {
    return pool;
  }

  // Skip database initialization in development mode
  if (isDevelopmentMode) {
    logger.info('Skipping database initialization in development mode');
    // Create a mock pool for development
    pool = {} as Pool;
    return pool;
  }

  return await withRetry(
    async () => {
      logger.info('Initializing database connection pool', {
        host: dbConfig.host,
        port: dbConfig.port,
        database: dbConfig.database,
        maxConnections: dbConfig.max
      });
      
      pool = new Pool({
        host: dbConfig.host,
        port: dbConfig.port,
        database: dbConfig.database,
        user: dbConfig.user,
        password: dbConfig.password,
        ssl: dbConfig.ssl ? { rejectUnauthorized: false } : false,
        max: dbConfig.max,
        idleTimeoutMillis: dbConfig.idleTimeoutMillis,
        connectionTimeoutMillis: dbConfig.connectionTimeoutMillis
      });
      
      // Set up pool event handlers
      pool.on('connect', () => {
        logger.debug('New database client connected', {
          totalCount: pool!.totalCount,
          idleCount: pool!.idleCount
        });
      });
      
      pool.on('remove', () => {
        logger.debug('Database client removed', {
          totalCount: pool!.totalCount,
          idleCount: pool!.idleCount
        });
      });
      
      pool.on('error', (error) => {
        logger.error('Database pool error', {
          error: error.message,
          stack: error.stack
        });
        
        databaseHealth.isHealthy = false;
        databaseHealth.lastError = error.message;
      });
      
      // Test the connection
      const client = await pool.connect();
      try {
        const result = await client.query('SELECT NOW() as current_time, version() as pg_version');
        logger.info('Database connection test successful', {
          currentTime: result.rows[0].current_time,
          postgresVersion: result.rows[0].pg_version.split(' ')[0]
        });
      } finally {
        client.release();
      }
      
      // Start health monitoring
      startHealthChecks();
      
      logger.info('Database initialized successfully', {
        host: dbConfig.host,
        database: dbConfig.database,
        maxConnections: dbConfig.max
      });
      
      return pool;
    },
    'Database initialization'
  );
}

// Get database connection pool
export function getPool(): Pool {
  if (!pool) {
    throw new Error('Database not initialized. Call initializeDatabase() first.');
  }
  
  // In development mode, return mock pool
  if (isDevelopmentMode) {
    return pool;
  }
  
  return pool;
}

// Health check
export async function healthCheck(): Promise<boolean> {
  try {
    if (!pool) {
      return false;
    }
    
    const client = await pool.connect();
    await client.query('SELECT 1');
    client.release();
    return true;
  } catch (error) {
    console.error('Database health check failed:', error);
    return false;
  }
}

// Close database connection
export async function closeDatabase(): Promise<void> {
  if (pool) {
    await pool.end();
    pool = null;
    console.log('Database connection closed');
  }
}

// Query helper function
export async function query(text: string, params?: (string | number | boolean | Date | null)[]): Promise<QueryResult> {
  const client = await getPool().connect();
  try {
    const result = await client.query(text, params);
    return result;
  } finally {
    client.release();
  }
}

// Transaction helper function
export async function transaction<T>(callback: (client: PoolClient) => Promise<T>): Promise<T> {
  const client = await getPool().connect();
  try {
    await client.query('BEGIN');
    const result = await callback(client);
    await client.query('COMMIT');
    return result;
  } catch (error) {
    await client.query('ROLLBACK');
    throw error;
  } finally {
    client.release();
  }
}

// Batch query execution with retry
export async function batchQuery(queries: Array<{ text: string; params?: (string | number | boolean | Date | null)[] }>): Promise<QueryResult[]> {
  return await withRetry(
    async () => {
      const startTime = Date.now();
      const client = await getPool().connect();
      
      try {
        const results: QueryResult[] = [];
        
        logger.debug('Starting batch query execution', {
          queryCount: queries.length
        });
        
        for (let i = 0; i < queries.length; i++) {
          const query = queries[i];
          const result = await client.query(query.text, query.params);
          results.push(result);
          
          logger.debug(`Batch query ${i + 1}/${queries.length} completed`, {
            rowCount: result.rowCount
          });
        }
        
        const duration = Date.now() - startTime;
        logger.info('Batch query execution completed', {
          queryCount: queries.length,
          totalDuration: duration,
          averageDuration: duration / queries.length
        });
        
        return results;
      } finally {
        client.release();
      }
    },
    'Batch query execution'
  );
}

// Check if table exists with retry
export async function tableExists(tableName: string): Promise<boolean> {
  try {
    const result = await query(
      'SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = $1 AND table_name = $2)',
      ['public', tableName]
    );
    
    const exists = result.rows[0].exists;
    
    logger.debug('Table existence check', {
      tableName,
      exists
    });
    
    return exists;
  } catch (error) {
    logger.error('Error checking table existence', {
      tableName,
      error: error instanceof Error ? error.message : String(error)
    });
    return false;
  }
}

// Run migration file with retry
export async function runMigration(migrationSQL: string): Promise<void> {
  return await withRetry(
    async () => {
      try {
        logger.info('Running migration', {
          sqlLength: migrationSQL.length
        });
        
        await query(migrationSQL);
        
        logger.info('Migration executed successfully');
      } catch (error) {
        logger.error('Migration failed', {
          error: error instanceof Error ? error.message : String(error)
        });
        throw error;
      }
    },
    'Migration execution'
  );
}

export { config };
export default config;