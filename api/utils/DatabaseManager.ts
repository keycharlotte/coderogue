// CodeRogue Player Data Analytics - Enhanced Database Manager
// Advanced database connection management with error handling, retry mechanisms, and monitoring

import { Pool, PoolConfig, PoolClient, QueryResult } from 'pg';
import { logger, logDatabaseQuery } from './logger';
import { DatabaseError } from '../middleware/errors';
import { EventEmitter } from 'events';

// Database configuration interface
export interface DatabaseConfig {
  host: string;
  port: number;
  database: string;
  username: string;
  password: string;
  ssl?: boolean;
  maxConnections?: number;
  idleTimeoutMillis?: number;
  connectionTimeoutMillis?: number;

  createTimeoutMillis?: number;
  destroyTimeoutMillis?: number;
  reapIntervalMillis?: number;
  createRetryIntervalMillis?: number;
}

// Retry configuration
export interface RetryConfig {
  maxRetries: number;
  initialDelay: number;
  maxDelay: number;
  backoffMultiplier: number;
  retryableErrors: string[];
}

// Connection pool statistics
export interface PoolStats {
  totalConnections: number;
  idleConnections: number;
  activeConnections: number;
  waitingClients: number;
  totalQueries: number;
  successfulQueries: number;
  failedQueries: number;
  averageQueryTime: number;
  connectionErrors: number;
  lastError?: string;
  lastErrorTime?: Date;
}

// Query options
export interface QueryOptions {
  timeout?: number;
  retries?: number;
  logQuery?: boolean;
  transactionId?: string;
}

// Transaction context
export interface TransactionContext {
  id: string;
  client: PoolClient;
  startTime: Date;
  queries: number;
}

class DatabaseManager extends EventEmitter {
  private pool: Pool | null = null;
  private config: DatabaseConfig;
  private retryConfig: RetryConfig;
  private stats: PoolStats;
  private isInitialized: boolean = false;
  private reconnectAttempts: number = 0;
  private maxReconnectAttempts: number = 10;
  private reconnectInterval: number = 5000; // 5 seconds
  private healthCheckInterval: NodeJS.Timeout | null = null;
  private queryTimes: number[] = [];
  private maxQueryTimeHistory: number = 1000;

  constructor(config: DatabaseConfig, retryConfig?: Partial<RetryConfig>) {
    super();
    
    this.config = config;
    this.retryConfig = {
      maxRetries: 3,
      initialDelay: 1000,
      maxDelay: 10000,
      backoffMultiplier: 2,
      retryableErrors: [
        'ECONNRESET',
        'ECONNREFUSED',
        'ETIMEDOUT',
        'ENOTFOUND',
        'connection terminated unexpectedly',
        'server closed the connection unexpectedly',
        'Connection terminated',
        'Client has encountered a connection error'
      ],
      ...retryConfig
    };
    
    this.stats = {
      totalConnections: 0,
      idleConnections: 0,
      activeConnections: 0,
      waitingClients: 0,
      totalQueries: 0,
      successfulQueries: 0,
      failedQueries: 0,
      averageQueryTime: 0,
      connectionErrors: 0
    };
  }

