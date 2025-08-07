import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Download, FileText, Database, Calendar, Settings, CheckCircle, Clock, AlertCircle } from 'lucide-react';
import { toast } from 'sonner';

interface ExportJob {
  id: string;
  type: 'csv' | 'json' | 'xlsx';
  category: 'players' | 'sessions' | 'events' | 'cards' | 'analytics';
  status: 'pending' | 'processing' | 'completed' | 'failed';
  createdAt: string;
  completedAt?: string;
  downloadUrl?: string;
  fileSize?: number;
  recordCount?: number;
}

interface ExportConfig {
  type: 'csv' | 'json' | 'xlsx';
  category: 'players' | 'sessions' | 'events' | 'cards' | 'analytics';
  dateRange: {
    start: string;
    end: string;
  };
  fields: string[];
  filters: Record<string, any>;
}

const EXPORT_CATEGORIES = {
  players: {
    label: 'Player Data',
    description: 'Player profiles, statistics, and behavior data',
    fields: ['playerId', 'createdAt', 'lastActiveAt', 'totalSessions', 'totalPlayTime', 'level', 'achievements']
  },
  sessions: {
    label: 'Game Sessions',
    description: 'Individual game session records',
    fields: ['sessionId', 'playerId', 'startTime', 'endTime', 'duration', 'outcome', 'score']
  },
  events: {
    label: 'Game Events',
    description: 'Detailed game event logs',
    fields: ['eventId', 'sessionId', 'playerId', 'eventType', 'timestamp', 'data']
  },
  cards: {
    label: 'Card Usage',
    description: 'Card usage statistics and performance',
    fields: ['cardId', 'usageCount', 'winRate', 'averagePosition', 'combinations']
  },
  analytics: {
    label: 'Analytics Summary',
    description: 'Pre-computed analytics and insights',
    fields: ['metric', 'value', 'period', 'trend', 'comparison']
  }
};

