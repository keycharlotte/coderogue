import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { BarChart } from '@/components/charts/BarChart';
import { LineChart } from '@/components/charts/LineChart';
import { PieChart } from '@/components/charts/PieChart';
import { Search, Users, Clock, TrendingUp, Target, Activity, UserCheck, Filter } from 'lucide-react';
import { toast } from 'sonner';

interface PlayerRetention {
  day1: number;
  day7: number;
  day30: number;
}

interface SessionDuration {
  average: number;
  median: number;
  distribution: { range: string; count: number }[];
}

interface PlayerProgression {
  playerId: string;
  currentLevel: number;
  totalLevels: number;
  completionRate: number;
  averageAttempts: number;
}

interface EngagementPattern {
  timeOfDay: string;
  playerCount: number;
  averageSessionLength: number;
}

interface PlayerSegment {
  segment: string;
  playerCount: number;
  characteristics: string[];
}

interface ChurnAnalysis {
  riskLevel: 'low' | 'medium' | 'high';
  playerCount: number;
  factors: string[];
}

interface PlayerBehaviorResponse {
  retention: PlayerRetention;
  sessionDuration: SessionDuration;
  progression: PlayerProgression[];
  engagement: EngagementPattern[];
  segments: PlayerSegment[];
  churn: ChurnAnalysis[];
  totalPlayers: number;
  activePlayers: number;
}

