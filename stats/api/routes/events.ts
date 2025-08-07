// CodeRogue Player Data Analytics - Events API Routes
// This file handles game event reporting and data collection

import { Router, Request, Response } from 'express';
import { v4 as uuidv4 } from 'uuid';
import { EventReportRequest, EventReportResponse, GameEvent, Player, GameSession } from '../../shared/types';
import { 
  getPlayerRepository, 
  getGameSessionRepository, 
  getGameEventRepository
} from '../repositories';
import { getCardUsageRepository } from '../repositories/extended';
import { getPool } from '../config/database';
import { asyncHandler } from '../middleware/errorHandler';
import { validateGameEvent, validateBatchEvents, validateUuidParam, sanitizeRequest, validateContentType, validateEventQuery } from '../middleware/validation.js';

const router = Router();

// Validation utilities
class ValidationUtils {
  static isValidEventType(eventType: string): boolean {
    const validTypes = [
      'game_start', 'game_end', 'level_start', 'level_complete', 'level_failed',
      'card_used', 'card_drawn', 'player_death', 'boss_defeated', 'item_collected',
      'achievement_unlocked', 'session_pause', 'session_resume'
    ];
    return validTypes.includes(eventType);
  }

  static validatePlayerId(playerId: string): boolean {
    return typeof playerId === 'string' && playerId.length > 0 && playerId.length <= 255;
  }

  static validateSessionId(sessionId: string): boolean {
    return typeof sessionId === 'string' && sessionId.length > 0 && sessionId.length <= 255;
  }

  static validateEventData(eventData: any): boolean {
    return typeof eventData === 'object' && eventData !== null;
  }
}

// POST /api/v1/events - Report game events
router.post('/',
  validateContentType(),
  sanitizeRequest,
  validateGameEvent,
  asyncHandler(async (req: Request, res: Response) => {
    const eventData: EventReportRequest = req.body;

    // Validate required fields
    if (!eventData.playerId || !eventData.eventType || !eventData.sessionId) {
      return res.status(400).json({
        error: 'Bad Request',
        message: 'Missing required fields: playerId, eventType, sessionId',
        statusCode: 400,
        timestamp: new Date().toISOString()
      });
    }

    // Validate event type
    if (!ValidationUtils.isValidEventType(eventData.eventType)) {
      return res.status(400).json({
        error: 'Bad Request',
        message: `Invalid event type: ${eventData.eventType}`,
        statusCode: 400,
        timestamp: new Date().toISOString()
      });
    }

    // Validate player ID and session ID
    if (!ValidationUtils.validatePlayerId(eventData.playerId)) {
      return res.status(400).json({
        error: 'Bad Request',
        message: 'Invalid player ID format',
        statusCode: 400,
        timestamp: new Date().toISOString()
      });
    }

    if (!ValidationUtils.validateSessionId(eventData.sessionId)) {
      return res.status(400).json({
        error: 'Bad Request',
        message: 'Invalid session ID format',
        statusCode: 400,
        timestamp: new Date().toISOString()
      });
    }

    // Validate event data
    if (!ValidationUtils.validateEventData(eventData.eventData)) {
      return res.status(400).json({
        error: 'Bad Request',
        message: 'Invalid event data format',
        statusCode: 400,
        timestamp: new Date().toISOString()
      });
    }

    // Ensure player exists or create new player
    let player = await getPlayerRepository().findById(eventData.playerId);
    if (!player) {
      player = await getPlayerRepository().create({
        player_id: eventData.playerId,
        device_id: eventData.eventData.deviceId || null,
        first_seen: new Date().toISOString(),
        last_seen: new Date().toISOString(),
        player_segment: 'new',
        metadata: eventData.eventData.playerMetadata || {}
      });
    } else {
      // Update last seen
      await getPlayerRepository().updateLastSeen(eventData.playerId);
    }

    // Ensure session exists or create new session
    let session = await getGameSessionRepository().findById(eventData.sessionId);
    if (!session) {
      session = await getGameSessionRepository().create({
        session_id: eventData.sessionId,
        player_id: eventData.playerId,
        start_time: new Date(eventData.timestamp).toISOString(),
        game_version: eventData.eventData.gameVersion || '1.0.0',
        session_data: eventData.eventData.sessionData || {}
      });
    }

    // Create the game event
    const eventId = uuidv4();
    const gameEvent = await getGameEventRepository().create({
      event_id: eventId,
      session_id: eventData.sessionId,
      event_type: eventData.eventType,
      timestamp: new Date(eventData.timestamp).toISOString(),
      event_data: eventData.eventData,
      event_category: eventData.eventData.category || 'gameplay'
    });

    // Handle specific event types
    await handleSpecificEventType(eventData, gameEvent);

    const response: EventReportResponse = {
      success: true,
      message: 'Event reported successfully',
      eventId: eventId
    };

    res.status(201).json(response);
}));

