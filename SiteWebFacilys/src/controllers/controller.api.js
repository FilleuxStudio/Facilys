const User = require("../models/user.model");
const Team = require("../models/team.model");
const bcrypt = require('bcrypt');
const jwt = require("jsonwebtoken");
const ConnectionPoolService = require("../services/connectionPoolService");
const logger = require("../utils/logger");

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
    const match = await bcrypt.compare(password, user.password);
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
    logger.error("Erreur lors de la connexion", error);
    return res.status(500).json({
      success: false,
      message: "Une erreur interne est survenue",
      data: null,
    });
  }
};

exports.loginDatabase = async (req, res) => {
  try {
    const { email } = req.body;

    // Vérifiez si l'utilisateur existe
    let company = await User.findByEmail(email);

    if (!company) {
      // Si l'utilisateur n'existe pas, vérifiez si c'est une équipe
      user = await Team.findByEmail(email);
      if (!user) {
        return res.status(404).json({
          success: false,
          message: "Ni utilisateur ni équipe trouvés avec cet email",
          data: null,
        });
      }

      database = await User.findByEmail(user.manager);
    }

    // Réponse en fonction du type d'utilisateur
    return res.status(200).json({
      success: true,
      data: database,
    });
  } catch (error) {
    console.error("Erreur login database :", error);
    logger.error("Erreur login database", error);
    return res.status(500).json({
      success: false,
      message: "Une erreur interne est survenue",
      data: null,
    });
  }
};

exports.company =  async (req, res) => {
  try {
    const { email } = req.body;

    // Vérifiez si l'utilisateur existe
    let company = await findUserOrTeamByEmail(email);
    
    // Réponse en fonction du type d'utilisateur
    return res.status(200).json({
      success: true,
      data: company,
    });
  } catch (error) {
    console.error("Erreur récupération des données company :", error);
    logger.error("Erreur récupération des données company", error);
    return res.status(500).json({
      success: false,
      message: "Une erreur interne est survenue",
      data: null,
    });
  }
};

exports.updateCompany = async (req, res) => {
  try {
    const {
      email,
      companyName,
      logo,
      siret,
      addressclient,
      firstName,
      lastName,
      phone
    } = req.body;

    // 1. Chercher l'utilisateur ou l'équipe
    let company = await findUserOrTeamByEmail(email);

    // 2. Mettre à jour les données dans Firestore
    const updateData = {
      companyName,
      logo,
      firstName,
      lastName,
      siret,
      addressclient,
      phone,
      email 
    };

    const success = await User.update(email, updateData);
    if (!success) {
      throw new Error("Échec de la mise à jour dans Firestore");
    }

    const updatedUser = await User.findByEmail(email);
    return res.status(200).json({
      success: true,
      data: updatedUser,
    });
  } catch (error) {
    console.error("Erreur lors de la mise à jour :", error);
    logger.error("Erreur lors de la mise à jour", error);
    return res.status(500).json({
      success: false,
      message: "Erreur serveur lors de la mise à jour",
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
    logger.error("Erreur lors de l'exécution de la requête", error);
    res.status(500).json({
      success: false,
      message: "Une erreur est survenue lors de l'exécution de la requête",
    });
  }
};

exports.executeQueryAddClient = async (req, res) => {
  try {
    const { usermail, changes } = req.body;
    const user = await findUserOrTeamByEmail(usermail)
    const clientData = JSON.parse(changes)[0];
    if (!clientData) {
      return res.status(400).json({
        success: false,
        message: "Aucune donnée Client valide fournie",
      });
    }

    const table = "Clients";
    const query = `INSERT INTO ${table} SET ?`;
    const params = [ clientData ]; 

    const result = await ConnectionPoolService.executeQuery(user, query, params);

    res.status(200).json({
      success: true,
      message: "Client inséré avec succès",
      data: result,
    });
  } catch (error) {
    console.error("Erreur lors de l'exécution de la requête client :", error);
    logger.error("Erreur lors de l'exécution de la requête client ", error);
    res.status(500).json({
      success: false,
      message: "Une erreur est survenue lors de l'exécution de la requête",
    });
  }
};

exports.executeQueryAddPhone = async (req, res) => {
  try {
    const { usermail, changes } = req.body;
    const user = await findUserOrTeamByEmail(usermail);

    const phones = JSON.parse(changes)[0];

    if (!phones || phones.length === 0) {
      return res.status(400).json({
        success: false,
        message: "Aucun numéro de téléphone fourni",
      });
    }

    const table = "PhonesClients";
    const results = [];

    for (const phoneEntry of phones) {
      const clientId = phoneEntry.Client?.Id;
      const phone = phoneEntry.Phone;

      if (!clientId || !phone) continue;

      const data = {
        Id: phoneEntry.Id,
        IdClient: clientId,
        Phone: phone
      };

      const query = `INSERT INTO ${table} SET ?`;
      const params = [data];

      const result = await ConnectionPoolService.executeQuery(user, query, params);
      results.push(result);
    }

    res.status(200).json({
      success: true,
      message: "Téléphones insérés avec succès",
      data: results,
    });

  } catch (error) {
    console.error("Erreur lors de l'insertion des téléphones :", error);
    logger.error("Erreur lors de l'insertion des téléphones ", error);
    res.status(500).json({
      success: false,
      message: "Erreur lors de l'insertion des téléphones",
    });
  }
};

exports.executeQueryAddEmail = async (req, res) => {
  try {
    const { usermail, changes } = req.body;
    const user = await findUserOrTeamByEmail(usermail);

    const emails = JSON.parse(changes)[0];

    if (!emails || emails.length === 0) {
      return res.status(400).json({
        success: false,
        message: "Aucun email fourni",
      });
    }

    const table = "EmailsClients";
    const results = [];

    for (const emailEntry of emails) {
      const clientId = emailEntry.Client?.Id;
      const email = emailEntry.Email;

      if (!clientId || !email) continue;

      const data = {
        Id: emailEntry.Id,
        IdClient: clientId,
        Email: email
      };

      const query = `INSERT INTO ${table} SET ?`;
      const params = [data];

      const result = await ConnectionPoolService.executeQuery(user, query, params);
      results.push(result);
    }

    res.status(200).json({
      success: true,
      message: "Emails insérés avec succès",
      data: results,
    });

  } catch (error) {
    console.error("Erreur lors de l'insertion des emails :", error);
    logger.error("Erreur lors de l'insertion des email ", error);
    res.status(500).json({
      success: false,
      message: "Erreur lors de l'insertion des emails",
    });
  }
};


async function findUserOrTeamByEmail(email) {
  try {
    // Recherche de l'utilisateur par e-mail
    let user = await User.findByEmail(email);

    if (user) {
      return user;
    }

    // Si l'utilisateur n'est pas trouvé, recherche de l'équipe par e-mail
    const team = await Team.findByEmail(email);

    if (!team) {
      throw new Error("Ni utilisateur ni équipe trouvés avec cet e-mail.");
    }

    // Récupération de l'utilisateur manager de l'équipe
    user = await User.findByEmail(team.manager);

    if (!user) {
      throw new Error("Manager de l'équipe introuvable.");
    }

    return user;
  } catch (error) {
    // Gestion des erreurs
    throw new Error(error.message);
  }
}