export default function Players() {
  const [analytics, setAnalytics] = useState<PlayerBehaviorResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [dateRange, setDateRange] = useState('30');
  const [selectedSegment, setSelectedSegment] = useState('all');

  useEffect(() => {
    fetchPlayerAnalytics();
  }, [dateRange]);

  const fetchPlayerAnalytics = async () => {
    try {
      setLoading(true);
      const endDate = new Date();
      const startDate = new Date();
      startDate.setDate(endDate.getDate() - parseInt(dateRange));
      
      const params = new URLSearchParams({
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString()
      });
      
      const response = await fetch(`/api/v1/analytics/players/behavior?${params}`);
      if (!response.ok) {
        throw new Error('Failed to fetch player analytics');
      }
      const data = await response.json();
      setAnalytics(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const filteredProgression = analytics?.progression?.filter(player =>
    selectedSegment === 'all' || player.playerId.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  const getRetentionColor = (rate: number) => {
    if (rate >= 0.7) return 'bg-green-100 text-green-800';
    if (rate >= 0.4) return 'bg-yellow-100 text-yellow-800';
    return 'bg-red-100 text-red-800';
  };

  const getChurnRiskColor = (risk: string) => {
    switch (risk) {
      case 'low': return 'bg-green-100 text-green-800';
      case 'medium': return 'bg-yellow-100 text-yellow-800';
      case 'high': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const formatDuration = (minutes: number) => {
    const hours = Math.floor(minutes / 60);
    const mins = Math.round(minutes % 60);
    return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            <p className="mt-4 text-gray-600">Loading player analytics...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="text-red-600 text-xl mb-4">Error loading data</div>
            <p className="text-gray-600 mb-4">{error}</p>
            <Button onClick={fetchPlayerAnalytics}>Retry</Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Player Behavior Analytics</h1>
          <p className="text-gray-600">Analyze player retention, engagement patterns, and progression</p>
        </div>

        {/* Filters */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Filters &amp; Search</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <Input
                  placeholder="Search players..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
              
              <Select value={selectedSegment} onValueChange={setSelectedSegment}>
                <SelectTrigger>
                  <SelectValue placeholder="Player segment" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Players</SelectItem>
                  {analytics?.segments?.map((segment) => (
                    <SelectItem key={segment.segment} value={segment.segment}>
                      {segment.segment} ({segment.playerCount})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              
              <Select value={dateRange} onValueChange={setDateRange}>
                <SelectTrigger>
                  <SelectValue placeholder="Date range" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="7">Last 7 days</SelectItem>
                  <SelectItem value="30">Last 30 days</SelectItem>
                  <SelectItem value="90">Last 90 days</SelectItem>
                </SelectContent>
              </Select>
              
              <Button onClick={fetchPlayerAnalytics} className="bg-blue-600 hover:bg-blue-700">
                Refresh Data
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Key Metrics */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <Card className="bg-white shadow-lg">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600">Total Players</CardTitle>
              <Users className="h-4 w-4 text-blue-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900">{analytics?.totalPlayers?.toLocaleString() || 0}</div>
              <p className="text-xs text-gray-500 mt-1">Registered players</p>
            </CardContent>
          </Card>

          <Card className="bg-white shadow-lg">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600">Active Players</CardTitle>
              <UserCheck className="h-4 w-4 text-green-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900">{analytics?.activePlayers?.toLocaleString() || 0}</div>
              <p className="text-xs text-gray-500 mt-1">
                {analytics?.totalPlayers ? 
                  `${((analytics.activePlayers / analytics.totalPlayers) * 100).toFixed(1)}% of total`
                  : 'In selected period'
                }
              </p>
            </CardContent>
          </Card>

          <Card className="bg-white shadow-lg">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600">Avg Session</CardTitle>
              <Clock className="h-4 w-4 text-purple-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900">
                {analytics?.sessionDuration ? formatDuration(analytics.sessionDuration.average) : '0m'}
              </div>
              <p className="text-xs text-gray-500 mt-1">
                Median: {analytics?.sessionDuration ? formatDuration(analytics.sessionDuration.median) : '0m'}
              </p>
            </CardContent>
          </Card>

          <Card className="bg-white shadow-lg">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600">Day 1 Retention</CardTitle>
              <TrendingUp className="h-4 w-4 text-orange-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900">
                {analytics?.retention ? (analytics.retention.day1 * 100).toFixed(1) + '%' : '0%'}
              </div>
              <p className="text-xs text-gray-500 mt-1">Players returning next day</p>
            </CardContent>
          </Card>
        </div>

        {/* Retention Analysis */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Retention Analysis</CardTitle>
            <CardDescription>Player retention rates over time</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div className="text-center">
                <div className="text-3xl font-bold text-gray-900 mb-2">
                  {analytics?.retention ? (analytics.retention.day1 * 100).toFixed(1) + '%' : '0%'}
                </div>
                <Badge className={getRetentionColor(analytics?.retention?.day1 || 0)}>
                  Day 1 Retention
                </Badge>
                <p className="text-sm text-gray-500 mt-2">Players returning after 1 day</p>
              </div>
              
              <div className="text-center">
                <div className="text-3xl font-bold text-gray-900 mb-2">
                  {analytics?.retention ? (analytics.retention.day7 * 100).toFixed(1) + '%' : '0%'}
                </div>
                <Badge className={getRetentionColor(analytics?.retention?.day7 || 0)}>
                  Day 7 Retention
                </Badge>
                <p className="text-sm text-gray-500 mt-2">Players returning after 1 week</p>
              </div>
              
              <div className="text-center">
                <div className="text-3xl font-bold text-gray-900 mb-2">
                  {analytics?.retention ? (analytics.retention.day30 * 100).toFixed(1) + '%' : '0%'}
                </div>
                <Badge className={getRetentionColor(analytics?.retention?.day30 || 0)}>
                  Day 30 Retention
                </Badge>
                <p className="text-sm text-gray-500 mt-2">Players returning after 1 month</p>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Player Segments */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
          <Card className="bg-white shadow-lg">
            <CardHeader>
              <CardTitle className="text-xl font-semibold text-gray-900">Player Segments</CardTitle>
              <CardDescription>Player categorization based on behavior</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {analytics?.segments?.map((segment, index) => (
                  <div key={index} className="p-4 bg-gray-50 rounded-lg">
                    <div className="flex items-center justify-between mb-2">
                      <h3 className="font-semibold text-gray-900">{segment.segment}</h3>
                      <Badge variant="outline">{segment.playerCount} players</Badge>
                    </div>
                    <div className="space-y-1">
                      {segment.characteristics.map((char, charIndex) => (
                        <div key={charIndex} className="text-sm text-gray-600 flex items-center">
                          <Target className="h-3 w-3 mr-2 text-blue-500" />
                          {char}
                        </div>
                      ))}
                    </div>
                  </div>
                )) || (
                  <div className="text-center py-8 text-gray-500">
                    No segment data available
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          <Card className="bg-white shadow-lg">
            <CardHeader>
              <CardTitle className="text-xl font-semibold text-gray-900">Churn Risk Analysis</CardTitle>
              <CardDescription>Players at risk of leaving the game</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {analytics?.churn?.map((churnGroup, index) => (
                  <div key={index} className="p-4 bg-gray-50 rounded-lg">
                    <div className="flex items-center justify-between mb-2">
                      <Badge className={getChurnRiskColor(churnGroup.riskLevel)}>
                        {churnGroup.riskLevel.toUpperCase()} RISK
                      </Badge>
                      <span className="font-semibold text-gray-900">{churnGroup.playerCount} players</span>
                    </div>
                    <div className="space-y-1">
                      {churnGroup.factors.map((factor, factorIndex) => (
                        <div key={factorIndex} className="text-sm text-gray-600 flex items-center">
                          <Activity className="h-3 w-3 mr-2 text-red-500" />
                          {factor}
                        </div>
                      ))}
                    </div>
                  </div>
                )) || (
                  <div className="text-center py-8 text-gray-500">
                    No churn analysis available
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Engagement Patterns */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Engagement Patterns</CardTitle>
            <CardDescription>Player activity throughout the day</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
              {analytics?.engagement?.map((pattern, index) => (
                <div key={index} className="text-center p-4 bg-gray-50 rounded-lg">
                  <div className="text-lg font-bold text-gray-900 mb-1">{pattern.timeOfDay}</div>
                  <div className="text-sm text-blue-600 font-semibold mb-1">{pattern.playerCount} players</div>
                  <div className="text-xs text-gray-500">{formatDuration(pattern.averageSessionLength)}</div>
                </div>
              )) || (
                <div className="col-span-full text-center py-8 text-gray-500">
                  No engagement data available
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Session Duration Distribution */}
        <Card className="bg-white shadow-lg">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Session Duration Distribution</CardTitle>
            <CardDescription>How long players typically play</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {analytics?.sessionDuration?.distribution?.map((dist, index) => (
                <div key={index} className="text-center p-4 bg-gray-50 rounded-lg">
                  <div className="text-lg font-bold text-gray-900 mb-1">{dist.range}</div>
                  <div className="text-sm text-purple-600 font-semibold">{dist.count} sessions</div>
                  <div className="text-xs text-gray-500 mt-1">
                    {analytics.sessionDuration.distribution.reduce((sum, d) => sum + d.count, 0) > 0 ?
                      ((dist.count / analytics.sessionDuration.distribution.reduce((sum, d) => sum + d.count, 0)) * 100).toFixed(1) + '%'
                      : '0%'
                    }
                  </div>
                </div>
              )) || (
                <div className="col-span-full text-center py-8 text-gray-500">
                  No session duration data available
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}