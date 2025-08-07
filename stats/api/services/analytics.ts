// CodeRogue Player Data Analytics - Analytics Service
// This file contains core analytics logic and calculations

import { Pool } from 'pg';
import {
  AnalyticsQuery,
  CardAnalyticsResponse,
  PlayerBehaviorResponse,
  BalanceRecommendation
} from '../../shared/types';
import {
  PlayerRepositoryImpl,
  GameSessionRepositoryImpl,
  GameEventRepositoryImpl
} from '../repositories';
import {
  CardRepositoryImpl,
  CardUsageRepositoryImpl,
  LevelRepositoryImpl,
  LevelAttemptRepositoryImpl,
  PlayerStatsRepositoryImpl
} from '../repositories/extended';
import { getPool } from '../config/database';

export class AnalyticsService {
  private playerRepo: PlayerRepositoryImpl;
  private sessionRepo: GameSessionRepositoryImpl;
  private eventRepo: GameEventRepositoryImpl;
  private cardRepo: CardRepositoryImpl;
  private cardUsageRepo: CardUsageRepositoryImpl;
  private levelRepo: LevelRepositoryImpl;
  private levelAttemptRepo: LevelAttemptRepositoryImpl;
  private statsRepo: PlayerStatsRepositoryImpl;

  constructor(pool: Pool) {
    // Repository instances will be created lazily when needed
    // to avoid database initialization issues
  }

  private getPlayerRepo(): PlayerRepositoryImpl {
    if (!this.playerRepo) {
      this.playerRepo = new PlayerRepositoryImpl();
    }
    return this.playerRepo;
  }

  private getSessionRepo(): GameSessionRepositoryImpl {
    if (!this.sessionRepo) {
      this.sessionRepo = new GameSessionRepositoryImpl();
    }
    return this.sessionRepo;
  }

  private getEventRepo(): GameEventRepositoryImpl {
    if (!this.eventRepo) {
      this.eventRepo = new GameEventRepositoryImpl();
    }
    return this.eventRepo;
  }

  private getCardRepo(): CardRepositoryImpl {
    if (!this.cardRepo) {
      this.cardRepo = new CardRepositoryImpl();
    }
    return this.cardRepo;
  }

  private getCardUsageRepo(): CardUsageRepositoryImpl {
    if (!this.cardUsageRepo) {
      this.cardUsageRepo = new CardUsageRepositoryImpl();
    }
    return this.cardUsageRepo;
  }

  private getLevelRepo(): LevelRepositoryImpl {
    if (!this.levelRepo) {
      this.levelRepo = new LevelRepositoryImpl();
    }
    return this.levelRepo;
  }

  private getLevelAttemptRepo(): LevelAttemptRepositoryImpl {
    if (!this.levelAttemptRepo) {
      this.levelAttemptRepo = new LevelAttemptRepositoryImpl();
    }
    return this.levelAttemptRepo;
  }

  private getStatsRepo(): PlayerStatsRepositoryImpl {
    if (!this.statsRepo) {
      this.statsRepo = new PlayerStatsRepositoryImpl();
    }
    return this.statsRepo;
  }

  // Card Analytics
  async getCardAnalytics(query: AnalyticsQuery): Promise<CardAnalyticsResponse> {
    const { startDate, endDate } = query;
    
    try {
      // Get all cards or specific card
      const cards = await this.getCardRepo().findAll();

      const cardStats: any[] = [];
      
      for (const card of cards) {
        if (!card) continue;
        
        // Get usage statistics
        // Mock data for now - these methods need to be implemented
        const winRate = 0.5;
        const totalUsage = 100;
        const popularityTrend = 'stable';

        cardStats.push({
          cardId: card.card_id,
          cardName: card.card_name,
          totalUsage: totalUsage || 0,
          winRate: winRate || 0,
          averageUsagePerGame: 0,
          popularityRank: 0,
          trend: popularityTrend || 'stable',
          lastUsed: new Date()
        });
      }

      // Get card combinations
      const combinations = await this.getCardCombinations(new Date(startDate), new Date(endDate));

      // Get usage trends
      const trends = await this.getCardUsageTrends(new Date(startDate), new Date(endDate));

      return {
        cards: cardStats,
        totalCount: cardStats.length,
        dateRange: {
          start: startDate || new Date().toISOString(),
          end: endDate || new Date().toISOString()
        }
      };
    } catch (error) {
      console.error('Error in getCardAnalytics:', error);
      throw error;
    }
  }

