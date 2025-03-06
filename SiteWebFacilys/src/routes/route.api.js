const express = require("express");
const User = require("../models/user.model");
const Team = require("../models/team.model");
const argon2 = require("@node-rs/argon2");
const router = express.Router();

router.post("/version", (req, res) => {
  res.status(200).json({
    success: true,
    data: "version 1",
  });
});

router.post("/login", async (req, res) => {
  try {
    const { email, password } = req.body;

    // Vérifiez si l'utilisateur existe
    let user = await User.findByEmail(email);
    let isTeam = false;

    if (!user) {
      // Si l'utilisateur n'existe pas, vérifiez si c'est une équipe
      user = await Team.findByEmail(email);
      isTeam = true;

      if (!user) {
        return res.status(404).json({
          success: false,
          message: "Ni utilisateur ni équipe trouvés avec cet email",
          data: null,
        });
      }
    }

    // Vérifiez le mot de passe
    const match = await argon2.verify(user.password, password);
    if (!match) {
      return res.status(401).json({
        success: false,
        message: "Email ou mot de passe incorrect",
        data: null,
      });
    }

    // Réponse en fonction du type d'utilisateur
    return res.status(200).json({
      success: true,
      message: isTeam ? "Équipe trouvée avec succès" : "Utilisateur trouvé avec succès",
      data: user,
    });
  } catch (error) {
    console.error("Erreur lors de la connexion :", error);
    return res.status(500).json({
      success: false,
      message: "Une erreur interne est survenue",
      data: null,
    });
  }
});


const checkUserAuth = async (req, res, next) => {
  const userId = req.body.userId; // Supposons que l'ID est envoyé dans le corps de la requête

  if (!userId) {
    return res.status(401).json({
      success: false,
      message: "ID utilisateur non fourni",
    });
  }

  try {
    // Vérifiez d'abord si c'est un utilisateur
    const user = await User.findById(userId);
    if (user) {
      req.user = user;
      return next();
    }

    // Si ce n'est pas un utilisateur, vérifiez si c'est une équipe
    const team = await Team.findById(userId);
    if (team) {
      req.user = team;
      return next();
    }

    // Si ni utilisateur ni équipe n'est trouvé
    return res.status(404).json({
      success: false,
      message: "Utilisateur ou équipe non trouvé",
    });
  } catch (error) {
    console.error("Erreur lors de la vérification de l'utilisateur:", error);
    return res.status(500).json({
      success: false,
      message: "Erreur interne du serveur",
    });
  }
};


module.exports = router;
