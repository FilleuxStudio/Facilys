const User = require("../models/user.model");
const cookieConfig = require("../config/cookie-config");
const jwt = require("jsonwebtoken");
const bcrypt = require("bcrypt");
const { createCanvas } = require("canvas");
const databaseService = require("../services/databaseService");

exports.login = async (req, res) => {
  const { _csrf, email, password, rememberMe } = req.body;
  const pathLink = req.query || null;

  if (!req.csrfToken() || _csrf !== req.csrfToken()) {
    return res.status(403).redirect("/login?message=Erreur tocken");
  }

  try {
    // Rechercher l'utilisateur dans la base de données
    const user = await User.findByEmail(email);
    if (!user) {
      if (pathLink.link == undefined) {
        return res
          .status(400)
          .redirect("/login?message=Email ou mot de passe incorrect.");
      } else {
        return res
          .status(400)
          .redirect(
            "/shop-checkout?id=" +
              req.body.idProduct +
              "&message=Email ou mot de passe incorrect."
          );
      }
    }

    // Comparer le mot de passe
    const match = await bcrypt.compareSync(user.password, password);

    if (!match) {
      if (pathLink.link == undefined) {
        res.redirect("/login");
      } else {
        res.redirect(
          "/shop-checkout?id=" +
            req.body.idProduct +
            "&message=Email ou mot de passe incorrect."
        );
      }
    }

    // Générer un token JWT
    const token = jwt.sign({ id: user.email }, cookieConfig.cookieSecret, {
      expiresIn: rememberMe ? "7d" : "1h", // Token valide 1 heure ou 7 jours
    });

    // Stocker les informations de l'utilisateur dans la session
    req.session.user = {
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      manager: user.manager,
    };

    // Définir la durée du cookie selon "Remember me"
    const cookieDuration = rememberMe
      ? cookieConfig.rememberMeDuration
      : cookieConfig.sessionDuration;

    // Configurer les options du cookie
    res.cookie("authToken", token, {
      ...cookieConfig.cookieOptions,
      maxAge: cookieDuration, // Durée de vie du cookie
    });

    if (pathLink.link == undefined) {
      res.redirect("/account");
    } else {
      res.redirect(
        "/shop-checkout?id=" +
          req.body.idProduct +
          "&message=Vous êtes connecté."
      );
    }
  } catch (err) {
    console.error("Erreur lors de la connexion :", err);
    res.status(500).send("Erreur serveur");
  }
};

exports.logout = (req, res) => {
  // Détruire la session
  req.session.destroy((err) => {
    if (err) {
      console.error("Erreur lors de la destruction de la session:", err);
    }

    // Supprimer le cookie de session
    res.clearCookie("connect.sid", { path: "/" });

    // Supprimer le cookie d'authentification
    res.clearCookie("authToken", { path: "/" });

    res.clearCookie("facylis.x-csrf-token", { path: "/" });

    // Rediriger vers la page de connexion
    res.redirect("/");
  });
};

// Contrôleur d'inscription
exports.register = async (req, res) => {
  const {
    _csrf,
    companyName,
    firstName,
    lastName,
    email,
    password,
    passwordControl,
  } = req.body;

  const pathLink = req.query || null;

  if (!req.csrfToken() || _csrf !== req.csrfToken()) {
    return res.status(403).redirect("/register?message=Erreur tocken");
  }

  const checkTerm = transformCheckboxValue(req.body.checkTerm);
  try {
    // Hachage du mot de passe avec bcrypt
    const saltRounds = 10;
    var hashedPassword = await bcrypt.hash(password, saltRounds);
    // Créer une instance de l'utilisateur
    const user = new User({
      companyName,
      logo: generateRandomProfileImage(128),
      firstName,
      lastName,
      email,
      siret: "null",
      addressclient: "null",
      phone: "null",
      password: hashedPassword, // Mot de passe haché
      manager: false, // Par défaut, false, peut être ajusté selon vos besoins
      mariadbUser: "null",
      mariadbPassword: "null",
      mariadbDb: "null",
    });

    // Sauvegarder l'utilisateur dans Firestore
    if (checkTerm == true) {
      await user.save();
      var userId = user.companyName.toLowerCase().trim();
      var dbInfo = await databaseService.createDatabase(userId);

      // Mettre à jour les infos de connexion à la base dans votre utilisateur
      await user.updateMariaDBInfo(user.email, {
        user: dbInfo.user,
        password: dbInfo.password,
        name: dbInfo.name,
      });
    } else {
      if (pathLink.link == undefined) {
        return res
          .status(201)
          .redirect(
            "/register?message=Vous devez accepter les conditions d'utilisation"
          );
      } else {
        return res
          .status(201)
          .redirect(
            "/shop-checkout?id=" +
              req.body.idProduct +
              "&message=Vous devez accepter les conditions d'utilisation"
          );
      }
    }

    if (pathLink.link == undefined) {
      res.redirect("/login");
    } else {
      res.redirect(
        "/shop-checkout?id=" +
          req.body.idProduct +
          "&message= Compte créé, maintenant, connectez-vous pour valider vos identifiants."
      );
    }
  } catch (error) {
    console.error("Erreur lors de la création de l'utilisateur:", error);

    // Nettoyage en cas d'erreur après la création de l'utilisateur
    if (user && user.id) {
      try {
        await admin.firestore().collection("users").doc(user.id).delete();
        await databaseService.deleteUserDatabase(user.id); // À implémenter si nécessaire
      } catch (cleanupError) {
        console.error("Erreur lors du nettoyage:", cleanupError);
      }
    }

    res.status(500).send("Erreur lors de la création de l'utilisateur");
  }
};

function transformCheckboxValue(value) {
  if (value === "on" || value === true) {
    return true;
  }
  return false;
}

function generateRandomProfileImage(size = 128) {
  const canvas = createCanvas(size, size);
  const ctx = canvas.getContext("2d");

  // Palette de couleurs modernes
  const colors = [
    "#f6fdf6",
    "#d3fdd6",
    "#83fb91",
    "#78e5c1",
    "#24cc8c",
    "#78badb",
    "#6dade7",
    "#77a3e1",
    "#6a8edb",
    "#7971ce",
    "#a9978a",
    "#a18884",
    "#947579",
    "#896c7c",
    "#665e83",
    "#7b68b8",
    "#9f63c1",
    "#c367b1",
    "#d47391",
    "#ea7661",
  ];

  // Fonction pour choisir une couleur aléatoire
  const randomColor = () => colors[Math.floor(Math.random() * colors.length)];

  // Fond aléatoire
  ctx.fillStyle = randomColor();
  ctx.fillRect(0, 0, size, size);

  // Formes aléatoires
  for (let i = 0; i < 5; i++) {
    ctx.beginPath();
    ctx.fillStyle = randomColor();
    ctx.arc(
      Math.random() * size,
      Math.random() * size,
      (Math.random() * size) / 3,
      0,
      Math.PI * 2
    );
    ctx.fill();
  }

  // Convertir le canvas en PNG base64 avec le préfixe
  const pngBase64 = canvas.toDataURL("image/png");

  return pngBase64;
}
