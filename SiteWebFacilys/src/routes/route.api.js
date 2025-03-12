const express = require("express");
const router = express.Router();
const apiController = require("../controllers/api.controller");
const { authenticateToken } = require("../middleware/auth");

router.get("/version", apiController.getVersion);
router.post("/login", apiController.login);
router.post("/query", authenticateToken, apiController.executeQuery);

module.exports = router;