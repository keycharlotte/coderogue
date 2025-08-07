/**
 * local server entry file, for local development
 */
import dotenv from 'dotenv';
import createApp from './app.js';

// Load environment variables
dotenv.config();

/**
 * Start the server
 */
async function startServer() {
  try {
    const app = await createApp();
    const PORT = process.env.PORT || 3000;
    const server = app.listen(PORT, () => {
      console.log(`🚀 Server running on port ${PORT}`);
      console.log(`📊 Analytics API available at http://localhost:${PORT}/api/v1/analytics`);
      console.log(`📋 API documentation at http://localhost:${PORT}/api/docs`);
      console.log(`❤️  Health check at http://localhost:${PORT}/health`);
    });

    return server;
  } catch (error) {
    console.error('Failed to start server:', error);
    process.exit(1);
  }
}

const server = await startServer();

/**
 * close server
 */
process.on('SIGTERM', () => {
  console.log('SIGTERM signal received');
  server.close(() => {
    console.log('Server closed');
    process.exit(0);
  });
});

process.on('SIGINT', () => {
  console.log('SIGINT signal received');
  server.close(() => {
    console.log('Server closed');
    process.exit(0);
  });
});

export default server;