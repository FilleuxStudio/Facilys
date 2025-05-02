const cookieConfig = require("../config/cookie-config");
const jwt = require("jsonwebtoken");
const bcrypt = require('bcrypt');
const User = require("../models/user.model");
const Product = require("../models/product.model");
const Order = require("../models/order.model");
const Invoice = require("../models/invoice.model");
const Team = require("../models/team.model");
const logger = require("../utils/logger");

const now = new Date().getFullYear();

exports.account = async (req, res) => {
  try {
    // Vérifier si le cookie authToken existe
    var token = await CheckTokenConnection(req, res);

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);

      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect("/login");
      }

      const userData = await User.findByEmail(req.session.user.email);

      const csrfToken = req.csrfToken(true);
      const message = req.query.message || null;
      res.render("account", {
        user: userData,
        currentDateTime: now,
        currentRoute: req.path,
        message: message,
        session: req.session,
        csrfToken,
      });
    } catch (error) {
      // Si le token est invalide ou expiré
      console.error("Token invalide ou expiré:", error);
      res.clearCookie("authToken");
      return res.redirect("/login");
    }
  } catch (err) {
    console.error("Erreur lors de l'accès au compte :", err);
    logger.error("Erreur lors de l'accès au compte :", err);
    res.status(500).send("Erreur serveur");
  }
};

exports.accountOrders = async (req, res) => {
  try {
    // Vérifier si le cookie authToken existe
    var token = await CheckTokenConnection(req, res);

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);

      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect("/login");
      }

      var OrderList = [];
      const userData = await User.findByEmail(req.session.user.email);
      OrderList = await Order.findByUserId(req.session.user.email);

      const csrfToken = req.csrfToken(true);
      const message = req.query.message || null;
      res.render("account-orders", {
        user: userData,
        currentDateTime: now,
        currentRoute: req.path,
        message: message,
        Orders: OrderList,
        session: req.session,
        csrfToken,
      });
    } catch (error) {
      // Si le token est invalide ou expiré
      console.error("Token invalide ou expiré:", error);
      res.clearCookie("authToken");
      return res.redirect("/login");
    }
  } catch (err) {
    console.error("Erreur lors de l'accès au commande :", err);
    logger.error("Erreur lors de l'accès au commande :", err);
    res.status(500).send("Erreur serveur");
  }
};

exports.accountInvoices = async (req, res) => {
  try {
    // Vérifier si le cookie authToken existe
    var token = await CheckTokenConnection(req, res);

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);

      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect("/login");
      }

      var InvoiceList = [];
      const userData = await User.findByEmail(req.session.user.email);
      InvoiceList = await Invoice.findByUserId(req.session.user.email);

      const csrfToken = req.csrfToken(true);
      const message = req.query.message || null;
      res.render("account-invoices", {
        user: userData,
        currentDateTime: now,
        currentRoute: req.path,
        message: message,
        Invoices: InvoiceList,
        session: req.session,
        csrfToken,
      });
    } catch (error) {
      // Si le token est invalide ou expiré
      console.error("Token invalide ou expiré:", error);
      logger.error("Token invalide ou expiré:", error);
      res.clearCookie("authToken");
      return res.redirect("/login");
    }
  } catch (err) {
    console.error("Erreur lors de l'accès au facture :", err);
    logger.error("Erreur lors de l'accès au facture :", err);
    res.status(500).send("Erreur serveur");
  }
};

exports.accountTeams = async (req, res) => {
  try {
    // Vérifier si le cookie authToken existe
    var token = await CheckTokenConnection(req, res);

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);

      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect("/login");
      }

      var TeamList = [];
      const userData = await User.findByEmail(req.session.user.email);
      TeamList = await Team.findByUserId(req.session.user.email);

      const csrfToken = req.csrfToken(true);
      const message = req.query.message || null;
      res.render("account-teams", {
        user: userData,
        currentDateTime: now,
        currentRoute: req.path,
        message: message,
        Teams: TeamList,
        session: req.session,
        csrfToken,
      });
    } catch (error) {
      // Si le token est invalide ou expiré
      console.error("Token invalide ou expiré:", error);
      logger.error("Token invalide ou expiré:", error);
      res.clearCookie("authToken");
      return res.redirect("/login");
    }
  } catch (err) {
    console.error("Erreur lors de l'accès a l'équipe :", err);
    logger.error("Erreur lors de l'accès a l'équipe :", err);
    res.status(500).send("Erreur serveur");
  }
};