  // Player Behavior Analytics
  async getPlayerBehaviorAnalytics(query: AnalyticsQuery): Promise<PlayerBehaviorResponse> {
    const { startDate, endDate } = query;
    
    try {
      // Get retention data
      const retention = await this.calculatePlayerRetention(new Date(startDate), new Date(endDate));
      
      // Get session analytics
      const avgSessionDuration = await this.getSessionRepo().getAverageSessionDuration(new Date(startDate), new Date(endDate));
      const sessionDistribution = await this.getSessionDurationDistribution(new Date(startDate), new Date(endDate));
      
      // Get progression analytics
      const progressionData = await this.getPlayerProgression(new Date(startDate), new Date(endDate));
      
      // Get engagement patterns
      const engagementPatterns = await this.getEngagementPatterns(new Date(startDate), new Date(endDate));
      
      // Get player segments
      const playerSegments = await this.getPlayerSegments(new Date(startDate), new Date(endDate));
      
      // Get churn analysis
      const churnAnalysis = await this.getChurnAnalysis(new Date(startDate), new Date(endDate));

      // Create behavior data based on metric type
      let behaviorData: any[] = [];
      
      const metric = 'retention'; // Default metric
      
      behaviorData = retention.map(r => ({
        metric: 'retention',
        value: r.retentionRate,
        date: r.date,
        segment: r.segment
      }));
      
      return {
        metric: metric,
        data: behaviorData,
        summary: {
          average: behaviorData.length > 0 ? behaviorData.reduce((sum, d) => sum + d.value, 0) / behaviorData.length : 0,
          trend: 'stable' as const,
          changePercent: 0
        }
      };
    } catch (error) {
      console.error('Error in getPlayerBehaviorAnalytics:', error);
      throw error;
    }
  }

  // Game Balance Analytics
  async getGameBalanceAnalytics(query: AnalyticsQuery): Promise<any> {
    const { startDate, endDate } = query;
    
    try {
      // Get level difficulty analysis
      const levelDifficulty = await this.analyzeLevelDifficulty(new Date(startDate), new Date(endDate));
      
      // Get card balance analysis
      const cardBalance = await this.analyzeCardBalance(new Date(startDate), new Date(endDate));
      
      // Get progression bottlenecks
      const bottlenecks = await this.findProgressionBottlenecks(new Date(startDate), new Date(endDate));
      
      // Get win/loss distribution
      const winLossDistribution = await this.getWinLossDistribution(new Date(startDate), new Date(endDate));
      
      // Get game flow analysis
      const gameFlow = await this.analyzeGameFlow(new Date(startDate), new Date(endDate));
      
      // Generate recommendations
      const recommendations = await this.generateBalanceRecommendations({
        levelDifficulty,
        cardBalance,
        bottlenecks,
        winLossDistribution
      });

      return {
        levelDifficulty,
        cardBalance,
        bottlenecks,
        winLossDistribution,
        gameFlow,
        recommendations
      };
    } catch (error) {
      console.error('Error in getGameBalanceAnalytics:', error);
      throw error;
    }
  }

  // Helper Methods
  private async getCardCombinations(startDate?: Date, endDate?: Date) {
    // Implementation for card combination analysis
    return [];
  }

  private async getCardUsageTrends(startDate?: Date, endDate?: Date) {
    // Implementation for card usage trends
    return [];
  }

