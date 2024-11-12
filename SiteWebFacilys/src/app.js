const express = require("express");
const session = require("express-session");
const cookieParser = require("cookie-parser");
const path = require("path");
const bodyParser = require("body-parser");
const csrf = require("csurf");
const dotenv = require("dotenv");
const cookieConfig = require("./config/cookie-config");
const crypto = require("crypto");
const fileUpload = require("express-fileupload");
// Charger les variables d'environnement
dotenv.config();

// Initialiser l'application Express
const app = express();
const sessionSecret = crypto
  .createHash("sha256")
  .update("Facilys-2024-session")
  .digest("base64");

// Configuration du moteur de vue EJS
app.set("view engine", "ejs");
app.set("views", path.join(__dirname, "views"));
app.use(express.static(path.join(__dirname, "..", "public")));

// Middleware
app.use(cookieParser(cookieConfig.cookieSecret));
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

app.use(
  session({
    secret: sessionSecret,
    resave: false,
    saveUninitialized: true,
    cookie: { secure: false }, // Mettez à true en production avec HTTPS
  })
);

// Configuration de express-fileupload
app.use(
  fileUpload({
    createParentPath: true,
    limits: {
      fileSize: 5 * 1024 * 1024, // limite à 5MB
    },
  })
);

// Configuration de CSRF
const csrfProtection = csrf({ cookie: true });
app.use(csrfProtection);
// Middleware pour générer le token CSRF
/*app.use((req, res, next) => {
  // Générer le token CSRF si ce n'est pas déjà fait
  if (!req.session.csrfToken) {
    req.session.csrfToken = req.csrfToken();
  }
  next();
});*/

// Middleware pour générer le token CSRF dans les vues
app.use((req, res, next) => {
  res.locals.csrfToken = req.csrfToken(); // Génère et injecte le token dans les vues

  if (req.is("multipart/form-data")) {
    return next();
  }
  csrfProtection(req, res, next);
});

app.use((req, res, next) => {
  res.locals.session = req.session;
  next();
});

app.use((req, res, next) => {
  console.log("Request URL:", req.originalUrl);
  console.log("Request Method:", req.method);
  console.log("Cookies:", req.cookies);
  next();
});

// Router principal
const navigationRoutes = require("./routes/route.navigation");
app.use("/", navigationRoutes);

const subscribeRoute = require("./routes/route.subscribe");
app.use("/subscribe", subscribeRoute);

// API
const navigationAPI = require("./routes/route.api");
app.use("/api", navigationAPI);

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
const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`Serveur démarré sur le port ${PORT}`);
});

module.exports = app;