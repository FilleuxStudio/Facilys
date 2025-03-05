const express = require('express');
const session = require('express-session');
const fileUpload = require('express-fileupload');
const cookieParser = require('cookie-parser');
const path = require('path');
const crypto = require('crypto');
//const bodyParser = require("body-parser");
const dotenv = require('dotenv');
const helmet = require('helmet');
const compression = require('compression');
const { doubleCsrf } = require('csrf-csrf');

const app = express();

const envPath = path.join(__dirname, '..', '.env');
dotenv.config({ path: envPath });

const port = process.env.PORT || 8056;
const hostname = process.env.HOST ||'localhost';
const isProduction = process.env.NODE_ENV === 'production';
const CSRF_SECRET = process.env.CSRF_SECRET || crypto.randomBytes(32).toString('hex');
const sessionSecret = crypto.createHash("sha256").update("Facilys-2025-session").digest("base64");

console.log(isProduction);
console.log(process.env.CSRF_SECRET);

// Configuration du moteur de vue EJS
app.set("view engine", "ejs");
app.set("views", path.join(__dirname, "..", "views"));
app.use(express.static(path.join(__dirname, "..", "public")));

// Middleware
app.use(cookieParser());
app.use(helmet());
app.use(compression());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
// Configuration de express-fileupload
app.use(
  fileUpload({
    createParentPath: true,
    limits: {
      fileSize: 5 * 1024 * 1024, // limite à 5MB
    },
  })
);

app.use(session({
  secret: sessionSecret,
  resave: false,
  saveUninitialized: true,
  cookie: { 
    secure: isProduction, // Mettez à true si vous utilisez HTTPS
    sameSite: isProduction ? 'None' : 'Lax', // Utilisez 'None' pour les cookies tiers en production
    httpOnly: true,
    maxAge: 24 * 60 * 60 * 1000 // 24 heures
  }
}));

const { generateToken, doubleCsrfProtection } = doubleCsrf({
  getSecret: () => CSRF_SECRET,
  cookieName: "facylis.x-csrf-token",
  cookieOptions: {
    httpOnly: true,
    sameSite: "Lax",
    secure: isProduction,
    maxAge: 24 * 60 * 60 * 1000 // 24 heures
  },
  size: 64,
  ignoredMethods: ["GET", "HEAD", "OPTIONS"],
  getTokenFromRequest: (req) => {   
    // Vérifiez si le formulaire est multipart et récupérez le token depuis le corps
    if (req.headers['content-type'].startsWith('multipart/form-data')) {
      return req.body._csrf; // Priorité au token dans le corps
    }
    // Pour les autres types de requêtes, essayez d'abord le header, puis le cookie
    return req.body._csrf || req.headers["x-csrf-token"] || req.cookies["facylis.x-csrf-token"];
  }
});

app.use(doubleCsrfProtection);


// Route pour générer et envoyer le token CSRF
app.get('/csrf-token', (req, res) => {
  const csrfToken = generateToken(req, res);
  res.json({ csrfToken });
});

// Middleware pour rendre le token CSRF disponible dans les vues
app.use((req, res, next) => {
  console.log(CSRF_SECRET);
  res.header("Access-Control-Allow-Origin", "localhost:3100");
  res.header("Access-Control-Allow-Credentials", "true");
  res.locals.csrfToken = req.csrfToken(); // Utilisation de req.csrfToken() après le middleware doubleCsrf Protection
  next();
});

//log pour le dev
app.use((req, res, next) => {
  console.log("Request URL:", req.originalUrl);
  console.log("Request Method:", req.method);
  console.log("Cookies:", req.cookies);
  next();
});

const navigationWeb = require("./routes/route.web");
const navigationAPI = require("./routes/route.api");
const errorHandler = require('./middleware/errorHandler');


// Router principal
app.use("/", navigationWeb);
// Routes API
app.use("/api", navigationAPI);

// Gestion des erreurs
app.use(errorHandler);

// Gestion des erreurs 404
app.use((req, res, next) => {
  res.status(404).render("404", { title: "Page non trouvée" });
});

// Gestion des erreurs globales
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).render("error", { title: "Erreur", error: err });
});

// Démarrage du serveur
app.listen(port, hostname, () => {
  console.log(`Server running on port ${port}`);
});

module.exports =  app;  // Exporte correctement les deux éléments