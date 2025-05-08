const User = require("../models/user.model");
const Team = require("../models/team.model");
const bcrypt = require("bcrypt");
const jwt = require("jsonwebtoken");
const ConnectionPoolService = require("../services/connectionPoolService");
const logger = require("../utils/logger");
const SqlString = require("sqlstring");

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

exports.company = async (req, res) => {
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
      phone,
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
      email,
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

exports.executeQueryAddClient = (req, res) =>
  handleGenericInsertOneOrMany(req, res, {
    tableName: "Clients",
    successMessage: "Client inséré avec succès",
  });

exports.executeQueryAddVehicle = (req, res) =>
  handleGenericInsertOneOrMany(req, res, {
    tableName: "Vehicles",
    successMessage: "Véhicule ajouté avec succès",
  });

exports.executeQueryAddOtherVehicle = (req, res) =>
  handleGenericInsertOneOrMany(req, res, {
    tableName: "OtherVehicles",
    successMessage: "Autre véhicule ajouté avec succès",
  });

exports.executeQueryAddInvoice = (req, res) =>
  handleGenericInsertOneOrMany(req, res, {
    tableName: "Invoices",
    successMessage: "Facture ajoutée avec succès",
  });

exports.executeQueryAddQuote = (req, res) =>
  handleGenericInsertOneOrMany(req, res, {
    tableName: "Quotes",
    successMessage: "Devis ajouté avec succès",
  });

exports.executeQueryAddPhone = (req, res) => {
  handleGenericInsert(req, res, {
    tableName: "Phones",
    fields: ["Id", "ClientId", "Phone"],
    successMessage: "Téléphones insérés avec succès",
  });
};
exports.executeQueryAddEmail = (req, res) => {
  handleGenericInsert(req, res, {
    tableName: "Emails",
    fields: ["Id", "ClientId", "Email"],
    successMessage: "Emails insérés avec succès",
  });
};

exports.executeQueryUpdateClient = (req, res) => {
  handleGenericUpdateWithCascadeDelete(req, res, {
    tableName: "Clients",
    cascadeDeleteConfigs: [
      { table: "Phones", foreignKey: "IdClient" },
      { table: "Emails", foreignKey: "IdClient" },
    ],
    successMessage: "Client mis à jour avec succès",
  });
};

exports.executeQueryUpdateVehicle = (req, res) => {
  handleGenericUpdateWithCascadeDelete(req, res, {
    tableName: "Vehicles",
    cascadeDeleteConfigs: [],
    successMessage: "Véhicule mis à jour avec succès",
  });
};

exports.executeQueryUpdateOtherVehicle = (req, res) => {
  handleGenericUpdateWithCascadeDelete(req, res, {
    tableName: "OtherVehicles",
    cascadeDeleteConfigs: [],
    successMessage: "Véhicule mis à jour avec succès",
  });
};

exports.executeQueryUpdateInvoice = (req, res) => {
  handleGenericUpdateWithCascadeDelete(req, res, {
    tableName: "Invoices",
    cascadeDeleteConfigs: [],
    successMessage: "Facture mise à jour avec succès",
  });
};

const handleGenericUpdateWithCascadeDelete = async (req, res, options) => {
  const { tableName, cascadeDeleteConfigs, successMessage } = options;

  try {
    const { usermail, changes } = req.body;

    // 🔐 Auth
    const user = await findUserOrTeamByEmail(usermail);
    if (!user) {
      return res
        .status(404)
        .json({ success: false, message: "Utilisateur introuvable" });
    }

    // 🧠 Parsing JSON
    let changesParsed;
    try {
      changesParsed = JSON.parse(changes);
      while (typeof changesParsed === "string") {
        changesParsed = JSON.parse(changesParsed);
      }
    } catch (err) {
      return res.status(400).json({
        success: false,
        message: "Format de 'changes' invalide",
      });
    }

    if (!Array.isArray(changesParsed) || changesParsed.length === 0) {
      return res.status(400).json({
        success: false,
        message: "'changes' doit être un tableau non vide",
      });
    }

    const entity = changesParsed[0];

    if (
      !entity ||
      typeof entity !== "object" ||
      Array.isArray(entity) ||
      !entity.Id
    ) {
      return res.status(400).json({
        success: false,
        message: `Entrée invalide pour la table ${tableName}`,
      });
    }

    // 🔄 Construction de la requête UPDATE
    const keys = Object.keys(entity).filter((k) => k !== "Id");
    const updateSet = keys
      .map(
        (k) => `\`${k}\` = ${SqlString.escape(formatDateForMariaDB(entity[k]))}`
      )
      .join(", ");
    const updateQuery = `UPDATE \`${tableName}\` SET ${updateSet} WHERE \`Id\` = ${SqlString.escape(
      entity.Id
    )};`;

    await ConnectionPoolService.executeQuery(user, updateQuery);

    // ❌ Suppressions en cascade
    for (const { table, foreignKey, matchId } of cascadeDeleteConfigs) {
      const idToDelete = matchId ?? entity.Id;
      const deleteQuery = `DELETE FROM \`${table}\` WHERE \`${foreignKey}\` = ${SqlString.escape(
        idToDelete
      )};`;
      await ConnectionPoolService.executeQuery(user, deleteQuery);
    }

    res.status(200).json({
      success: true,
      message: successMessage,
      data: { updatedId: entity.Id },
    });
  } catch (error) {
    console.error(
      `Erreur lors de la mise à jour de ${options.tableName} :`,
      error
    );
    logger.error(
      `Erreur lors de la mise à jour de ${options.tableName}`,
      error
    );
    res.status(500).json({
      success: false,
      message: `Erreur lors de la mise à jour de ${options.tableName}`,
    });
  }
};

const handleGenericInsertOneOrMany = async (req, res, options) => {
  const { tableName, successMessage } = options;

  try {
    const { usermail, changes } = req.body;

    // 🔐 Auth
    const user = await findUserOrTeamByEmail(usermail);
    if (!user) {
      return res
        .status(404)
        .json({ success: false, message: "Utilisateur introuvable" });
    }

    // 📦 JSON parsing
    let changesParsed;
    try {
      changesParsed = JSON.parse(changes);
      while (typeof changesParsed === "string") {
        changesParsed = JSON.parse(changesParsed);
      }
    } catch (err) {
      return res.status(400).json({
        success: false,
        message: "Format de 'changes' invalide",
      });
    }

    const entries = Array.isArray(changesParsed)
      ? changesParsed
      : [changesParsed];
    if (entries.length === 0) {
      return res.status(400).json({
        success: false,
        message: "'changes' doit contenir au moins une entrée",
      });
    }

    const results = [];

    for (const entry of entries) {
      if (!entry || typeof entry !== "object" || Array.isArray(entry)) continue;

      const keys = Object.keys(entry);
      const values = Object.values(entry).map(formatDateForMariaDB);

      const escapedValues = values.map(SqlString.escape).join(", ");
      const columns = keys.map((key) => `\`${key}\``).join(", ");

      const query = `INSERT INTO \`${tableName}\` (${columns}) VALUES (${escapedValues});`;

      const result = await ConnectionPoolService.executeQuery(user, query);
      results.push(result);
    }

    res.status(200).json({
      success: true,
      message: successMessage,
      data: results,
    });
  } catch (error) {
    console.error(
      `Erreur lors de l'insertion dans ${options.tableName} :`,
      error
    );
    logger.error(`Erreur lors de l'insertion dans ${options.tableName}`, error);
    res.status(500).json({
      success: false,
      message: `Erreur lors de l'insertion dans ${options.tableName}`,
    });
  }
};

const handleGenericInsert = async (req, res, options) => {
  const { tableName, fields, successMessage } = options;

  try {
    const { usermail, changes } = req.body;

    // 🔍 Auth
    const user = await findUserOrTeamByEmail(usermail);
    if (!user) {
      return res
        .status(404)
        .json({ success: false, message: "Utilisateur introuvable" });
    }

    // 🔄 JSON parsing
    let changesParsed;
    try {
      changesParsed = JSON.parse(changes);
      while (typeof changesParsed === "string") {
        changesParsed = JSON.parse(changesParsed);
      }
    } catch {
      return res.status(400).json({
        success: false,
        message: "Format de 'changes' invalide",
      });
    }

    if (!Array.isArray(changesParsed) || changesParsed.length === 0) {
      return res.status(400).json({
        success: false,
        message: "'changes' doit être un tableau non vide",
      });
    }

    const results = [];

    for (const entry of changesParsed) {
      const values = fields.map((f) => entry[f]);
      if (values.some((v) => v === undefined || v === null || v === ""))
        continue;

      const escaped = values.map((v) => SqlString.escape(v)).join(", ");
      const columns = fields
        .map((f) => `\`${f === "ClientId" ? "IdClient" : f}\``)
        .join(", ");

      const query = `INSERT INTO \`${tableName}\` (${columns}) VALUES (${escaped});`;

      const result = await ConnectionPoolService.executeQuery(user, query);
      results.push(result);
    }

    res.status(200).json({
      success: true,
      message: successMessage,
      data: results,
    });
  } catch (error) {
    console.error(
      `Erreur lors de l'insertion dans ${options.tableName} :`,
      error
    );
    logger.error(
      `Erreur lors de l'insertion dans ${options.tableName} :`,
      error
    );
    res.status(500).json({
      success: false,
      message: `Erreur lors de l'insertion dans ${options.tableName}`,
    });
  }
};

function formatDateForMariaDB(val) {
  if (typeof val === "string" && /^\d{4}-\d{2}-\d{2}T/.test(val)) {
    const d = new Date(val);
    return d.toISOString().slice(0, 19).replace("T", " ");
  }
  return val;
}

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
