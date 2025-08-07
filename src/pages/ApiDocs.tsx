import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { Book, Code, Search, ChevronDown, ChevronRight, Copy, Play, Database, BarChart3, Users, Gamepad2 } from 'lucide-react';
import { toast } from 'sonner';

interface ApiEndpoint {
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  path: string;
  summary: string;
  description: string;
  parameters?: Parameter[];
  requestBody?: RequestBody;
  responses: Response[];
  examples: Example[];
}

interface Parameter {
  name: string;
  in: 'query' | 'path' | 'header';
  required: boolean;
  type: string;
  description: string;
  example?: any;
}

interface RequestBody {
  contentType: string;
  schema: any;
  example: any;
}

interface Response {
  status: number;
  description: string;
  schema?: any;
  example?: any;
}

interface Example {
  title: string;
  description: string;
  request?: any;
  response: any;
}

const API_ENDPOINTS: Record<string, ApiEndpoint[]> = {
  events: [
    {
      method: 'POST',
      path: '/api/v1/events',
      summary: 'Submit Game Event',
      description: 'Submit a game event for analytics tracking. This endpoint receives real-time game events from the client.',
      requestBody: {
        contentType: 'application/json',
        schema: {
          type: 'object',
          properties: {
            playerId: { type: 'string', description: 'Unique player identifier' },
            sessionId: { type: 'string', description: 'Current game session ID' },
            eventType: { type: 'string', description: 'Type of event (card_played, level_completed, etc.)' },
            timestamp: { type: 'string', format: 'date-time', description: 'Event timestamp' },
            data: { type: 'object', description: 'Event-specific data payload' }
          },
          required: ['playerId', 'sessionId', 'eventType', 'timestamp']
        },
        example: {
          playerId: 'player_123',
          sessionId: 'session_456',
          eventType: 'card_played',
          timestamp: '2024-01-15T10:30:00Z',
          data: {
            cardId: 'fire_spell',
            position: 2,
            targetType: 'enemy',
            damage: 25
          }
        }
      },
      responses: [
        {
          status: 200,
          description: 'Event successfully recorded',
          example: { success: true, eventId: 'evt_789' }
        },
        {
          status: 400,
          description: 'Invalid event data',
          example: { error: 'Missing required field: playerId' }
        }
      ],
      examples: [
        {
          title: 'Card Played Event',
          description: 'Recording when a player uses a card',
          request: {
            playerId: 'player_123',
            sessionId: 'session_456',
            eventType: 'card_played',
            timestamp: '2024-01-15T10:30:00Z',
            data: { cardId: 'fire_spell', position: 2, damage: 25 }
          },
          response: { success: true, eventId: 'evt_789' }
        },
        {
          title: 'Level Completed Event',
          description: 'Recording level completion',
          request: {
            playerId: 'player_123',
            sessionId: 'session_456',
            eventType: 'level_completed',
            timestamp: '2024-01-15T10:35:00Z',
            data: { levelId: 'level_5', score: 1250, timeElapsed: 300 }
          },
          response: { success: true, eventId: 'evt_790' }
        }
      ]
    }
  ],
  analytics: [
    {
      method: 'GET',
      path: '/api/v1/analytics/cards',
      summary: 'Get Card Analytics',
      description: 'Retrieve comprehensive analytics data for card usage, performance, and trends.',
      parameters: [
        { name: 'cardId', in: 'query', required: false, type: 'string', description: 'Filter by specific card ID' },
        { name: 'startDate', in: 'query', required: false, type: 'string', description: 'Start date for analytics period' },
        { name: 'endDate', in: 'query', required: false, type: 'string', description: 'End date for analytics period' },
        { name: 'limit', in: 'query', required: false, type: 'integer', description: 'Maximum number of results', example: 50 }
      ],
      responses: [
        {
          status: 200,
          description: 'Card analytics data',
          example: {
            totalCards: 45,
            usageStats: {
              totalUsage: 15420,
              averageUsagePerCard: 342.7,
              mostUsedCard: { cardId: 'fire_spell', usageCount: 1250 }
            },
            performanceMetrics: {
              averageWinRate: 0.67,
              topPerformingCards: [
                { cardId: 'lightning_bolt', winRate: 0.85, usageCount: 890 }
              ]
            }
          }
        }
      ],
      examples: [
        {
          title: 'All Cards Analytics',
          description: 'Get analytics for all cards',
          response: {
            totalCards: 45,
            usageStats: { totalUsage: 15420, averageUsagePerCard: 342.7 },
            performanceMetrics: { averageWinRate: 0.67 }
          }
        }
      ]
    },
    {
      method: 'GET',
      path: '/api/v1/analytics/players/behavior',
      summary: 'Get Player Behavior Analytics',
      description: 'Retrieve detailed analytics about player behavior patterns, retention, and engagement.',
      parameters: [
        { name: 'playerId', in: 'query', required: false, type: 'string', description: 'Filter by specific player ID' },
        { name: 'segment', in: 'query', required: false, type: 'string', description: 'Player segment (new, casual, hardcore, etc.)' },
        { name: 'period', in: 'query', required: false, type: 'string', description: 'Analysis period (daily, weekly, monthly)', example: 'weekly' }
      ],
      responses: [
        {
          status: 200,
          description: 'Player behavior analytics',
          example: {
            totalPlayers: 12450,
            retention: {
              day1: 0.75,
              day7: 0.45,
              day30: 0.25
            },
            engagement: {
              averageSessionDuration: 1800,
              averageSessionsPerDay: 2.3,
              dailyActiveUsers: 3200
            }
          }
        }
      ],
      examples: [
        {
          title: 'Overall Player Behavior',
          description: 'Get general player behavior metrics',
          response: {
            totalPlayers: 12450,
            retention: { day1: 0.75, day7: 0.45, day30: 0.25 },
            engagement: { averageSessionDuration: 1800 }
          }
        }
      ]
    },
    {
      method: 'GET',
      path: '/api/v1/analytics/balance',
      summary: 'Get Game Balance Analytics',
      description: 'Retrieve analytics data for game balance, including level difficulty, card balance, and progression bottlenecks.',
      parameters: [
        { name: 'levelId', in: 'query', required: false, type: 'string', description: 'Filter by specific level ID' },
        { name: 'includeRecommendations', in: 'query', required: false, type: 'boolean', description: 'Include balance recommendations', example: true }
      ],
      responses: [
        {
          status: 200,
          description: 'Game balance analytics',
          example: {
            levelDifficulty: {
              averageCompletionRate: 0.68,
              difficultySpike: [
                { levelId: 'level_8', completionRate: 0.32, difficultyScore: 8.5 }
              ]
            },
            cardBalance: {
              overpoweredCards: [
                { cardId: 'mega_blast', winRate: 0.95, usageRate: 0.85 }
              ],
              underpoweredCards: [
                { cardId: 'weak_heal', winRate: 0.25, usageRate: 0.05 }
              ]
            }
          }
        }
      ],
      examples: [
        {
          title: 'Game Balance Overview',
          description: 'Get overall game balance metrics',
          response: {
            levelDifficulty: { averageCompletionRate: 0.68 },
            cardBalance: { balanceScore: 7.2 }
          }
        }
      ]
    },
    {
      method: 'GET',
      path: '/api/v1/analytics/summary',
      summary: 'Get Analytics Summary',
      description: 'Retrieve a high-level summary of all analytics data for dashboard display.',
      responses: [
        {
          status: 200,
          description: 'Analytics summary',
          example: {
            overview: {
              totalPlayers: 12450,
              totalSessions: 45230,
              totalEvents: 892340,
              averageSessionDuration: 1800
            },
            trends: {
              playerGrowth: 0.15,
              engagementTrend: 0.08,
              retentionTrend: -0.02
            },
            topCards: [
              { cardId: 'fire_spell', usageCount: 1250, winRate: 0.72 }
            ]
          }
        }
      ],
      examples: [
        {
          title: 'Dashboard Summary',
          description: 'Get summary data for main dashboard',
          response: {
            overview: { totalPlayers: 12450, totalSessions: 45230 },
            trends: { playerGrowth: 0.15 }
          }
        }
      ]
    }
  ]
};

