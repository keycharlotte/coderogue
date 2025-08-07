// CodeRogue Player Data Analytics - Data Access Layer Implementation
// This file contains concrete implementations of all repository interfaces

import { Pool } from 'pg';
import {
  Player,
  GameSession,
  GameEvent,
  Card,
  CardUsage,
  Level,
  LevelAttempt,
  PlayerStats,
  EventType,
  CardType,
  CardRarity,
  PlayerRepository,
  GameSessionRepository,
  GameEventRepository,
  CardRepository,
  CardUsageRepository,
  LevelRepository,
  LevelAttemptRepository,
  PlayerStatsRepository
} from '../models';
import { getPool } from '../config/database';

// Base repository implementation
abstract class BaseRepositoryImpl<T> {
  protected pool: Pool;
  protected tableName: string;

  constructor(tableName: string) {
    this.pool = getPool();
    this.tableName = tableName;
  }

  async findById(id: string): Promise<T | null> {
    const query = `SELECT * FROM ${this.tableName} WHERE ${this.getIdColumn()} = $1`;
    const result = await this.pool.query(query, [id]);
    return result.rows[0] || null;
  }

  async findAll(limit = 100, offset = 0): Promise<T[]> {
    const query = `SELECT * FROM ${this.tableName} ORDER BY created_at DESC LIMIT $1 OFFSET $2`;
    const result = await this.pool.query(query, [limit, offset]);
    return result.rows;
  }

  async delete(id: string): Promise<boolean> {
    const query = `DELETE FROM ${this.tableName} WHERE ${this.getIdColumn()} = $1`;
    const result = await this.pool.query(query, [id]);
    return result.rowCount > 0;
  }

  protected abstract getIdColumn(): string;
  abstract create(entity: Omit<T, 'created_at' | 'updated_at'>): Promise<T>;
  abstract update(id: string, updates: Partial<T>): Promise<T | null>;
}

// Player repository implementation
export class PlayerRepositoryImpl extends BaseRepositoryImpl<Player> implements PlayerRepository {
  constructor() {
    super('players');
  }

  protected getIdColumn(): string {
    return 'player_id';
  }

  async create(entity: Omit<Player, 'created_at' | 'updated_at'>): Promise<Player> {
    const query = `
      INSERT INTO players (player_id, device_id, first_seen, last_seen, player_segment, metadata)
      VALUES ($1, $2, $3, $4, $5, $6)
      RETURNING *
    `;
    const values = [
      entity.player_id,
      entity.device_id,
      entity.first_seen,
      entity.last_seen,
      entity.player_segment,
      JSON.stringify(entity.metadata)
    ];
    const result = await this.pool.query(query, values);
    return result.rows[0];
  }

  async update(id: string, updates: Partial<Player>): Promise<Player | null> {
    const setClause = Object.keys(updates)
      .map((key, index) => `${key} = $${index + 2}`)
      .join(', ');
    
    const query = `
      UPDATE players 
      SET ${setClause}, updated_at = CURRENT_TIMESTAMP 
      WHERE player_id = $1 
      RETURNING *
    `;
    
    const values = [id, ...Object.values(updates)];
    const result = await this.pool.query(query, values);
    return result.rows[0] || null;
  }

  async findByDeviceId(deviceId: string): Promise<Player | null> {
    const query = 'SELECT * FROM players WHERE device_id = $1';
    const result = await this.pool.query(query, [deviceId]);
    return result.rows[0] || null;
  }

  async updateLastSeen(playerId: string): Promise<void> {
    const query = 'UPDATE players SET last_seen = CURRENT_TIMESTAMP WHERE player_id = $1';
    await this.pool.query(query, [playerId]);
  }

  async getPlayerSegmentCounts(): Promise<Record<string, number>> {
    const query = 'SELECT player_segment, COUNT(*) as count FROM players GROUP BY player_segment';
    const result = await this.pool.query(query);
    
    const counts: Record<string, number> = {};
    result.rows.forEach(row => {
      counts[row.player_segment] = parseInt(row.count);
    });
    return counts;
  }

  async getActivePlayersCount(days: number): Promise<number> {
    const query = `
      SELECT COUNT(DISTINCT player_id) as count 
      FROM players 
      WHERE last_seen >= CURRENT_DATE - INTERVAL '${days} days'
    `;
    const result = await this.pool.query(query);
    return parseInt(result.rows[0].count);
  }