exports.accountDownload = async (req, res) => {
  try {
    // Vérifier si le cookie authToken existe
    var token = await CheckTokenConnection(req, res);

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);

      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect("/login");
      }

      const csrfToken = req.csrfToken(true);
      const userData = await User.findByEmail(req.session.user.email);
      const message = req.query.message || null;
      res.render("account-download", {
        user: userData,
        currentDateTime: now,
        currentRoute: req.path,
        message: message,
        session: req.session,
        csrfToken,
      });
    } catch (error) {
      // Si le token est invalide ou expiré
      console.error("Token invalide ou expiré:", error);
      logger.error("Token invalide ou expiré:", error);
      res.clearCookie("authToken");
      return res.redirect("/login");
    }
  } catch (err) {
    console.error("Erreur lors de l'accès au téléchargement :", err);
    logger.error("Erreur lors de l'accès au téléchargement :", err);
    res.status(500).send("Erreur serveur");
  }
}

exports.accoutUpdate = async (req, res) => {
  try {
    var logoDataUrl = "";
    if (req.files != null) {
      if (req.files || req.files.filelogo) {
        logoDataUrl = await convertToBase64Html(req.files.filelogo);
      }
    } else {
      logoDataUrl = req.body.imagelogo;
    }

    // Préparation des données de base
    const updateData = {
      companyName: req.body.companyName,
      logo: logoDataUrl,
      firstName: req.body.firstName,
      lastName: req.body.lastName,
      siret: req.body.siret,
      addressclient: req.body.addressclient,
      phone: req.body.phone
    };

    // Gestion du mot de passe
    if (req.body.password && req.body.passwordConfirm) {
      if (req.body.password !== req.body.passwordConfirm) {
        return res.status(400).redirect("/account?error=Les mots de passe ne correspondent pas");
      }
      
      const hashedPassword = await bcrypt.hash(req.body.password, 10);
      updateData.password = hashedPassword;
    }

    // Mise à jour Firestore
    await User.update(req.body.email, updateData);

    res.status(200).redirect(
        "/account?message=Vos informations ont été mises à jour avec succès."
      );
  } catch (err) {
    console.error("Erreur lors de la récupération des informations du compte:",err );
    logger.error("Erreur lors de la récupération des informations du compte:",err);
    res.status(500).json({ error: "Erreur serveur" });
  }
};

exports.accountAddTeam = async (req, res) => {
  const { tlname, tfname, tpassword, ttype, temail, emailmanager } = req.body;
  try {
    var userManager = await User.findByEmail(emailmanager);
    // Créer une instance de produit
    const team = new Team({
      fname: tfname,
      lname: tlname,
      type: ttype,
      password: await bcrypt.hash(tpassword, 10),
      team: userManager.companyName,
      manager: userManager.email,
      email: temail,
    });

    await team.save();

    res.status(201).redirect("/account-teams");
  } catch (error) {
    console.error("Erreur lors de l'ajout d'un utilisateur client team", error);
    logger.error("Erreur lors de l'ajout d'un utilisateur client team", error);
    res.status(500).send("Erreur lors de l'ajout du produit");
  }
};

exports.accountDeleteMember = async (req, res) => {
  try {
    console.log("Suppression de l'équipe avec l'ID:", req.params.id);
    const deleteResult = await Team.delete(req.params.id);
    if (deleteResult === true) {
      res.redirect('/account-teams');
    }
  } catch (error) {
    console.error('Erreur lors de la suppression', error);
    logger.error("Erreur lors de la suppression", error);
    req.flash('error', 'Erreur lors de la suppression');
    res.redirect('/account-teams');
  }
};

async function CheckTokenConnection(req, res) {
  var token = req.cookies.authToken;
  if (!token) {
    // Si le cookie n'existe pas, rediriger vers la page de connexion
    return res.redirect("/login");
  } else {
    return token;
  }
}

async function convertToBase64Html(file) {
  if (!file) {
    throw new Error("Aucun fichier n'a été fourni");
  }
  // Convertir le buffer du fichier en base64
  var base64 = file.data.toString("base64");
  // Déterminer le type MIME du fichier
  var mimeType = file.mimetype;
  // Créer la chaîne data URL
  return `data:${mimeType};base64,${base64}`;
}