  private async calculatePlayerRetention(startDate?: Date, endDate?: Date): Promise<any[]> {
    const day1Retention = await this.getStatsRepo().getRetentionRate(1, startDate, endDate);
    const day7Retention = await this.getStatsRepo().getRetentionRate(7, startDate, endDate);
    const day30Retention = await this.getStatsRepo().getRetentionRate(30, startDate, endDate);
    
    return [
      { retentionRate: day1Retention || 0, date: new Date().toISOString(), segment: 'day1' },
      { retentionRate: day7Retention || 0, date: new Date().toISOString(), segment: 'day7' },
      { retentionRate: day30Retention || 0, date: new Date().toISOString(), segment: 'day30' }
    ];
  }

  private async getSessionDurationDistribution(startDate?: Date, endDate?: Date) {
    // Implementation for session duration distribution
    return [
      { averageDuration: 3, date: new Date().toISOString() },
      { averageDuration: 10, date: new Date().toISOString() },
      { averageDuration: 22, date: new Date().toISOString() },
      { averageDuration: 45, date: new Date().toISOString() }
    ];
  }

  private async getPlayerProgression(startDate?: Date, endDate?: Date) {
    // Implementation for player progression analysis
    return {
      averageLevelReached: 0,
      completionRates: [],
      stuckPoints: []
    };
  }

  private async getEngagementPatterns(startDate?: Date, endDate?: Date) {
    // Implementation for engagement patterns
    return {
      dailyActiveUsers: 0,
      weeklyActiveUsers: 0,
      monthlyActiveUsers: 0,
      sessionFrequency: 0
    };
  }

  private async getPlayerSegments(startDate?: Date, endDate?: Date) {
    // Implementation for player segmentation
    return {
      newPlayers: 0,
      returningPlayers: 0,
      powerUsers: 0,
      atRiskPlayers: 0
    };
  }

  private async getChurnAnalysis(startDate?: Date, endDate?: Date) {
    // Implementation for churn analysis
    return {
      churnRate: 0,
      riskFactors: [],
      predictedChurn: []
    };
  }

  private async analyzeLevelDifficulty(startDate?: Date, endDate?: Date): Promise<any[]> {
    const levels = await this.levelRepo.findAll();
    const difficulties: any[] = [];
    
    for (const level of levels) {
      const completionRate = await this.levelAttemptRepo.getLevelCompletionRate(level.level_id, startDate, endDate);
      const averageAttempts = await this.levelAttemptRepo.getAverageAttempts(level.level_id, startDate, endDate);

      difficulties.push({
        levelId: level.level_id,
        levelName: level.level_name,
        completionRate: completionRate || 0,
        averageAttempts: averageAttempts || 0,
        difficulty: this.calculateDifficultyScore(completionRate || 0, averageAttempts || 0)
      });
    }
    
    return difficulties;
  }

  private async analyzeCardBalance(startDate?: Date, endDate?: Date) {
    // Implementation for card balance analysis
    return {
      overpoweredCards: [],
      underpoweredCards: [],
      balanceScore: 0
    };
  }

  private async findProgressionBottlenecks(startDate?: Date, endDate?: Date) {
    // Implementation for finding progression bottlenecks
    return [];
  }

  private async getWinLossDistribution(startDate?: Date, endDate?: Date) {
    // Implementation for win/loss distribution
    return {
      wins: 0,
      losses: 0,
      winRate: 0
    };
  }

  private async analyzeGameFlow(startDate?: Date, endDate?: Date) {
    // Implementation for game flow analysis
    return {
      averageGameLength: 0,
      dropoffPoints: [],
      flowEfficiency: 0
    };
  }