  async count(): Promise<number> {
    const query = 'SELECT COUNT(*) as count FROM players';
    const result = await this.pool.query(query);
    return parseInt(result.rows[0].count);
  }
}

// Game session repository implementation
export class GameSessionRepositoryImpl extends BaseRepositoryImpl<GameSession> implements GameSessionRepository {
  constructor() {
    super('game_sessions');
  }

  protected getIdColumn(): string {
    return 'session_id';
  }

  async create(entity: Omit<GameSession, 'created_at' | 'updated_at'>): Promise<GameSession> {
    const query = `
      INSERT INTO game_sessions (session_id, player_id, start_time, end_time, duration_seconds, game_version, session_data)
      VALUES ($1, $2, $3, $4, $5, $6, $7)
      RETURNING *
    `;
    const values = [
      entity.session_id,
      entity.player_id,
      entity.start_time,
      entity.end_time,
      entity.duration_seconds,
      entity.game_version,
      JSON.stringify(entity.session_data)
    ];
    const result = await this.pool.query(query, values);
    return result.rows[0];
  }

  async update(id: string, updates: Partial<GameSession>): Promise<GameSession | null> {
    const setClause = Object.keys(updates)
      .map((key, index) => `${key} = $${index + 2}`)
      .join(', ');
    
    const query = `
      UPDATE game_sessions 
      SET ${setClause}, updated_at = CURRENT_TIMESTAMP 
      WHERE session_id = $1 
      RETURNING *
    `;
    
    const values = [id, ...Object.values(updates)];
    const result = await this.pool.query(query, values);
    return result.rows[0] || null;
  }

  async findByPlayerId(playerId: string, limit = 50): Promise<GameSession[]> {
    const query = `
      SELECT * FROM game_sessions 
      WHERE player_id = $1 
      ORDER BY start_time DESC 
      LIMIT $2
    `;
    const result = await this.pool.query(query, [playerId, limit]);
    return result.rows;
  }

  async getAverageSessionLength(): Promise<number> {
    const query = `
      SELECT AVG(duration_seconds) as avg_duration 
      FROM game_sessions 
      WHERE duration_seconds IS NOT NULL
    `;
    const result = await this.pool.query(query);
    return parseFloat(result.rows[0].avg_duration) || 0;
  }

  async getSessionCountByDate(startDate: string, endDate: string): Promise<Array<{ date: string; count: number }>> {
    const query = `
      SELECT DATE(start_time) as date, COUNT(*) as count
      FROM game_sessions
      WHERE start_time >= $1 AND start_time <= $2
      GROUP BY DATE(start_time)
      ORDER BY date
    `;
    const result = await this.pool.query(query, [startDate, endDate]);
    return result.rows.map(row => ({
      date: row.date,
      count: parseInt(row.count)
    }));
  }

  async endSession(sessionId: string): Promise<void> {
    const query = `
      UPDATE game_sessions 
      SET end_time = CURRENT_TIMESTAMP,
          duration_seconds = EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - start_time))
      WHERE session_id = $1
    `;
    await this.pool.query(query, [sessionId]);
  }

  async count(): Promise<number> {
    const query = 'SELECT COUNT(*) as count FROM game_sessions';
    const result = await this.pool.query(query);
    return parseInt(result.rows[0].count);
  }

  async getAverageSessionDuration(startDate?: Date, endDate?: Date): Promise<number> {
    let query = `
      SELECT AVG(duration_seconds) as avg_duration 
      FROM game_sessions 
      WHERE duration_seconds IS NOT NULL
    `;
    const params: any[] = [];
    
    if (startDate && endDate) {
      query += ' AND start_time >= $1 AND start_time <= $2';
      params.push(startDate, endDate);
    }
    
    const result = await this.pool.query(query, params);
    return parseFloat(result.rows[0].avg_duration) || 0;
  }
}

// Game event repository implementation
export class GameEventRepositoryImpl extends BaseRepositoryImpl<GameEvent> implements GameEventRepository {
  constructor() {
    super('game_events');
  }

  protected getIdColumn(): string {
    return 'event_id';
  }