export default function ApiDocs() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('events');
  const [expandedEndpoints, setExpandedEndpoints] = useState<string[]>([]);

  const toggleEndpoint = (endpointId: string) => {
    setExpandedEndpoints(prev => 
      prev.includes(endpointId) 
        ? prev.filter(id => id !== endpointId)
        : [...prev, endpointId]
    );
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    toast.success('Copied to clipboard');
  };

  const getMethodColor = (method: string) => {
    switch (method) {
      case 'GET': return 'bg-green-100 text-green-800';
      case 'POST': return 'bg-blue-100 text-blue-800';
      case 'PUT': return 'bg-yellow-100 text-yellow-800';
      case 'DELETE': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'events': return <Gamepad2 className="h-5 w-5" />;
      case 'analytics': return <BarChart3 className="h-5 w-5" />;
      default: return <Database className="h-5 w-5" />;
    }
  };

  const filteredEndpoints = API_ENDPOINTS[selectedCategory]?.filter(endpoint =>
    endpoint.summary.toLowerCase().includes(searchTerm.toLowerCase()) ||
    endpoint.path.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">API Documentation</h1>
          <p className="text-gray-600">Complete reference for the Player Data Analytics API</p>
        </div>

        {/* Search and Navigation */}
        <Card className="bg-white shadow-lg mb-8">
          <CardContent className="p-6">
            <div className="flex flex-col md:flex-row gap-4">
              <div className="flex-1">
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                  <Input
                    placeholder="Search endpoints..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="pl-10"
                  />
                </div>
              </div>
              <Tabs value={selectedCategory} onValueChange={setSelectedCategory}>
                <TabsList>
                  <TabsTrigger value="events" className="flex items-center space-x-2">
                    <Gamepad2 className="h-4 w-4" />
                    <span>Events</span>
                  </TabsTrigger>
                  <TabsTrigger value="analytics" className="flex items-center space-x-2">
                    <BarChart3 className="h-4 w-4" />
                    <span>Analytics</span>
                  </TabsTrigger>
                </TabsList>
              </Tabs>
            </div>
          </CardContent>
        </Card>

        {/* API Overview */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card className="bg-white shadow-lg">
            <CardHeader>
              <CardTitle className="text-lg font-semibold text-gray-900 flex items-center">
                <Database className="h-5 w-5 mr-2" />
                Base URL
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="bg-gray-100 p-3 rounded-lg font-mono text-sm">
                https://api.coderogue.com
              </div>
            </CardContent>
          </Card>

          <Card className="bg-white shadow-lg">
            <CardHeader>
              <CardTitle className="text-lg font-semibold text-gray-900 flex items-center">
                <Code className="h-5 w-5 mr-2" />
                Authentication
              </CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-gray-600">API Key required in header:</p>
              <div className="bg-gray-100 p-3 rounded-lg font-mono text-sm mt-2">
                X-API-Key: your_api_key
              </div>
            </CardContent>
          </Card>

          <Card className="bg-white shadow-lg">
            <CardHeader>
              <CardTitle className="text-lg font-semibold text-gray-900 flex items-center">
                <Book className="h-5 w-5 mr-2" />
                Rate Limits
              </CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="text-sm text-gray-600 space-y-1">
                <li>Events: 1000/min</li>
                <li>Analytics: 100/min</li>
                <li>Burst: 10 req/sec</li>
              </ul>
            </CardContent>
          </Card>
        </div>

        {/* API Endpoints */}
        <div className="space-y-6">
          {filteredEndpoints.map((endpoint, index) => {
            const endpointId = `${selectedCategory}-${index}`;
            const isExpanded = expandedEndpoints.includes(endpointId);

            return (
              <Card key={endpointId} className="bg-white shadow-lg">
                <Collapsible open={isExpanded} onOpenChange={() => toggleEndpoint(endpointId)}>
                  <CollapsibleTrigger asChild>
                    <CardHeader className="cursor-pointer hover:bg-gray-50 transition-colors">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center space-x-4">
                          <Badge className={getMethodColor(endpoint.method)}>
                            {endpoint.method}
                          </Badge>
                          <div>
                            <CardTitle className="text-lg font-semibold text-gray-900">
                              {endpoint.summary}
                            </CardTitle>
                            <CardDescription className="font-mono text-sm">
                              {endpoint.path}
                            </CardDescription>
                          </div>
                        </div>
                        {isExpanded ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
                      </div>
                    </CardHeader>
                  </CollapsibleTrigger>

                  <CollapsibleContent>
                    <CardContent className="pt-0">
                      <div className="space-y-6">
                        {/* Description */}
                        <div>
                          <h3 className="font-semibold text-gray-900 mb-2">Description</h3>
                          <p className="text-gray-600">{endpoint.description}</p>
                        </div>

                        {/* Parameters */}
                        {endpoint.parameters && endpoint.parameters.length > 0 && (
                          <div>
                            <h3 className="font-semibold text-gray-900 mb-3">Parameters</h3>
                            <div className="overflow-x-auto">
                              <table className="w-full text-sm">
                                <thead>
                                  <tr className="border-b">
                                    <th className="text-left py-2 px-3 font-medium">Name</th>
                                    <th className="text-left py-2 px-3 font-medium">Type</th>
                                    <th className="text-left py-2 px-3 font-medium">Required</th>
                                    <th className="text-left py-2 px-3 font-medium">Description</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {endpoint.parameters.map((param, paramIndex) => (
                                    <tr key={paramIndex} className="border-b">
                                      <td className="py-2 px-3 font-mono">{param.name}</td>
                                      <td className="py-2 px-3">{param.type}</td>
                                      <td className="py-2 px-3">
                                        <Badge variant={param.required ? 'destructive' : 'secondary'}>
                                          {param.required ? 'Required' : 'Optional'}
                                        </Badge>
                                      </td>
                                      <td className="py-2 px-3 text-gray-600">{param.description}</td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </div>
                          </div>
                        )}

                        {/* Request Body */}
                        {endpoint.requestBody && (
                          <div>
                            <h3 className="font-semibold text-gray-900 mb-3">Request Body</h3>
                            <div className="bg-gray-100 p-4 rounded-lg">
                              <div className="flex items-center justify-between mb-2">
                                <span className="text-sm font-medium">Content-Type: {endpoint.requestBody.contentType}</span>
                                <Button
                                  size="sm"
                                  variant="ghost"
                                  onClick={() => copyToClipboard(JSON.stringify(endpoint.requestBody.example, null, 2))}
                                >
                                  <Copy className="h-4 w-4" />
                                </Button>
                              </div>
                              <pre className="text-sm overflow-x-auto">
                                <code>{JSON.stringify(endpoint.requestBody.example, null, 2)}</code>
                              </pre>
                            </div>
                          </div>
                        )}

                        {/* Responses */}
                        <div>
                          <h3 className="font-semibold text-gray-900 mb-3">Responses</h3>
                          <div className="space-y-3">
                            {endpoint.responses.map((response, responseIndex) => (
                              <div key={responseIndex} className="border rounded-lg p-4">
                                <div className="flex items-center justify-between mb-2">
                                  <div className="flex items-center space-x-2">
                                    <Badge className={response.status === 200 ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}>
                                      {response.status}
                                    </Badge>
                                    <span className="font-medium">{response.description}</span>
                                  </div>
                                  {response.example && (
                                    <Button
                                      size="sm"
                                      variant="ghost"
                                      onClick={() => copyToClipboard(JSON.stringify(response.example, null, 2))}
                                    >
                                      <Copy className="h-4 w-4" />
                                    </Button>
                                  )}
                                </div>
                                {response.example && (
                                  <div className="bg-gray-50 p-3 rounded">
                                    <pre className="text-sm overflow-x-auto">
                                      <code>{JSON.stringify(response.example, null, 2)}</code>
                                    </pre>
                                  </div>
                                )}
                              </div>
                            ))}
                          </div>
                        </div>

                        {/* Examples */}
                        {endpoint.examples && endpoint.examples.length > 0 && (
                          <div>
                            <h3 className="font-semibold text-gray-900 mb-3">Examples</h3>
                            <div className="space-y-4">
                              {endpoint.examples.map((example, exampleIndex) => (
                                <div key={exampleIndex} className="border rounded-lg p-4">
                                  <h4 className="font-medium text-gray-900 mb-1">{example.title}</h4>
                                  <p className="text-sm text-gray-600 mb-3">{example.description}</p>
                                  
                                  {example.request && (
                                    <div className="mb-3">
                                      <div className="flex items-center justify-between mb-2">
                                        <span className="text-sm font-medium">Request:</span>
                                        <Button
                                          size="sm"
                                          variant="ghost"
                                          onClick={() => copyToClipboard(JSON.stringify(example.request, null, 2))}
                                        >
                                          <Copy className="h-4 w-4" />
                                        </Button>
                                      </div>
                                      <div className="bg-blue-50 p-3 rounded">
                                        <pre className="text-sm overflow-x-auto">
                                          <code>{JSON.stringify(example.request, null, 2)}</code>
                                        </pre>
                                      </div>
                                    </div>
                                  )}
                                  
                                  <div>
                                    <div className="flex items-center justify-between mb-2">
                                      <span className="text-sm font-medium">Response:</span>
                                      <Button
                                        size="sm"
                                        variant="ghost"
                                        onClick={() => copyToClipboard(JSON.stringify(example.response, null, 2))}
                                      >
                                        <Copy className="h-4 w-4" />
                                      </Button>
                                    </div>
                                    <div className="bg-green-50 p-3 rounded">
                                      <pre className="text-sm overflow-x-auto">
                                        <code>{JSON.stringify(example.response, null, 2)}</code>
                                      </pre>
                                    </div>
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}

                        {/* Try It Out */}
                        <div className="pt-4 border-t">
                          <Button className="bg-blue-600 hover:bg-blue-700">
                            <Play className="h-4 w-4 mr-2" />
                            Try It Out
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </CollapsibleContent>
                </Collapsible>
              </Card>
            );
          })}
        </div>

        {filteredEndpoints.length === 0 && (
          <Card className="bg-white shadow-lg">
            <CardContent className="text-center py-12">
              <Search className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-500">No endpoints found matching your search</p>
              <p className="text-sm text-gray-400">Try adjusting your search terms</p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}