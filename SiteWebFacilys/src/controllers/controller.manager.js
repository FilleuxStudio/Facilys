const cookieConfig = require("../config/cookie-config");
const jwt = require("jsonwebtoken");
const bcrypt = require('bcrypt');
const User = require("../models/user.model");
const Product = require("../models/product.model");
const logger = require("../utils/logger");

const now = new Date().getFullYear();

exports.managerProducts = async (req, res) => {
  try {
    // Vérifier si le cookie authToken existe
    var token = await CheckTokenConnection(req);

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);

      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect("/login");
      }

      var productList = [];

      if (req.session.user.manager == true) {
        productList = await Product.findAll();
      }
      const userData = await User.findByEmail(req.session.user.email);

      const csrfToken = req.csrfToken(true);
      const message = req.query.message || null;
      res.render("manager-product", {
        user: userData,
        currentDateTime: now,
        currentRoute: req.path,
        message: message,
        Products: productList,
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
    console.error("Erreur lors de l'accès au produit :", err);
    logger.error("Erreur lors de l'accès au produit :", err);
    res.status(500).send("Erreur serveur");
  }
};

exports.managerClients = async (req, res) => {
  try {
    // Vérifier si le cookie authToken existe
    var token = await CheckTokenConnection(req);

    // Vérifier la validité du token JWT
    try {
      const decoded = jwt.verify(token, cookieConfig.cookieSecret);

      // Vérifier si la session utilisateur existe
      if (!req.session.user) {
        return res.redirect("/login");
      }

      var clientList = [];

      if (req.session.user.manager == true) {
        clientList = await User.findAll();
      }
      const userData = await User.findByEmail(req.session.user.email);

      const csrfToken = req.csrfToken(true);
      const message = req.query.message || null;
      res.render("manager-clients", {
        user: userData,
        currentDateTime: now,
        currentRoute: req.path,
        message: message,
        Clients: clientList,
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
    console.error("Erreur lors de l'accès au client :", err);
    logger.error("Erreur lors de l'accès au client :", err);
    res.status(500).send("Erreur serveur");
  }
};

// Ajouter un produit
exports.managerAddProduct = async (req, res) => {
  const { title, subtitle, price, oldPrice, advice, type, colorBadge } =
    req.body;
  try {
    var logoDataUrl = "";
    if (req.files != null) {
      if (req.files || req.files.pictureProduct) {
        logoDataUrl = await convertToBase64Html(req.files.pictureProduct);
      }
    }
    // Créer une instance de produit
    const product = new Product({
      title: title,
      subtitle: subtitle,
      picture: logoDataUrl,
      price: price,
      oldPrice: oldPrice,
      advice: advice,
      type: type,
      colorBadge: colorBadge,
    });

    await product.save();

    res.status(201).redirect("/manager-product");
    
  } catch (error) {
    console.error("Erreur lors de l'ajout du produit", error);
    logger.error("Erreur lors de l'ajout du produit", error);
    res.status(500).send("Erreur lors de l'ajout du produit");
  }
};

exports.managerGetProduct = async (req, res) => {
  try {
    const product = await Product.findById(req.body.id);
    if (!product) {
      return res.status(404).json({ message: "Produit non trouvé" });
    }
    return res.status(200).json(product);
  } catch (error) {
    console.error("Erreur lors de la récupération du produit", error);
    logger.error("Erreur lors de la récupération du produit", error);
    return res.status(500).json({ message: "Erreur lors de la récupération du produit" });
  }
};

exports.managerGetAllProducts = async (req, res) => {
  try {
    return await Product.findAll();
  } catch (error) {
    console.error("Erreur lors de la récupération du produit", error);
    logger.error("Erreur lors de la récupération du produit", error);
    res.status(500).send("Erreur lors de la récupération du produit");
  }
};

exports.managerEditProduct = async (req, res) => {
  const {
    editProductId,
    editProductTitle,
    editProductSubtitle,
    editProductPrice,
    editProductOldPrice,
    editProductAdvice,
    editProductType,
    editProductColorBadge,
  } = req.body;
  try {
    let logoDataUrl = "";

    // Si un fichier image est fourni, le convertir en base64
    if (req.files && req.files.pictureProductEdit) {
      logoDataUrl = await convertToBase64Html(req.files.pictureProductEdit);
    }

    // Préparer les données de mise à jour
    const updateData = {
      title: editProductTitle,
      subtitle: editProductSubtitle,
      price: editProductPrice,
      oldPrice: editProductOldPrice,
      advice: editProductAdvice,
      type: editProductType,
      colorBadge: editProductColorBadge,
    };

    // Si une nouvelle image est fournie, inclure son URL
    if (logoDataUrl) {
      updateData.picture = logoDataUrl;
    }

    // Mettre à jour le produit dans Firestore
    const updateResult = await Product.update(editProductId, updateData);

    if (updateResult) {
      return res.status(200).redirect("/manager-product");
    } else {
      return res.status(403).send("Erreur lors de la modification du produit.");
    }
  } catch (error) {
    console.error("Erreur lors de la modification du produit", error);
    logger.error("Erreur lors de la modification du produit", error);
    res.status(500).send("Erreur lors de la modification du produit");
  }
};

exports.managerDeleteProduct = async (req, res) => {
  try {
    var deleteResult = await Product.delete(req.params.id);
    if (deleteResult === true) {
      return res.status(200).redirect("/manager-product");
    } else {
      return res.status(403).send("error");
    }
  } catch (error) {
    console.error("Erreur lors de la suppression du produit", error);
    logger.error("Erreur lors de la suppression du produit", error);
    res.status(500).send("Erreur lors de la suppression du produit");
  }
};

async function CheckTokenConnection(req) {
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