  async create(entity: Omit<GameEvent, 'created_at' | 'updated_at'>): Promise<GameEvent> {
    const query = `
      INSERT INTO game_events (event_id, session_id, event_type, timestamp, event_data, event_category)
      VALUES ($1, $2, $3, $4, $5, $6)
      RETURNING *
    `;
    const values = [
      entity.event_id,
      entity.session_id,
      entity.event_type,
      entity.timestamp,
      JSON.stringify(entity.event_data),
      entity.event_category
    ];
    const result = await this.pool.query(query, values);
    return result.rows[0];
  }

  async update(id: string, updates: Partial<GameEvent>): Promise<GameEvent | null> {
    const setClause = Object.keys(updates)
      .map((key, index) => `${key} = $${index + 2}`)
      .join(', ');
    
    const query = `
      UPDATE game_events 
      SET ${setClause}, updated_at = CURRENT_TIMESTAMP 
      WHERE event_id = $1 
      RETURNING *
    `;
    
    const values = [id, ...Object.values(updates)];
    const result = await this.pool.query(query, values);
    return result.rows[0] || null;
  }

  async findBySessionId(sessionId: string): Promise<GameEvent[]> {
    const query = `
      SELECT * FROM game_events 
      WHERE session_id = $1 
      ORDER BY timestamp ASC
    `;
    const result = await this.pool.query(query, [sessionId]);
    return result.rows;
  }

  async findByEventType(eventType: EventType, limit = 100): Promise<GameEvent[]> {
    const query = `
      SELECT * FROM game_events 
      WHERE event_type = $1 
      ORDER BY timestamp DESC 
      LIMIT $2
    `;
    const result = await this.pool.query(query, [eventType, limit]);
    return result.rows;
  }

  async getEventCountsByType(startDate: string, endDate: string): Promise<Record<EventType, number>> {
    const query = `
      SELECT event_type, COUNT(*) as count
      FROM game_events
      WHERE timestamp >= $1 AND timestamp <= $2
      GROUP BY event_type
    `;
    const result = await this.pool.query(query, [startDate, endDate]);
    
    const counts: Record<string, number> = {};
    result.rows.forEach(row => {
      counts[row.event_type] = parseInt(row.count);
    });
    return counts as Record<EventType, number>;
  }

  async createBatch(events: Omit<GameEvent, 'event_id' | 'created_at'>[]): Promise<GameEvent[]> {
    if (events.length === 0) return [];

    const values: any[] = [];
    const placeholders: string[] = [];
    
    events.forEach((event, index) => {
      const baseIndex = index * 6;
      placeholders.push(`($${baseIndex + 1}, $${baseIndex + 2}, $${baseIndex + 3}, $${baseIndex + 4}, $${baseIndex + 5}, $${baseIndex + 6})`);
      values.push(
        `event_${Date.now()}_${index}`,
        event.session_id,
        event.event_type,
        event.timestamp,
        JSON.stringify(event.event_data),
        event.event_category
      );
    });

    const query = `
      INSERT INTO game_events (event_id, session_id, event_type, timestamp, event_data, event_category)
      VALUES ${placeholders.join(', ')}
      RETURNING *
    `;
    
    const result = await this.pool.query(query, values);
    return result.rows;
  }

  async count(): Promise<number> {
    const query = 'SELECT COUNT(*) as count FROM game_events';
    const result = await this.pool.query(query);
    return parseInt(result.rows[0].count);
  }
}

// Export repository factory functions
let _playerRepository: PlayerRepositoryImpl | null = null;
let _gameSessionRepository: GameSessionRepositoryImpl | null = null;
let _gameEventRepository: GameEventRepositoryImpl | null = null;

export const getPlayerRepository = (): PlayerRepositoryImpl => {
  if (!_playerRepository) {
    _playerRepository = new PlayerRepositoryImpl();
  }
  return _playerRepository;
};

export const getGameSessionRepository = (): GameSessionRepositoryImpl => {
  if (!_gameSessionRepository) {
    _gameSessionRepository = new GameSessionRepositoryImpl();
  }
  return _gameSessionRepository;
};

export const getGameEventRepository = (): GameEventRepositoryImpl => {
  if (!_gameEventRepository) {
    _gameEventRepository = new GameEventRepositoryImpl();
  }
  return _gameEventRepository;
};



// Export all repository classes
export * from '../models';