  private async generateBalanceRecommendations(data: any): Promise<BalanceRecommendation[]> {
    const recommendations: BalanceRecommendation[] = [];
    
    // Analyze level difficulty
    if (data.levelDifficulty) {
      const hardLevels = data.levelDifficulty.filter((l: any) => l.completionRate < 0.3);
      const easyLevels = data.levelDifficulty.filter((l: any) => l.completionRate > 0.9);
      
      if (hardLevels.length > 0) {
        recommendations.push({
          type: 'nerf',
          target: hardLevels.map((l: any) => l.levelName).join(', '),
          reason: `${hardLevels.length} levels have very low completion rates (< 30%). Consider reducing difficulty or providing more hints/tutorials`,
          priority: 'high'
        });
      }
      
      if (easyLevels.length > 0) {
        recommendations.push({
          type: 'buff',
          target: easyLevels.map((l: any) => l.levelName).join(', '),
          reason: `${easyLevels.length} levels have very high completion rates (> 90%). Consider increasing difficulty to maintain engagement`,
          priority: 'medium'
        });
      }
    }
    
    return recommendations;
  }

  private calculateDifficultyScore(completionRate: number, averageAttempts: number): 'easy' | 'medium' | 'hard' | 'very_hard' {
    if (completionRate > 0.8 && averageAttempts < 2) return 'easy';
    if (completionRate > 0.5 && averageAttempts < 4) return 'medium';
    if (completionRate > 0.2) return 'hard';
    return 'very_hard';
  }

  // Summary Analytics
  async getAnalyticsSummary(query: AnalyticsQuery) {
    const { startDate, endDate } = query;
    
    try {
      const totalPlayers = await this.playerRepo.count();
      const totalSessions = await this.sessionRepo.count();
      const totalEvents = await this.eventRepo.count();
      const avgSessionDuration = await this.sessionRepo.getAverageSessionDuration(new Date(startDate), new Date(endDate));
      
      // Get top cards
      const topCards = await this.cardUsageRepo.getTopCards(5, new Date(startDate), new Date(endDate));
      
      // Get player activity trend
      const activityTrend = await this.getPlayerActivityTrend(new Date(startDate), new Date(endDate));
      
      return {
        overview: {
          totalPlayers,
          totalSessions,
          totalEvents,
          averageSessionDuration: avgSessionDuration || 0
        },
        topCards,
        activityTrend,
        generatedAt: new Date()
      };
    } catch (error) {
      console.error('Error in getAnalyticsSummary:', error);
      throw error;
    }
  }

  private async getPlayerActivityTrend(startDate?: Date, endDate?: Date): Promise<Array<{ date: string; activeUsers: number; newUsers: number }>> {
    const query = `
      WITH date_series AS (
        SELECT generate_series(
          COALESCE($1::date, CURRENT_DATE - INTERVAL '30 days'),
          COALESCE($2::date, CURRENT_DATE),
          '1 day'::interval
        )::date as date
      ),
      daily_activity AS (
        SELECT 
          DATE(gs.start_time) as activity_date,
          COUNT(DISTINCT gs.player_id) as active_users,
          COUNT(DISTINCT CASE WHEN DATE(p.first_seen) = DATE(gs.start_time) THEN gs.player_id END) as new_users
        FROM game_sessions gs
        JOIN players p ON gs.player_id = p.player_id
        WHERE gs.start_time >= COALESCE($1, CURRENT_DATE - INTERVAL '30 days')
          AND gs.start_time <= COALESCE($2, CURRENT_DATE)
        GROUP BY DATE(gs.start_time)
      )
      SELECT 
        ds.date::text as date,
        COALESCE(da.active_users, 0) as active_users,
        COALESCE(da.new_users, 0) as new_users
      FROM date_series ds
      LEFT JOIN daily_activity da ON ds.date = da.activity_date
      ORDER BY ds.date
    `;
    
    const pool = getPool();
    const result = await pool.query(query, [startDate, endDate]);
    return result.rows.map(row => ({
      date: row.date,
      activeUsers: parseInt(row.active_users),
      newUsers: parseInt(row.new_users)
    }));
  }
}