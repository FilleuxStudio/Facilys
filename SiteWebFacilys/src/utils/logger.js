const fs = require('fs');
const { createLogger, format, transports } = require('winston');
require('winston-daily-rotate-file');
const path = require('path');

// Définir le dossier de logs
const logDirectory = path.join(__dirname, '../../../logs');

// Créer le dossier de logs s'il n'existe pas
try {
  fs.mkdirSync(logDirectory, { recursive: true });
} catch (err) {
  console.error('Erreur lors de la création du dossier de logs:', err);
}

// Format commun pour les logs
const logFormat = format.combine(
  format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
  format.printf(({ timestamp, level, message }) => `${timestamp} [${level}]: ${message}`)
);

// Transport pour les logs de niveau 'error'
const errorTransport = new transports.DailyRotateFile({
  filename: path.join(logDirectory, 'error-%DATE%.log'),
  datePattern: 'YYYY-MM-DD',
  zippedArchive: true,
  maxFiles: '3d',
  level: 'error',
  format: logFormat,
});

// Transport pour les logs de niveau 'warn'
const warnTransport = new transports.DailyRotateFile({
  filename: path.join(logDirectory, 'warn-%DATE%.log'),
  datePattern: 'YYYY-MM-DD',
  zippedArchive: true,
  maxFiles: '3d',
  level: 'warn',
  format: logFormat,
});

// Transport pour les logs de niveau 'info'
const infoTransport = new transports.DailyRotateFile({
  filename: path.join(logDirectory, 'info-%DATE%.log'),
  datePattern: 'YYYY-MM-DD',
  zippedArchive: true,
  maxFiles: '3d',
  level: 'info',
  format: logFormat,
});

// Transport pour la console
const consoleTransport = new transports.Console({
  format: format.combine(
    format.colorize(),
    format.simple()
  )
});

// Création du logger
const logger = createLogger({
  level: 'info',
  transports: [
    errorTransport,
    warnTransport,
    infoTransport,
    consoleTransport
  ],
  exceptionHandlers: [
    new transports.File({ filename: path.join(logDirectory, 'exceptions.log') })
  ],
  rejectionHandlers: [
    new transports.File({ filename: path.join(logDirectory, 'rejections.log') })
  ]
});

module.exports = logger;