  // Initialize database connection with retry logic
  async initialize(): Promise<void> {
    if (this.isInitialized) {
      return;
    }

    try {
      await this.createPool();
      await this.testConnection();
      this.setupEventHandlers();
      this.startHealthCheck();
      
      this.isInitialized = true;
      this.reconnectAttempts = 0;
      
      logger.info('Database manager initialized successfully', {
        host: this.config.host,
        port: this.config.port,
        database: this.config.database,
        maxConnections: this.config.maxConnections
      });
      
      this.emit('connected');
    } catch (error) {
      logger.error('Failed to initialize database manager', {
        error: error instanceof Error ? error.message : 'Unknown error',
        stack: error instanceof Error ? error.stack : undefined,
        config: {
          host: this.config.host,
          port: this.config.port,
          database: this.config.database
        }
      });
      
      throw new DatabaseError(`Database initialization failed: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  }

  // Create connection pool
  private async createPool(): Promise<void> {
    const poolConfig: PoolConfig = {
      host: this.config.host,
      port: this.config.port,
      database: this.config.database,
      user: this.config.username,
      password: this.config.password,
      ssl: this.config.ssl ? { rejectUnauthorized: false } : false,
      max: this.config.maxConnections || 20,
      min: 2, // Minimum connections
      idleTimeoutMillis: this.config.idleTimeoutMillis || 30000,
      connectionTimeoutMillis: this.config.connectionTimeoutMillis || 5000,

      // // createTimeoutMillis: this.config.createTimeoutMillis || 5000, // Removed - not supported in pg PoolConfig // Property not available in PoolConfig
      // destroyTimeoutMillis: this.config.destroyTimeoutMillis || 5000, // Property not available in PoolConfig
      // // reapIntervalMillis: this.config.reapIntervalMillis || 1000, // Property not available in PoolConfig // Property not available in PoolConfig
      // createRetryIntervalMillis: this.config.createRetryIntervalMillis || 200 // Property not available in PoolConfig
    };

    this.pool = new Pool(poolConfig);
  }

  // Setup event handlers for the pool
  private setupEventHandlers(): void {
    if (!this.pool) return;

    this.pool.on('error', (error) => {
      this.stats.connectionErrors++;
      this.stats.lastError = error.message;
      this.stats.lastErrorTime = new Date();
      
      logger.error('Database pool error', {
        error: error.message,
        stack: error.stack,
        timestamp: new Date().toISOString()
      });
      
      this.emit('error', error);
      this.handleConnectionError(error);
    });

    this.pool.on('connect', (client) => {
      this.stats.totalConnections++;
      logger.debug('New database client connected', {
        totalConnections: this.pool?.totalCount,
        idleConnections: this.pool?.idleCount
      });
      
      this.emit('clientConnected', client);
    });

    this.pool.on('acquire', (client) => {
      logger.debug('Database client acquired from pool');
      this.emit('clientAcquired', client);
    });

    this.pool.on('remove', (client) => {
      logger.debug('Database client removed from pool');
      this.emit('clientRemoved', client);
    });
  }

  // Test database connection
  private async testConnection(): Promise<void> {
    if (!this.pool) {
      throw new Error('Pool not initialized');
    }

    const client = await this.pool.connect();
    try {
      await client.query('SELECT 1 as test');
      logger.debug('Database connection test successful');
    } finally {
      client.release();
    }
  }

  // Handle connection errors and attempt reconnection
  private async handleConnectionError(error: Error): Promise<void> {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      logger.error('Maximum reconnection attempts reached', {
        attempts: this.reconnectAttempts,
        maxAttempts: this.maxReconnectAttempts
      });
      
      this.emit('maxReconnectAttemptsReached', error);
      return;
    }

    this.reconnectAttempts++;
    
    logger.warn('Attempting to reconnect to database', {
      attempt: this.reconnectAttempts,
      maxAttempts: this.maxReconnectAttempts,
      delay: this.reconnectInterval
    });

    setTimeout(async () => {
      try {
        await this.reconnect();
      } catch (reconnectError) {
        logger.error('Reconnection attempt failed', {
          attempt: this.reconnectAttempts,
          error: reconnectError instanceof Error ? reconnectError.message : 'Unknown error'
        });
        
        await this.handleConnectionError(reconnectError as Error);
      }
    }, this.reconnectInterval);
  }

  // Reconnect to database
  private async reconnect(): Promise<void> {
    if (this.pool) {
      await this.pool.end();
      this.pool = null;
    }

    await this.createPool();
    await this.testConnection();
    this.setupEventHandlers();
    
    this.reconnectAttempts = 0;
    logger.info('Database reconnection successful');
    this.emit('reconnected');
  }

  // Start periodic health checks
  private startHealthCheck(): void {
    this.healthCheckInterval = setInterval(async () => {
      try {
        await this.healthCheck();
      } catch (error) {
        logger.warn('Health check failed', {
          error: error instanceof Error ? error.message : 'Unknown error'
        });
      }
    }, 30000); // Every 30 seconds
  }

  // Perform health check
  async healthCheck(): Promise<boolean> {
    try {
      if (!this.pool) {
        return false;
      }

      const startTime = Date.now();
      const client = await this.pool.connect();
      
      try {
        await client.query('SELECT 1 as health_check');
        const responseTime = Date.now() - startTime;
        
        logger.debug('Database health check successful', {
          responseTime,
          totalConnections: this.pool.totalCount,
          idleConnections: this.pool.idleCount
        });
        
        return true;
      } finally {
        client.release();
      }
    } catch (error) {
      logger.error('Database health check failed', {
        error: error instanceof Error ? error.message : 'Unknown error'
      });
      
      return false;
    }
  }

  // Execute query with retry logic
  async query<T = any>(text: string, params?: any[], options: QueryOptions = {}): Promise<QueryResult<T>> {
    const {
      timeout = 30000,
      retries = this.retryConfig.maxRetries,
      logQuery = true,
      transactionId
    } = options;

    const queryId = `query_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    const startTime = Date.now();

    if (logQuery) {
      logDatabaseQuery('query_start', 'database', 0);
    }

    let lastError: Error | null = null;
    
    for (let attempt = 0; attempt <= retries; attempt++) {
      try {
        const result = await this.executeQuery<T>(text, params, timeout);
        const duration = Date.now() - startTime;
        
        // Update statistics
        this.stats.totalQueries++;
        this.stats.successfulQueries++;
        this.updateQueryTimeStats(duration);
        
        if (logQuery) {
          logDatabaseQuery('query_success', 'database', duration);
        }
        
        return result;
      } catch (error) {
        lastError = error as Error;
        const duration = Date.now() - startTime;
        
        this.stats.totalQueries++;
        this.stats.failedQueries++;
        
        if (logQuery) {
          logDatabaseQuery('query_error', 'database', duration, lastError);
        }
        
        // Check if error is retryable
        if (attempt < retries && this.isRetryableError(lastError)) {
          const delay = this.calculateRetryDelay(attempt);
          
          logger.warn('Query failed, retrying', {
            queryId,
            attempt: attempt + 1,
            maxRetries: retries,
            delay,
            error: lastError.message
          });
          
          await this.sleep(delay);
          continue;
        }
        
        break;
      }
    }
    
    // All retries exhausted
    const finalError = new DatabaseError(
      `Query failed after ${retries + 1} attempts: ${lastError?.message || 'Unknown error'}`,
      lastError
    );
    
    logger.error('Query failed permanently', {
      queryId,
      sql: text,
      attempts: retries + 1,
      error: lastError?.message,
      stack: lastError?.stack
    });
    
    throw finalError;
  }

  // Execute single query
  private async executeQuery<T = any>(text: string, params?: any[], timeout?: number): Promise<QueryResult<T>> {
    if (!this.pool) {
      throw new DatabaseError('Database not initialized');
    }

    const client = await this.pool.connect();
    
    try {
      if (timeout) {
        // Set query timeout
        const timeoutPromise = new Promise<never>((_, reject) => {
          setTimeout(() => {
            reject(new Error(`Query timeout after ${timeout}ms`));
          }, timeout);
        });
        
        return await Promise.race([
          client.query<T>(text, params),
          timeoutPromise
        ]);
      } else {
        return await client.query<T>(text, params);
      }
    } finally {
      client.release();
    }
  }

  // Execute transaction
  async transaction<T>(callback: (context: TransactionContext) => Promise<T>, options: QueryOptions = {}): Promise<T> {
    if (!this.pool) {
      throw new DatabaseError('Database not initialized');
    }

    const transactionId = `tx_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    const client = await this.pool.connect();
    const context: TransactionContext = {
      id: transactionId,
      client,
      startTime: new Date(),
      queries: 0
    };

    logger.debug('Transaction started', { transactionId });

    try {
      await client.query('BEGIN');
      const result = await callback(context);
      await client.query('COMMIT');
      
      logger.debug('Transaction committed', {
        transactionId,
        queries: context.queries,
        duration: Date.now() - context.startTime.getTime()
      });
      
      return result;
    } catch (error) {
      await client.query('ROLLBACK');
      
      logger.error('Transaction rolled back', {
        transactionId,
        queries: context.queries,
        error: error instanceof Error ? error.message : 'Unknown error',
        duration: Date.now() - context.startTime.getTime()
      });
      
      throw error;
    } finally {
      client.release();
    }
  }

  // Check if error is retryable
  private isRetryableError(error: Error): boolean {
    const errorMessage = error.message.toLowerCase();
    const errorCode = (error as any).code;
    
    return this.retryConfig.retryableErrors.some(retryableError => 
      errorMessage.includes(retryableError.toLowerCase()) || 
      errorCode === retryableError
    );
  }

  // Calculate retry delay with exponential backoff
  private calculateRetryDelay(attempt: number): number {
    const delay = Math.min(
      this.retryConfig.initialDelay * Math.pow(this.retryConfig.backoffMultiplier, attempt),
      this.retryConfig.maxDelay
    );
    
    // Add jitter to prevent thundering herd
    const jitter = Math.random() * 0.1 * delay;
    return Math.floor(delay + jitter);
  }

  // Sleep utility
  private sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  // Update query time statistics
  private updateQueryTimeStats(duration: number): void {
    this.queryTimes.push(duration);
    
    if (this.queryTimes.length > this.maxQueryTimeHistory) {
      this.queryTimes = this.queryTimes.slice(-this.maxQueryTimeHistory);
    }
    
    this.stats.averageQueryTime = this.queryTimes.reduce((sum, time) => sum + time, 0) / this.queryTimes.length;
  }

  // Get pool statistics
  getStats(): PoolStats {
    if (this.pool) {
      this.stats.totalConnections = this.pool.totalCount;
      this.stats.idleConnections = this.pool.idleCount;
      this.stats.activeConnections = this.pool.totalCount - this.pool.idleCount;
      this.stats.waitingClients = this.pool.waitingCount;
    }
    
    return { ...this.stats };
  }

  // Get pool instance
  getPool(): Pool | null {
    return this.pool;
  }

  // Check if initialized
  isReady(): boolean {
    return this.isInitialized && this.pool !== null;
  }

  // Graceful shutdown
  async shutdown(): Promise<void> {
    logger.info('Shutting down database manager');
    
    if (this.healthCheckInterval) {
      clearInterval(this.healthCheckInterval);
      this.healthCheckInterval = null;
    }
    
    if (this.pool) {
      await this.pool.end();
      this.pool = null;
    }
    
    this.isInitialized = false;
    this.emit('shutdown');
    
    logger.info('Database manager shutdown complete');
  }
}

// Singleton instance
let databaseManager: DatabaseManager | null = null;

// Initialize database manager
export function initializeDatabaseManager(config: DatabaseConfig, retryConfig?: Partial<RetryConfig>): DatabaseManager {
  if (!databaseManager) {
    databaseManager = new DatabaseManager(config, retryConfig);
  }
  return databaseManager;
}

// Get database manager instance
export function getDatabaseManager(): DatabaseManager {
  if (!databaseManager) {
    throw new Error('Database manager not initialized. Call initializeDatabaseManager() first.');
  }
  return databaseManager;
}

export { DatabaseManager };
export default DatabaseManager;