export default function Export() {
  const [exportJobs, setExportJobs] = useState<ExportJob[]>([]);
  const [loading, setLoading] = useState(false);
  const [config, setConfig] = useState<ExportConfig>({
    type: 'csv',
    category: 'players',
    dateRange: {
      start: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      end: new Date().toISOString().split('T')[0]
    },
    fields: EXPORT_CATEGORIES.players.fields,
    filters: {}
  });

  useEffect(() => {
    fetchExportJobs();
  }, []);

  useEffect(() => {
    setConfig(prev => ({
      ...prev,
      fields: EXPORT_CATEGORIES[config.category].fields
    }));
  }, [config.category]);

  const fetchExportJobs = async () => {
    try {
      // Mock data for demonstration
      const mockJobs: ExportJob[] = [
        {
          id: '1',
          type: 'csv',
          category: 'players',
          status: 'completed',
          createdAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
          completedAt: new Date(Date.now() - 1.5 * 60 * 60 * 1000).toISOString(),
          downloadUrl: '/exports/players_2024.csv',
          fileSize: 2048576,
          recordCount: 15420
        },
        {
          id: '2',
          type: 'json',
          category: 'sessions',
          status: 'processing',
          createdAt: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
          recordCount: 45230
        },
        {
          id: '3',
          type: 'xlsx',
          category: 'analytics',
          status: 'failed',
          createdAt: new Date(Date.now() - 4 * 60 * 60 * 1000).toISOString()
        }
      ];
      setExportJobs(mockJobs);
    } catch (error) {
      toast.error('Failed to fetch export jobs');
    }
  };

  const createExport = async () => {
    try {
      setLoading(true);
      
      // Mock API call
      const newJob: ExportJob = {
        id: Date.now().toString(),
        type: config.type,
        category: config.category,
        status: 'pending',
        createdAt: new Date().toISOString(),
        recordCount: Math.floor(Math.random() * 50000) + 1000
      };
      
      setExportJobs(prev => [newJob, ...prev]);
      toast.success('Export job created successfully');
      
      // Simulate processing
      setTimeout(() => {
        setExportJobs(prev => prev.map(job => 
          job.id === newJob.id 
            ? { ...job, status: 'processing' }
            : job
        ));
      }, 1000);
      
      setTimeout(() => {
        setExportJobs(prev => prev.map(job => 
          job.id === newJob.id 
            ? { 
                ...job, 
                status: 'completed',
                completedAt: new Date().toISOString(),
                downloadUrl: `/exports/${config.category}_${Date.now()}.${config.type}`,
                fileSize: Math.floor(Math.random() * 5000000) + 100000
              }
            : job
        ));
        toast.success('Export completed successfully');
      }, 5000);
      
    } catch (error) {
      toast.error('Failed to create export');
    } finally {
      setLoading(false);
    }
  };

  const downloadExport = (job: ExportJob) => {
    if (job.downloadUrl) {
      // In a real app, this would trigger the actual download
      toast.success(`Downloading ${job.category} data...`);
    }
  };

  const getStatusIcon = (status: ExportJob['status']) => {
    switch (status) {
      case 'completed':
        return <CheckCircle className="h-4 w-4 text-green-600" />;
      case 'processing':
        return <Clock className="h-4 w-4 text-blue-600" />;
      case 'failed':
        return <AlertCircle className="h-4 w-4 text-red-600" />;
      default:
        return <Clock className="h-4 w-4 text-gray-600" />;
    }
  };

  const getStatusColor = (status: ExportJob['status']) => {
    switch (status) {
      case 'completed':
        return 'bg-green-100 text-green-800';
      case 'processing':
        return 'bg-blue-100 text-blue-800';
      case 'failed':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const formatFileSize = (bytes: number) => {
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 to-teal-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Data Export Center</h1>
          <p className="text-gray-600">Export and download game analytics data in various formats</p>
        </div>

        {/* Export Configuration */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900 flex items-center">
              <Settings className="h-5 w-5 mr-2" />
              Export Configuration
            </CardTitle>
            <CardDescription>Configure your data export settings</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              {/* Data Category */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Data Category</label>
                <Select value={config.category} onValueChange={(value: any) => setConfig(prev => ({ ...prev, category: value }))}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Object.entries(EXPORT_CATEGORIES).map(([key, category]) => (
                      <SelectItem key={key} value={key}>
                        {category.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <p className="text-xs text-gray-500 mt-1">
                  {EXPORT_CATEGORIES[config.category].description}
                </p>
              </div>

              {/* Export Format */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Export Format</label>
                <Select value={config.type} onValueChange={(value: any) => setConfig(prev => ({ ...prev, type: value }))}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="csv">CSV (Comma Separated)</SelectItem>
                    <SelectItem value="json">JSON (JavaScript Object)</SelectItem>
                    <SelectItem value="xlsx">XLSX (Excel Spreadsheet)</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* Date Range */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Start Date</label>
                <Input
                  type="date"
                  value={config.dateRange.start}
                  onChange={(e) => setConfig(prev => ({
                    ...prev,
                    dateRange: { ...prev.dateRange, start: e.target.value }
                  }))}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">End Date</label>
                <Input
                  type="date"
                  value={config.dateRange.end}
                  onChange={(e) => setConfig(prev => ({
                    ...prev,
                    dateRange: { ...prev.dateRange, end: e.target.value }
                  }))}
                />
              </div>
            </div>

            {/* Field Selection */}
            <div className="mt-6">
              <label className="block text-sm font-medium text-gray-700 mb-3">Fields to Export</label>
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
                {EXPORT_CATEGORIES[config.category].fields.map((field) => (
                  <div key={field} className="flex items-center space-x-2">
                    <Checkbox
                      id={field}
                      checked={config.fields.includes(field)}
                      onCheckedChange={(checked) => {
                        if (checked) {
                          setConfig(prev => ({
                            ...prev,
                            fields: [...prev.fields, field]
                          }));
                        } else {
                          setConfig(prev => ({
                            ...prev,
                            fields: prev.fields.filter(f => f !== field)
                          }));
                        }
                      }}
                    />
                    <label htmlFor={field} className="text-sm text-gray-700 cursor-pointer">
                      {field}
                    </label>
                  </div>
                ))}
              </div>
            </div>

            {/* Export Button */}
            <div className="mt-6 flex justify-end">
              <Button 
                onClick={createExport} 
                disabled={loading || config.fields.length === 0}
                className="bg-green-600 hover:bg-green-700"
              >
                <Download className="h-4 w-4 mr-2" />
                {loading ? 'Creating Export...' : 'Create Export'}
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Export Jobs */}
        <Card className="bg-white shadow-lg">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900 flex items-center">
              <Database className="h-5 w-5 mr-2" />
              Export History
            </CardTitle>
            <CardDescription>Recent export jobs and downloads</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {exportJobs.map((job) => (
                <div key={job.id} className="p-4 bg-gray-50 rounded-lg border">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-4">
                      <div className="flex items-center space-x-2">
                        {getStatusIcon(job.status)}
                        <Badge className={getStatusColor(job.status)}>
                          {job.status.toUpperCase()}
                        </Badge>
                      </div>
                      
                      <div>
                        <h3 className="font-semibold text-gray-900">
                          {EXPORT_CATEGORIES[job.category].label} ({job.type.toUpperCase()})
                        </h3>
                        <p className="text-sm text-gray-600">
                          Created: {formatDate(job.createdAt)}
                          {job.completedAt && ` • Completed: ${formatDate(job.completedAt)}`}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center space-x-4">
                      <div className="text-right">
                        {job.recordCount && (
                          <p className="text-sm font-medium text-gray-900">
                            {job.recordCount.toLocaleString()} records
                          </p>
                        )}
                        {job.fileSize && (
                          <p className="text-xs text-gray-500">
                            {formatFileSize(job.fileSize)}
                          </p>
                        )}
                      </div>
                      
                      {job.status === 'completed' && job.downloadUrl && (
                        <Button
                          onClick={() => downloadExport(job)}
                          size="sm"
                          className="bg-blue-600 hover:bg-blue-700"
                        >
                          <Download className="h-4 w-4 mr-1" />
                          Download
                        </Button>
                      )}
                    </div>
                  </div>
                  
                  {job.status === 'processing' && (
                    <div className="mt-3">
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div className="bg-blue-600 h-2 rounded-full animate-pulse" style={{ width: '60%' }}></div>
                      </div>
                      <p className="text-xs text-gray-500 mt-1">Processing export...</p>
                    </div>
                  )}
                  
                  {job.status === 'failed' && (
                    <div className="mt-3 p-2 bg-red-50 border border-red-200 rounded">
                      <p className="text-sm text-red-600">Export failed. Please try again or contact support.</p>
                    </div>
                  )}
                </div>
              ))}
              
              {exportJobs.length === 0 && (
                <div className="text-center py-12">
                  <FileText className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                  <p className="text-gray-500">No export jobs yet</p>
                  <p className="text-sm text-gray-400">Create your first export to get started</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Export Guidelines */}
        <Card className="bg-white shadow-lg mt-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Export Guidelines</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h3 className="font-semibold text-gray-900 mb-2">File Formats</h3>
                <ul className="text-sm text-gray-600 space-y-1">
                  <li><strong>CSV:</strong> Best for spreadsheet applications and data analysis</li>
                  <li><strong>JSON:</strong> Ideal for programmatic access and API integration</li>
                  <li><strong>XLSX:</strong> Excel format with advanced formatting support</li>
                </ul>
              </div>
              
              <div>
                <h3 className="font-semibold text-gray-900 mb-2">Data Retention</h3>
                <ul className="text-sm text-gray-600 space-y-1">
                  <li>Export files are available for download for 7 days</li>
                  <li>Large exports (&gt;100MB) may take several minutes to process</li>
                  <li>Maximum export size is limited to 1GB per job</li>
                </ul>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}