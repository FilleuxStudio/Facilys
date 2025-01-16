// middleware/errorHandler.js

const errorHandler = (err, req, res, next) => {
    // Déterminer le statut de l'erreur
    const statusCode = res.statusCode === 200 ? 500 : res.statusCode;
    res.status(statusCode);
  
    // Préparer la réponse d'erreur
    const errorResponse = {
      message: err.message,
      stack: process.env.NODE_ENV === 'production' ? '🥞' : err.stack,
    };
  
    // Logger l'erreur (vous pouvez remplacer ceci par un logger plus sophistiqué)
    console.error(`[Error] ${err.message}`);
  
    // Envoyer la réponse d'erreur
    res.json(errorResponse);
  };
  
  module.exports = errorHandler;
  