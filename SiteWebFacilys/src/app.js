const express = require('express');
const path = require('path');
const bodyParser = require('body-parser');
const dotenv = require('dotenv');

// Charger les variables d'environnement
dotenv.config();

// Importer les routes
//const clientRoutes = require('./routes/clientRoutes');

// Initialiser l'application Express
const app = express();

// Configuration du moteur de vue EJS
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

// Middleware
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(express.static(path.join(__dirname, '..', 'public')));

// Routes
//app.use('/clients', clientRoutes);

// Route de base
app.get('/', (req, res) => {
  const now =  new Date().getFullYear();
  res.render('home', { currentDateTime: now, title: 'Accueil' })
});

const navigationRoutes = require('./routes/navigation');
app.use('/', navigationRoutes);

const subscribeRoute = require('./routes/subscribe');
app.use('/subscribe', subscribeRoute);


// Gestion des erreurs 404
app.use((req, res, next) => {
  res.status(404).render('404', { title: 'Page non trouvée' });
});

// Gestion des erreurs globales
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).render('error', { title: 'Erreur', error: err });
});

// Démarrage du serveur
const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`Serveur démarré sur le port ${PORT}`);
});

module.exports = app;
