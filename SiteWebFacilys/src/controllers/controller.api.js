const User = require("../models/user.model");
const Team = require("../models/team.model");
const argon2 = require("@node-rs/argon2");
const jwt = require("jsonwebtoken");
const ConnectionPoolService = require("../services/connectionPoolService");

exports.getVersion = (req, res) => {
  res.status(200).json({
    success: true,
    data: "version 1.0.0",
  });
};

exports.login = async (req, res) => {
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
      message: isTeam
        ? "Équipe trouvée avec succès"
        : "Utilisateur trouvé avec succès",
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
};

exports.executeQuery = async (req, res) => {
  try {
    const { operation, table, data, conditions } = req.body;
    let query, params;

    switch (operation.toUpperCase()) {
      case "SELECT":
        query = `SELECT * FROM ${table} WHERE ?`;
        params = [conditions];
        break;
      case "INSERT":
        query = `INSERT INTO ${table} SET ?`;
        params = [data];
        break;
      case "UPDATE":
        query = `UPDATE ${table} SET ? WHERE ?`;
        params = [data, conditions];
        break;
      case "DELETE":
        query = `DELETE FROM ${table} WHERE ?`;
        params = [conditions];
        break;
      default:
        return res.status(400).json({
          success: false,
          message: "Opération non supportée",
        });
    }

    const user = await User.findByEmail(req.user.email);
    const result = await ConnectionPoolService.executeQuery(
      user,
      query,
      params
    );

    res.status(200).json({
      success: true,
      data: result,
    });
  } catch (error) {
    console.error("Erreur lors de l'exécution de la requête :", error);
    res.status(500).json({
      success: false,
      message: "Une erreur est survenue lors de l'exécution de la requête",
    });
  }
};
