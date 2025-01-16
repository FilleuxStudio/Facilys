const express = require("express");
const router = express.Router();
const authenticationController = require("../controllers/controller.authentication");
const accountController = require("../controllers/controller.account");

const now = new Date().getFullYear();

router.get("/", async (req, res) => {
  const now = new Date().getFullYear();
  //const productShop = await managerController.managerGetAllProducts();
  res.render("home", { currentDateTime: now, currentRoute: req.path });
  //res.render('home', { currentDateTime: now, title: 'Accueil', products: productShop})
});

// Route pour la page "À propos de nous"
router.get("/about", async (req, res) => {
  res.render("about", { currentDateTime: now, currentRoute: req.path }); // Affiche la page about.ejs
});

// Route pour la page "contact"
router.get("/contact", (req, res) => {
  res.render("contact", { currentDateTime: now, currentRoute: req.path });
});

// Route pour la page "/term-of-use"
router.get("/term-of-use", (req, res) => {
  res.render("term-of-use", { currentDateTime: now, currentRoute: req.path });
});


// Route pour la page login et register
router.get("/login", (req, res) => {
  const csrfToken = req.csrfToken(); // Générer le token CSRF
  const message = req.query.message || null;
  res.render("login", { currentDateTime: now, currentRoute: req.path, message: message, csrfToken });
});

router.post("/login", authenticationController.login);

// Route pour la page login et register
router.get("/register", (req, res) => {
  const csrfToken = req.csrfToken(true);
  const message = req.query.message || null;
  res.render("register", { currentDateTime: now, currentRoute: req.path, message: message, csrfToken });
});

router.post("/register", authenticationController.register);

router.get("/logout", authenticationController.logout);

router.get("/account", accountController.account);

router.post("/update-account", accountController.accoutUpdate);

module.exports = router;