// POST /api/v1/events/batch - Report multiple events in batch
router.post('/batch',
  validateContentType(),
  sanitizeRequest,
  validateBatchEvents,
  asyncHandler(async (req: Request, res: Response) => {
    const events: EventReportRequest[] = req.body.events;

    if (!Array.isArray(events) || events.length === 0) {
      return res.status(400).json({
        error: 'Bad Request',
        message: 'Events array is required and must not be empty',
        statusCode: 400,
        timestamp: new Date().toISOString()
      });
    }

    if (events.length > 100) {
      return res.status(400).json({
        error: 'Bad Request',
        message: 'Batch size cannot exceed 100 events',
        statusCode: 400,
        timestamp: new Date().toISOString()
      });
    }

    const results = [];
    const errors = [];

    for (let i = 0; i < events.length; i++) {
      try {
        const eventData = events[i];
        
        // Individual event validation is handled by the middleware
        // Additional business logic validation can be added here if needed

        // Create event
        const eventId = uuidv4();
        const gameEvent = await getGameEventRepository().create({
          event_id: eventId,
          session_id: eventData.sessionId,
          event_type: eventData.eventType,
          timestamp: new Date(eventData.timestamp).toISOString(),
          event_data: eventData.eventData,
          event_category: eventData.eventData.category || 'gameplay'
        });

        results.push({
          index: i,
          eventId: eventId,
          success: true
        });

        // Handle specific event types
        await handleSpecificEventType(eventData, gameEvent);
      } catch (error) {
        errors.push({
          index: i,
          error: error instanceof Error ? error.message : 'Unknown error'
        });
      }
    }

    res.status(200).json({
      success: true,
      message: `Processed ${results.length} events successfully`,
      results: results,
      errors: errors,
      totalProcessed: results.length,
      totalErrors: errors.length
    });
}));

// GET /api/v1/events/session/:sessionId - Get events for a session
router.get('/session/:sessionId',
  validateEventQuery,
  asyncHandler(async (req: Request, res: Response) => {
    const { sessionId } = req.params;

    const events = await getGameEventRepository().findBySessionId(sessionId);
    
    res.json({
      sessionId: sessionId,
      events: events,
      totalEvents: events.length
    });
}));

// Helper function to handle specific event types
async function handleSpecificEventType(eventData: EventReportRequest, gameEvent: any): Promise<void> {
  try {
    switch (eventData.eventType) {
      case 'game_end':
        // End the session
        await getGameSessionRepository().endSession(eventData.sessionId);
        break;
        
      case 'card_used':
        // Record card usage
        if (eventData.eventData.cardId) {
          await getCardUsageRepository().create({
            usage_id: uuidv4(),
            card_id: eventData.eventData.cardId,
            session_id: eventData.sessionId,
            used_at: new Date(eventData.timestamp).toISOString(),
            context: eventData.eventData.context || null,
            resulted_in_win: eventData.eventData.resultedInWin || false
          });
        }
        break;
        
      case 'level_complete':
      case 'level_failed':
        // This will be handled by level attempt tracking
        // Implementation depends on level attempt repository
        break;
        
      default:
        // No special handling needed for other event types
        break;
    }
  } catch (error) {
    console.error(`Error handling specific event type ${eventData.eventType}:`, error);
    // Don't throw error here to avoid failing the main event creation
  }
}

export default router;